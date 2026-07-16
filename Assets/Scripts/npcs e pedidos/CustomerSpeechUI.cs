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

    [Header("numero do pedido")]
    [SerializeField] private float orderNumberScale = 0.35f;
    [SerializeField] private Vector2 orderNumberImageSize = new Vector2(120f, 120f);

    // altura (eixo Y local, a partir do proprio passaro) a que o numero fica acima da cabeca
    [SerializeField] private float orderNumberHeightOffset = 2.5f;

    // imagens dos numeros 1, 2 e 3 (indice 0 = numero 1) - maximo 3 porque so ha 3 pontos de servico
    [SerializeField] private Sprite[] orderNumberSprites;

    private Coroutine speechCoroutine;
    private Vector3 originalLocalScale = Vector3.one;
    private Vector3 originalLocalPosition;
    private bool showingOrderNumber = false;

    private GameObject numberImageObject;
    private Image numberImage;

    private const float EMERGENCY_MIN_FONT_SIZE = 4f;

    void Start()
    {
        originalLocalScale = transform.localScale;
        originalLocalPosition = transform.localPosition;

        TryFindPanelImage();
        HideSpeech();
    }

    void LateUpdate()
    {
        if (!faceCamera)
            return;

        if (Camera.main == null)
            return;

        if (showingOrderNumber)
        {
            // o numero so roda no eixo horizontal (yaw) para a camera, fica sempre direito por cima da cabeca
            // (rodar tambem no pitch/roll, como o balao de fala faz, e o que causava o numero a "fugir" para o lado)
            Vector3 toCamera = Camera.main.transform.position - transform.position;
            toCamera.y = 0f;

            if (toCamera.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(-toCamera, Vector3.up);
            }

            return;
        }

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

    public void ShowOrderNumber(int number)
    {
        if (speechPanel == null)
        {
            Debug.LogWarning("SpeechPanel nao esta ligado");
            return;
        }

        Sprite sprite = GetOrderNumberSprite(number);

        if (sprite == null)
        {
            Debug.LogWarning("CustomerSpeechUI: falta a imagem do numero " + number + " em orderNumberSprites");
            return;
        }

        if (speechCoroutine != null)
        {
            StopCoroutine(speechCoroutine);
            speechCoroutine = null;
        }

        showingOrderNumber = true;
        transform.localScale = originalLocalScale * orderNumberScale;

        // o balao de fala tem um deslocamento para a frente da cara (para nao entrar na cabeca ao falar);
        // o numero nao deve herdar esse Z (senao fica "a frente" do passaro em vez de "em cima"), mas tem
        // de manter a altura Y original (ja afinada por passaro) e somar-lhe o deslocamento extra para cima
        transform.localPosition = new Vector3(0f, originalLocalPosition.y + orderNumberHeightOffset, 0f);

        TryFindPanelImage();

        // o numero fica so com a imagem, sem o balao de fundo do balao de comentarios
        if (speechPanelImage != null)
            speechPanelImage.enabled = false;

        if (speechText != null)
            speechText.gameObject.SetActive(false);

        speechPanel.SetActive(true);

        EnsureNumberImage();
        numberImage.sprite = sprite;
        numberImageObject.SetActive(true);
    }

    private Sprite GetOrderNumberSprite(int number)
    {
        int index = number - 1;

        if (orderNumberSprites == null || index < 0 || index >= orderNumberSprites.Length)
            return null;

        return orderNumberSprites[index];
    }

    private void EnsureNumberImage()
    {
        if (numberImageObject != null)
            return;

        numberImageObject = new GameObject("PedidoNumeroImagem");
        numberImageObject.transform.SetParent(speechPanel.transform, false);

        numberImage = numberImageObject.AddComponent<Image>();
        numberImage.preserveAspect = true;
        numberImage.raycastTarget = false;

        RectTransform rect = numberImageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = orderNumberImageSize;

        numberImageObject.SetActive(false);
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

        if (speechPanelImage != null)
            speechPanelImage.enabled = true;

        if (speechText != null)
            speechText.gameObject.SetActive(true);

        if (numberImageObject != null)
            numberImageObject.SetActive(false);

        if (speechCoroutine != null)
        {
            StopCoroutine(speechCoroutine);
        }

        showingOrderNumber = false;
        transform.localScale = originalLocalScale;
        transform.localPosition = originalLocalPosition;

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