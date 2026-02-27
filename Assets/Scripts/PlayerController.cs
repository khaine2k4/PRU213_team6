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

    private bool isGrounded;
    private Animator animator;
    private Rigidbody2D rb;
    private GameManager gameManager;
    private AudioManager audioManager;

    public int currentDamage = 10;
    public GameObject weaponOnHand;
    private bool hasWeapon = false;
    public float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.1f;

    private bool isDashing = false;

    [SerializeField] private GameObject dashEffectObject;
    



    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindAnyObjectByType<GameManager>();
        audioManager = FindAnyObjectByType<AudioManager>();
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
    }

    void Update()
    {
        if (gameManager.IsGameOver() || gameManager.IsGameWin()) return;

        HandleMovement();
        HandleJump();
        HandleAttack();
        HandleDash();
        UpdateAnimation();

        if (Input.GetKeyDown(KeyCode.H))
        {
            GetComponent<Health>().TakeDamage(1);
        }
    }

    private void HandleMovement()
    {
        if (isDashing) return;

        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (audioManager) audioManager.playjumpsound();
        }
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.J) && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    private void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            Dash();
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
                continue;
            }

            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentDamage);
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
    }

    public void EquipWeapon(int damageBonus)
    {
        currentDamage += damageBonus;
        hasWeapon = true;
        if (weaponOnHand != null) weaponOnHand.SetActive(true);
        Debug.Log("Đã trang bị vũ khí! Damage mới: " + currentDamage);
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
            damage = currentDamage,
            hasWeapon = hasWeapon,
            weaponActive = weaponOnHand != null && weaponOnHand.activeSelf
        };
        
        Debug.Log($"Player Save: Pos({data.posX:F1},{data.posY:F1}), HP:{data.health}, Damage:{data.damage}, Weapon:{data.hasWeapon}");
        return data;
    }

    public void LoadData(object data)
    {
        if (data is PlayerData playerData)
        {
            // Restore position
            transform.position = new Vector3(playerData.posX, playerData.posY, playerData.posZ);

            // Restore health
            Health health = GetComponent<Health>();
            if (health != null)
            {
                health.currentHealth = playerData.health;
            }

            // Restore damage
            currentDamage = playerData.damage;

            // Restore weapon state
            hasWeapon = playerData.hasWeapon;
            if (weaponOnHand != null)
            {
                weaponOnHand.SetActive(playerData.weaponActive);
            }

            Debug.Log($"Player Load: Pos({playerData.posX:F1},{playerData.posY:F1}), HP:{playerData.health}, Damage:{playerData.damage}, Weapon:{playerData.hasWeapon}");
        }
    }
}