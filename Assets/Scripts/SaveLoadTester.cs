using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple F5 Save and Quit to Menu
/// Attach to GameManager or any GameObject in game scene
/// </summary>
public class SaveLoadTester : MonoBehaviour
{
    private void Update()
    {
        // F5 = Save and return to menu
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveAndReturnToMenu();
        }
    }

    private void SaveAndReturnToMenu()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
            Debug.Log("Game saved! Returning to menu...");
        }

        Time.timeScale = 1; // Reset time scale
        SceneManager.LoadScene("Menu");
    }
}
