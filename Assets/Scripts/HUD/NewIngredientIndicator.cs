using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Assinala as estacoes que sao novidade em cada nivel. A indicacao so e
/// considerada vista depois de o jogador apanhar o ingrediente.
/// </summary>
public sealed class NewIngredientIndicator : MonoBehaviour
{
    private sealed class Marker
    {
        public string ingredient;
        public Transform target;
        public RectTransform root;
    }

    private static NewIngredientIndicator instance;
    private readonly List<Marker> markers = new List<Marker>();

    private Canvas canvas;
    private RectTransform canvasRect;
    private Camera gameplayCamera;
    private PlayerHand playerHand;
    private static Sprite roundedSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene != "Level2" && scene != "Level3")
            return;

        if (FindFirstObjectByType<NewIngredientIndicator>() == null)
            new GameObject("New Ingredient Indicator").AddComponent<NewIngredientIndicator>();
    }

    private IEnumerator Start()
    {
        instance = this;
        BuildCanvas();
        yield return null;

        IntroDialogue intro = FindFirstObjectByType<IntroDialogue>();
        while (intro != null && intro.IsDialogueActive())
            yield return null;

        string scene = SceneManager.GetActiveScene().name;
        if (scene == "Level2")
        {
            AddMarker("Tomato", "TomatoBox", "TOMATE");
            AddMarker("Pepper", "PepperBox", "PIMENTO");
        }
        else if (scene == "Level3")
        {
            AddMarker("FrozenFries", "FriesBox", "BATATAS");
            AddMarker("EmptyCup", "Copos", "COPOS / BEBIDAS");
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void Update()
    {
        if (canvas == null)
            return;

        if (gameplayCamera == null)
            gameplayCamera = Camera.main;
        if (playerHand == null)
            playerHand = FindFirstObjectByType<PlayerHand>();

        if (playerHand != null && playerHand.HasItem())
            NotifyIngredientUsed(playerHand.GetCurrentItem());

        bool uiAllowed = Time.timeScale > 0f && gameplayCamera != null;
        float bob = Mathf.Sin(Time.unscaledTime * 4.2f) * 8f;

        for (int i = 0; i < markers.Count; i++)
        {
            Marker marker = markers[i];
            if (marker.target == null || !uiAllowed)
            {
                marker.root.gameObject.SetActive(false);
                continue;
            }

            Vector3 screenPoint = gameplayCamera.WorldToScreenPoint(marker.target.position + Vector3.up * 1.1f);
            bool visible = screenPoint.z > 0f
                && screenPoint.x > 30f && screenPoint.x < Screen.width - 30f
                && screenPoint.y > 30f && screenPoint.y < Screen.height - 30f;

            marker.root.gameObject.SetActive(visible);
            if (!visible)
                continue;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint);
            marker.root.anchoredPosition = localPoint + new Vector2(0f, 112f + bob);
        }
    }

    public static void NotifyIngredientUsed(string ingredient)
    {
        if (string.IsNullOrEmpty(ingredient) || instance == null || !instance.HasMarker(ingredient))
            return;

        PlayerPrefs.SetInt(GetSeenKey(ingredient), 1);
        PlayerPrefs.Save();
        instance.RemoveMarker(ingredient);
    }

    private void AddMarker(string ingredient, string objectName, string displayName)
    {
        if (PlayerPrefs.GetInt(GetSeenKey(ingredient), 0) != 0)
            return;

        GameObject station = GameObject.Find(objectName);
        if (station == null)
            return;

        markers.Add(new Marker
        {
            ingredient = ingredient,
            target = station.transform,
            root = CreateMarker(displayName)
        });
    }

    private bool HasMarker(string ingredient)
    {
        for (int i = 0; i < markers.Count; i++)
            if (markers[i].ingredient == ingredient)
                return true;
        return false;
    }

    private void RemoveMarker(string ingredient)
    {
        for (int i = markers.Count - 1; i >= 0; i--)
        {
            if (markers[i].ingredient != ingredient)
                continue;

            if (markers[i].root != null)
                Destroy(markers[i].root.gameObject);
            markers.RemoveAt(i);
        }
    }

    private void BuildCanvas()
    {
        GameObject canvasObject = new GameObject("Novidades Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 72;
        canvasRect = canvasObject.GetComponent<RectTransform>();

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
    }

    private RectTransform CreateMarker(string displayName)
    {
        GameObject rootObject = new GameObject("Novo " + displayName, typeof(RectTransform), typeof(CanvasGroup));
        rootObject.transform.SetParent(canvas.transform, false);
        RectTransform root = rootObject.GetComponent<RectTransform>();
        root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(340f, 116f);

        Image shadow = CreateImage("Sombra", root, new Color(0.18f, 0.07f, 0.03f, 0.32f));
        SetRect(shadow.rectTransform, new Vector2(5f, 35f), new Vector2(292f, 60f));

        Image border = CreateImage("Moldura", root, new Color(0.28f, 0.12f, 0.055f, 1f));
        SetRect(border.rectTransform, new Vector2(0f, 41f), new Vector2(292f, 60f));

        Image background = CreateImage("Fundo", border.rectTransform, new Color(1f, 0.965f, 0.82f, 1f));
        RectTransform backgroundRect = background.rectTransform;
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = new Vector2(4f, 4f);
        backgroundRect.offsetMax = new Vector2(-4f, -4f);

        Image newBadge = CreateImage("Novo", background.rectTransform, new Color(0.95f, 0.29f, 0.18f, 1f));
        RectTransform badgeRect = newBadge.rectTransform;
        badgeRect.anchorMin = badgeRect.anchorMax = new Vector2(0f, 0.5f);
        badgeRect.pivot = new Vector2(0f, 0.5f);
        badgeRect.anchoredPosition = new Vector2(9f, 0f);
        badgeRect.sizeDelta = new Vector2(78f, 39f);
        TMP_Text badgeText = CreateText("Texto Novo", newBadge.rectTransform, "NOVO", 18f);
        badgeText.color = Color.white;

        TMP_Text nameText = CreateText("Nome", backgroundRect, displayName, 24f);
        RectTransform nameRect = nameText.rectTransform;
        nameRect.offsetMin = new Vector2(96f, 4f);
        nameRect.offsetMax = new Vector2(-12f, -4f);
        nameText.color = new Color(0.28f, 0.12f, 0.055f, 1f);
        nameText.alignment = TextAlignmentOptions.MidlineLeft;

        TMP_Text arrow = CreateText("Seta", root, "▼", 58f);
        RectTransform arrowRect = arrow.rectTransform;
        arrowRect.anchorMin = arrowRect.anchorMax = new Vector2(0.5f, 0f);
        arrowRect.pivot = new Vector2(0.5f, 0f);
        arrowRect.anchoredPosition = new Vector2(0f, -9f);
        arrowRect.sizeDelta = new Vector2(90f, 62f);
        arrow.color = new Color(0.95f, 0.29f, 0.18f, 1f);
        arrow.outlineColor = new Color(0.28f, 0.12f, 0.055f, 0.8f);
        arrow.outlineWidth = 0.18f;
        return root;
    }

    private static string GetSeenKey(string ingredient)
    {
        return "FlyingBurger.NewIngredientSeen." + ingredient;
    }

    private static Image CreateImage(string name, Transform parent, Color color)
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

    private static TMP_Text CreateText(string name, Transform parent, string value, float size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(8f, 3f);
        rect.offsetMax = new Vector2(-8f, -3f);
        TMP_Text text = obj.GetComponent<TMP_Text>();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null)
            return roundedSprite;

        const int size = 64;
        const float radius = 18f;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "New Ingredient Rounded UI";
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
