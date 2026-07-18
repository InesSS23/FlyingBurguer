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
    private TMP_Text promptKeyText;
    private CanvasGroup handGroup;
    private TMP_Text handText;
    private CanvasGroup feedbackGroup;
    private TMP_Text feedbackText;
    private PlayerHand playerHand;
    private Component currentTarget;
    private Coroutine pulseRoutine;
    private Coroutine feedbackRoutine;
    private bool showInteractionInstructions;
    private bool hiddenByMenu;
    private Sprite roundedRectSprite;
    private string lastFeedbackMessage = string.Empty;
    private float lastFeedbackTime = -10f;

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
        Application.logMessageReceived += HandleGameplayLog;
    }

    private void Start() => playerHand = FindFirstObjectByType<PlayerHand>();

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleGameplayLog;
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

        promptGroup = CreatePill("Painel de interação", new Vector2(0.5f, 0.5f), new Vector2(0f, -76f), new Vector2(540f, 72f));

        Image keyBadge = CreateImage("Botão de clique", promptGroup.transform, coral);
        ApplyRoundedStyle(keyBadge);
        RectTransform keyRect = keyBadge.rectTransform;
        keyRect.anchorMin = keyRect.anchorMax = new Vector2(0f, 0.5f);
        keyRect.pivot = new Vector2(0f, 0.5f);
        keyRect.anchoredPosition = new Vector2(12f, 0f);
        keyRect.sizeDelta = new Vector2(126f, 48f);

        promptKeyText = CreateText("Comando", keyBadge.transform, 18);
        promptKeyText.text = "<b>CLIQUE</b>";
        promptKeyText.color = cream;
        promptKeyText.textWrappingMode = TextWrappingModes.NoWrap;
        promptKeyText.overflowMode = TextOverflowModes.Overflow;
        RectTransform keyTextRect = promptKeyText.rectTransform;
        keyTextRect.offsetMin = new Vector2(5f, 3f);
        keyTextRect.offsetMax = new Vector2(-5f, -3f);

        promptText = CreateText("Ação", promptGroup.transform, 25);
        promptText.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform promptRect = promptText.rectTransform;
        promptRect.offsetMin = new Vector2(154f, 7f);
        promptRect.offsetMax = new Vector2(-18f, -7f);
        promptGroup.alpha = 0f;

        handGroup = CreatePill("Item na mão", new Vector2(0.5f, 0f), new Vector2(0f, 58f), new Vector2(430f, 62f));
        handText = CreateText("Item", handGroup.transform, 23);
        handGroup.alpha = 0f;

        feedbackGroup = CreatePill("Aviso de jogabilidade", new Vector2(0.5f, 1f), new Vector2(0f, -128f), new Vector2(650f, 78f));

        Image warningBadge = CreateImage("Símbolo", feedbackGroup.transform, coral);
        ApplyRoundedStyle(warningBadge);
        RectTransform warningRect = warningBadge.rectTransform;
        warningRect.anchorMin = warningRect.anchorMax = new Vector2(0f, 0.5f);
        warningRect.pivot = new Vector2(0f, 0.5f);
        warningRect.anchoredPosition = new Vector2(12f, 0f);
        warningRect.sizeDelta = new Vector2(58f, 54f);

        TMP_Text warningText = CreateText("Exclamação", warningBadge.transform, 34);
        warningText.text = "<b>!</b>";
        warningText.color = cream;
        warningText.rectTransform.offsetMin = Vector2.zero;
        warningText.rectTransform.offsetMax = Vector2.zero;

        feedbackText = CreateText("Mensagem", feedbackGroup.transform, 24);
        feedbackText.alignment = TextAlignmentOptions.MidlineLeft;
        feedbackText.textWrappingMode = TextWrappingModes.NoWrap;
        feedbackText.overflowMode = TextOverflowModes.Ellipsis;
        RectTransform feedbackRect = feedbackText.rectTransform;
        feedbackRect.offsetMin = new Vector2(88f, 8f);
        feedbackRect.offsetMax = new Vector2(-22f, -8f);
        feedbackGroup.alpha = 0f;
        feedbackGroup.transform.localScale = Vector3.one * 0.94f;
    }

    public void ShowFeedback(string message)
    {
        if (feedbackGroup == null || feedbackText == null || string.IsNullOrWhiteSpace(message))
            return;

        if (message.Trim().Equals("pedido incorreto", System.StringComparison.OrdinalIgnoreCase))
            message = "O pedido não corresponde a nenhum cliente.";

        if (message == lastFeedbackMessage && Time.unscaledTime - lastFeedbackTime < 0.45f)
            return;

        lastFeedbackMessage = message;
        lastFeedbackTime = Time.unscaledTime;

        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackText.text = "<b>" + message + "</b>";
        feedbackRoutine = StartCoroutine(ShowFeedbackRoutine());
    }

    private void HandleGameplayLog(string condition, string stackTrace, LogType type)
    {
        if (type != LogType.Log || hiddenByMenu || Time.timeScale <= 0f)
            return;

        string message = TranslateGameplayLog(condition);
        if (!string.IsNullOrEmpty(message))
            ShowFeedback(message);
    }

    private string TranslateGameplayLog(string log)
    {
        if (string.IsNullOrEmpty(log)) return string.Empty;
        string value = log.ToLowerInvariant();

        if (value.Contains("ja tens outra coisa na mao") || value.Contains("ja tenho algo na mao"))
            return "A tua mão já está ocupada.";
        if (value.Contains("n tens nada para deitar fora"))
            return "Não tens nada para deitar fora.";
        if (value.Contains("n tens nada na mao e a mesa esta vazia"))
            return "A mesa está vazia e não tens nada na mão.";
        if (value.Contains("n podes meter carne crua"))
            return "Grelha a carne antes de a colocares no pedido.";
        if (value.Contains("batatas ainda estao congeladas"))
            return "Frita as batatas antes de as colocares no tabuleiro.";
        if (value.Contains("encher o copo antes"))
            return "Enche o copo na máquina de bebidas primeiro.";
        if (value.Contains("tabuleiro ja tem batatas"))
            return "Este tabuleiro já tem batatas.";
        if (value.Contains("tabuleiro ja tem bebida"))
            return "Este tabuleiro já tem uma bebida.";
        if (value.Contains("a tua mao n esta vazia"))
            return "Precisas de ter a mão vazia para pegar no tabuleiro.";
        if (value.Contains("a mesa ja tem um pedido"))
            return "A mesa de montagem já está ocupada.";
        if (value.Contains("carne ainda esta a cozinhar"))
            return "A carne ainda está a grelhar.";
        if (value.Contains("n tens carne para meter na grelha"))
            return "Apanha carne crua antes de usares o grelhador.";
        if (value.Contains("isto n pode ir para a grelha"))
            return "Só podes colocar carne crua no grelhador.";
        if (value.Contains("mao vazia para tirar a carne"))
            return "Liberta a mão para retirar a carne.";
        if (value.Contains("batatas ainda estao a fritar"))
            return "As batatas ainda estão a fritar.";
        if (value.Contains("n tens batatas congeladas"))
            return "Apanha batatas congeladas antes de usares a fritadeira.";
        if (value.Contains("isto n pode ir para a fritadeira"))
            return "Só podes colocar batatas congeladas na fritadeira.";
        if (value.Contains("mao vazia para tirar as batatas"))
            return "Liberta a mão para retirar as batatas.";
        if (value.Contains("bebida ainda esta a encher"))
            return "A bebida ainda está a encher.";
        if (value.Contains("n tens copo vazio"))
            return "Apanha um copo vazio antes de usares a máquina.";
        if (value.Contains("isto n pode ir para a maquina de bebidas"))
            return "Só podes colocar um copo vazio nesta máquina.";
        if (value.Contains("mao vazia para tirar a bebida"))
            return "Liberta a mão para retirar a bebida.";
        if (value.Contains("n podes entregar so um ingrediente"))
            return "Monta o pedido completo num tabuleiro antes de o entregar.";
        if (value.Contains("pedidos agora devem ser entregues no tabuleiro"))
            return "Coloca o hambúrguer num tabuleiro antes da entrega.";
        if (value.Contains("n tens tabuleiro para entregar"))
            return "Não tens nenhum tabuleiro para entregar.";
        if (value.Contains("tabuleiro vazio"))
            return "O tabuleiro está vazio.";

        return string.Empty;
    }

    private void UpdatePrompt()
    {
        bool hasTarget = showInteractionInstructions
            && currentTarget != null
            && currentTarget.gameObject.activeInHierarchy;
        promptGroup.alpha = Mathf.MoveTowards(promptGroup.alpha, hasTarget ? 1f : 0f, Time.unscaledDeltaTime * 8f);
        float promptScale = Mathf.Lerp(0.94f, 1f, Mathf.SmoothStep(0f, 1f, promptGroup.alpha));
        promptGroup.transform.localScale = Vector3.one * promptScale;
        crosshair.color = Color.Lerp(crosshair.color, hasTarget ? mint : cream, Time.unscaledDeltaTime * 12f);
        if (hasTarget)
            promptText.text = "<b>" + GetActionName(currentTarget) + "</b>";
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
        GameObject wrapper = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        wrapper.transform.SetParent(transform, false);

        RectTransform rect = wrapper.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor.y == 0f ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image shadow = CreateImage("Sombra", wrapper.transform, new Color(cocoa.r, cocoa.g, cocoa.b, 0.24f));
        ApplyRoundedStyle(shadow);
        RectTransform shadowRect = shadow.rectTransform;
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(-4f, -7f);
        shadowRect.offsetMax = new Vector2(4f, -1f);

        Image border = CreateImage("Moldura", wrapper.transform, new Color(0.34f, 0.17f, 0.09f, 0.98f));
        ApplyRoundedStyle(border);
        RectTransform borderRect = border.rectTransform;
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;

        Image background = CreateImage("Fundo creme", border.transform, cream);
        ApplyRoundedStyle(background);
        RectTransform backgroundRect = background.rectTransform;
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = new Vector2(3f, 3f);
        backgroundRect.offsetMax = new Vector2(-3f, -3f);

        Image highlight = CreateImage("Brilho superior", border.transform, new Color(1f, 0.78f, 0.46f, 0.48f));
        ApplyRoundedStyle(highlight);
        RectTransform highlightRect = highlight.rectTransform;
        highlightRect.anchorMin = new Vector2(0f, 1f);
        highlightRect.anchorMax = new Vector2(1f, 1f);
        highlightRect.pivot = new Vector2(0.5f, 1f);
        highlightRect.offsetMin = new Vector2(14f, -9f);
        highlightRect.offsetMax = new Vector2(-14f, -5f);

        CanvasGroup group = wrapper.GetComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;
        return group;
    }

    private void ApplyRoundedStyle(Image image)
    {
        if (roundedRectSprite == null)
            roundedRectSprite = CreateRoundedRectSprite();

        image.sprite = roundedRectSprite;
        image.type = Image.Type.Sliced;
    }

    private Sprite CreateRoundedRectSprite()
    {
        const int size = 64;
        const float radius = 18f;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "HUD Rounded Rectangle";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float nearestX = Mathf.Clamp(x, radius, size - 1f - radius);
            float nearestY = Mathf.Clamp(y, radius, size - 1f - radius);
            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(nearestX, nearestY));
            float alpha = Mathf.Clamp01(radius + 0.5f - distance);
            texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }

        texture.Apply();
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            Vector2.one * 0.5f,
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(20f, 20f, 20f, 20f)
        );
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

    private IEnumerator ShowFeedbackRoutine()
    {
        feedbackGroup.alpha = 0f;
        feedbackGroup.transform.localScale = Vector3.one * 0.94f;

        for (float elapsed = 0f; elapsed < 0.16f; elapsed += Time.unscaledDeltaTime)
        {
            float t = Mathf.Clamp01(elapsed / 0.16f);
            feedbackGroup.alpha = t;
            feedbackGroup.transform.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, t);
            yield return null;
        }

        feedbackGroup.alpha = 1f;
        feedbackGroup.transform.localScale = Vector3.one;
        yield return new WaitForSecondsRealtime(2.15f);

        for (float elapsed = 0f; elapsed < 0.28f; elapsed += Time.unscaledDeltaTime)
        {
            float t = Mathf.Clamp01(elapsed / 0.28f);
            feedbackGroup.alpha = 1f - t;
            feedbackGroup.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.97f, t);
            yield return null;
        }

        feedbackGroup.alpha = 0f;
        feedbackRoutine = null;
    }
}
