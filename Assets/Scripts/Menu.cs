using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Menu Buttons (Optional)")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button newGameButton;

    /// <summary>
    /// Play game - Auto continue if save exists, otherwise start new game
    /// </summary>
    public void PlayGame()
    {
        Time.timeScale = 1;
        
        // Check if save data exists
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        {
            // Has save → Load and continue
            Debug.Log("Save found! Continuing...");
            SaveManager.Instance.LoadGame();
        }
        else
        {
            // No save → Start new game
            Debug.Log("No save - Starting new game");
            SceneManager.LoadScene("Game");
        }
    }
    
    /// <summary>
    /// Start completely new game (deletes save)
    /// </summary>
    public void StartNewGame()
    {
        // Delete old save
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        {
            SaveManager.Instance.DeleteSave();
            Debug.Log("Old save deleted");
        }

        Time.timeScale = 1;
        SceneManager.LoadScene("Game");
    }

    /// <summary>
    /// Quit game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    /// <summary>
    /// Mở giao diện Cửa hàng bằng cách load Scene Shop
    /// </summary>
    public void OpenShopScene()
    {
        SceneManager.LoadScene("Shop");
    }
}
