using System.Collections;
using TMPro;
using UnityEngine;

public class FeedbackMessageUI : MonoBehaviour
{
    [Header("ui")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;

    [Header("tempos")]
    [SerializeField] private float visibleTime = 2f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (messageText == null)
        {
            messageText = GetComponentInChildren<TMP_Text>(true);
        }

        HideInstant();
    }

    public void ShowMessage(string message)
    {
        if (GameplayHUDPolish.Instance != null)
        {
            GameplayHUDPolish.Instance.ShowFeedback(message);
            return;
        }

        if (messageText == null)
        {
            Debug.LogWarning("FeedbackMessageUI nao tem Message Text ligado.");
            return;
        }

        if (canvasGroup == null)
        {
            Debug.LogWarning("FeedbackMessageUI nao tem CanvasGroup ligado.");
            return;
        }

        gameObject.SetActive(true);

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(ShowMessageRoutine(message));
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        messageText.text = message;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        yield return new WaitForSeconds(visibleTime);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);

            yield return null;
        }

        HideInstant();
        currentRoutine = null;
    }

    private void HideInstant()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // MUITO IMPORTANTE:
        // nao desligamos o GameObject, porque senao a coroutine nao arranca.
        gameObject.SetActive(true);
    }
}
