using UnityEngine;
using UnityEngine.UI; 

public class Enemy : MonoBehaviour, ISaveable
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float distance = 5f;
    private Vector3 startPos;
    private bool movingRight = true;

    [Header("Health Settings")] 
    [SerializeField] private float maxHealth = 100f; 
    private float currentHealth;
    [SerializeField] private Image healthBarFill;

    void Awake()
    {
        // Save starting position immediately when object is created
        startPos = transform.position;
    }

    void Start()
    {
        // startPos already set in Awake
        if (startPos == Vector3.zero)
        {
            startPos = transform.position;
        }
        currentHealth = maxHealth; 
        UpdateHealthBar();
    }

    void Update()
    {
        
        float leftBound = startPos.x - distance;
        float rightBound = startPos.x + distance;

        if (movingRight)
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime);
            if (transform.position.x >= rightBound)
            {
                movingRight = false;
                Flip();
            }
        }
        else
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime);
            if (transform.position.x <= leftBound)
            {
                movingRight = true;
                Flip();
            }
        }
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy bị đánh! Máu còn: " + currentHealth);

        
        UpdateHealthBar();

        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        Debug.Log("Enemy đã chết!");
        Destroy(gameObject);
    }
  

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }
    }

    // ===== ISaveable Implementation =====
    public object SaveData()
    {
        EnemyData data = new EnemyData
        {
            enemyID = GetEnemyID(),
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,
            currentHealth = currentHealth,
            maxHealth = maxHealth,
            isAlive = currentHealth > 0,
            movingRight = movingRight
        };
        return data;
    }

    public void LoadData(object data)
    {
        if (data is EnemyData enemyData)
        {
            // Restore position (current position, not starting position)
            transform.position = new Vector3(enemyData.posX, enemyData.posY, enemyData.posZ);
            
            // IMPORTANT: Don't change startPos! Keep the original starting position
            // startPos should remain as set in Awake()

            // Restore health
            currentHealth = enemyData.currentHealth;
            maxHealth = enemyData.maxHealth;
            UpdateHealthBar();

            // Restore direction
            movingRight = enemyData.movingRight;
            
            // Make sure sprite is facing correct direction
            if ((movingRight && transform.localScale.x < 0) || 
                (!movingRight && transform.localScale.x > 0))
            {
                Flip();
            }

            Debug.Log($"Enemy {enemyData.enemyID} loaded! StartPos: {startPos}, CurrentPos: {transform.position}");
        }
    }

    // Public method để SaveManager có thể get ID
    public string GetEnemyID()
    {
        // Use starting position as unique ID
        Vector3 pos = startPos != Vector3.zero ? startPos : transform.position;
        return $"Enemy_{pos.x:F2}_{pos.y:F2}";
    }
}