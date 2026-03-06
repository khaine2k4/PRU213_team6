using UnityEngine;

public class SwordPickup : MonoBehaviour, ISaveable
{
    public int damageBonus = 5;
    private bool isPickedUp = false;
    private string itemID;

    private void Awake()
    {
        // Generate unique ID based on position
        Vector3 pos = transform.position;
        itemID = $"Sword_{pos.x:F2}_{pos.y:F2}";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp) return; // Already picked up
        
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.EquipWeapon(damageBonus);
                isPickedUp = true;
                gameObject.SetActive(false); // Hide instead of destroy
                Debug.Log($"Sword picked up: {itemID}");
            }
        }
    }

    // ISaveable Implementation
    public object SaveData()
    {
        return new ItemData
        {
            itemID = this.itemID,
            isPickedUp = this.isPickedUp
        };
    }

    public void LoadData(object data)
    {
        if (data is ItemData itemData)
        {
            isPickedUp = itemData.isPickedUp;
            
            if (isPickedUp)
            {
                gameObject.SetActive(false); // Hide if already picked up
                Debug.Log($"Sword {itemID} was already picked up - hiding");
            }
            else
            {
                gameObject.SetActive(true); // Show if not yet picked up
                Debug.Log($"Sword {itemID} not picked up - showing");
            }
        }
    }

    public string GetItemID()
    {
        return itemID;
    }
}
