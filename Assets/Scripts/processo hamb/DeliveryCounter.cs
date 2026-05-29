using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : MonoBehaviour, IInteractable
{
    [Header("manager dos clientes")]
    [SerializeField] private CustomerManager customerManager;

    [Header("manager do dia / pontos")]
    [SerializeField] private DayManager dayManager;

    [Header("pontos")]
    [SerializeField] private int pointsPerDelivery = 10;

    public void Interact()
    {
        PlayerHand playerHand = FindFirstObjectByType<PlayerHand>();

        if (playerHand == null)
        {
            Debug.Log("n encontrei a mao do player");
            return;
        }

        if (customerManager == null)
        {
            Debug.Log("n tenho CustomerManager ligado no delivery");
            return;
        }

        if (playerHand.HasItem())
        {
            Debug.Log("n podes entregar so um ingrediente");
            return;
        }

        if (!playerHand.HasBurger())
        {
            Debug.Log("n tens hamburger para entregar");
            return;
        }

        List<string> burger = playerHand.GetBurgerCopy();

        bool delivered = customerManager.TryServeBurgerToCustomer(burger);

        if (!delivered)
        {
            Debug.Log("pedido errado, n entreguei");
            return;
        }

        playerHand.ClearHand();

        if (dayManager != null)
        {
            dayManager.AddScore(pointsPerDelivery);
        }
        else
        {
            Debug.Log("n tenho DayManager ligado no DeliveryCounter");
        }

        Debug.Log("hamburger entregue ao cliente certo");
    }
}