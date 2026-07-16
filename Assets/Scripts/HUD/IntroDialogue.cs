using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroDialogue : MonoBehaviour
{
    [Header("ui")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject dialogueBoxBackground;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button skipButton;
    [SerializeField] private TMP_Text skipButtonText;

    [Header("personagem a falar")]
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private bool keepPreviousSpeakerWhenEmpty = true;

    [Header("imagem da cutscene")]
    [SerializeField] private Image cutsceneImage;
    [SerializeField] private bool keepPreviousImageWhenFrameImageIsEmpty = true;

    [Header("legendas com voz")]
    [SerializeField] private bool animateSubtitles = true;
    [SerializeField, Range(0.01f, 0.08f)] private float characterDelay = 0.026f;

    [Header("hud para esconder durante a cutscene")]
    [SerializeField] private GameObject[] hudObjectsToHideDuringCutscene;

    [Header("falas antigas - fallback")]
    [TextArea(2, 5)]
    [SerializeField] private string[] dialogueLines;

    [Header("falas novas com imagem")]
    [SerializeField] private DialogueFrame[] dialogueFrames;

    [Header("scripts do jogador para bloquear")]
    [SerializeField] private FirstPersonPlayer firstPersonPlayer;
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("clientes")]
    [SerializeField] private CustomerManager customerManager;

    [Header("timer / pontuacao")]
    [SerializeField] private DayManager dayManager;

    private int currentLine = 0;
    private bool dialogueActive = false;
    private bool[] previousHudStates;
    private SubtitleVoicePlayer subtitleVoice;
    private Coroutine subtitleRoutine;

    public bool IsDialogueActive()
    {
        return dialogueActive;
    }

    void Start()
    {
        subtitleVoice = GetComponent<SubtitleVoicePlayer>();
        if (subtitleVoice == null)
            subtitleVoice = gameObject.AddComponent<SubtitleVoicePlayer>();

        TryLoadDialogueFromLevelConfig();

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(NextLine);

            if (skipButtonText == null)
            {
                skipButtonText = skipButton.GetComponentInChildren<TMP_Text>(true);
            }
        }

        StartDialogue();
    }

    private void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSFX();
        }
    }

    private void TryLoadDialogueFromLevelConfig()
    {
        if (dayManager == null)
            return;

        LevelConfig config = dayManager.GetLevelConfig();

        if (config == null)
            return;

        if (config.introFrames != null && config.introFrames.Length > 0)
        {
            dialogueFrames = config.introFrames;
        }
    }

    private void StartDialogue()
    {
        dialogueActive = true;
        currentLine = 0;

        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeBackgroundMusic();

        if (subtitleVoice != null)
            subtitleVoice.StartAmbience();

        if (GameplayHUDPolish.Instance != null)
            GameplayHUDPolish.Instance.SetMenuVisible(true);

        HideGameplayHud();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (firstPersonPlayer != null)
        {
            firstPersonPlayer.ResetPlayerAndCameraToStart();
        }

        BloquearJogador();

        if (GetLineCount() <= 0)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
        UpdateButtonText();

        Debug.Log("dialogo inicial comecou");
    }

    private int GetLineCount()
    {
        if (dialogueFrames != null && dialogueFrames.Length > 0)
            return dialogueFrames.Length;

        if (dialogueLines != null)
            return dialogueLines.Length;

        return 0;
    }

    private DialogueFrame GetCurrentFrame()
    {
        if (dialogueFrames != null && currentLine >= 0 && currentLine < dialogueFrames.Length)
        {
            return dialogueFrames[currentLine];
        }

        return null;
    }

    private string GetCurrentText()
    {
        DialogueFrame frame = GetCurrentFrame();

        if (frame != null)
        {
            return frame.text;
        }

        if (dialogueLines != null && currentLine >= 0 && currentLine < dialogueLines.Length)
            return dialogueLines[currentLine];

        return "";
    }

    private Sprite GetCurrentImage()
    {
        DialogueFrame frame = GetCurrentFrame();

        if (frame != null)
        {
            return frame.image;
        }

        return null;
    }

    private void ShowCurrentLine()
    {
        DialogueFrame frame = GetCurrentFrame();
        bool hideDialogueBox = frame != null && frame.hideDialogueBox;

        SetDialogueBoxVisible(!hideDialogueBox);

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(!hideDialogueBox);
            if (hideDialogueBox)
                CompleteSubtitle("");
            else
                BeginSubtitle(GetCurrentText(), frame != null ? frame.speakerName : "");
        }

        UpdateSpeaker(frame, hideDialogueBox);
        UpdateCutsceneImage();
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

    private void UpdateCutsceneImage()
    {
        if (cutsceneImage == null)
            return;

        Sprite frameImage = GetCurrentImage();

        if (frameImage != null)
        {
            cutsceneImage.gameObject.SetActive(true);
            cutsceneImage.sprite = frameImage;
            cutsceneImage.enabled = true;
            ConfigureFullscreenCover(cutsceneImage, frameImage);
        }
        else if (!keepPreviousImageWhenFrameImageIsEmpty)
        {
            cutsceneImage.sprite = null;
            cutsceneImage.enabled = false;
            cutsceneImage.gameObject.SetActive(false);
        }
    }

    private void NextLine()
    {
        PlayButtonSound();

        if (!dialogueActive)
            return;

        if (subtitleRoutine != null)
        {
            CompleteSubtitle(GetCurrentText());
            return;
        }

        currentLine++;

        if (currentLine >= GetLineCount())
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        if (skipButtonText == null)
            return;

        if (currentLine == GetLineCount() - 1)
            skipButtonText.text = "Começar Dia";
        else
            skipButtonText.text = "Próximo";
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        CompleteSubtitle("");

        if (subtitleVoice != null)
            subtitleVoice.StopAmbience();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (cutsceneImage != null)
            cutsceneImage.gameObject.SetActive(false);

        RestoreGameplayHud();

        if (GameplayHUDPolish.Instance != null)
            GameplayHUDPolish.Instance.SetMenuVisible(false);

        DesbloquearJogador();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySpawnSFX();
        }

        if (dayManager != null)
        {
            dayManager.StartDay();
        }

        if (customerManager != null)
        {
            customerManager.StartSpawning();
        }

        Debug.Log("dialogo inicial terminou");
    }

    private void BeginSubtitle(string text, string speakerName)
    {
        CompleteSubtitle(text);

        if (dialogueText == null || !animateSubtitles || string.IsNullOrEmpty(text))
            return;

        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();
        subtitleRoutine = StartCoroutine(RevealSubtitle(speakerName));
    }

    private IEnumerator RevealSubtitle(string speakerName)
    {
        int characterCount = dialogueText.textInfo.characterCount;
        int spokenCharacters = 0;

        for (int i = 0; i < characterCount; i++)
        {
            dialogueText.maxVisibleCharacters = i + 1;
            char character = dialogueText.textInfo.characterInfo[i].character;

            if (!char.IsWhiteSpace(character))
            {
                if (spokenCharacters % 2 == 0 && subtitleVoice != null)
                    subtitleVoice.PlayCharacter(speakerName, spokenCharacters);

                spokenCharacters++;
            }

            yield return new WaitForSecondsRealtime(characterDelay);
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        subtitleRoutine = null;
    }

    private void CompleteSubtitle(string finalText)
    {
        if (subtitleRoutine != null)
        {
            StopCoroutine(subtitleRoutine);
            subtitleRoutine = null;
        }

        if (subtitleVoice != null)
            subtitleVoice.StopVoice();

        if (dialogueText != null)
        {
            dialogueText.text = finalText ?? "";
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }
    }

    private static void ConfigureFullscreenCover(Image image, Sprite sprite)
    {
        if (image == null || sprite == null)
            return;

        RectTransform rect = image.rectTransform;

        // Nas cenas antigas a imagem estava dentro da pequena caixa inferior.
        // Movemo-la para o contentor do ecra e deixamos a caixa de dialogo por cima.
        Canvas rootCanvas = image.canvas != null ? image.canvas.rootCanvas : null;
        if (rootCanvas != null && rootCanvas.transform is RectTransform screenParent)
        {
            if (rect.parent != screenParent)
                rect.SetParent(screenParent, false);

            // Fundo da cutscene: ocupa o Canvas inteiro e fica atras de toda a UI.
            rect.SetAsFirstSibling();
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        AspectRatioFitter fitter = image.GetComponent<AspectRatioFitter>();
        if (fitter == null)
            fitter = image.gameObject.AddComponent<AspectRatioFitter>();

        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = sprite.rect.width / Mathf.Max(sprite.rect.height, 1f);
        image.preserveAspect = true;
        image.raycastTarget = false;
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

    private void BloquearJogador()
    {
        if (firstPersonPlayer != null)
        {
            firstPersonPlayer.enabled = false;
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void DesbloquearJogador()
    {
        if (firstPersonPlayer != null)
        {
            firstPersonPlayer.enabled = true;
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
