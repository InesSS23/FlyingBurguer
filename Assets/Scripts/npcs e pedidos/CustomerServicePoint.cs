using System.Collections.Generic;
using UnityEngine;

public class CustomerServicePoint : MonoBehaviour
{
    [Header("slot do hud")]
    [SerializeField] private int orderSlotIndex = 0;

    [Header("estado")]
    [SerializeField] private bool isOccupied = false;

    [Header("cliente atual")]
    [SerializeField] private CustomerNPC currentCustomer;

    [Header("visual do hamburger do cliente")]
    [SerializeField] private BurgerVisualSpawner burgerVisualSpawner;

    public bool IsFree()
    {
        return !isOccupied;
    }

    public void SetCustomer(CustomerNPC customer)
    {
        currentCustomer = customer;
        isOccupied = true;
    }

    public void SetFree()
    {
        currentCustomer = null;
        isOccupied = false;

        ClearFoodVisual();

        OrderHUDManager hud = FindFirstObjectByType<OrderHUDManager>();

        if (hud != null)
        {
            hud.ClearOrder(orderSlotIndex);
        }
    }

    public CustomerNPC GetCurrentCustomer()
    {
        return currentCustomer;
    }

    public int GetOrderSlotIndex()
    {
        return orderSlotIndex;
    }

    public void ShowFoodVisual(List<string> burger)
    {
        if (burgerVisualSpawner != null)
        {
            burgerVisualSpawner.ShowBurger(burger);
        }
    }

    public void ClearFoodVisual()
    {
        if (burgerVisualSpawner != null)
        {
            burgerVisualSpawner.ClearVisuals();
        }
    }
}