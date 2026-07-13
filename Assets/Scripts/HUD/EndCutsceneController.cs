using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndCutsceneController : MonoBehaviour
{
    [Header("ui")]
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonText;

    [Header("imagem")]
    [SerializeField] private Image cutsceneImage;
    [SerializeField] private bool keepPreviousImageWhenFrameImageIsEmpty = true;

    [Header("fallback")]
    [SerializeField] private string fallbackMenuSceneName = "MainMenu";

    private DialogueFrame[] frames;
    private int currentFrame = 0;
    private string sceneToLoadAfterCutscene = "MainMenu";
    private bool active = false;

    private void Awake()
    {
        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextFrame);

            if (nextButtonText == null)
            {
                nextButtonText = nextButton.GetComponentInChildren<TMP_Text>(true);
            }
        }
    }

    private void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSFX();
        }
    }

    public bool HasFrames(LevelConfig config)
    {
        return config != null && config.endFrames != null && config.endFrames.Length > 0;
    }

    public void StartEndCutscene(LevelConfig config)
    {
        if (!HasFrames(config))
        {
            GoToFinalScene(config);
            return;
        }

        frames = config.endFrames;
        currentFrame = 0;
        active = true;

        sceneToLoadAfterCutscene = string.IsNullOrWhiteSpace(config.finalSceneName)
            ? fallbackMenuSceneName
            : config.finalSceneName;

        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        ShowCurrentFrame();
    }

    private void ShowCurrentFrame()
    {
        if (frames == null || frames.Length == 0)
            return;

        if (dialogueText != null)
        {
            dialogueText.text = frames[currentFrame].text;
        }

        if (cutsceneImage != null)
        {
            Sprite image = frames[currentFrame].image;

            if (image != null)
            {
                cutsceneImage.sprite = image;
                cutsceneImage.enabled = true;
            }
            else if (!keepPreviousImageWhenFrameImageIsEmpty)
            {
                cutsceneImage.sprite = null;
                cutsceneImage.enabled = false;
            }
        }

        if (nextButtonText != null)
        {
            if (currentFrame >= frames.Length - 1)
            {
                nextButtonText.text = "Terminar";
            }
            else
            {
                nextButtonText.text = "Proximo";
            }
        }
    }

    private void NextFrame()
    {
        PlayButtonSound();

        if (!active)
            return;

        currentFrame++;

        if (currentFrame >= frames.Length)
        {
            FinishCutscene();
            return;
        }

        ShowCurrentFrame();
    }

    private void FinishCutscene()
    {
        active = false;

        LevelProgress.ClearProgress();

        Time.timeScale = 1f;

        SceneManager.LoadScene(sceneToLoadAfterCutscene);
    }

    private void GoToFinalScene(LevelConfig config)
    {
        LevelProgress.ClearProgress();

        Time.timeScale = 1f;

        string sceneName = fallbackMenuSceneName;

        if (config != null && !string.IsNullOrWhiteSpace(config.finalSceneName))
        {
            sceneName = config.finalSceneName;
        }

        SceneManager.LoadScene(sceneName);
    }
}