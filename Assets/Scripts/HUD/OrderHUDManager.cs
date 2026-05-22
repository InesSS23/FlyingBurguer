using UnityEngine;

public class OrderHUDManager : MonoBehaviour
{
    [Header("slots dos pedidos")]
    [SerializeField] private OrderSlotUI[] orderSlots;

    [Header("sprites dos ingredientes")]
    [SerializeField] private Sprite breadSprite;
    [SerializeField] private Sprite cookedMeatSprite;
    [SerializeField] private Sprite cheeseSprite;
    [SerializeField] private Sprite lettuceSprite;

    void Start()
    {
        ClearAllOrders();
    }

    public void SetOrder(int slotIndex, BurgerOrder order)
    {
        if (slotIndex < 0 || slotIndex >= orderSlots.Length)
        {
            Debug.Log("slot de pedido invalido: " + slotIndex);
            return;
        }

        if (orderSlots[slotIndex] != null)
        {
            orderSlots[slotIndex].SetOrder(order, this);
        }
    }

    public void ClearOrder(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= orderSlots.Length)
            return;

        if (orderSlots[slotIndex] != null)
        {
            orderSlots[slotIndex].ClearOrder();
        }
    }

    public void ClearAllOrders()
    {
        for (int i = 0; i < orderSlots.Length; i++)
        {
            ClearOrder(i);
        }
    }

    public Sprite GetSpriteForIngredient(string ingredient)
    {
        if (ingredient == "Bread")
            return breadSprite;

        if (ingredient == "CookedMeat")
            return cookedMeatSprite;

        if (ingredient == "Cheese")
            return cheeseSprite;

        if (ingredient == "Lettuce")
            return lettuceSprite;

        return null;
    }
}