using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;  
public class GameManager : MonoBehaviour
{
    public int score = 0 ;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverUi;
     [SerializeField] private GameObject GameWinUi  ;
    private bool isGameOver = false;  
    private bool isGameWin = false;   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         UpdateScore();  
         gameOverUi.SetActive(false);
         GameWinUi.SetActive(false);
    }

  

    public void addScore(int points)
    {  if(!isGameOver&&!isGameWin   )
        {
             score += points;
             UpdateScore(); 
        }
    }


    private void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score:" + score.ToString();
        }
    }

    // Public method để SaveManager có thể gọi sau khi load
    public void UpdateScoreUI()
    {
        UpdateScore();
    }   
    
    public void GameOver()
    {
        isGameOver = true;
        score = 0;
        Time.timeScale = 0;
        gameOverUi.SetActive(true);
    }
    public void GameWin()
    {
        isGameWin = true;
     
        Time.timeScale = 0;
        GameWinUi.SetActive(true);
    }   
    public void RestartGame()
    {
        isGameOver = false;
        score = 0;
        UpdateScore();
        Time.timeScale = 1;
       SceneManager.LoadScene("Game");
    }
    public void GotoMenu()
    {
      
        Time.timeScale = 1;
       SceneManager.LoadScene("Menu");
    }
    public bool IsGameOver()
    {
        return isGameOver;
    }   
    public bool IsGameWin()
    {
        return isGameWin;
    }
}    
