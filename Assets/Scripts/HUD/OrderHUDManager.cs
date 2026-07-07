using System.Collections.Generic;
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

    private readonly List<ActiveOrderHUD> activeOrders = new List<ActiveOrderHUD>();

    void Start()
    {
        ClearAllOrders();
    }

    void Update()
    {
        bool changedTimer = false;

        for (int i = 0; i < activeOrders.Count; i++)
        {
            ActiveOrderHUD activeOrder = activeOrders[i];

            if (activeOrder.maxTime <= 0f)
                continue;

            activeOrder.remainingTime -= Time.deltaTime;

            if (activeOrder.remainingTime < 0f)
                activeOrder.remainingTime = 0f;

            changedTimer = true;
        }

        if (changedTimer)
            RefreshSlots();
    }

    public void SetOrder(int sourceIndex, BurgerOrder order)
    {
        SetOrder(sourceIndex, order, 0f);
    }

    public void SetOrder(int sourceIndex, BurgerOrder order, float patienceTime)
    {
        if (order == null)
        {
            ClearOrder(sourceIndex);
            return;
        }

        ActiveOrderHUD activeOrder = GetOrCreateActiveOrder(sourceIndex);
        activeOrder.order = order;
        activeOrder.remainingTime = patienceTime;
        activeOrder.maxTime = patienceTime;

        RefreshSlots();
    }

    public void SetOrderTimer(int sourceIndex, float remainingTime, float maxTime)
    {
        ActiveOrderHUD activeOrder = FindActiveOrder(sourceIndex);

        if (activeOrder == null)
            return;

        activeOrder.remainingTime = remainingTime;
        activeOrder.maxTime = maxTime;

        RefreshSlots();
    }

    public void ClearOrder(int sourceIndex)
    {
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            if (activeOrders[i].sourceIndex == sourceIndex)
            {
                activeOrders.RemoveAt(i);
                break;
            }
        }

        RefreshSlots();
    }

    public void ClearAllOrders()
    {
        activeOrders.Clear();

        if (orderSlots == null)
            return;

        for (int i = 0; i < orderSlots.Length; i++)
        {
            if (orderSlots[i] != null)
                orderSlots[i].ClearOrder();
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

    private void RefreshSlots()
    {
        if (orderSlots == null)
            return;

        activeOrders.Sort(CompareOrdersByRemainingTime);

        for (int i = 0; i < orderSlots.Length; i++)
        {
            OrderSlotUI slot = orderSlots[i];

            if (slot == null)
                continue;

            if (i >= activeOrders.Count)
            {
                slot.ClearOrder();
                continue;
            }

            ActiveOrderHUD activeOrder = activeOrders[i];

            if (!slot.IsShowingSource(activeOrder.sourceIndex))
            {
                slot.SetOrder(
                    activeOrder.sourceIndex,
                    activeOrder.order,
                    this,
                    activeOrder.remainingTime,
                    activeOrder.maxTime
                );
            }
            else
            {
                slot.UpdateTimer(activeOrder.remainingTime, activeOrder.maxTime);
            }
        }
    }

    private int CompareOrdersByRemainingTime(ActiveOrderHUD a, ActiveOrderHUD b)
    {
        int timeComparison = a.remainingTime.CompareTo(b.remainingTime);

        if (timeComparison != 0)
            return timeComparison;

        return a.sourceIndex.CompareTo(b.sourceIndex);
    }

    private ActiveOrderHUD GetOrCreateActiveOrder(int sourceIndex)
    {
        ActiveOrderHUD activeOrder = FindActiveOrder(sourceIndex);

        if (activeOrder != null)
            return activeOrder;

        activeOrder = new ActiveOrderHUD();
        activeOrder.sourceIndex = sourceIndex;
        activeOrders.Add(activeOrder);

        return activeOrder;
    }

    private ActiveOrderHUD FindActiveOrder(int sourceIndex)
    {
        for (int i = 0; i < activeOrders.Count; i++)
        {
            if (activeOrders[i].sourceIndex == sourceIndex)
                return activeOrders[i];
        }

        return null;
    }

    private class ActiveOrderHUD
    {
        public int sourceIndex;
        public BurgerOrder order;
        public float remainingTime;
        public float maxTime;
    }
}