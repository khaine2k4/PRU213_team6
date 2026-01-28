using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
     private GameManager gameManager;
         private AudioManager audioManager;
    private Health playerHealth; 
    private void Awake()
     {
        gameManager = FindAnyObjectByType<GameManager>(); 
        audioManager = FindAnyObjectByType<AudioManager>();
        playerHealth = GetComponent<Health>(); 
    }
         
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            gameManager.addScore(1);
            Destroy(collision.gameObject);
            // Debug.Log("Coin");
            audioManager.playcoinsound();
        }
        if (collision.CompareTag("Trap"))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1); // Trừ 1 máu
            }
        }

        // Chạm Kẻ địch (Enemy) - CHỈ TRỪ MÁU
        if (collision.CompareTag("Enemy"))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1); // Trừ 1 máu
            }
        }
        if (collision.CompareTag("Key"))
        {
            gameManager.GameWin();  
              Debug.Log("wimnnnn");
            Destroy(collision.gameObject);
           
        }
    }

    
}
