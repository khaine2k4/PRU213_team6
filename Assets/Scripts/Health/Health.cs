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
        // Nếu đang trong thời gian bất tử thì không nhận thêm sát thương
        if (iFramesTimer > 0) return;

        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0)
        {
            if (anim != null) anim.SetTrigger("Hurt");
            iFramesTimer = iFramesDuration; // Kích hoạt thời gian bất tử
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
}