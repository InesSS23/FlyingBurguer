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

    [Header("falas")]
    [TextArea(2, 5)]
    [SerializeField] private string[] dialogueLines;

    [Header("scripts do jogador para bloquear")]
    [SerializeField] private FirstPersonPlayer firstPersonPlayer;
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("clientes")]
    [SerializeField] private CustomerManager customerManager;

    [Header("timer / pontuação")]
    [SerializeField] private DayManager dayManager;

    private int currentLine = 0;
    private bool dialogueActive = false;

    void Start()
    {
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(NextLine);
        }

        StartDialogue();
    }

    private void StartDialogue()
    {
        dialogueActive = true;
        currentLine = 0;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // CORREÇÃO: o Level1 força o jogador e a câmara para o ponto inicial antes de bloquear o jogador.
        // Isto resolve o bug em que a câmara ficava virada para a parede/chão no WebGL ou ao reiniciar o dia.
        if (firstPersonPlayer != null)
        {
            firstPersonPlayer.ResetPlayerAndCameraToStart();
        }

        BloquearJogador();
        ShowCurrentLine();
        UpdateButtonText();

        Debug.Log("dialogo inicial começou");
    }

    private void ShowCurrentLine()
    {
        if (dialogueText == null)
        {
            Debug.Log("falta ligar DialogueText");
            return;
        }

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            EndDialogue();
            return;
        }

        dialogueText.text = dialogueLines[currentLine];
    }

    private void NextLine()
    {
        if (!dialogueActive)
            return;

        currentLine++;

        if (currentLine >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        if (skipButtonText == null || dialogueLines == null || dialogueLines.Length == 0)
            return;

        if (currentLine == dialogueLines.Length - 1)
            skipButtonText.text = "Começar Dia";
        else
            skipButtonText.text = "Skip";
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
