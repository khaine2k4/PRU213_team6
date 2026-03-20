using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float startingHealth = 10f;
    public float currentHealth { get; set; } // Changed to set accessible
    private Animator anim;
    private GameManager gameManager;

    [Header("Invincibility")]
    [SerializeField] private float iFramesDuration = 1f; // 1 giây bất tử
    private float iFramesTimer;

    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        // Đếm ngược thời gian bất tử
        if (iFramesTimer > 0)
            iFramesTimer -= Time.deltaTime;
    }

    public void TakeDamage(float _damage)
    {
        TakeDamage(_damage, null);
    }

    public void TakeDamage(float _damage, Vector2? attackerPosition)
    {
        // Nếu đang trong thời gian bất tử thì không nhận thêm sát thương
        if (iFramesTimer > 0) return;

        float finalDamage = _damage;
        bool blocked = false;

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null && playerController.IsBlocking)
        {
            bool canBlock = !attackerPosition.HasValue || playerController.CanBlockAttackFrom(attackerPosition.Value);
            if (canBlock)
            {
                blocked = true;

                if (playerController.IsPerfectBlocking)
                {
                    finalDamage = 0f;
                }
                else
                {
                    finalDamage *= Mathf.Clamp01(playerController.BlockedDamageMultiplier);
                }
            }
        }

        currentHealth = Mathf.Clamp(currentHealth - finalDamage, 0, startingHealth);

        if (currentHealth > 0)
        {
            if (finalDamage > 0f && anim != null)
            {
                anim.SetTrigger("Hurt");
            }

            if (blocked)
            {
                iFramesTimer = finalDamage <= 0f ? 0.12f : iFramesDuration * 0.5f;
            }
            else
            {
                iFramesTimer = iFramesDuration;
            }
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        if (anim != null) anim.SetTrigger("Death");
        if (GetComponent<PlayerController>() != null)
            GetComponent<PlayerController>().enabled = false;

        if (gameManager != null) gameManager.GameOver();
    }

    public float GetHealthPercentage() => currentHealth / startingHealth;

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, startingHealth);
    }
}