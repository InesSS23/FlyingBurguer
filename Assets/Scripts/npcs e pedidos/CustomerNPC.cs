using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerNPC : MonoBehaviour
{
    [Header("movimento")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("tempo a comer/comentar")]
    [SerializeField] private float eatTime = 3f;

    [Header("comentarios")]
    [SerializeField] private string[] happyComments;

    private Transform targetPoint;
    private Transform exitPoint;

    private BurgerOrder currentOrder;
    private CustomerServicePoint servicePoint;

    private bool isWaiting = false;
    private bool isEating = false;

    public void SetupCustomer(Transform serviceTarget, Transform exitTarget, BurgerOrder order, CustomerServicePoint point)
    {
        targetPoint = serviceTarget;
        exitPoint = exitTarget;
        currentOrder = order;
        servicePoint = point;

        isWaiting = false;
        isEating = false;

        Debug.Log("cliente recebeu pedido: " + currentOrder.GetOrderText());
    }

    void Update()
    {
        if (targetPoint == null)
            return;

        if (isWaiting || isEating)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            ChegouAoPonto();
        }
    }

    private void ChegouAoPonto()
    {
        if (targetPoint != exitPoint)
        {
            isWaiting = true;
            Debug.Log("cliente chegou e esta a espera: " + currentOrder.GetOrderText());
        }
        else
        {
            if (servicePoint != null)
            {
                servicePoint.SetFree();
            }

            Destroy(gameObject);
        }
    }

    public bool CanReceiveBurger(List<string> burger)
    {
        if (!isWaiting)
            return false;

        if (currentOrder == null)
            return false;

        return currentOrder.MatchesBurger(burger);
    }

    public void ReceiveCorrectBurger(List<string> burger)
    {
        if (!CanReceiveBurger(burger))
        {
            Debug.Log("este hamburger n é para este cliente");
            return;
        }

        StartCoroutine(EatAndLeave(burger));
    }

    private IEnumerator EatAndLeave(List<string> burger)
    {
        isWaiting = false;
        isEating = true;

        if (servicePoint != null)
        {
            servicePoint.ShowFoodVisual(burger);
        }

        Debug.Log("pedido certo, cliente recebeu o hamburger");

        SayRandomComment();

        yield return new WaitForSeconds(eatTime);

        if (servicePoint != null)
        {
            servicePoint.ClearFoodVisual();
        }

        isEating = false;
        targetPoint = exitPoint;
    }

    private void SayRandomComment()
    {
        if (happyComments == null || happyComments.Length == 0)
        {
            Debug.Log("cliente: estava bom!");
            return;
        }

        string comment = happyComments[Random.Range(0, happyComments.Length)];
        Debug.Log("cliente: " + comment);
    }

    public BurgerOrder GetCurrentOrder()
    {
        return currentOrder;
    }
}