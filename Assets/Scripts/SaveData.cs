using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main save data container - contains all game state
/// </summary>
[System.Serializable]
public class GameSaveData
{
    // Meta information
    public string saveDate;
    public string sceneName;
    public float playTime;
    
    // Game state
    public int score;
    public bool isGameOver;
    public bool isGameWin;
    
    // Player data
    public PlayerData player;
    
    // Boss data (if exists in scene)
    public BossData boss;
    
    // All enemies
    public List<EnemyData> enemies;

    // All items (weapons, pickups, etc.)
    public List<ItemData> items;

    // All coins
    public List<ItemData> coins;

    public GameSaveData()
    {
        saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        enemies = new List<EnemyData>();
        items = new List<ItemData>();
        coins = new List<ItemData>();
        player = new PlayerData();
        boss = new BossData();
    }
}

/// <summary>
/// Player save data
/// </summary>
[System.Serializable]
public class PlayerData
{
    public float posX;
    public float posY;
    public float posZ;
    public float health;
    public int damage;
    public bool hasWeapon;
    public bool weaponActive;
}

/// <summary>
/// Enemy save data with unique ID
/// </summary>
[System.Serializable]
public class EnemyData
{
    public string enemyID;          // Unique identifier
    public float posX;
    public float posY;
    public float posZ;
    public float currentHealth;
    public float maxHealth;
    public bool isAlive;
    public bool movingRight;
}

/// <summary>
/// Boss save data
/// </summary>
[System.Serializable]
public class BossData
{
    public bool exists;             // Does boss exist in scene?
    public float posX;
    public float posY;
    public float posZ;
    public float currentHealth;
    public float maxHealth;
    public bool facingRight;
    public bool isDead;
}

/// <summary>
/// Item save data (weapons, pickups, etc.)
/// </summary>
[System.Serializable]
public class ItemData
{
    public string itemID;           // Unique identifier
    public bool isPickedUp;         // Has been picked up?
}
