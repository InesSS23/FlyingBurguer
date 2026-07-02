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
    [SerializeField] private string impatientComment = "Demoraste muito, vou embora!";


    [Header("fala visual")]
    [SerializeField] private CustomerSpeechUI speechUI;

[Header("tucano")]
[SerializeField] private bool isTucano = false;

[Header("arara")]
[SerializeField] private bool isArara = false;

public bool IsTucano() => isTucano;
public bool IsArara() => isArara;


    [Header("animacao do tucano")]
    [SerializeField] private string flyParameterName = "isFlying";



    private Transform targetPoint;
    private Transform exitPoint;


    private BurgerOrder currentOrder;
    private CustomerServicePoint servicePoint;
    private CustomerManager customerManager;


    private bool isWaiting = false;
    private bool isEating = false;
    private Animator tucanoAnimator;
    private Coroutine patienceCoroutine;
    private float patienceTime = 25f;
    private bool warnedMissingTucanoAnimator = false;

    
    [Header("animacao da arara")]
    [SerializeField] private string araraFlyParameterName = "isFlying";
    private Animator araraAnimator;



    public void SetTucanoAnimator(Animator anim)
    {
        tucanoAnimator = anim;
        Debug.Log("tucano animator recebido: " + (anim != null ? "SIM" : "NAO"));
    }
        public void SetAraraAnimator(Animator anim)
    {
        araraAnimator = anim;
        Debug.Log("arara animator recebido: " + (anim != null ? "SIM" : "NAO"));
    }


    public void SetupCustomer(Transform serviceTarget, Transform exitTarget, BurgerOrder order, CustomerServicePoint point, float maxPatienceTime, CustomerManager manager)
    {
        targetPoint = serviceTarget;
        exitPoint = exitTarget;
        currentOrder = order;
        servicePoint = point;
        customerManager = manager;
        patienceTime = maxPatienceTime;


        isWaiting = false;
        isEating = false;


        SetFlyAnimation(true);
        if (isArara) SetAraraFlyAnimation(true);


        Debug.Log("cliente nasceu com pedido: " + currentOrder.GetOrderText());
    }


    void Update()
    {
         if (isTucano && tucanoAnimator == null)
            Debug.Log("ANIMATOR PERDIDO (tucano) no frame " + Time.frameCount);
        if (isArara && araraAnimator == null)
            Debug.Log("ANIMATOR PERDIDO (arara) no frame " + Time.frameCount);

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


            if (isTucano) SetFlyAnimation(false);
            if (isArara) SetAraraFlyAnimation(false);


            if (servicePoint != null)
            {
                servicePoint.ShowOrderHUD(currentOrder);
            }


            Debug.Log("cliente chegou ao service point e mostrou pedido: " + currentOrder.GetOrderText());
        }
        else
        {
            if (isTucano) SetFlyAnimation(true);
            if (isArara) SetAraraFlyAnimation(true);


            if (servicePoint != null)
            {
                servicePoint.SetFree();
            }


            Destroy(gameObject);
        }
    }


    private void SetFlyAnimation(bool flying)
    {
        if (tucanoAnimator == null) return;

        tucanoAnimator.Rebind();
        tucanoAnimator.Update(0f);
        tucanoAnimator.SetBool(flyParameterName, flying);
    }

    private void SetAraraFlyAnimation(bool flying)
    {
        if (araraAnimator == null) return;

        araraAnimator.Rebind();
        araraAnimator.Update(0f);
        araraAnimator.SetBool(araraFlyParameterName, flying);
    }

    private bool AnimatorHasParam(string paramName)
    {
        foreach (var p in tucanoAnimator.parameters)
            if (p.name == paramName) return true;
        return false;
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

        StopPatienceTimer();

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


    private void StartPatienceTimer()
    {
        StopPatienceTimer();

        if (patienceTime <= 0f)
            return;

        patienceCoroutine = StartCoroutine(PatienceRoutine());
    }


    private void StopPatienceTimer()
    {
        if (patienceCoroutine != null)
        {
            StopCoroutine(patienceCoroutine);
            patienceCoroutine = null;
        }
    }


    private IEnumerator PatienceRoutine()
    {
        yield return new WaitForSeconds(patienceTime);

        if (!isWaiting || isEating)
            yield break;

        LeaveBecauseImpatient();
    }


    private void LeaveBecauseImpatient()
    {
        isWaiting = false;
        isEating = false;

        if (servicePoint != null)
        {
            servicePoint.ClearOrderHUD();
            servicePoint.SetFree();
        }

        if (speechUI != null)
        {
            speechUI.ShowSpeech(impatientComment);
        }

        if (customerManager != null)
        {
            customerManager.NotifyCustomerLeftImpatient(this);
        }

        SetFlyAnimation(true);
        if (isArara) SetAraraFlyAnimation(true);
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

    public void SetVisualsActive(bool active, Transform excludeSubtree = null)
    {
        Renderer[] rends = GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends)
        {
            if (excludeSubtree != null && r.transform.IsChildOf(excludeSubtree))
                continue;
            r.enabled = active;
        }
    }


    public BurgerOrder GetCurrentOrder()
    {
        return currentOrder;
    }


}