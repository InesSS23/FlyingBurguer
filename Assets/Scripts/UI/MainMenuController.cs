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

    private void Start()
    {
        ShowMainMenu();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(levelSceneName);
    }

    public void ShowOptions()
    {
        mainButtonsPanel.SetActive(false);
        optionsPanel.SetActive(true);
        creditsPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        mainButtonsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        mainButtonsPanel.SetActive(true);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Sair do jogo");
        Application.Quit();
    }
}