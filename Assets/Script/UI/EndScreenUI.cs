using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenUI : MonoBehaviour
{
    public static EndScreenUI Instance;

    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool gameEnded;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Time.timeScale = 1f;
        gameEnded = false;

        if (losePanel) losePanel.SetActive(false);
        if (winPanel) winPanel.SetActive(false);
    }

    public void ShowLose()
    {
        if (gameEnded) return;
        gameEnded = true;

        Time.timeScale = 0f;

        if (losePanel)
            losePanel.SetActive(true);
    }

    public void ShowWin()
    {
        if (gameEnded) return;
        gameEnded = true;

        Time.timeScale = 0f;

        if (winPanel)
            winPanel.SetActive(true);
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}