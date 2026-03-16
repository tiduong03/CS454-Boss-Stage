using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }
    public static bool IsGameFrozen { get; private set; }

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    public bool IsPaused => currentState == MenuState.Paused;
    public bool HasWon => currentState == MenuState.Win;
    public bool HasLost => currentState == MenuState.Lose;

    private enum MenuState
    {
        None,
        Paused,
        Win,
        Lose
    }

    private MenuState currentState = MenuState.None;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        Time.timeScale = 1f;
        IsGameFrozen = false;
        currentState = MenuState.None;

        HideAllPanels();
    }

    private void Update()
    {
        if (currentState == MenuState.Win || currentState == MenuState.Lose)
            return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (currentState == MenuState.Paused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        currentState = MenuState.Paused;
        IsGameFrozen = true;
        Time.timeScale = 0f;

        HideAllPanels();
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        currentState = MenuState.None;
        IsGameFrozen = false;
        Time.timeScale = 1f;

        HideAllPanels();
    }

    public void ShowWin()
    {
        currentState = MenuState.Win;
        IsGameFrozen = true;
        Time.timeScale = 0f;

        HideAllPanels();
        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void ShowLose()
    {
        currentState = MenuState.Lose;
        IsGameFrozen = true;
        Time.timeScale = 0f;

        HideAllPanels();
        if (losePanel != null)
            losePanel.SetActive(true);
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        IsGameFrozen = false;
        currentState = MenuState.None;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        IsGameFrozen = false;
        currentState = MenuState.None;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void HideAllPanels()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }
}