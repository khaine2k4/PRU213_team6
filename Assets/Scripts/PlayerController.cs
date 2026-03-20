using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISaveable
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    [Header("Combat Settings")]
    [SerializeField] public Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float attackCooldown = 0.5f;
    private float lastAttackTime = 0f;

    [Header("Defense Settings")]
    [SerializeField] private KeyCode blockKey = KeyCode.K;
    [SerializeField, Range(0f, 1f)] private float blockedDamageMultiplier = 0.2f;
    [SerializeField] private float perfectBlockWindow = 0.15f;
    [SerializeField, Range(30f, 180f)] private float blockFrontAngle = 120f;
    
    [Header("Stamina Settings (Hidden)")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 20f;
    [SerializeField] private float jumpStaminaCost = 15f;
    [SerializeField] private float dashStaminaCost = 30f;
    [SerializeField] private float staminaRegenDelay = 1f;
    [SerializeField, Range(0f, 1f)] private float lowStaminaSpeedMultiplier = 0.5f;
    [SerializeField] private float lowStaminaThreshold = 20f;

    private float currentStamina;
    private float lastStaminaUseTime;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;

    private bool isGrounded;
    private Animator animator;
    private Rigidbody2D rb;
    private GameManager gameManager;
    private AudioManager audioManager;

    public int currentDamage = 10;
    public int currentLevel = 1;
    public int currentExp = 0;
    private int expToNextLevel = 10;
    [SerializeField] private float lifeStealAmount = 0.5f;
    public GameObject weaponOnHand;
    private bool hasWeapon = false;
    public float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.1f;

    private bool isDashing = false;
    private bool isBlocking = false;
    private bool hasBlockingAnimParam = false;
    private float blockStartTime = -999f;
    private bool lastShieldUIState = false;
    private Health playerHealth;

    [SerializeField] private GameObject dashEffectObject;
    [SerializeField] private GameObject shieldUIObject;

    public bool IsBlocking => isBlocking;
    public float BlockedDamageMultiplier => blockedDamageMultiplier;
    public bool IsPerfectBlocking => isBlocking && Time.time <= blockStartTime + perfectBlockWindow;
    

    [Header("Ranged Combat (Object Pooling)")]
    [SerializeField] public bool canShoot = false;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] fireballs; // Kéo các quả cầu lửa đã tạo sẵn trong Scene vào đây
  

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindAnyObjectByType<GameManager>();
        audioManager = FindAnyObjectByType<AudioManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<Health>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        currentStamina = maxStamina;

        if (animator != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Bool && param.name == "isBlocking")
                {
                    hasBlockingAnimParam = true;
                    break;
                }
            }
        }
    }

    void Start()
    {
        // Always hide weapon at start - only show via EquipWeapon() or LoadData()
        hasWeapon = false;
        if (weaponOnHand != null)
        {
            weaponOnHand.SetActive(false);
        }

        // Tắt hiệu ứng dash khi bắt đầu
        if (dashEffectObject != null)
        {
            dashEffectObject.SetActive(false);
        }

        if (shieldUIObject != null)
        {
            shieldUIObject.SetActive(false);
            lastShieldUIState = false;
        }
    }

    void Update()
    {
        if (gameManager.IsGameOver() || gameManager.IsGameWin()) return;

        HandleBlock();
        SyncShieldUI();
        HandleMovement();
        HandleJump();
        HandleAttack();
        HandleDash();
        RegenerateStamina();
        UpdateAnimation();

        if (Input.GetKeyDown(KeyCode.H))
        {
            GetComponent<Health>().TakeDamage(1);
        }
    }

    private void HandleMovement()
    {
        if (isDashing) return;

        if (isBlocking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float moveInput = Input.GetAxis("Horizontal");
        
        // Calculate move speed based on stamina
        float effectiveMoveSpeed = moveSpeed;
        if (currentStamina < lowStaminaThreshold)
        {
            effectiveMoveSpeed *= lowStaminaSpeedMultiplier;
        }

        rb.linearVelocity = new Vector2(moveInput * effectiveMoveSpeed, rb.linearVelocity.y);
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    private void HandleJump()
    {
        if (isBlocking) return;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            if (currentStamina >= jumpStaminaCost)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                if (audioManager) audioManager.playjumpsound();
                
                currentStamina -= jumpStaminaCost;
                lastStaminaUseTime = Time.time;
            }
            else
            {
                // Feedback: Flash Red when not enough stamina
                FlashFeedback();
            }
        }
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void HandleAttack()
    {
        if (isBlocking) return;

        // Kiểm tra cooldown và phím bấm (J)
        if (Input.GetKeyDown(KeyCode.J) && Time.time >= lastAttackTime + attackCooldown)
        {
            if (canShoot)
            {
                ShootAttack(); // Tấn công tầm xa cho nhân vật mới
            }
            else
            {
                Attack(); // Tấn công cận chiến cho nhân vật cũ
            }

            lastAttackTime = Time.time;
        }
    }

    private void HandleBlock()
    {
        if (isDashing)
        {
            isBlocking = false;
            return;
        }

        bool isHoldingBlock = Input.GetKey(blockKey);

        if (isHoldingBlock && !isBlocking)
        {
            blockStartTime = Time.time;
        }

        isBlocking = isHoldingBlock;
    }

    void ShootAttack()
    {
        // 1. Kích hoạt Animation
        animator.SetTrigger("Attack");

        // 2. Tìm quả cầu lửa đang rảnh (Active = false)
        int index = FindFireball();

        // 3. Thiết lập vị trí và hướng
        if (fireballs[index] != null)
        {
            // Đặt vị trí về FirePoint
            fireballs[index].transform.position = firePoint.position;

            // Quan trọng: Phải Active trước khi gọi SetDirection
            fireballs[index].SetActive(true);

            // Truyền hướng dựa trên Scale của Player (1 hoặc -1)
            float direction = Mathf.Sign(transform.localScale.x);
            fireballs[index].GetComponent<Projectile>().SetDirection(direction);
        }
    }

    private int FindFireball()
    {
        for (int i = 0; i < fireballs.Length; i++)
        {
            if (!fireballs[i].activeInHierarchy)
                return i;
        }
        return 0; // Nếu tất cả đang bay, lấy cái đầu tiên (hoặc có thể mở rộng mảng)
    }

    private void SyncShieldUI()
    {
        if (shieldUIObject == null) return;
        if (lastShieldUIState == isBlocking) return;

        shieldUIObject.SetActive(isBlocking);
        lastShieldUIState = isBlocking;
    }

    public bool CanBlockAttackFrom(Vector2 attackerPosition)
    {
        if (!isBlocking) return false;

        Vector2 toAttacker = attackerPosition - (Vector2)transform.position;
        if (toAttacker.sqrMagnitude < 0.0001f) return true;

        toAttacker.Normalize();
        float facingX = transform.localScale.x >= 0f ? 1f : -1f;
        Vector2 forward = new Vector2(facingX, 0f);
        float minDot = Mathf.Cos((blockFrontAngle * 0.5f) * Mathf.Deg2Rad);

        return Vector2.Dot(forward, toAttacker) >= minDot;
    }

    private void HandleDash()
    {
        if (isBlocking) return;

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            if (currentStamina >= dashStaminaCost)
            {
                Dash();
                currentStamina -= dashStaminaCost;
                lastStaminaUseTime = Time.time;
            }
            else
            {
                FlashFeedback();
            }
        }
    }

    private void Dash()
    {
        float dashDirection = transform.localScale.x;
        rb.linearVelocity = new Vector2(dashForce * dashDirection, rb.linearVelocity.y);
        isDashing = true;
        dashEffectObject.SetActive(true);
        StartCoroutine(StopDash());
    }

    private IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashDuration);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        isDashing = false;
        dashEffectObject.SetActive(false);  
    }

    private void RegenerateStamina()
    {
        if (Time.time >= lastStaminaUseTime + staminaRegenDelay)
        {
            currentStamina = Mathf.MoveTowards(currentStamina, maxStamina, staminaRegenRate * Time.deltaTime);
        }
    }

    private void FlashFeedback()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    void Attack()
    {
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");

        if (hasWeapon && weaponOnHand != null)
        {
            weaponOnHand.SetActive(false);
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Debug.Log("Đánh trúng: " + enemyCollider.name);

            BossController boss = enemyCollider.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(currentDamage);
                ApplyLifeSteal();
                continue;
            }

            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentDamage);
                ApplyLifeSteal();
            }

        }
    }

    public void ShowWeapon()
    {
        if (hasWeapon && weaponOnHand != null)
        {
            weaponOnHand.SetActive(true);
        }
    }


    private void UpdateAnimation()
    {
        bool isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        animator.SetBool("isRunning", isRunning);
        bool isJumping = !isGrounded;
        animator.SetBool("isJumping", isJumping);

        if (hasBlockingAnimParam)
        {
            animator.SetBool("isBlocking", isBlocking);
        }
    }

    public void EquipWeapon(int damageBonus)
    {
        currentDamage += damageBonus;
        hasWeapon = true;
        if (weaponOnHand != null) weaponOnHand.SetActive(true);
        Debug.Log("Đã trang bị vũ khí! Damage mới: " + currentDamage);
    }

    public void GainExp(int amount)
    {
        currentExp += amount;
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            currentLevel++;
            expToNextLevel *= 2;
            currentDamage += 1;
            lifeStealAmount += 0.5f;
            if (playerHealth != null)
            {
                playerHealth.Heal(1f);
            }
            Debug.Log($"Level Up! Level: {currentLevel}, Damage: {currentDamage}, Lifesteal: {lifeStealAmount}, EXP cần cho level tiếp: {expToNextLevel}");
        }
        Debug.Log($"EXP: {currentExp}/{expToNextLevel}");
    }

    private void ApplyLifeSteal()
    {
        if (playerHealth == null) return;
        playerHealth.Heal(lifeStealAmount);
    }

    public void OnDealDamage()
    {
        ApplyLifeSteal();
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    // ===== ISaveable Implementation =====
    public object SaveData()
    {
        PlayerData data = new PlayerData
        {
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,
            health = GetComponent<Health>() ? GetComponent<Health>().currentHealth : 10f,
            currentStamina = currentStamina,
            maxStamina = maxStamina,
            damage = currentDamage,
            hasWeapon = hasWeapon,
            weaponActive = weaponOnHand != null && weaponOnHand.activeSelf,
            currentLevel = currentLevel,
            currentExp = currentExp,
            expToNextLevel = expToNextLevel,
            lifeStealAmount = lifeStealAmount
        };
        
        Debug.Log($"Player Save: Pos({data.posX:F1},{data.posY:F1}), HP:{data.health}, Stamina:{data.currentStamina:F0}, Damage:{data.damage}, Weapon:{data.hasWeapon}, Level:{data.currentLevel}, EXP:{data.currentExp}/{data.expToNextLevel}, Lifesteal:{data.lifeStealAmount}");
        return data;
    }

    public void LoadData(object data)
    {
        if (data is PlayerData playerData)
        {
            // Restore position
            transform.position = new Vector3(playerData.posX, playerData.posY, playerData.posZ);

            // Restore HP/Stamina
            Health health = GetComponent<Health>();
            if (health != null) health.currentHealth = playerData.health;
            currentStamina = playerData.currentStamina > 0 ? playerData.currentStamina : playerData.maxStamina;
            maxStamina = playerData.maxStamina > 0 ? playerData.maxStamina : 100f;

            // Restore damage
            currentDamage = playerData.damage;
            
            // ... (rest of weapon/exp load)
            hasWeapon = playerData.hasWeapon;
            if (weaponOnHand != null)
            {
                weaponOnHand.SetActive(playerData.weaponActive);
            }

            // Restore EXP / level
            currentLevel = playerData.currentLevel > 0 ? playerData.currentLevel : 1;
            currentExp = playerData.currentExp;
            expToNextLevel = playerData.expToNextLevel > 0 ? playerData.expToNextLevel : 10;
            lifeStealAmount = playerData.lifeStealAmount > 0 ? playerData.lifeStealAmount : 0.5f;

            Debug.Log($"Player Load: Pos({playerData.posX:F1},{playerData.posY:F1}), HP:{playerData.health}, Stamina:{currentStamina:F0}, Damage:{playerData.damage}, Weapon:{playerData.hasWeapon}, Level:{currentLevel}, EXP:{currentExp}/{expToNextLevel}, Lifesteal:{lifeStealAmount}");
        }
    }
}