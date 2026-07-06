using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerNPC : MonoBehaviour
{
    [Header("movimento")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("tempo a comer/comentar")]
    [SerializeField] private float eatTime = 3f;

    [Header("comentarios positivos")]
    [SerializeField]
    private string[] happyComments =
    {
        "Muito bom!",
        "Isto sim é serviço!",
        "Adoro hambúrgueres voadores!"
    };

    [Header("comentarios negativos")]
    [SerializeField]
    private string[] impatientComments =
    {
        "Demoraste muito! Vou embora!",
        "Estou farto de esperar!",
        "Que serviço lento!",
        "Já perdi a fome!",
        "Vou comer noutro sítio!"
    };

    [SerializeField] private float impatientLeaveDelay = 3f;

    [Header("fala visual")]
    [SerializeField] private CustomerSpeechUI speechUI;

    [Header("tucano")]
    [SerializeField] private bool isTucano = false;

    [Header("arara")]
    [SerializeField] private bool isArara = false;

    [Header("animacao do tucano")]
    [SerializeField] private string flyParameterName = "isFlying";

    [Header("animacao da arara")]
    [SerializeField] private string araraFlyParameterName = "isFlying";

    private Transform targetPoint;
    private Transform exitPoint;

    private BurgerOrder currentOrder;
    private CustomerServicePoint servicePoint;
    private CustomerManager customerManager;

    private bool isWaiting = false;
    private bool isEating = false;

    private Animator tucanoAnimator;
    private Animator araraAnimator;
    private Transform flyingVisualRoot;

    private Coroutine patienceCoroutine;
    private float patienceTime = 25f;

    private bool warnedMissingTucanoAnimator = false;
    private bool warnedMissingAraraAnimator = false;

    public bool IsTucano()
    {
        return isTucano;
    }

    public bool IsArara()
    {
        return isArara;
    }

    public void SetTucanoAnimator(Animator anim)
    {
        tucanoAnimator = anim;

        if (anim != null)
        {
            Debug.Log("tucano animator recebido");
        }
    }

    public void SetAraraAnimator(Animator anim)
    {
        araraAnimator = anim;

        if (anim != null)
        {
            Debug.Log("arara animator recebido");
        }
    }

    public void SetFlyingVisualRoot(Transform visualRoot)
    {
        flyingVisualRoot = visualRoot;
        ApplyFlyingVisualMode(true);
    }

    public void SetupCustomer(
        Transform serviceTarget,
        Transform exitTarget,
        BurgerOrder order,
        CustomerServicePoint point,
        float maxPatienceTime,
        CustomerManager manager
    )
    {
        targetPoint = serviceTarget;
        exitPoint = exitTarget;
        currentOrder = order;
        servicePoint = point;
        customerManager = manager;
        patienceTime = maxPatienceTime;

        isWaiting = false;
        isEating = false;

        StopPatienceTimer();

        SetFlyAnimation(true);

        Debug.Log("cliente nasceu com pedido: " + currentOrder.GetOrderText());
    }

    void Update()
    {
        CheckAnimatorWarnings();

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

    private void CheckAnimatorWarnings()
    {
        if (isTucano && tucanoAnimator == null && !warnedMissingTucanoAnimator)
        {
            warnedMissingTucanoAnimator = true;
            Debug.LogWarning("CustomerNPC: este cliente está marcado como Tucano, mas não recebeu Animator.");
        }

        if (isArara && araraAnimator == null && !warnedMissingAraraAnimator)
        {
            warnedMissingAraraAnimator = true;
            Debug.LogWarning("CustomerNPC: este cliente está marcado como Arara, mas não recebeu Animator.");
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

            StartPatienceTimer();

            Debug.Log("cliente chegou ao service point e mostrou pedido: " + currentOrder.GetOrderText());
        }
        else
        {
            SetFlyAnimation(true);

            StopPatienceTimer();

            if (servicePoint != null)
            {
                servicePoint.SetFree();
            }

            Destroy(gameObject);
        }
    }

    private void SetFlyAnimation(bool flying)
    {
        if (flying)
        {
            ApplyFlyingVisualMode(true);
        }

        if (isTucano)
        {
            SetAnimatorBool(tucanoAnimator, flyParameterName, flying);
        }

        if (isArara)
        {
            SetAnimatorBool(araraAnimator, araraFlyParameterName, flying);
        }

        if (!flying)
        {
            ApplyFlyingVisualMode(false);
        }
    }

    private void ApplyFlyingVisualMode(bool flying)
    {
        if (flyingVisualRoot == null)
            return;

        if (flying)
        {
            flyingVisualRoot.gameObject.SetActive(true);
            SetVisualsActive(false, flyingVisualRoot);
        }
        else
        {
            SetVisualsActive(true, flyingVisualRoot);
            flyingVisualRoot.gameObject.SetActive(false);
        }
    }

    private void SetAnimatorBool(Animator animator, string parameterName, bool value)
    {
        if (animator == null)
            return;

        if (string.IsNullOrEmpty(parameterName))
            return;

        if (!AnimatorHasParameter(animator, parameterName))
        {
            Debug.LogWarning("Animator não tem o parâmetro: " + parameterName);
            return;
        }

        animator.SetBool(parameterName, value);
    }

    private bool AnimatorHasParameter(Animator animator, string parameterName)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName)
                return true;
        }

        return false;
    }

    public bool CanReceiveBurger(List<string> burger)
    {
        if (!isWaiting)
            return false;

        if (currentOrder == null)
            return false;

        return !currentOrder.wantsFries && !currentOrder.wantsDrink && currentOrder.MatchesBurger(burger);
    }

    public bool CanReceiveTray(MealTray tray)
    {
        if (!isWaiting)
            return false;

        if (currentOrder == null)
            return false;

        return currentOrder.MatchesTray(tray);
    }

    public void ReceiveCorrectBurger(List<string> burger)
    {
        if (!CanReceiveBurger(burger))
        {
            Debug.Log("este hamburger n e para este cliente");
            return;
        }

        StopPatienceTimer();

        StartCoroutine(EatAndLeave(burger));
    }

    public void ReceiveCorrectTray(MealTray tray)
    {
        if (!CanReceiveTray(tray))
        {
            Debug.Log("este tabuleiro n e para este cliente");
            return;
        }

        StopPatienceTimer();

        StartCoroutine(EatAndLeave(tray));
    }

    private IEnumerator EatAndLeave(List<string> burger)
    {
        MealTray tray = new MealTray();
        tray.burger = new List<string>(burger);

        yield return EatAndLeave(tray);
    }

    private IEnumerator EatAndLeave(MealTray tray)
    {
        isWaiting = false;
        isEating = true;

        if (servicePoint != null)
        {
            servicePoint.ClearOrderHUD();
            servicePoint.ShowFoodVisual(tray);
        }

        Debug.Log("pedido certo, cliente recebeu o tabuleiro");

        string comment = GetRandomHappyComment();

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

        SetFlyAnimation(true);
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

        StartCoroutine(LeaveBecauseImpatientRoutine());
    }

    private IEnumerator LeaveBecauseImpatientRoutine()
    {
        isWaiting = false;
        isEating = true;

        StopPatienceTimer();

        if (servicePoint != null)
        {
            servicePoint.ClearOrderHUD();
        }

        string impatientComment = GetRandomImpatientComment();

        if (speechUI != null)
        {
            speechUI.ShowAngrySpeech(impatientComment);
        }

        Debug.Log("cliente perdeu a paciencia: " + impatientComment);

        if (customerManager != null)
        {
            customerManager.NotifyCustomerLeftImpatient(this);
        }

        yield return new WaitForSeconds(impatientLeaveDelay);

        if (speechUI != null)
        {
            speechUI.HideSpeech();
        }

        isEating = false;

        SetFlyAnimation(true);

        targetPoint = exitPoint;
    }

    private string GetRandomHappyComment()
    {
        if (happyComments == null || happyComments.Length == 0)
        {
            return "Estava bom!";
        }

        return happyComments[Random.Range(0, happyComments.Length)];
    }

    private string GetRandomImpatientComment()
    {
        if (impatientComments == null || impatientComments.Length == 0)
        {
            return "Demoraste muito! Vou embora!";
        }

        return impatientComments[Random.Range(0, impatientComments.Length)];
    }

    public void SetVisualsActive(bool active, Transform excludeSubtree = null)
    {
        Renderer[] rends = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer rend in rends)
        {
            if (excludeSubtree != null && rend.transform.IsChildOf(excludeSubtree))
                continue;

            rend.enabled = active;
        }
    }

    public BurgerOrder GetCurrentOrder()
    {
        return currentOrder;
    }
}