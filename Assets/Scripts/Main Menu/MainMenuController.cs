using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainButtonsPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Botoes")]
    [SerializeField] private Button continueButton;

    [Header("Scene Names")]
    public string levelSceneName = "Level1";

    [Header("Audio")]
    [SerializeField] private AudioClip menuBackgroundMusic;

    private void Start()
    {
        Time.timeScale = 1f;

        GameSettings.LoadSettings();

        ShowMainMenu();
        UpdateContinueButton();

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

        // Novo jogo começa sempre do início e apaga progresso antigo.
        LevelProgress.ClearProgress();

        SceneManager.LoadScene(levelSceneName);
    }

    public void ContinueGame()
    {
        if (!LevelProgress.HasProgress())
        {
            Debug.Log("nao existe progresso guardado");
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSFX();
            AudioManager.Instance.StopBackgroundMusic();
        }

        string sceneToLoad = LevelProgress.GetCurrentLevel();

        SceneManager.LoadScene(sceneToLoad);
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

        UpdateContinueButton();
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

    private void UpdateContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(LevelProgress.HasProgress());
        }
    }
}