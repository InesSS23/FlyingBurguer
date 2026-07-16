using System.Collections;
using UnityEngine;

public class GrillStation : MonoBehaviour, IInteractable
{
    [Header("nomes")]
    [SerializeField] private string rawMeatName = "RawMeat";
    [SerializeField] private string cookedMeatName = "CookedMeat";

    [Header("tempo")]
    [SerializeField] private float cookTime = 2f;

    [Header("visual")]
    [SerializeField] private Transform grillVisualPoint;
    [SerializeField] private GameObject rawMeatVisualPrefab;
    [SerializeField] private GameObject cookedMeatVisualPrefab;

    private bool hasMeat = false;
    private bool isCooking = false;
    private bool isCooked = false;
    private float cookingProgress = 0f;

    public bool IsProcessing => isCooking;
    public float ProcessingProgress => cookingProgress;

    private GameObject currentVisual;
    private AudioSource cookingLoopSource;

    private void OnDisable()
    {
        StopCookingSound();
    }

    public void Interact()
    {
        PlayerHand playerHand = FindFirstObjectByType<PlayerHand>();

        if (playerHand == null)
        {
            Debug.Log("n encontrei a mao do player");
            return;
        }

        if (!hasMeat)
        {
            ColocarCarneNaGrelha(playerHand);
            return;
        }

        if (isCooking)
        {
            Debug.Log("a carne ainda esta a cozinhar");
            return;
        }

        if (isCooked)
        {
            TirarCarneDaGrelha(playerHand);
        }
    }

    private void ColocarCarneNaGrelha(PlayerHand playerHand)
    {
        if (!playerHand.HasItem())
        {
            Debug.Log("n tens carne para meter na grelha");
            return;
        }

        if (playerHand.GetCurrentItem() != rawMeatName)
        {
            Debug.Log("isto n pode ir para a grelha: " + playerHand.GetCurrentItem());
            return;
        }

        playerHand.ClearHand();

        hasMeat = true;
        isCooking = true;
        isCooked = false;
        cookingProgress = 0f;

        CriarVisual(rawMeatVisualPrefab);
        PlayCookingSound();

        Debug.Log("carne crua metida na grelha");

        StartCoroutine(CozinharCarne());
    }

    private IEnumerator CozinharCarne()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, cookTime);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cookingProgress = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        cookingProgress = 1f;
        isCooking = false;
        isCooked = true;

        StopCookingSound();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayReadySFX();
        }

        CriarVisual(cookedMeatVisualPrefab);

        Debug.Log("carne pronta");
    }

    private void TirarCarneDaGrelha(PlayerHand playerHand)
    {
        if (!playerHand.IsEmpty())
        {
            Debug.Log("tens de ter a mao vazia para tirar a carne");
            return;
        }

        if (!playerHand.TrySetItem(cookedMeatName))
            return;

        hasMeat = false;
        isCooking = false;
        isCooked = false;
        cookingProgress = 0f;

        LimparVisual();

        Debug.Log("tirei a carne cozinhada da grelha");
    }

    private void PlayCookingSound()
    {
        StopCookingSound();

        if (AudioManager.Instance != null)
        {
            cookingLoopSource = AudioManager.Instance.PlayGrillMeatLoopSFX();
        }
    }

    private void StopCookingSound()
    {
        if (AudioManager.Instance != null && cookingLoopSource != null)
        {
            AudioManager.Instance.StopLoopingSFX(cookingLoopSource);
        }

        cookingLoopSource = null;
    }

    private void CriarVisual(GameObject prefab)
    {
        LimparVisual();

        if (prefab == null || grillVisualPoint == null)
        {
            Debug.Log("falta visual da grelha");
            return;
        }

        currentVisual = Instantiate(prefab, grillVisualPoint);
        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;
    }

    private void LimparVisual()
    {
        if (currentVisual != null)
        {
            Destroy(currentVisual);
        }
    }
}
