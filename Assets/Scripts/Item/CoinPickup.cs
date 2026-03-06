using UnityEngine;

public class CoinPickup : MonoBehaviour, ISaveable
{
    private bool isCollected = false;
    private string coinID;

    private void Awake()
    {
        // Generate unique ID based on initial position
        Vector3 pos = transform.position;
        coinID = $"Coin_{pos.x:F2}_{pos.y:F2}";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            GameManager gameManager = FindAnyObjectByType<GameManager>();
            if (gameManager != null) gameManager.addScore(1);

            AudioManager audioManager = FindAnyObjectByType<AudioManager>();
            if (audioManager != null) audioManager.playcoinsound();

            isCollected = true;
            gameObject.SetActive(false);
            Debug.Log($"Coin collected: {coinID}");
        }
    }

    // ISaveable Implementation
    public object SaveData()
    {
        return new ItemData
        {
            itemID = this.coinID,
            isPickedUp = this.isCollected
        };
    }

    public void LoadData(object data)
    {
        if (data is ItemData itemData)
        {
            isCollected = itemData.isPickedUp;
            gameObject.SetActive(!isCollected);
            Debug.Log($"Coin {coinID} loaded - collected: {isCollected}");
        }
    }

    public string GetCoinID()
    {
        return coinID;
    }
}
