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
    [SerializeField] private Sprite tomatoSprite;
    [SerializeField] private Sprite pepperSprite;
    [SerializeField] private Sprite friesSprite;
    [SerializeField] private Sprite drinkSprite;

    void Start()
    {
        ClearAllOrders();
    }

    public void SetOrder(int slotIndex, BurgerOrder order)
    {
        SetOrder(slotIndex, order, 0f);
    }

    public void SetOrder(int slotIndex, BurgerOrder order, float patienceTime)
    {
        if (slotIndex < 0 || slotIndex >= orderSlots.Length)
        {
            Debug.Log("slot de pedido invalido: " + slotIndex);
            return;
        }

        if (orderSlots[slotIndex] != null)
        {
            orderSlots[slotIndex].SetOrder(order, this, patienceTime);
        }
    }

    public void SetOrderTimer(int slotIndex, float remainingTime, float maxTime)
    {
        if (slotIndex < 0 || slotIndex >= orderSlots.Length)
            return;

        if (orderSlots[slotIndex] != null)
        {
            orderSlots[slotIndex].UpdateTimer(remainingTime, maxTime);
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

        if (ingredient == "Tomato")
            return tomatoSprite;

        if (ingredient == "Pepper")
            return pepperSprite;

        if (ingredient == "CookedFries")
            return friesSprite;

        if (ingredient == "Drink")
            return drinkSprite;

        return null;
    }
}
