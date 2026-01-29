using UnityEngine;

public class PlayerController : MonoBehaviour
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

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindAnyObjectByType<GameManager>();
        audioManager = FindAnyObjectByType<AudioManager>();
    }

    void Start()
    {
        if (weaponOnHand != null && weaponOnHand.activeSelf)
        {
            hasWeapon = true;
        }
    }

    void Update()
    {
        if (gameManager.IsGameOver() || gameManager.IsGameWin()) return;

        HandleMovement();
        HandleJump();
        HandleAttack();
        UpdateAnimation();

        if (Input.GetKeyDown(KeyCode.H))
        {
            GetComponent<Health>().TakeDamage(1);
        }
    }

    private void HandleMovement()
    {
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
}