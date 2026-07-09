using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("pause menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject pauseButtonsPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("cenas")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("controlo do dia")]
    [SerializeField] private DayManager dayManager;

    [Header("áudio")]
    [SerializeField] private AudioClip menuBackgroundMusic;

    private bool isPaused = false;

    private void Start()
    {
        GameSettings.LoadSettings();

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
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

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        if (pauseButtonsPanel != null)
        {
            pauseButtonsPanel.SetActive(true);
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

        if (pauseButtonsPanel != null)
        {
            pauseButtonsPanel.SetActive(true);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowOptions()
    {
        if (pauseButtonsPanel != null)
        {
            pauseButtonsPanel.SetActive(false);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    public void BackToPauseButtons()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        if (pauseButtonsPanel != null)
        {
            pauseButtonsPanel.SetActive(true);
        }
    }

    public void ForceClosePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        if (pauseButtonsPanel != null)
        {
            pauseButtonsPanel.SetActive(true);
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