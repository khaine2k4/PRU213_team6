using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    
    public int damageBonus = 5;

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.EquipWeapon(damageBonus);
                Destroy(gameObject);
            }
        }
    }
}
