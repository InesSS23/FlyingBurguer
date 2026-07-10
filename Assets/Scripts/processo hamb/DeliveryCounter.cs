using UnityEngine;

public class DeliveryCounter : MonoBehaviour, IInteractable
{
    [Header("manager dos clientes")]
    [SerializeField] private CustomerManager customerManager;

    [Header("manager do dia / pontos")]
    [SerializeField] private DayManager dayManager;

    [Header("feedback visual")]
    [SerializeField] private FeedbackMessageUI feedbackMessageUI;

    [Header("pontos fallback se nao houver DayManager")]
    [SerializeField] private int pointsPerDelivery = 10;

    [Header("penalizacao")]
    [SerializeField] private int wrongOrderPenalty = 5;

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

        if (playerHand.HasTray())
        {
            EntregarTabuleiro(playerHand);
            return;
        }

        if (playerHand.HasBurger())
        {
            Debug.Log("os pedidos agora devem ser entregues no tabuleiro");
            return;
        }

        Debug.Log("n tens tabuleiro para entregar");
    }

    private void EntregarTabuleiro(PlayerHand playerHand)
    {
        MealTray tray = playerHand.GetTrayCopy();

        if (tray == null || !tray.HasBurger())
        {
            Debug.Log("o tabuleiro n tem hamburger para entregar");
            return;
        }

        bool delivered = customerManager.TryServeTrayToCustomer(tray);

        if (!delivered)
        {
            PenalizarPedidoIncorreto();
            Debug.Log("pedido errado, n entreguei");
            return;
        }

        playerHand.ClearHand();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlaceObjectSFX();
        }

        int pointsToAdd = pointsPerDelivery;

        if (dayManager != null)
        {
            pointsToAdd = dayManager.GetPointsPerDelivery();
            dayManager.AddScore(pointsToAdd);
        }
        else
        {
            Debug.Log("n tenho DayManager ligado no DeliveryCounter");
        }

        Debug.Log("tabuleiro entregue ao cliente certo. Pontos ganhos: " + pointsToAdd);
    }

    private void PenalizarPedidoIncorreto()
    {
        if (dayManager != null)
        {
            dayManager.AddScore(-wrongOrderPenalty);
        }

        if (feedbackMessageUI != null)
        {
            feedbackMessageUI.ShowMessage("pedido incorreto");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWrongOrderSFX();
        }
    }
}