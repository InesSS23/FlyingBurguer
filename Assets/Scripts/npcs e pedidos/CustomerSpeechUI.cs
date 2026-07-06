using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerSpeechUI : MonoBehaviour
{
    [Header("ui da fala")]
    [SerializeField] private GameObject speechPanel;
    [SerializeField] private TMP_Text speechText;
    [SerializeField] private Image speechPanelImage;

    [Header("cores normais")]
    [SerializeField] private Color normalPanelColor = new Color(1f, 0.95f, 0.82f, 1f);
    [SerializeField] private Color normalTextColor = Color.black;

    [Header("cores zangado")]
    [SerializeField] private Color angryPanelColor = new Color(1f, 0.35f, 0.2f, 1f);
    [SerializeField] private Color angryTextColor = Color.white;

    [Header("tempo visivel")]
    [SerializeField] private float speechDuration = 2.5f;

    [Header("limites do texto")]
    [SerializeField] private bool autoFitText = true;
    [SerializeField] private int maxVisibleLines = 4;
    [SerializeField] private float minFontSize = 10f;
    [SerializeField] private float maxFontSize = 22f;

    [Header("camera")]
    [SerializeField] private bool faceCamera = true;

    private Coroutine speechCoroutine;

    void Start()
    {
        TryFindPanelImage();
        HideSpeech();
    }

    void LateUpdate()
    {
        if (!faceCamera)
            return;

        if (Camera.main == null)
            return;

        transform.LookAt(Camera.main.transform);
        transform.Rotate(0f, 180f, 0f);
    }

    public void ShowSpeech(string message)
    {
        ShowSpeechWithStyle(message, false);
    }

    public void ShowAngrySpeech(string message)
    {
        ShowSpeechWithStyle(message, true);
    }

    private void ShowSpeechWithStyle(string message, bool angry)
    {
        if (speechPanel == null)
        {
            Debug.LogWarning("SpeechPanel nao esta ligado");
            return;
        }

        if (speechText == null)
        {
            Debug.LogWarning("SpeechText nao esta ligado");
            return;
        }

        TryFindPanelImage();
        ApplyColors(angry);

        if (speechCoroutine != null)
        {
            StopCoroutine(speechCoroutine);
        }

        speechCoroutine = StartCoroutine(ShowSpeechRoutine(message));
    }

    private IEnumerator ShowSpeechRoutine(string message)
    {
        PrepareSpeechText();

        speechText.text = message;
        speechPanel.SetActive(true);

        yield return new WaitForSeconds(speechDuration);

        HideSpeech();
    }

    private void ApplyColors(bool angry)
    {
        if (speechPanelImage != null)
        {
            speechPanelImage.color = angry ? angryPanelColor : normalPanelColor;
        }

        if (speechText != null)
        {
            speechText.color = angry ? angryTextColor : normalTextColor;
        }
    }

    private void PrepareSpeechText()
    {
        if (speechText == null || !autoFitText)
            return;

        speechText.enableWordWrapping = true;
        speechText.enableAutoSizing = true;
        speechText.fontSizeMin = minFontSize;
        speechText.fontSizeMax = maxFontSize;
        speechText.maxVisibleLines = maxVisibleLines;
        speechText.overflowMode = TextOverflowModes.Overflow;
        speechText.alignment = TextAlignmentOptions.Center;
    }

    private void TryFindPanelImage()
    {
        if (speechPanelImage != null)
            return;

        if (speechPanel == null)
            return;

        speechPanelImage = speechPanel.GetComponent<Image>();
    }

    public void HideSpeech()
    {
        if (speechPanel != null)
        {
            speechPanel.SetActive(false);
        }
    }
}
