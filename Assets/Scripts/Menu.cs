using UnityEngine;
using UnityEngine.SceneManagement;
public class Menu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }   
    public void QuitGame()
    {
        Application.Quit();
    }   
}
