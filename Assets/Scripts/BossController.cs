using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour, ISaveable
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;

    [Header("HP")]
    [SerializeField] private float maxHealth = 300f;
    private float currentHealth;
    [SerializeField] private Image healthBarFill;

    [Header("Contact Damage (optional)")]
    [SerializeField] private float touchDamage = 1f;

    [Header("Face Player")]
    [SerializeField] private float faceDeadzone = 0.05f;
    private bool facingRight = true;

    [Header("Melee: 3 hits + step forward")]
    [SerializeField] private int meleeHits = 3;
    [SerializeField] private float meleeHitInterval = 0.35f;
    [SerializeField] private float meleeStepSpeed = 5f;
    [SerializeField] private float meleeStepTime = 0.3f;
    [SerializeField] private float meleeHitboxDuration = 0.10f;
    [SerializeField] private Vector2 meleeHitboxSize = new Vector2(1.2f, 1.0f);
    [SerializeField] private Vector2 meleeHitboxOffset = new Vector2(0.9f, 0f);

    [Header("Charge (Dash)")]
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeDuration = 0.9f;
    [SerializeField] private float chargeHitboxExtraWidth = 0.2f;
    [SerializeField] private float chargeCooldown = 0.5f;

    [Header("Loop")]
    [SerializeField] private float loopDelay = 0.3f;
    private bool busy;

    [Header("Death")]
    [SerializeField] private float destroyDelay = 1.0f; // chỉnh = length clip BossDie

    private Health playerHealth;
    private const float playerIFrameNudge = 0.02f;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
            playerHealth = player.GetComponent<Health>();

        UpdateHealthBar();
        StartCoroutine(BossLoop());
    }

    private void Update()
    {
        if (!busy) FacePlayer();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Boss take damage!!! Current health: " + currentHealth);

        UpdateHealthBar();
        if (currentHealth <= 0f) Die();
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
    }

    private void Die()
    {
        if (animator != null) animator.SetBool("Dead", true);

        StopAllCoroutines();
        busy = true;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        PlayerController playerController = FindAnyObjectByType<PlayerController>();
        if (playerController != null) playerController.GainExp(10);

        Debug.Log("Boss die");
        Destroy(gameObject, destroyDelay);
    }

    // ===== LOOP =====
    private IEnumerator BossLoop()
    {
        while (true)
        {
            if (player == null)
            {
                yield return null;
                continue;
            }

            // 1) melee 3 hits
            yield return MeleeCombo();

            yield return new WaitForSeconds(loopDelay);

            // 2) charge
            yield return Charge();

            yield return new WaitForSeconds(chargeCooldown);
        }
    }

    // ===== FACE PLAYER =====
    private void FacePlayer()
    {
        if (player == null) return;

        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) < faceDeadzone) return;

        bool shouldFaceRight = dx > 0f;
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            Vector3 s = transform.localScale;
            s.x *= -1;
            transform.localScale = s;
        }
    }

    // ===== SKILL 1: MELEE 3 HIT =====
    private IEnumerator MeleeCombo()
    {
        busy = true;
        if (animator != null) animator.SetTrigger("Melee");

        for (int i = 0; i < meleeHits; i++)
        {
            FacePlayer();

            yield return DoMeleeHitbox(meleeHitboxDuration);

            float t = 0f;
            while (t < meleeStepTime)
            {
                t += Time.deltaTime;
                float dir = facingRight ? 1f : -1f;

                rb.MovePosition(rb.position + Vector2.right * dir * meleeStepSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(meleeHitInterval);
        }

        busy = false;
    }

    private IEnumerator DoMeleeHitbox(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            Vector2 center = (Vector2)transform.position + GetFacingOffset(meleeHitboxOffset);
            Collider2D hit = Physics2D.OverlapBox(center, meleeHitboxSize, 0f);

            if (hit != null && hit.CompareTag("Player"))
            {
                DealDamageToPlayer(touchDamage);
                yield break;
            }

            yield return null;
        }
    }

    // ===== SKILL 2: CHARGE =====
    private IEnumerator Charge()
    {
        busy = true;
        if (animator != null) animator.SetTrigger("Charge");

        FacePlayer();
        float dir = facingRight ? 1f : -1f;

        float elapsed = 0f;
        while (elapsed < chargeDuration)
        {
            elapsed += Time.deltaTime;

            rb.MovePosition(rb.position + Vector2.right * dir * chargeSpeed * Time.deltaTime);

            Vector2 size = GetChargeHitboxSize();
            Vector2 center = (Vector2)transform.position + new Vector2(dir * (size.x * 0.5f), 0f);
            Collider2D hit = Physics2D.OverlapBox(center, size, 0f);

            if (hit != null && hit.CompareTag("Player"))
                DealDamageToPlayer(touchDamage);

            yield return null;
        }

        busy = false;
    }

    private Vector2 GetChargeHitboxSize()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
            return new Vector2(col.size.x + chargeHitboxExtraWidth, col.size.y);

        return new Vector2(1.2f, 1.2f);
    }

    private void DealDamageToPlayer(float dmg)
    {
        if (playerHealth == null) return;

        playerHealth.TakeDamage(dmg, transform.position);

        if (rb != null) rb.position += Vector2.up * playerIFrameNudge;
    }

    private Vector2 GetFacingOffset(Vector2 offset)
    {
        return new Vector2(facingRight ? offset.x : -offset.x, offset.y);
    }

    // ===== DEBUG HITBOX =====
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 center = (Vector2)transform.position + GetFacingOffset(meleeHitboxOffset);
        Gizmos.DrawWireCube(center, meleeHitboxSize);
    }

    // ===== ISaveable Implementation =====
    public object SaveData()
    {
        BossData data = new BossData
        {
            exists = true,
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,
            currentHealth = currentHealth,
            maxHealth = maxHealth,
            facingRight = facingRight,
            isDead = currentHealth <= 0
        };
        return data;
    }

    public void LoadData(object data)
    {
        if (data is BossData bossData)
        {
            // Restore position
            transform.position = new Vector3(bossData.posX, bossData.posY, bossData.posZ);

            // Restore health
            currentHealth = bossData.currentHealth;
            maxHealth = bossData.maxHealth;
            UpdateHealthBar();

            // Restore facing direction
            facingRight = bossData.facingRight;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
            transform.localScale = scale;

            // If boss was alive, restart loop
            if (!bossData.isDead && currentHealth > 0)
            {
                StopAllCoroutines();
                StartCoroutine(BossLoop());
            }

            Debug.Log($"Boss loaded! Health: {currentHealth}/{maxHealth}");
        }
    }
}
