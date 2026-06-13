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


    [Header("fala visual")]
    [SerializeField] private CustomerSpeechUI speechUI;


    [Header("animacao do tucano")]
    [SerializeField] private string flyParameterName = "isFlying";


    private Transform targetPoint;
    private Transform exitPoint;


    private BurgerOrder currentOrder;
    private CustomerServicePoint servicePoint;


    private bool isWaiting = false;
    private bool isEating = false;
    private Animator tucanoAnimator;


    public void SetTucanoAnimator(Animator anim)
    {
        tucanoAnimator = anim;
        Debug.Log("tucano animator recebido: " + (anim != null ? "SIM" : "NAO"));
    }


    public void SetupCustomer(Transform serviceTarget, Transform exitTarget, BurgerOrder order, CustomerServicePoint point)
    {
        targetPoint = serviceTarget;
        exitPoint = exitTarget;
        currentOrder = order;
        servicePoint = point;


        isWaiting = false;
        isEating = false;


        SetFlyAnimation(true);


        Debug.Log("cliente nasceu com pedido: " + currentOrder.GetOrderText());
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


            SetFlyAnimation(false);


            if (servicePoint != null)
            {
                servicePoint.ShowOrderHUD(currentOrder);
            }


            Debug.Log("cliente chegou ao service point e mostrou pedido: " + currentOrder.GetOrderText());
        }
        else
        {
            SetFlyAnimation(true);


            if (servicePoint != null)
            {
                servicePoint.SetFree();
            }


            Destroy(gameObject);
        }
    }


    private void SetFlyAnimation(bool flying)
    {
        Debug.Log("SetFlyAnimation: " + flying + " | animator: " + (tucanoAnimator == null ? "NULL" : "OK"));


        if (tucanoAnimator == null)
            return;


        tucanoAnimator.SetBool(flyParameterName, flying);
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
            servicePoint.ClearOrderHUD();
            servicePoint.ShowFoodVisual(burger);
        }


        Debug.Log("pedido certo, cliente recebeu o hamburger");


        string comment = GetRandomComment();


        if (speechUI != null)
        {
            speechUI.ShowSpeech(comment);
        }


        Debug.Log("cliente: " + comment);


        yield return new WaitForSeconds(eatTime);


        if (servicePoint != null)
        {
            servicePoint.ClearFoodVisual();
        }


        isEating = false;
        targetPoint = exitPoint;
    }


    private string GetRandomComment()
    {
        if (happyComments == null || happyComments.Length == 0)
        {
            return "Estava bom!";
        }


        return happyComments[Random.Range(0, happyComments.Length)];
    }


    public BurgerOrder GetCurrentOrder()
    {
        return currentOrder;
    }


}