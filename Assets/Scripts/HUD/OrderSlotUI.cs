using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderSlotUI : MonoBehaviour
{
    [Header("objetos da ui")]
    [SerializeField] private GameObject slotRoot;
    [SerializeField] private Transform iconsParent;

    private List<GameObject> spawnedIcons = new List<GameObject>();
    private TMP_Text timerText;

    private static readonly Vector2 IconSize = new Vector2(70f, 70f);
    private static readonly Vector2 TimerSize = new Vector2(68f, 32f);
    private const float ExtraIconSpacing = 5f;
    private const float ExtraIconGapFromBurgerBox = 8f;
    private const float TimerGapFromExtras = 8f;
    private const float TimerAlertSeconds = 10f;
    private const float TimerFontSize = 24f;
    private static readonly Color TimerNormalColor = Color.black;
    private static readonly Color TimerAlertColor = Color.red;

    void Awake()
    {
        EnsureTimerText();
        ClearOrder();
    }

    public void SetOrder(BurgerOrder order, OrderHUDManager hudManager)
    {
        SetOrder(order, hudManager, 0f);
    }

    public void SetOrder(BurgerOrder order, OrderHUDManager hudManager, float patienceTime)
    {
        ClearIcons();
        EnsureTimerText();

        if (slotRoot != null)
        {
            slotRoot.SetActive(true);
        }

        if (order == null)
        {
            ClearOrder();
            return;
        }

        AddExtraIcons(order, hudManager);

        for (int i = 0; i < order.ingredients.Count; i++)
        {
            AddIcon(order.ingredients[i], hudManager);
        }

        UpdateTimer(patienceTime, patienceTime);
    }

    private void AddExtraIcons(BurgerOrder order, OrderHUDManager hudManager)
    {
        List<string> extras = new List<string>();

        if (order.wantsDrink)
            extras.Add("Drink");

        if (order.wantsFries)
            extras.Add("CookedFries");

        for (int i = 0; i < extras.Count; i++)
        {
            AddExtraIcon(extras[i], i, extras.Count, hudManager);
        }
    }

    private void AddIcon(string itemName, OrderHUDManager hudManager)
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
        rect.sizeDelta = IconSize;

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

        RectTransform slotRect = slotRoot != null
            ? slotRoot.GetComponent<RectTransform>()
            : GetComponent<RectTransform>();

        if (slotRect == null)
        {
            AddIcon(itemName, hudManager);
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
        rect.sizeDelta = IconSize;

        float slotWidth = slotRect.rect.width;

        if (slotWidth <= 1f && iconsParent != null)
        {
            RectTransform iconsRect = iconsParent.GetComponent<RectTransform>();

            if (iconsRect != null)
                slotWidth = iconsRect.rect.width;
        }

        if (slotWidth <= 1f)
            slotWidth = 430f;

        float slotHalfWidth = slotWidth * 0.5f;
        float rightToLeftIndex = totalExtras - 1 - index;
        float x = -slotHalfWidth - ExtraIconGapFromBurgerBox - (IconSize.x * 0.5f) - rightToLeftIndex * (IconSize.x + ExtraIconSpacing);

        rect.anchoredPosition = new Vector2(x, 0f);

        spawnedIcons.Add(iconObject);
    }

    public void UpdateTimer(float remainingTime, float maxTime)
    {
        EnsureTimerText();

        if (timerText == null)
            return;

        if (maxTime <= 0f)
        {
            timerText.gameObject.SetActive(false);
            return;
        }

        timerText.gameObject.SetActive(true);

        float remaining = Mathf.Max(0f, remainingTime);
        int seconds = Mathf.CeilToInt(remaining);
        timerText.text = seconds.ToString();
        timerText.color = remaining <= TimerAlertSeconds ? TimerAlertColor : TimerNormalColor;

        PositionTimer();
    }

    public void ClearOrder()
    {
        ClearIcons();

        if (timerText != null)
        {
            timerText.text = "";
            timerText.gameObject.SetActive(false);
        }

        if (slotRoot != null)
        {
            slotRoot.SetActive(false);
        }
    }

    private void ClearIcons()
    {
        for (int i = 0; i < spawnedIcons.Count; i++)
        {
            if (spawnedIcons[i] != null)
            {
                Destroy(spawnedIcons[i]);
            }
        }

        spawnedIcons.Clear();
    }

    private void EnsureTimerText()
    {
        if (timerText != null)
            return;

        RectTransform slotRect = GetSlotRect();

        if (slotRect == null)
            return;

        GameObject timerObject = new GameObject("PedidoTimer");
        timerObject.transform.SetParent(slotRect, false);

        timerText = timerObject.AddComponent<TextMeshProUGUI>();
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.fontSize = TimerFontSize;
        timerText.enableAutoSizing = false;
        timerText.color = TimerNormalColor;
        timerText.raycastTarget = false;

        RectTransform timerRect = timerObject.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.5f, 0.5f);
        timerRect.anchorMax = new Vector2(0.5f, 0.5f);
        timerRect.pivot = new Vector2(0.5f, 0.5f);
        timerRect.sizeDelta = TimerSize;

        PositionTimer();
        timerObject.SetActive(false);
    }

    private void PositionTimer()
    {
        if (timerText == null)
            return;

        RectTransform slotRect = GetSlotRect();
        RectTransform timerRect = timerText.GetComponent<RectTransform>();

        if (slotRect == null || timerRect == null)
            return;

        float slotWidth = slotRect.rect.width;

        if (slotWidth <= 1f && iconsParent != null)
        {
            RectTransform iconsRect = iconsParent.GetComponent<RectTransform>();

            if (iconsRect != null)
                slotWidth = iconsRect.rect.width;
        }

        if (slotWidth <= 1f)
            slotWidth = 430f;

        float slotHalfWidth = slotWidth * 0.5f;
        int extraCount = 0;

        for (int i = 0; i < spawnedIcons.Count; i++)
        {
            if (spawnedIcons[i] != null && spawnedIcons[i].name.StartsWith("ExtraIcon_"))
                extraCount++;
        }

        float extrasWidth = extraCount > 0
            ? extraCount * IconSize.x + (extraCount - 1) * ExtraIconSpacing + TimerGapFromExtras
            : 0f;

        float x = -slotHalfWidth - ExtraIconGapFromBurgerBox - extrasWidth - (TimerSize.x * 0.5f);
        timerRect.anchoredPosition = new Vector2(x, 0f);
    }

    private RectTransform GetSlotRect()
    {
        if (slotRoot != null)
            return slotRoot.GetComponent<RectTransform>();

        return GetComponent<RectTransform>();
    }
}
