using System.Collections.Generic;
using UnityEngine;

public class CustomerServicePoint : MonoBehaviour
{
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
    }

    public CustomerNPC GetCurrentCustomer()
    {
        return currentCustomer;
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