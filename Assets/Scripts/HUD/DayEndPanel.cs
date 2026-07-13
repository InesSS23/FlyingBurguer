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

    [Header("botao de proximo nivel")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private TMP_Text nextLevelButtonText;

    [Header("cutscene final")]
    [SerializeField] private EndCutsceneController endCutsceneController;

    [Header("cenas")]
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private string fallbackNextSceneName = "";
    [SerializeField] private string fallbackFinalSceneName = "";

    private string nextSceneToLoad = "";
    private LevelConfig activeLevelConfig;
    private bool shouldPlayEndCutscene = false;

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

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(GoToNextLevelOrEndCutscene);
            nextLevelButton.gameObject.SetActive(false);

            if (nextLevelButtonText == null)
            {
                nextLevelButtonText = nextLevelButton.GetComponentInChildren<TMP_Text>(true);
            }
        }
    }

    public void ShowResult(bool success, int finalScore, int targetScore)
    {
        ShowResult(success, finalScore, targetScore, null);
    }

    public void ShowResult(bool success, int finalScore, int targetScore, LevelConfig levelConfig)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        activeLevelConfig = levelConfig;
        nextSceneToLoad = "";
        shouldPlayEndCutscene = false;

        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(false);
        }

        if (success)
        {
            ShowSuccess(finalScore, targetScore, levelConfig);
        }
        else
        {
            ShowFailure(finalScore, targetScore);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ShowSuccess(int finalScore, int targetScore, LevelConfig levelConfig)
    {
        if (titleText != null)
            titleText.text = "Dia Concluído!";

        if (resultText != null)
            resultText.text = "Conseguiste " + finalScore + " / " + targetScore + " pontos.";

        bool hasNextAction = false;

        if (levelConfig != null)
        {
            if (levelConfig.isFinalLevel)
            {
                if (titleText != null)
                    titleText.text = "Jogo Concluído!";

                shouldPlayEndCutscene = endCutsceneController != null
                    && endCutsceneController.HasFrames(levelConfig);

                if (!string.IsNullOrWhiteSpace(levelConfig.finalSceneName))
                {
                    nextSceneToLoad = levelConfig.finalSceneName;
                }
                else if (!string.IsNullOrWhiteSpace(fallbackFinalSceneName))
                {
                    nextSceneToLoad = fallbackFinalSceneName;
                }
                else
                {
                    nextSceneToLoad = menuSceneName;
                }

                hasNextAction = true;

                if (nextLevelButtonText != null)
                    nextLevelButtonText.text = shouldPlayEndCutscene ? "Ver Final" : "Terminar";
            }
            else if (!string.IsNullOrWhiteSpace(levelConfig.nextSceneName))
            {
                nextSceneToLoad = levelConfig.nextSceneName;
                hasNextAction = true;

                LevelProgress.SaveCurrentLevel(nextSceneToLoad);

                if (nextLevelButtonText != null)
                    nextLevelButtonText.text = "Próximo Dia";
            }
        }

        if (!hasNextAction && !string.IsNullOrWhiteSpace(fallbackNextSceneName))
        {
            nextSceneToLoad = fallbackNextSceneName;
            hasNextAction = true;

            LevelProgress.SaveCurrentLevel(nextSceneToLoad);

            if (nextLevelButtonText != null)
                nextLevelButtonText.text = "Próximo Dia";
        }

        if (nextLevelButton != null && hasNextAction)
        {
            nextLevelButton.gameObject.SetActive(true);
        }
    }

    private void ShowFailure(int finalScore, int targetScore)
    {
        if (titleText != null)
            titleText.text = "Objetivo Falhado";

        if (resultText != null)
            resultText.text = "Fizeste " + finalScore + " / " + targetScore + " pontos.";
    }

    private void RestartDay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    private void GoToNextLevelOrEndCutscene()
    {
        if (shouldPlayEndCutscene && endCutsceneController != null)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }

            endCutsceneController.StartEndCutscene(activeLevelConfig);
            return;
        }

        if (string.IsNullOrWhiteSpace(nextSceneToLoad))
        {
            Debug.LogWarning("Nao existe proxima scene definida.");
            return;
        }

        if (activeLevelConfig != null && activeLevelConfig.isFinalLevel)
        {
            LevelProgress.ClearProgress();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneToLoad);
    }
}