using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderSlotUI : MonoBehaviour
{
    [Header("objetos da ui")]
    [SerializeField] private GameObject slotRoot;
    [SerializeField] private Transform iconsParent;

    [Header("tamanhos")]
    [SerializeField] private Vector2 ingredientIconSize = new Vector2(90f, 90f);
    [SerializeField] private Vector2 extraIconSize = new Vector2(80f, 80f);

    [Header("barra de tempo")]
    [SerializeField] private Vector2 horizontalTimeBarSize = new Vector2(580f, 13f);
    [SerializeField] private float timeBarGapBelowOrder = 33f;
    [SerializeField] private float timeAlertSeconds = 15f;
    [SerializeField] private Color timeBarNormalColor = new Color(0.1f, 0.75f, 0.2f, 1f);
    [SerializeField] private Color timeBarAlertColor = new Color(0.9f, 0.05f, 0.05f, 1f);
    [SerializeField] private Color timeBarBackgroundColor = new Color(0f, 0f, 0f, 0.25f);

    [Header("numero do pedido")]
    [SerializeField] private Vector2 numberLabelSize = new Vector2(133f, 133f);

    private const float ExtraIconSpacing = 2f;
    private const float ExtraIconGapFromBurgerBox = 4f;

    private readonly List<GameObject> spawnedIcons = new List<GameObject>();

    private GameObject timeBarRoot;
    private Image timeBarFill;
    private RectTransform timeBarFillRect;

    private GameObject numberLabelObject;
    private Image numberLabelImage;

    private int shownSourceIndex = -1;
    private int currentIngredientCount = 0;

    void Awake()
    {
        EnsureTimeBar();
        EnsureNumberLabel();
        ClearOrder();
    }

    public bool IsShowingSource(int sourceIndex)
    {
        return shownSourceIndex == sourceIndex;
    }

    public void SetOrder(int sourceIndex, BurgerOrder order, OrderHUDManager hudManager, float remainingTime, float maxTime)
    {
        ClearIcons();
        EnsureTimeBar();
        EnsureNumberLabel();

        shownSourceIndex = sourceIndex;

        if (slotRoot != null)
            slotRoot.SetActive(true);

        if (order == null)
        {
            ClearOrder();
            return;
        }

        int totalExtras = AddExtraIcons(order, hudManager);

        for (int i = 0; i < order.ingredients.Count; i++)
            AddIngredientIcon(order.ingredients[i], hudManager);

        currentIngredientCount = order.ingredients.Count;

        UpdateTimer(remainingTime, maxTime);
        UpdateNumberLabel(order.orderNumber, hudManager, totalExtras);

        // forca o recalculo do layout no mesmo frame - sem isto, os grupos de layout aninhados
        // (ticket dentro do painel) por vezes so se ajustam num frame seguinte, e nesse frame
        // intermedio o conteudo pode aparecer fora da moldura
        RectTransform slotRect = GetSlotRect();

        if (iconsParent is RectTransform iconsRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(iconsRect);

        if (slotRect != null && slotRect.parent is RectTransform panelRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
    }

    public void UpdateTimer(float remainingTime, float maxTime)
    {
        EnsureTimeBar();

        if (timeBarRoot == null || timeBarFill == null || timeBarFillRect == null)
            return;

        if (maxTime <= 0f)
        {
            timeBarRoot.SetActive(false);
            return;
        }

        timeBarRoot.SetActive(true);

        // reposiciona/redimensiona primeiro (usa a largura real dos icones do pedido)
        // para depois calcularmos o preenchimento em cima dessa largura, e nao da largura fixa antiga
        PositionTimeBar();

        float remaining = Mathf.Clamp(remainingTime, 0f, maxTime);
        float normalizedTime = remaining / maxTime;

        timeBarFill.color = remaining <= timeAlertSeconds ? timeBarAlertColor : timeBarNormalColor;

        RectTransform barRect = timeBarRoot.GetComponent<RectTransform>();
        float fullWidth = barRect != null ? barRect.sizeDelta.x : horizontalTimeBarSize.x;
        timeBarFillRect.sizeDelta = new Vector2(fullWidth * normalizedTime, horizontalTimeBarSize.y);
    }

    public void ClearOrder()
    {
        shownSourceIndex = -1;
        currentIngredientCount = 0;
        ClearIcons();

        if (timeBarRoot != null)
            timeBarRoot.SetActive(false);

        if (numberLabelObject != null)
            numberLabelObject.SetActive(false);

        if (slotRoot != null)
            slotRoot.SetActive(false);
    }

    private int AddExtraIcons(BurgerOrder order, OrderHUDManager hudManager)
    {
        List<string> extras = new List<string>();

        if (order.wantsDrink)
            extras.Add("Drink");

        if (order.wantsFries)
            extras.Add("CookedFries");

        for (int i = 0; i < extras.Count; i++)
            AddExtraIcon(extras[i], i, extras.Count, hudManager);

        return extras.Count;
    }

    private void AddIngredientIcon(string itemName, OrderHUDManager hudManager)
    {
        Sprite sprite = hudManager.GetSpriteForIngredient(itemName);

        if (sprite == null)
        {
            Debug.Log("n tenho sprite para: " + itemName);
            return;
        }

        GameObject iconObject = new GameObject("Icon_" + itemName);
        iconObject.transform.SetParent(iconsParent, false);

        Image image = iconObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;

        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.sizeDelta = ingredientIconSize;

        spawnedIcons.Add(iconObject);
    }

    private void AddExtraIcon(string itemName, int index, int totalExtras, OrderHUDManager hudManager)
    {
        Sprite sprite = hudManager.GetSpriteForIngredient(itemName);

        if (sprite == null)
        {
            Debug.Log("n tenho sprite para: " + itemName);
            return;
        }

        RectTransform slotRect = GetSlotRect();

        if (slotRect == null)
        {
            AddIngredientIcon(itemName, hudManager);
            return;
        }

        GameObject iconObject = new GameObject("ExtraIcon_" + itemName);
        iconObject.transform.SetParent(slotRect, false);

        Image image = iconObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;

        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = extraIconSize;

        float slotHalfWidth = GetSlotWidth(slotRect) * 0.5f;
        float rightToLeftIndex = totalExtras - 1 - index;

        float x = -slotHalfWidth
            - ExtraIconGapFromBurgerBox
            - (extraIconSize.x * 0.5f)
            - rightToLeftIndex * (extraIconSize.x + ExtraIconSpacing);

        rect.anchoredPosition = new Vector2(x, 0f);

        spawnedIcons.Add(iconObject);
    }

    private void EnsureTimeBar()
    {
        if (timeBarRoot != null)
            return;

        RectTransform slotRect = GetSlotRect();

        if (slotRect == null)
            return;

        timeBarRoot = new GameObject("PedidoTimeBar");
        timeBarRoot.transform.SetParent(slotRect, false);

        Image background = timeBarRoot.AddComponent<Image>();
        background.color = timeBarBackgroundColor;
        background.raycastTarget = false;

        RectTransform rootRect = timeBarRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = horizontalTimeBarSize;

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(timeBarRoot.transform, false);

        timeBarFill = fillObject.AddComponent<Image>();
        timeBarFill.color = timeBarNormalColor;
        timeBarFill.raycastTarget = false;

        timeBarFillRect = fillObject.GetComponent<RectTransform>();
        timeBarFillRect.anchorMin = new Vector2(0f, 0.5f);
        timeBarFillRect.anchorMax = new Vector2(0f, 0.5f);
        timeBarFillRect.pivot = new Vector2(0f, 0.5f);
        timeBarFillRect.anchoredPosition = Vector2.zero;
        timeBarFillRect.sizeDelta = horizontalTimeBarSize;

        PositionTimeBar();
        timeBarRoot.SetActive(false);
    }

    private void EnsureNumberLabel()
    {
        if (numberLabelObject != null)
            return;

        RectTransform slotRect = GetSlotRect();

        if (slotRect == null)
            return;

        numberLabelObject = new GameObject("PedidoNumero");
        numberLabelObject.transform.SetParent(slotRect, false);

        numberLabelImage = numberLabelObject.AddComponent<Image>();
        numberLabelImage.preserveAspect = true;
        numberLabelImage.raycastTarget = false;

        // ancorado ao centro (tal como os icones extra de batatas/bebida) em vez do canto do RectTransform,
        // porque o RectTransform do slot pode ter margem/padding e o canto ficava fora da area visivel do ticket
        RectTransform rect = numberLabelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = numberLabelSize;

        numberLabelObject.SetActive(false);

        PositionNumberLabel(0);
    }

    private void PositionNumberLabel(int totalExtras)
    {
        if (numberLabelObject == null)
            return;

        RectTransform slotRect = GetSlotRect();
        RectTransform rect = numberLabelObject.GetComponent<RectTransform>();

        if (slotRect == null || rect == null)
            return;

        float slotHalfWidth = GetSlotWidth(slotRect) * 0.5f;

        // segue a mesma logica do AddExtraIcon: fica sempre um passo mais a esquerda do que o
        // ultimo extra (batatas/bebida), ou logo a esquerda dos ingredientes se nao houver nenhum
        float x = -slotHalfWidth
            - ExtraIconGapFromBurgerBox
            - (numberLabelSize.x * 0.5f)
            - totalExtras * (extraIconSize.x + ExtraIconSpacing);

        rect.anchoredPosition = new Vector2(x, 0f);
    }

    private void UpdateNumberLabel(int number, OrderHUDManager hudManager, int totalExtras)
    {
        EnsureNumberLabel();

        if (numberLabelObject == null)
        {
            Debug.LogWarning("OrderSlotUI: numberLabelObject nao foi criado (falta o slotRoot?)");
            return;
        }

        if (numberLabelImage == null || hudManager == null)
            return;

        Sprite sprite = hudManager.GetSpriteForOrderNumber(number);

        if (sprite == null)
        {
            Debug.LogWarning("OrderSlotUI: falta a imagem do numero " + number + " em Order Number Sprites (no OrderHUDManager)");
            numberLabelObject.SetActive(false);
            return;
        }

        numberLabelImage.sprite = sprite;
        PositionNumberLabel(totalExtras);
        numberLabelObject.SetActive(true);

        // garante que fica sempre por cima dos icones extra (batatas/bebida), que sao recriados
        // a cada pedido novo e por isso ficariam com um indice de renderizacao mais recente
        numberLabelObject.transform.SetAsLastSibling();
    }

    private void PositionTimeBar()
    {
        if (timeBarRoot == null)
            return;

        RectTransform slotRect = GetSlotRect();
        RectTransform barRect = timeBarRoot.GetComponent<RectTransform>();

        if (slotRect == null || barRect == null)
            return;

        float contentWidth;
        float contentCenterX;
        GetIngredientRowBounds(out contentWidth, out contentCenterX);

        barRect.sizeDelta = new Vector2(contentWidth, horizontalTimeBarSize.y);

        if (timeBarFillRect != null)
            timeBarFillRect.sizeDelta = new Vector2(timeBarFillRect.sizeDelta.x, horizontalTimeBarSize.y);

        float slotHeight = slotRect.rect.height;

        if (slotHeight <= 1f)
            slotHeight = 70f;

        float y = -(slotHeight * 0.5f) - timeBarGapBelowOrder - (horizontalTimeBarSize.y * 0.5f);
        barRect.anchoredPosition = new Vector2(contentCenterX, y);
    }

    // faz a barra ocupar exatamente a largura da fila de icones do burger (do primeiro ao
    // ultimo icone), em vez de uma largura fixa - assim fica sempre alinhada com a comida,
    // seja qual for o numero de ingredientes do pedido
    private void GetIngredientRowBounds(out float width, out float centerX)
    {
        if (iconsParent == null || currentIngredientCount <= 0 || !(iconsParent is RectTransform iconsRect))
        {
            width = horizontalTimeBarSize.x;
            centerX = 0f;
            return;
        }

        HorizontalLayoutGroup layoutGroup = iconsParent.GetComponent<HorizontalLayoutGroup>();

        float spacing = layoutGroup != null ? layoutGroup.spacing : 0f;
        float paddingLeft = layoutGroup != null ? layoutGroup.padding.left : 0f;

        width = currentIngredientCount * ingredientIconSize.x
            + Mathf.Max(0, currentIngredientCount - 1) * spacing;

        float iconsHalfWidth = iconsRect.rect.width * 0.5f;
        float leftEdgeInIcons = -iconsHalfWidth + paddingLeft;

        centerX = iconsRect.anchoredPosition.x + leftEdgeInIcons + (width * 0.5f);
    }

    private float GetSlotWidth(RectTransform slotRect)
    {
        float slotWidth = slotRect.rect.width;

        if (slotWidth <= 1f && iconsParent != null)
        {
            RectTransform iconsRect = iconsParent.GetComponent<RectTransform>();

            if (iconsRect != null)
                slotWidth = iconsRect.rect.width;
        }

        if (slotWidth <= 1f)
            slotWidth = 430f;

        return slotWidth;
    }

    private RectTransform GetSlotRect()
    {
        if (slotRoot != null)
            return slotRoot.GetComponent<RectTransform>();

        return GetComponent<RectTransform>();
    }

    private void ClearIcons()
    {
        for (int i = 0; i < spawnedIcons.Count; i++)
        {
            if (spawnedIcons[i] != null)
                Destroy(spawnedIcons[i]);
        }

        spawnedIcons.Clear();
    }
}