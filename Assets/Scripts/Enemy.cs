using UnityEngine;
using UnityEngine.UI; 

public class Enemy : MonoBehaviour
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

    void Start()
    {
        startPos = transform.position;
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
}