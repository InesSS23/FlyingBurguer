using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderSlotUI : MonoBehaviour
{
    [Header("objetos da ui")]
    [SerializeField] private GameObject slotRoot;
    [SerializeField] private Transform iconsParent;

    [Header("tamanho dos icons")]
    [SerializeField] private Vector2 iconSize = new Vector2(45, 45);
    [SerializeField] private float extraIconSpacing = 5f;
    [SerializeField] private float extraIconGapFromBurgerBox = 8f;

    private List<GameObject> spawnedIcons = new List<GameObject>();

    void Awake()
    {
        ClearOrder();
    }

    public void SetOrder(BurgerOrder order, OrderHUDManager hudManager)
    {
        ClearIcons();

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
        rect.sizeDelta = iconSize;

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
        rect.sizeDelta = iconSize;

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
        float x = -slotHalfWidth - extraIconGapFromBurgerBox - (iconSize.x * 0.5f) - rightToLeftIndex * (iconSize.x + extraIconSpacing);

        rect.anchoredPosition = new Vector2(x, 0f);

        spawnedIcons.Add(iconObject);
    }

    public void ClearOrder()
    {
        ClearIcons();

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
}
