using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DayEndPanel : MonoBehaviour
{
    [Header("ui")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    [Header("cenas")]
    [SerializeField] private string menuSceneName = "MainMenu";

    void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartDay);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(GoToMenu);
        }
    }

    public void ShowResult(bool success, int finalScore, int targetScore)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        if (success)
        {
            if (titleText != null)
                titleText.text = "Dia Concluído!";

            if (resultText != null)
                resultText.text = "Conseguiste " + finalScore + " / " + targetScore + " pontos.";
        }
        else
        {
            if (titleText != null)
                titleText.text = "Objetivo Falhado";

            if (resultText != null)
                resultText.text = "Fizeste " + finalScore + " / " + targetScore + " pontos.";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestartDay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}