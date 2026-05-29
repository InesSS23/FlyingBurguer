using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainButtonsPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Scene Names")]
    public string levelSceneName = "Level1";

    [Header("Audio")]
    [SerializeField] private AudioClip menuBackgroundMusic;

    private void Start()
    {
        GameSettings.LoadSettings();

        ShowMainMenu();

        if (AudioManager.Instance != null && menuBackgroundMusic != null)
        {
            AudioManager.Instance.PlayBackgroundMusic(menuBackgroundMusic);
        }
    }

    public void PlayGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSFX();
            AudioManager.Instance.StopBackgroundMusic();
        }

        SceneManager.LoadScene(levelSceneName);
    }

    public void ShowOptions()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSFX();
        }

        mainButtonsPanel.SetActive(false);
        optionsPanel.SetActive(true);
        creditsPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSFX();
        }

        mainButtonsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSFX();
        }

        mainButtonsPanel.SetActive(true);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSFX();
        }

        Debug.Log("Sair do jogo");
        Application.Quit();
    }
}