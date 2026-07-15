using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndCutsceneController : MonoBehaviour
{
    [Header("ui")]
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private GameObject dialogueBoxBackground;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonText;

    [Header("personagem a falar")]
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private bool keepPreviousSpeakerWhenEmpty = true;

    [Header("imagem")]
    [SerializeField] private Image cutsceneImage;
    [SerializeField] private bool keepPreviousImageWhenFrameImageIsEmpty = true;

    [Header("hud para esconder durante a cutscene")]
    [SerializeField] private GameObject[] hudObjectsToHideDuringCutscene;

    [Header("fallback")]
    [SerializeField] private string fallbackMenuSceneName = "MainMenu";

    private DialogueFrame[] frames;
    private int currentFrame = 0;
    private string sceneToLoadAfterCutscene = "MainMenu";
    private bool active = false;
    private bool[] previousHudStates;

    private void Awake()
    {
        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(NextFrame);
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

        if (cutscenePanel == null)
        {
            Debug.LogError("EndCutsceneController: falta ligar o Cutscene Panel.");
            return;
        }

        if (dialogueText == null)
        {
            Debug.LogError("EndCutsceneController: falta ligar o Dialogue Text.");
            return;
        }

        if (nextButton == null)
        {
            Debug.LogError("EndCutsceneController: falta ligar o Next Button.");
            return;
        }

        frames = config.endFrames;
        currentFrame = 0;
        active = true;

        sceneToLoadAfterCutscene = string.IsNullOrWhiteSpace(config.finalSceneName)
            ? fallbackMenuSceneName
            : config.finalSceneName;

        HideGameplayHud();

        cutscenePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        ShowCurrentFrame();
    }

    private void ShowCurrentFrame()
    {
        if (frames == null || frames.Length == 0)
            return;

        DialogueFrame frame = frames[currentFrame];
        bool hideDialogueBox = frame != null && frame.hideDialogueBox;

        SetDialogueBoxVisible(!hideDialogueBox);

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(!hideDialogueBox);
            dialogueText.text = hideDialogueBox ? "" : frame.text;
        }

        UpdateSpeaker(frame, hideDialogueBox);
        UpdateCutsceneImage(frame);
        UpdateButtonText();
    }

    private void SetDialogueBoxVisible(bool visible)
    {
        if (dialogueBoxBackground != null)
        {
            dialogueBoxBackground.SetActive(visible);
        }
    }

    private void UpdateSpeaker(DialogueFrame frame, bool hideDialogueBox)
    {
        if (frame == null)
            return;

        if (speakerNameText != null)
        {
            if (hideDialogueBox || frame.hideSpeakerName)
            {
                speakerNameText.gameObject.SetActive(false);
            }
            else if (!string.IsNullOrWhiteSpace(frame.speakerName))
            {
                speakerNameText.text = frame.speakerName;
                speakerNameText.gameObject.SetActive(true);
            }
            else if (!keepPreviousSpeakerWhenEmpty)
            {
                speakerNameText.text = "";
                speakerNameText.gameObject.SetActive(false);
            }
        }

        if (speakerPortraitImage != null)
        {
            if (hideDialogueBox || frame.hideSpeakerPortrait)
            {
                speakerPortraitImage.gameObject.SetActive(false);
                speakerPortraitImage.enabled = false;
            }
            else if (frame.speakerPortrait != null)
            {
                speakerPortraitImage.sprite = frame.speakerPortrait;
                speakerPortraitImage.enabled = true;
                speakerPortraitImage.gameObject.SetActive(true);
            }
            else if (!keepPreviousSpeakerWhenEmpty)
            {
                speakerPortraitImage.sprite = null;
                speakerPortraitImage.enabled = false;
                speakerPortraitImage.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateCutsceneImage(DialogueFrame frame)
    {
        if (cutsceneImage == null)
            return;

        if (frame != null && frame.image != null)
        {
            cutsceneImage.gameObject.SetActive(true);
            cutsceneImage.sprite = frame.image;
            cutsceneImage.enabled = true;
        }
        else if (!keepPreviousImageWhenFrameImageIsEmpty)
        {
            cutsceneImage.sprite = null;
            cutsceneImage.enabled = false;
            cutsceneImage.gameObject.SetActive(false);
        }
    }

    private void UpdateButtonText()
    {
        if (nextButtonText == null)
            return;

        if (currentFrame >= frames.Length - 1)
        {
            nextButtonText.text = "Terminar";
        }
        else
        {
            nextButtonText.text = "Próximo";
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

        RestoreGameplayHud();

        LevelProgress.ClearProgress();

        Time.timeScale = 1f;

        if (string.IsNullOrWhiteSpace(sceneToLoadAfterCutscene))
        {
            sceneToLoadAfterCutscene = fallbackMenuSceneName;
        }

        SceneManager.LoadScene(sceneToLoadAfterCutscene);
    }

    private void GoToFinalScene(LevelConfig config)
    {
        RestoreGameplayHud();

        LevelProgress.ClearProgress();

        Time.timeScale = 1f;

        string sceneName = fallbackMenuSceneName;

        if (config != null && !string.IsNullOrWhiteSpace(config.finalSceneName))
        {
            sceneName = config.finalSceneName;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void HideGameplayHud()
    {
        if (hudObjectsToHideDuringCutscene == null || hudObjectsToHideDuringCutscene.Length == 0)
            return;

        previousHudStates = new bool[hudObjectsToHideDuringCutscene.Length];

        for (int i = 0; i < hudObjectsToHideDuringCutscene.Length; i++)
        {
            if (hudObjectsToHideDuringCutscene[i] == null)
                continue;

            previousHudStates[i] = hudObjectsToHideDuringCutscene[i].activeSelf;
            hudObjectsToHideDuringCutscene[i].SetActive(false);
        }
    }

    private void RestoreGameplayHud()
    {
        if (hudObjectsToHideDuringCutscene == null || previousHudStates == null)
            return;

        for (int i = 0; i < hudObjectsToHideDuringCutscene.Length; i++)
        {
            if (hudObjectsToHideDuringCutscene[i] == null)
                continue;

            if (i < previousHudStates.Length)
            {
                hudObjectsToHideDuringCutscene[i].SetActive(previousHudStates[i]);
            }
        }
    }
}