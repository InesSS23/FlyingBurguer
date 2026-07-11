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

    [Header("gaivota")]
    [SerializeField] private bool isGaivota = false;

    [Header("coruja")]
    [SerializeField] private bool isCoruja = false;

    [Header("animacao do tucano")]
    [SerializeField] private string flyParameterName = "isFlying";

    [Header("animacao da arara")]
    [SerializeField] private string araraFlyParameterName = "isFlying";

    [Header("animacao da gaivota")]
    [SerializeField] private string gaivotaFlyParameterName = "isFlying";

    [Header("animacao da coruja")]
    [SerializeField] private string corujaFlyParameterName = "isFlying";

    private Transform targetPoint;
    private Transform exitPoint;

    private BurgerOrder currentOrder;
    private CustomerServicePoint servicePoint;
    private CustomerManager customerManager;

    private bool isWaiting = false;
    private bool isEating = false;
    private bool isDespawning = false;

    private Animator tucanoAnimator;
    private Animator araraAnimator;
    private Animator gaivotaAnimator;
    private Animator corujaAnimator;

    private Coroutine patienceCoroutine;
    private float patienceTime = 25f;

    private bool warnedMissingTucanoAnimator = false;
    private bool warnedMissingAraraAnimator = false;
    private bool warnedMissingGaivotaAnimator = false;
    private bool warnedMissingCorujaAnimator = false;

    private CustomerFadeEffect fadeEffect;

    private const float SPAWN_FADE_DURATION = 0.35f;
    private const float DESPAWN_FADE_DURATION = 0.35f;

    public bool IsTucano()
    {
        return isTucano;
    }

    public bool IsArara()
    {
        return isArara;
    }

    public bool IsGaivota()
    {
        return isGaivota;
    }

    public bool IsCoruja()
    {
        return isCoruja;
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

    public void SetGaivotaAnimator(Animator anim)
    {
        gaivotaAnimator = anim;

        if (anim != null)
        {
            Debug.Log("gaivota animator recebido");
        }
    }

    public void SetCorujaAnimator(Animator anim)
    {
        corujaAnimator = anim;

        if (anim != null)
        {
            Debug.Log("coruja animator recebido");
        }
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
        isDespawning = false;

        StopPatienceTimer();

        if (isTucano)
            SetFlyAnimation(true);

        if (isArara)
            SetAraraFlyAnimation(true);

        if (isGaivota)
            SetGaivotaFlyAnimation(true);

        if (isCoruja)
            SetCorujaFlyAnimation(true);

        StartSpawnFade();

        Debug.Log("cliente nasceu com pedido: " + currentOrder.GetOrderText());
    }

    void Update()
    {
        CheckAnimatorWarnings();

        if (isDespawning)
            return;

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

    private void StartSpawnFade()
    {
        if (fadeEffect == null)
        {
            fadeEffect = GetComponent<CustomerFadeEffect>();
        }

        if (fadeEffect == null)
        {
            fadeEffect = gameObject.AddComponent<CustomerFadeEffect>();
        }

        fadeEffect.FadeIn(SPAWN_FADE_DURATION);
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

        if (isGaivota && gaivotaAnimator == null && !warnedMissingGaivotaAnimator)
        {
            warnedMissingGaivotaAnimator = true;
            Debug.LogWarning("CustomerNPC: este cliente está marcado como Gaivota, mas não recebeu Animator.");
        }

        if (isCoruja && corujaAnimator == null && !warnedMissingCorujaAnimator)
        {
            warnedMissingCorujaAnimator = true;
            Debug.LogWarning("CustomerNPC: este cliente está marcado como Coruja, mas não recebeu Animator.");
        }
    }

    private void ChegouAoPonto()
    {
        if (targetPoint != exitPoint)
        {
            isWaiting = true;

            if (isTucano)
                SetFlyAnimation(false);

            if (isArara)
                SetAraraFlyAnimation(false);

            if (isGaivota)
                SetGaivotaFlyAnimation(false);

            if (isCoruja)
                SetCorujaFlyAnimation(false);

            if (servicePoint != null)
            {
                servicePoint.ShowOrderHUD(currentOrder, patienceTime);
            }

            StartPatienceTimer();

            Debug.Log("cliente chegou ao service point e mostrou pedido: " + currentOrder.GetOrderText());
        }
        else
        {
            if (isDespawning)
                return;

            if (isTucano)
                SetFlyAnimation(true);

            if (isArara)
                SetAraraFlyAnimation(true);

            if (isGaivota)
                SetGaivotaFlyAnimation(true);

            if (isCoruja)
                SetCorujaFlyAnimation(true);

            StartCoroutine(DespawnWithFade());
        }
    }

    private IEnumerator DespawnWithFade()
    {
        isDespawning = true;

        StopPatienceTimer();

        if (speechUI != null)
        {
            speechUI.HideSpeech();
        }

        if (servicePoint != null)
        {
            servicePoint.SetFree();
        }

        if (fadeEffect == null)
        {
            fadeEffect = GetComponent<CustomerFadeEffect>();
        }

        if (fadeEffect == null)
        {
            fadeEffect = gameObject.AddComponent<CustomerFadeEffect>();
        }

        yield return fadeEffect.FadeOutRoutine(DESPAWN_FADE_DURATION);

        Destroy(gameObject);
    }

    private void SetFlyAnimation(bool flying)
    {
        if (tucanoAnimator == null)
            return;

        tucanoAnimator.Rebind();
        tucanoAnimator.Update(0f);
        SetAnimatorBool(tucanoAnimator, flyParameterName, flying);
    }

    private void SetAraraFlyAnimation(bool flying)
    {
        if (araraAnimator == null)
            return;

        araraAnimator.Rebind();
        araraAnimator.Update(0f);
        SetAnimatorBool(araraAnimator, araraFlyParameterName, flying);
    }

    private void SetGaivotaFlyAnimation(bool flying)
    {
        if (gaivotaAnimator == null)
            return;

        gaivotaAnimator.Rebind();
        gaivotaAnimator.Update(0f);
        SetAnimatorBool(gaivotaAnimator, gaivotaFlyParameterName, flying);
    }

    private void SetCorujaFlyAnimation(bool flying)
    {
        if (corujaAnimator == null)
            return;

        corujaAnimator.Rebind();
        corujaAnimator.Update(0f);
        SetAnimatorBool(corujaAnimator, corujaFlyParameterName, flying);
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

        SetCorrectFlyAnimation(true);
        targetPoint = exitPoint;
    }

    private void SetCorrectFlyAnimation(bool flying)
    {
        if (isTucano)
            SetFlyAnimation(flying);

        if (isArara)
            SetAraraFlyAnimation(flying);

        if (isGaivota)
            SetGaivotaFlyAnimation(flying);

        if (isCoruja)
            SetCorujaFlyAnimation(flying);
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
        float remainingTime = patienceTime;

        while (remainingTime > 0f)
        {
            if (!isWaiting || isEating)
                yield break;

            if (servicePoint != null)
            {
                servicePoint.UpdateOrderHUDTimer(remainingTime, patienceTime);
            }

            remainingTime -= Time.deltaTime;
            yield return null;
        }

        if (servicePoint != null)
        {
            servicePoint.UpdateOrderHUDTimer(0f, patienceTime);
        }

        patienceCoroutine = null;
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

        SetCorrectFlyAnimation(true);

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