using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Core Save/Load system using JSON file
/// Singleton pattern - accessible from anywhere
/// </summary>
public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SaveManager");
                _instance = go.AddComponent<SaveManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private string saveFilePath;
    private const string SAVE_FILE_NAME = "savegame.json";

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Set save file path
        saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        Debug.Log($"Save file path: {saveFilePath}");
    }

    /// <summary>
    /// Save entire game state
    /// </summary>
    public void SaveGame()
    {
        try
        {
            GameSaveData saveData = new GameSaveData();

            // Save meta info
            saveData.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            saveData.sceneName = SceneManager.GetActiveScene().name;
            saveData.playTime = Time.timeSinceLevelLoad;

            // Find and save GameManager
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                saveData.score = gameManager.score;
                Debug.Log($"Saving Score: {saveData.score}");
            }

            // Find and save Player
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && player is ISaveable)
            {
                saveData.player = (PlayerData)((ISaveable)player).SaveData();
            }

            // Find and save Boss
            BossController boss = FindFirstObjectByType<BossController>();
            if (boss != null && boss is ISaveable)
            {
                saveData.boss = (BossData)((ISaveable)boss).SaveData();
            }
            else
            {
                saveData.boss.exists = false;
            }

            // Find and save all Enemies
            Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            saveData.enemies = new List<EnemyData>();
            foreach (Enemy enemy in enemies)
            {
                if (enemy is ISaveable)
                {
                    EnemyData enemyData = (EnemyData)((ISaveable)enemy).SaveData();
                    saveData.enemies.Add(enemyData);
                    Debug.Log($"Saved enemy: {enemyData.enemyID}, Health: {enemyData.currentHealth}, Alive: {enemyData.isAlive}");
                }
            }

            // Find and save all Items (weapons, pickups) - include inactive (already picked up) ones
            SwordPickup[] items = FindObjectsByType<SwordPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            saveData.items = new List<ItemData>();
            foreach (SwordPickup item in items)
            {
                if (item is ISaveable)
                {
                    ItemData itemData = (ItemData)((ISaveable)item).SaveData();
                    saveData.items.Add(itemData);
                    Debug.Log($"Saved item: {itemData.itemID}, PickedUp: {itemData.isPickedUp}");
                }
            }

            // Find and save all Coins - include inactive (already collected) ones
            CoinPickup[] coins = FindObjectsByType<CoinPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            saveData.coins = new List<ItemData>();
            foreach (CoinPickup coin in coins)
            {
                if (coin is ISaveable)
                {
                    ItemData coinData = (ItemData)((ISaveable)coin).SaveData();
                    saveData.coins.Add(coinData);
                    Debug.Log($"Saved coin: {coinData.itemID}, Collected: {coinData.isPickedUp}");
                }
            }

            // Convert to JSON
            string json = JsonUtility.ToJson(saveData, true); // true = pretty print

            // Write to file
            File.WriteAllText(saveFilePath, json);

            Debug.Log($"=== GAME SAVED === {saveData.enemies.Count} enemies, {saveData.items.Count} items, Boss: {saveData.boss.exists}");
            Debug.Log($"Save location: {saveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    /// <summary>
    /// Load game state from file
    /// </summary>
    public void LoadGame()
    {
        if (!HasSaveData())
        {
            Debug.LogWarning("No save data found!");
            return;
        }

        try
        {
            // Read JSON from file
            string json = File.ReadAllText(saveFilePath);

            // Parse JSON
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

            if (saveData == null)
            {
                Debug.LogError("Failed to parse save data!");
                return;
            }

            Debug.Log($"Loading save from: {saveData.saveDate}");

            // Load scene if different
            if (saveData.sceneName != SceneManager.GetActiveScene().name)
            {
                // Store data temporarily and load after scene loads
                PlayerPrefs.SetString("TempSaveData", json);
                SceneManager.LoadScene(saveData.sceneName);
                return;
            }

            // Apply loaded data
            ApplySaveData(saveData);

            Debug.Log("Game loaded successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
        }
    }

    /// <summary>
    /// Apply save data to game objects
    /// </summary>
    private void ApplySaveData(GameSaveData saveData)
    {
        // Load GameManager and update score
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.score = saveData.score;
            Debug.Log($"Loading Score: {saveData.score}");
            // Force update UI
            gameManager.SendMessage("UpdateScore", SendMessageOptions.DontRequireReceiver);
        }

        // Load Player
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null && player is ISaveable)
        {
            ((ISaveable)player).LoadData(saveData.player);
        }

        // Load Boss
        if (saveData.boss.exists)
        {
            BossController boss = FindFirstObjectByType<BossController>();
            if (boss != null && boss is ISaveable)
            {
                ((ISaveable)boss).LoadData(saveData.boss);
            }
        }
        else
        {
            // Boss was dead, destroy it if it exists
            BossController boss = FindFirstObjectByType<BossController>();
            if (boss != null)
            {
                Destroy(boss.gameObject);
            }
        }

        // Load Enemies
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        // Create dictionary for quick lookup
        Dictionary<string, EnemyData> savedEnemies = new Dictionary<string, EnemyData>();
        foreach (var enemyData in saveData.enemies)
        {
            if (enemyData.isAlive)
            {
                savedEnemies[enemyData.enemyID] = enemyData;
                Debug.Log($"Saved enemy in data: {enemyData.enemyID}, Health: {enemyData.currentHealth}");
            }
        }

        Debug.Log($"Found {enemies.Length} enemies in scene, {savedEnemies.Count} in save data");

        // Apply data or destroy
        foreach (Enemy enemy in enemies)
        {
            // Get enemy unique ID - use Enemy's own method to ensure consistency
            string enemyID = enemy.GetEnemyID();
            
            Debug.Log($"Checking enemy {enemyID} in scene...");
            
            if (savedEnemies.ContainsKey(enemyID))
            {
                // This enemy was alive, load its data
                Debug.Log($"✓ Restoring enemy {enemyID}");
                if (enemy is ISaveable)
                {
                    ((ISaveable)enemy).LoadData(savedEnemies[enemyID]);
                }
            }
            else
            {
                // This enemy was dead, destroy it
                Debug.Log($"✗ Destroying enemy {enemyID} - not in save data");
                Destroy(enemy.gameObject);
            }
        }

        // Load Items (weapons, pickups) - include inactive ones so we can restore their state
        SwordPickup[] items = FindObjectsByType<SwordPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        // Create dictionary for quick lookup
        Dictionary<string, ItemData> savedItems = new Dictionary<string, ItemData>();
        foreach (var itemData in saveData.items)
        {
            savedItems[itemData.itemID] = itemData;
        }

        Debug.Log($"Found {items.Length} items in scene, {savedItems.Count} in save data");

        // Apply data to items
        foreach (SwordPickup item in items)
        {
            string itemID = item.GetItemID();
            
            if (savedItems.ContainsKey(itemID))
            {
                // Load item state
                if (item is ISaveable)
                {
                    ((ISaveable)item).LoadData(savedItems[itemID]);
                    Debug.Log($"✓ Restored item {itemID}, PickedUp: {savedItems[itemID].isPickedUp}");
                }
            }
        }

        // Load Coins - include inactive (already collected) ones
        CoinPickup[] coins = FindObjectsByType<CoinPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        Dictionary<string, ItemData> savedCoins = new Dictionary<string, ItemData>();
        if (saveData.coins != null)
        {
            foreach (var coinData in saveData.coins)
            {
                savedCoins[coinData.itemID] = coinData;
            }
        }

        Debug.Log($"Found {coins.Length} coins in scene, {savedCoins.Count} in save data");

        foreach (CoinPickup coin in coins)
        {
            string coinID = coin.GetCoinID();
            if (savedCoins.ContainsKey(coinID))
            {
                if (coin is ISaveable)
                {
                    ((ISaveable)coin).LoadData(savedCoins[coinID]);
                    Debug.Log($"✓ Restored coin {coinID}, Collected: {savedCoins[coinID].isPickedUp}");
                }
            }
            else
            {
                // Coin not in save data = new coin, make sure it's visible
                coin.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Check if save file exists
    /// </summary>
    public bool HasSaveData()
    {
        return File.Exists(saveFilePath);
    }

    /// <summary>
    /// Delete save file
    /// </summary>
    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Save file deleted!");
        }
    }

    /// <summary>
    /// Get save file info
    /// </summary>
    public string GetSaveInfo()
    {
        if (!HasSaveData()) return "No save data";

        try
        {
            string json = File.ReadAllText(saveFilePath);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            return $"Last Save: {data.saveDate}\nScene: {data.sceneName}\nScore: {data.score}";
        }
        catch
        {
            return "Error reading save";
        }
    }

    /// <summary>
    /// Called after scene loads to apply temp data
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if we have temp save data
        if (PlayerPrefs.HasKey("TempSaveData"))
        {
            string json = PlayerPrefs.GetString("TempSaveData");
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
            
            // Wait a frame for scene to initialize
            StartCoroutine(DelayedLoad(saveData));
            
            PlayerPrefs.DeleteKey("TempSaveData");
        }
    }

    private System.Collections.IEnumerator DelayedLoad(GameSaveData saveData)
    {
        yield return null; // Wait one frame
        ApplySaveData(saveData);
    }
}
