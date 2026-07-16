using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Mostra o progresso dos equipamentos de cozinha por cima da estacao
/// correspondente, sem alterar os modelos ou as colisoes da cozinha.
/// </summary>
public sealed class StationProgressHUD : MonoBehaviour
{
    private sealed class StationMarker
    {
        public Transform target;
        public RectTransform root;
        public RectTransform fill;
        public Image fillImage;
        public TMP_Text label;
        public string actionLabel;
        public Func<bool> isProcessing;
        public Func<float> getProgress;
        public bool wasProcessing;
        public float readyUntil;
    }

    private readonly List<StationMarker> markers = new List<StationMarker>();
    private Canvas canvas;
    private RectTransform canvasRect;
    private Camera gameplayCamera;
    private static Sprite roundedSprite;

    private readonly Color cocoa = new Color(0.25f, 0.105f, 0.05f, 1f);
    private readonly Color cream = new Color(1f, 0.965f, 0.84f, 1f);
    private readonly Color coral = new Color(0.96f, 0.30f, 0.18f, 1f);
    private readonly Color peach = new Color(1f, 0.67f, 0.26f, 1f);
    private readonly Color mint = new Color(0.30f, 0.78f, 0.45f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterForSceneLoads()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.StartsWith("Level"))
            return;

        if (FindFirstObjectByType<StationProgressHUD>() == null)
            new GameObject("Station Progress HUD").AddComponent<StationProgressHUD>();
    }

    private IEnumerator Start()
    {
        BuildCanvas();
        yield return null;

        GrillStation[] grills = FindObjectsByType<GrillStation>(FindObjectsSortMode.None);
        for (int i = 0; i < grills.Length; i++)
        {
            GrillStation station = grills[i];
            AddMarker(station.transform, "A GRELHAR", () => station.IsProcessing, () => station.ProcessingProgress);
        }

        FryerStation[] fryers = FindObjectsByType<FryerStation>(FindObjectsSortMode.None);
        for (int i = 0; i < fryers.Length; i++)
        {
            FryerStation station = fryers[i];
            AddMarker(station.transform, "A FRITAR", () => station.IsProcessing, () => station.ProcessingProgress);
        }

        DrinkMachine[] machines = FindObjectsByType<DrinkMachine>(FindObjectsSortMode.None);
        for (int i = 0; i < machines.Length; i++)
        {
            DrinkMachine station = machines[i];
            AddMarker(station.transform, "A ENCHER", () => station.IsProcessing, () => station.ProcessingProgress);
        }
    }

    private void Update()
    {
        if (gameplayCamera == null)
            gameplayCamera = Camera.main;

        bool uiAllowed = Time.timeScale > 0f && gameplayCamera != null;

        for (int i = 0; i < markers.Count; i++)
        {
            StationMarker marker = markers[i];
            bool processing = marker.target != null && marker.isProcessing();
            float progress = marker.target != null ? Mathf.Clamp01(marker.getProgress()) : 0f;

            if (marker.wasProcessing && !processing && progress >= 0.99f)
                marker.readyUntil = Time.unscaledTime + 1.15f;
            marker.wasProcessing = processing;

            bool ready = !processing && Time.unscaledTime < marker.readyUntil;
            bool visible = uiAllowed && marker.target != null && (processing || ready);

            if (visible)
            {
                Vector3 screenPoint = gameplayCamera.WorldToScreenPoint(marker.target.position + Vector3.up * 1.35f);
                visible = screenPoint.z > 0f
                    && screenPoint.x > 35f && screenPoint.x < Screen.width - 35f
                    && screenPoint.y > 35f && screenPoint.y < Screen.height - 35f;

                if (visible)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint);
                    marker.root.anchoredPosition = localPoint + new Vector2(0f, 72f);
                }
            }

            marker.root.gameObject.SetActive(visible);
            if (!visible)
                continue;

            float shownProgress = ready ? 1f : progress;
            marker.fill.sizeDelta = new Vector2(274f * shownProgress, 15f);
            marker.fillImage.color = ready ? mint : Color.Lerp(coral, peach, shownProgress);
            marker.label.text = ready
                ? "<b>PRONTO!</b>"
                : "<b>" + marker.actionLabel + "  " + Mathf.RoundToInt(shownProgress * 100f) + "%</b>";

            float pulse = ready ? 1f + Mathf.Sin(Time.unscaledTime * 12f) * 0.035f : 1f;
            marker.root.localScale = Vector3.one * pulse;
        }
    }

    private void AddMarker(Transform target, string label, Func<bool> isProcessing, Func<float> getProgress)
    {
        StationMarker marker = CreateMarker(label);
        marker.target = target;
        marker.actionLabel = label;
        marker.isProcessing = isProcessing;
        marker.getProgress = getProgress;
        markers.Add(marker);
    }

    private void BuildCanvas()
    {
        GameObject canvasObject = new GameObject("Progresso das estações", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 76;
        canvasRect = canvasObject.GetComponent<RectTransform>();

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
    }

    private StationMarker CreateMarker(string labelValue)
    {
        GameObject rootObject = new GameObject(labelValue, typeof(RectTransform), typeof(CanvasGroup));
        rootObject.transform.SetParent(canvas.transform, false);
        RectTransform root = rootObject.GetComponent<RectTransform>();
        root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(330f, 78f);

        Image shadow = CreateImage("Sombra", root, new Color(cocoa.r, cocoa.g, cocoa.b, 0.3f));
        Stretch(shadow.rectTransform, new Vector2(-4f, -7f), new Vector2(5f, -1f));

        Image border = CreateImage("Moldura", root, cocoa);
        Stretch(border.rectTransform, Vector2.zero, Vector2.zero);

        Image background = CreateImage("Fundo", border.transform, cream);
        Stretch(background.rectTransform, new Vector2(4f, 4f), new Vector2(-4f, -4f));

        TMP_Text label = CreateText("Estado", background.transform, 22f);
        RectTransform labelRect = label.rectTransform;
        labelRect.offsetMin = new Vector2(18f, 29f);
        labelRect.offsetMax = new Vector2(-18f, -5f);

        Image track = CreateImage("Calha", background.transform, new Color(cocoa.r, cocoa.g, cocoa.b, 0.25f));
        RectTransform trackRect = track.rectTransform;
        trackRect.anchorMin = trackRect.anchorMax = new Vector2(0.5f, 0f);
        trackRect.anchoredPosition = new Vector2(0f, 14f);
        trackRect.sizeDelta = new Vector2(280f, 21f);

        Image fillImage = CreateImage("Progresso", track.transform, coral);
        RectTransform fillRect = fillImage.rectTransform;
        fillRect.anchorMin = fillRect.anchorMax = new Vector2(0f, 0.5f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = new Vector2(3f, 0f);
        fillRect.sizeDelta = new Vector2(0f, 15f);

        rootObject.SetActive(false);
        return new StationMarker
        {
            root = root,
            fill = fillRect,
            fillImage = fillImage,
            label = label,
            actionLabel = labelValue
        };
    }

    private Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        return image;
    }

    private TMP_Text CreateText(string name, Transform parent, float size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        TMP_Text text = obj.GetComponent<TMP_Text>();
        text.fontSize = size;
        text.color = cocoa;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(10f, 5f);
        rect.offsetMax = new Vector2(-10f, -5f);
        return text;
    }

    private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null)
            return roundedSprite;

        const int size = 64;
        const float radius = 18f;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "Station Progress Rounded UI";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float px = Mathf.Clamp(x, radius, size - 1f - radius);
            float py = Mathf.Clamp(y, radius, size - 1f - radius);
            float alpha = Mathf.Clamp01(radius + 0.5f - Vector2.Distance(new Vector2(x, y), new Vector2(px, py)));
            texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }

        texture.Apply();
        roundedSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), Vector2.one * 0.5f, 100f, 0,
            SpriteMeshType.FullRect, new Vector4(20f, 20f, 20f, 20f));
        return roundedSprite;
    }
}
