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

        for (int i = 0; i < order.ingredients.Count; i++)
        {
            Sprite sprite = hudManager.GetSpriteForIngredient(order.ingredients[i]);

            if (sprite == null)
            {
                Debug.Log("n tenho sprite para: " + order.ingredients[i]);
                continue;
            }

            GameObject iconObject = new GameObject("Icon_" + order.ingredients[i]);

            iconObject.transform.SetParent(iconsParent, false);

            Image image = iconObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;

            RectTransform rect = iconObject.GetComponent<RectTransform>();
            rect.sizeDelta = iconSize;

            spawnedIcons.Add(iconObject);
        }
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