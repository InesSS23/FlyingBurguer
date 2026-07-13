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

    [Header("tamanho do texto")]
    [SerializeField] private bool autoFitText = true;
    [SerializeField] private float minFontSize = 7f;
    [SerializeField] private float maxFontSize = 22f;
    [SerializeField] private float fontSizeStep = 0.25f;

    [Header("margens internas do balao")]
    [SerializeField] private float horizontalPadding = 6f;
    [SerializeField] private float verticalPadding = 4f;

    [Header("camera")]
    [SerializeField] private bool faceCamera = true;

    private Coroutine speechCoroutine;

    private const float EMERGENCY_MIN_FONT_SIZE = 4f;

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
        speechPanel.SetActive(true);

        PrepareSpeechText();
        ConfigureTextAreaInsideBubble();

        yield return null;

        if (autoFitText)
        {
            FitTextInsideBubble(message);
        }
        else
        {
            speechText.text = message;
            speechText.fontSize = maxFontSize;
        }

        yield return new WaitForSeconds(speechDuration);

        HideSpeech();
    }

    private void PrepareSpeechText()
    {
        if (speechText == null)
            return;

        speechText.textWrappingMode = TextWrappingModes.Normal;
        speechText.overflowMode = TextOverflowModes.Overflow;
        speechText.enableAutoSizing = false;
        speechText.alignment = TextAlignmentOptions.Center;
        speechText.raycastTarget = false;
        speechText.margin = Vector4.zero;
        speechText.lineSpacing = -8f;
        speechText.characterSpacing = 0f;
        speechText.wordSpacing = 0f;
    }

    private void ConfigureTextAreaInsideBubble()
    {
        if (speechPanel == null || speechText == null)
            return;

        RectTransform panelRect = speechPanel.GetComponent<RectTransform>();
        RectTransform textRect = speechText.rectTransform;

        if (panelRect == null || textRect == null)
            return;

        float width = Mathf.Max(10f, panelRect.rect.width - horizontalPadding * 2f);
        float height = Mathf.Max(10f, panelRect.rect.height - verticalPadding * 2f);

        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(width, height);
    }

    private void FitTextInsideBubble(string message)
    {
        if (speechText == null)
            return;

        RectTransform textRect = speechText.rectTransform;

        float boxWidth = textRect.rect.width;
        float boxHeight = textRect.rect.height;

        if (boxWidth <= 0f || boxHeight <= 0f)
        {
            speechText.text = message;
            speechText.fontSize = EMERGENCY_MIN_FONT_SIZE;
            return;
        }

        speechText.text = message;

        float realMinFontSize = Mathf.Min(minFontSize, EMERGENCY_MIN_FONT_SIZE);
        float realMaxFontSize = Mathf.Max(maxFontSize, realMinFontSize);

        float chosenSize = realMinFontSize;

        for (float size = realMaxFontSize; size >= realMinFontSize; size -= fontSizeStep)
        {
            speechText.fontSize = size;
            speechText.ForceMeshUpdate();

            Vector2 preferredSize = speechText.GetPreferredValues(message, boxWidth, Mathf.Infinity);

            bool fitsWidth = preferredSize.x <= boxWidth + 0.5f;
            bool fitsHeight = preferredSize.y <= boxHeight + 0.5f;

            if (fitsWidth && fitsHeight)
            {
                chosenSize = size;
                break;
            }
        }

        speechText.fontSize = chosenSize;
        speechText.ForceMeshUpdate();
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