using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
     private GameManager gameManager;
         private AudioManager audioManager;
     private void Awake()
     {
        gameManager = FindAnyObjectByType<GameManager>(); 
        audioManager = FindAnyObjectByType<AudioManager>(); 
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
        if(collision.CompareTag("Trap"))
        {
             gameManager.GameOver();    
            // Debug.Log("Ban da dinh bay");
        }
         if(collision.CompareTag("Enemy"))
        {
             gameManager.GameOver();    
            // Debug.Log("Ban da dinh bay");
        }
         if(collision.CompareTag("Key"))
        {
            gameManager.GameWin();  
              Debug.Log("wimnnnn");
            Destroy(collision.gameObject);
           
        }
    }
    
}
