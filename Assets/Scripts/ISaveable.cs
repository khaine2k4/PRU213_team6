using UnityEngine;

/// <summary>
/// Interface for objects that can be saved/loaded
/// Implement this on GameManager, PlayerController, Enemy, Boss, etc.
/// </summary>
public interface ISaveable
{
    /// <summary>
    /// Save current state and return data object
    /// </summary>
    object SaveData();

    /// <summary>
    /// Load and apply data to this object
    /// </summary>
    void LoadData(object data);
}
