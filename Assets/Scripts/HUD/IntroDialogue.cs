using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroDialogue : MonoBehaviour
{
    [Header("ui")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button skipButton;
    [SerializeField] private TMP_Text skipButtonText;

    [Header("imagem da cutscene")]
    [SerializeField] private Image cutsceneImage;
    [SerializeField] private bool keepPreviousImageWhenFrameImageIsEmpty = true;

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

    void Start()
    {
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

    private string GetCurrentText()
    {
        if (dialogueFrames != null && dialogueFrames.Length > 0)
        {
            if (currentLine >= 0 && currentLine < dialogueFrames.Length)
                return dialogueFrames[currentLine].text;

            return "";
        }

        if (dialogueLines != null && currentLine >= 0 && currentLine < dialogueLines.Length)
            return dialogueLines[currentLine];

        return "";
    }

    private Sprite GetCurrentImage()
    {
        if (dialogueFrames != null && dialogueFrames.Length > 0)
        {
            if (currentLine >= 0 && currentLine < dialogueFrames.Length)
                return dialogueFrames[currentLine].image;
        }

        return null;
    }

    private void ShowCurrentLine()
    {
        if (dialogueText == null)
        {
            Debug.Log("falta ligar DialogueText");
            return;
        }

        dialogueText.text = GetCurrentText();

        if (cutsceneImage != null)
        {
            Sprite frameImage = GetCurrentImage();

            if (frameImage != null)
            {
                cutsceneImage.sprite = frameImage;
                cutsceneImage.enabled = true;
            }
            else if (!keepPreviousImageWhenFrameImageIsEmpty)
            {
                cutsceneImage.sprite = null;
                cutsceneImage.enabled = false;
            }
        }
    }

    private void NextLine()
    {
        if (!dialogueActive)
            return;

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
            skipButtonText.text = "Comecar Dia";
        else
            skipButtonText.text = "Proximo";
    }

    private void EndDialogue()
    {
        dialogueActive = false;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        DesbloquearJogador();

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