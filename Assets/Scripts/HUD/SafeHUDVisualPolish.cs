using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Melhora apenas a apresentacao do HUD existente. Nao altera anchors,
/// tamanhos, layout groups nem os dados controlados pelos restantes scripts.
/// </summary>
public sealed class SafeHUDVisualPolish : MonoBehaviour
{
    private static Sprite roundedSprite;

    private static readonly Color Cocoa = new Color(0.24f, 0.105f, 0.055f, 1f);
    private static readonly Color Coral = new Color(0.95f, 0.29f, 0.18f, 1f);
    private static readonly Color Peach = new Color(1f, 0.66f, 0.28f, 1f);
    private static readonly Color Cream = new Color(1f, 0.965f, 0.84f, 1f);
    private static readonly Color Mint = new Color(0.58f, 0.84f, 0.48f, 1f);

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

        if (FindFirstObjectByType<SafeHUDVisualPolish>() == null)
            new GameObject("Safe HUD Visual Polish").AddComponent<SafeHUDVisualPolish>();
    }

    private IEnumerator Start()
    {
        yield return null;
        PolishDayPanel();
        PolishOrdersPanel();
    }

    private void PolishDayPanel()
    {
        GameObject panel = GameObject.Find("DayHUDPanel");
        if (panel == null)
            return;

        AddSoftShadow(panel, new Vector2(5f, -5f));
        TintNamedImage(panel.transform, "FundoDia", Coral);
        TintNamedImage(panel.transform, "FundoTimer", Peach);
        TintNamedImage(panel.transform, "FundoScore", new Color(0.83f, 0.95f, 0.72f, 1f));

        StyleNamedText(panel.transform, "DayText", Color.white, 0.16f);
        StyleNamedText(panel.transform, "TimerText", Cocoa, 0.08f);
        StyleNamedText(panel.transform, "ScoreText", Cocoa, 0.08f);
    }

    private void PolishOrdersPanel()
    {
        GameObject panel = GameObject.Find("OrdersPanel");
        if (panel == null)
            return;

        EnlargeOrderSlots(panel.transform);

        if (panel.transform.Find("Cabeçalho novo") != null)
            return;

        AddSoftShadow(panel, new Vector2(7f, -7f));

        Image header = CreateImage("Cabeçalho novo", panel.transform, Coral);
        AddIgnoreLayout(header.gameObject);
        ApplyRoundedStyle(header);
        RectTransform headerRect = header.rectTransform;
        headerRect.anchorMin = headerRect.anchorMax = new Vector2(0.5f, 1f);
        headerRect.pivot = new Vector2(0.5f, 0.5f);
        headerRect.anchoredPosition = new Vector2(0f, -37f);
        headerRect.sizeDelta = new Vector2(184f, 48f);

        Shadow headerShadow = header.gameObject.AddComponent<Shadow>();
        headerShadow.effectColor = new Color(Cocoa.r, Cocoa.g, Cocoa.b, 0.32f);
        headerShadow.effectDistance = new Vector2(4f, -4f);

        TMP_Text title = CreateText("Título", header.transform, "PEDIDOS", 25f);
        title.color = Color.white;
        title.fontStyle = FontStyles.Bold;
        title.outlineColor = new Color(Cocoa.r, Cocoa.g, Cocoa.b, 0.5f);
        title.outlineWidth = 0.12f;

        CreatePin(header.transform, new Vector2(-72f, 0f));
        CreatePin(header.transform, new Vector2(72f, 0f));

        Image underline = CreateImage("Sublinhado", panel.transform, Peach);
        AddIgnoreLayout(underline.gameObject);
        ApplyRoundedStyle(underline);
        RectTransform underlineRect = underline.rectTransform;
        underlineRect.anchorMin = underlineRect.anchorMax = new Vector2(0.5f, 1f);
        underlineRect.pivot = new Vector2(0.5f, 0.5f);
        underlineRect.anchoredPosition = new Vector2(0f, -66f);
        underlineRect.sizeDelta = new Vector2(118f, 5f);
    }

    private static void EnlargeOrderSlots(Transform panel)
    {
        for (int i = 0; i < panel.childCount; i++)
        {
            Transform slot = panel.GetChild(i);
            if (!slot.name.StartsWith("OrderSlot"))
                continue;

            // O layout continua a reservar o mesmo espaco. Apenas a representacao
            // visual do papel e dos ingredientes fica maior dentro do quadro.
            slot.localScale = new Vector3(0.85f, 0.82f, 1f);

            Transform icons = slot.Find("IconsParent");
            if (icons != null)
                icons.localScale = Vector3.one;
        }
    }

    private static void TintNamedImage(Transform root, string objectName, Color color)
    {
        Transform child = root.Find(objectName);
        if (child == null)
            return;

        Image image = child.GetComponent<Image>();
        if (image != null)
            image.color = color;
    }

    private static void StyleNamedText(Transform root, string objectName, Color color, float outlineWidth)
    {
        Transform child = root.Find(objectName);
        if (child == null)
            return;

        TMP_Text text = child.GetComponent<TMP_Text>();
        if (text == null)
            return;

        text.color = color;
        text.fontStyle = FontStyles.Bold;
        text.outlineColor = new Color(Cocoa.r, Cocoa.g, Cocoa.b, 0.5f);
        text.outlineWidth = outlineWidth;
    }

    private static void AddSoftShadow(GameObject target, Vector2 distance)
    {
        if (target.GetComponent<Shadow>() != null)
            return;

        Shadow shadow = target.AddComponent<Shadow>();
        shadow.effectColor = new Color(Cocoa.r, Cocoa.g, Cocoa.b, 0.28f);
        shadow.effectDistance = distance;
        shadow.useGraphicAlpha = true;
    }

    private static void CreatePin(Transform parent, Vector2 position)
    {
        Image pin = CreateImage("Rebite", parent, Cream);
        RectTransform rect = pin.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(13f, 13f);
        pin.sprite = CreateCircleSprite();
    }

    private static void AddIgnoreLayout(GameObject target)
    {
        LayoutElement layout = target.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text CreateText(string name, Transform parent, string value, float fontSize)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(18f, 3f);
        rect.offsetMax = new Vector2(-18f, -3f);

        TMP_Text text = obj.GetComponent<TMP_Text>();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        return text;
    }

    private static void ApplyRoundedStyle(Image image)
    {
        if (roundedSprite == null)
            roundedSprite = CreateRoundedSprite();
        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
    }

    private static Sprite CreateRoundedSprite()
    {
        const int size = 64;
        const float radius = 18f;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "Safe HUD Rounded Rectangle";
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
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), Vector2.one * 0.5f, 100f, 0,
            SpriteMeshType.FullRect, new Vector4(20f, 20f, 20f, 20f));
    }

    private static Sprite CreateCircleSprite()
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "Safe HUD Pin";
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = Vector2.one * (size - 1f) * 0.5f;
        float radius = size * 0.46f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float alpha = Mathf.Clamp01(radius + 0.5f - Vector2.Distance(new Vector2(x, y), center));
            texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), Vector2.one * 0.5f, 100f);
    }
}
