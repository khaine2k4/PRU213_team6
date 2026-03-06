using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple F5 to Save and Quit
/// Optional - can use SaveLoadTester instead
/// </summary>
public class SaveLoadUI : MonoBehaviour
{
    private void Update()
    {
        // F5 = Save and quit to menu
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveAndQuit();
        }
    }

    public void SaveAndQuit()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
            Debug.Log("Game saved! Returning to menu...");
        }

        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
}
}
    
    
