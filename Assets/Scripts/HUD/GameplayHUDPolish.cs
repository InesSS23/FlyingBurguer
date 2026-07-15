using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayHUDPolish : MonoBehaviour
{
    public static GameplayHUDPolish Instance { get; private set; }

    private readonly Color cream = new Color(1f, 0.96f, 0.84f, 0.96f);
    private readonly Color cocoa = new Color(0.20f, 0.10f, 0.07f, 1f);
    private readonly Color coral = new Color(1f, 0.39f, 0.27f, 1f);
    private readonly Color mint = new Color(0.32f, 0.78f, 0.57f, 1f);

    private Image crosshair;
    private Canvas hudCanvas;
    private CanvasGroup promptGroup;
    private TMP_Text promptText;
    private CanvasGroup handGroup;
    private TMP_Text handText;
    private PlayerHand playerHand;
    private Component currentTarget;
    private Coroutine pulseRoutine;
    private bool showInteractionInstructions;
    private bool hiddenByMenu;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (!SceneManager.GetActiveScene().name.StartsWith("Level"))
            return;

        if (FindFirstObjectByType<GameplayHUDPolish>() == null)
            new GameObject("Gameplay HUD Polish").AddComponent<GameplayHUDPolish>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        showInteractionInstructions = SceneManager.GetActiveScene().name == "Level1";
        BuildHUD();
    }

    private void Start() => playerHand = FindFirstObjectByType<PlayerHand>();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        bool gameplayVisible = !hiddenByMenu && Time.timeScale > 0f;
        if (hudCanvas.enabled != gameplayVisible)
            hudCanvas.enabled = gameplayVisible;

        if (!gameplayVisible)
            return;

        UpdatePrompt();
        UpdateHandStatus();
    }

    public void SetInteractionTarget(Component target) => currentTarget = target;

    public void SetMenuVisible(bool menuVisible)
    {
        hiddenByMenu = menuVisible;

        if (hudCanvas != null)
            hudCanvas.enabled = !menuVisible && Time.timeScale > 0f;
    }

    public void PulseInteraction(bool success)
    {
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulseCrosshair(success ? mint : coral));
    }

    private void BuildHUD()
    {
        hudCanvas = gameObject.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 80;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        crosshair = CreateImage("Mira", transform, cream);
        RectTransform crosshairRect = crosshair.rectTransform;
        crosshairRect.anchorMin = crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRect.sizeDelta = new Vector2(9f, 9f);

        Image ring = CreateImage("Anel", crosshair.transform, cream);
        ring.sprite = CreateRingSprite();
        ring.rectTransform.anchorMin = Vector2.zero;
        ring.rectTransform.anchorMax = Vector2.one;
        ring.rectTransform.offsetMin = new Vector2(-9f, -9f);
        ring.rectTransform.offsetMax = new Vector2(9f, 9f);

        promptGroup = CreatePill("Painel de interação", new Vector2(0.5f, 0.5f), new Vector2(0f, -68f), new Vector2(390f, 58f));
        promptText = CreateText("Ação", promptGroup.transform, 24);
        promptGroup.alpha = 0f;

        handGroup = CreatePill("Item na mão", new Vector2(0.5f, 0f), new Vector2(0f, 58f), new Vector2(430f, 62f));
        handText = CreateText("Item", handGroup.transform, 23);
        handGroup.alpha = 0f;
    }

    private void UpdatePrompt()
    {
        bool hasTarget = showInteractionInstructions
            && currentTarget != null
            && currentTarget.gameObject.activeInHierarchy;
        promptGroup.alpha = Mathf.MoveTowards(promptGroup.alpha, hasTarget ? 1f : 0f, Time.unscaledDeltaTime * 8f);
        crosshair.color = Color.Lerp(crosshair.color, hasTarget ? mint : cream, Time.unscaledDeltaTime * 12f);
        if (hasTarget) promptText.text = "<b>CLIQUE</b>  •  " + GetActionName(currentTarget);
    }

    private void UpdateHandStatus()
    {
        if (playerHand == null) playerHand = FindFirstObjectByType<PlayerHand>();
        string label = GetHandLabel();
        bool visible = !string.IsNullOrEmpty(label);
        handGroup.alpha = Mathf.MoveTowards(handGroup.alpha, visible ? 1f : 0f, Time.unscaledDeltaTime * 7f);
        if (visible) handText.text = "NA MÃO  •  <b>" + label + "</b>";
    }

    private string GetHandLabel()
    {
        if (playerHand == null || playerHand.IsEmpty()) return string.Empty;
        if (playerHand.HasTray()) return "Tabuleiro preparado";
        if (playerHand.HasBurger()) return "Hambúrguer (" + playerHand.GetBurgerCopy().Count + " camadas)";

        switch (playerHand.GetCurrentItem())
        {
            case "Bread": return "Pão";
            case "RawMeat": return "Carne crua";
            case "CookedMeat": return "Carne grelhada";
            case "Cheese": return "Queijo";
            case "Lettuce": return "Alface";
            case "Tomato": return "Tomate";
            case "Pepper": return "Pimento";
            case "FrozenFries": return "Batatas congeladas";
            case "CookedFries": return "Batatas fritas";
            case "EmptyCup": return "Copo vazio";
            case "Drink": return "Bebida";
            default: return playerHand.GetCurrentItem();
        }
    }

    private string GetActionName(Component target)
    {
        if (target is GrillStation) return "Usar grelhador";
        if (target is FryerStation) return "Usar fritadeira";
        if (target is DrinkMachine) return "Preparar bebida";
        if (target is AssemblyTable) return "Montar hambúrguer";
        if (target is DeliveryCounter) return "Entregar pedido";
        if (target is TrashBin) return "Deitar fora";
        if (target is IngredientStation) return "Pegar ou devolver ingrediente";
        return "Interagir";
    }

    private CanvasGroup CreatePill(string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        Image panel = CreateImage(name, transform, cream);
        RectTransform rect = panel.rectTransform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor.y == 0f ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;
        return group;
    }

    private Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private TMP_Text CreateText(string name, Transform parent, int size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.color = cocoa;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(20f, 6f);
        rect.offsetMax = new Vector2(-20f, -6f);
        return text;
    }

    private Sprite CreateRingSprite()
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = Vector2.one * ((size - 1) * 0.5f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float distance = Vector2.Distance(new Vector2(x, y), center);
            texture.SetPixel(x, y, new Color(1f, 1f, 1f, distance > 12f && distance < 14.5f ? 1f : 0f));
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), Vector2.one * 0.5f, 100f);
    }

    private IEnumerator PulseCrosshair(Color pulseColor)
    {
        RectTransform rect = crosshair.rectTransform;
        for (float elapsed = 0f; elapsed < 0.18f; elapsed += Time.unscaledDeltaTime)
        {
            float t = elapsed / 0.18f;
            rect.localScale = Vector3.one * (1f + Mathf.Sin(t * Mathf.PI) * 1.1f);
            crosshair.color = Color.Lerp(pulseColor, currentTarget != null ? mint : cream, t);
            yield return null;
        }
        rect.localScale = Vector3.one;
        pulseRoutine = null;
    }
}
