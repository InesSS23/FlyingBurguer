using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("pause menu")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("cenas")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("controlo do dia")]
    [SerializeField] private DayManager dayManager;

    [Header("áudio")]
    [SerializeField] private AudioClip menuBackgroundMusic;

    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (dayManager != null && dayManager.IsDayEnded())
            {
                return;
            }

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void PauseGame()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ForceClosePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void RestartDay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        
        // Para a música do jogo e toca a do menu
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBackgroundMusic();
            if (menuBackgroundMusic != null)
            {
                AudioManager.Instance.PlayBackgroundMusic(menuBackgroundMusic);
            }
        }
        
        SceneManager.LoadScene(mainMenuSceneName);
    }
}