using System.Collections;
using UnityEngine;

public class FryerStation : MonoBehaviour, IInteractable
{
    [Header("nomes")]
    [SerializeField] private string frozenFriesName = "FrozenFries";
    [SerializeField] private string cookedFriesName = "CookedFries";

    [Header("tempo")]
    [SerializeField] private float cookTime = 4f;

    [Header("visual ja na cena")]
    [SerializeField] private GameObject frozenFriesSceneVisual;
    [SerializeField] private GameObject cookedFriesSceneVisual;

    [Header("visual por prefab")]
    [SerializeField] private Transform fryerVisualPoint;
    [SerializeField] private GameObject frozenFriesVisualPrefab;
    [SerializeField] private GameObject cookedFriesVisualPrefab;

    private bool hasFries = false;
    private bool isCooking = false;
    private bool isCooked = false;

    private GameObject currentVisual;

    public void Interact()
    {
        PlayerHand playerHand = FindFirstObjectByType<PlayerHand>();

        if (playerHand == null)
        {
            Debug.Log("n encontrei a mao do player");
            return;
        }

        if (!hasFries)
        {
            ColocarBatatasNaFritadeira(playerHand);
            return;
        }

        if (isCooking)
        {
            Debug.Log("as batatas ainda estao a fritar");
            return;
        }

        if (isCooked)
        {
            TirarBatatasDaFritadeira(playerHand);
        }
    }

    private void ColocarBatatasNaFritadeira(PlayerHand playerHand)
    {
        if (!playerHand.HasItem())
        {
            Debug.Log("n tens batatas congeladas para meter na fritadeira");
            return;
        }

        if (playerHand.GetCurrentItem() != frozenFriesName)
        {
            Debug.Log("isto n pode ir para a fritadeira: " + playerHand.GetCurrentItem());
            return;
        }

        playerHand.ClearHand();

        hasFries = true;
        isCooking = true;
        isCooked = false;

        MostrarBatatasCongeladas();

        Debug.Log("batatas congeladas metidas na fritadeira");

        StartCoroutine(FritarBatatas());
    }

    private IEnumerator FritarBatatas()
    {
        yield return new WaitForSeconds(cookTime);

        isCooking = false;
        isCooked = true;

        MostrarBatatasFritas();

        Debug.Log("batatas fritas prontas");
    }

    private void TirarBatatasDaFritadeira(PlayerHand playerHand)
    {
        if (!playerHand.IsEmpty())
        {
            Debug.Log("tens de ter a mao vazia para tirar as batatas");
            return;
        }

        if (!playerHand.TrySetItem(cookedFriesName))
            return;

        hasFries = false;
        isCooking = false;
        isCooked = false;

        LimparVisual();

        Debug.Log("tirei as batatas fritas da fritadeira");
    }

    private void MostrarBatatasCongeladas()
    {
        MostrarVisual(frozenFriesSceneVisual, frozenFriesVisualPrefab);

        if (cookedFriesSceneVisual != null)
            cookedFriesSceneVisual.SetActive(false);
    }

    private void MostrarBatatasFritas()
    {
        MostrarVisual(cookedFriesSceneVisual, cookedFriesVisualPrefab);

        if (frozenFriesSceneVisual != null)
            frozenFriesSceneVisual.SetActive(false);
    }

    private void MostrarVisual(GameObject sceneVisual, GameObject prefab)
    {
        LimparVisual();

        if (sceneVisual != null)
        {
            sceneVisual.SetActive(true);
            return;
        }

        if (prefab == null || fryerVisualPoint == null)
        {
            Debug.Log("falta visual da fritadeira");
            return;
        }

        currentVisual = Instantiate(prefab, fryerVisualPoint);
        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;
    }

    private void LimparVisual()
    {
        if (currentVisual != null)
        {
            Destroy(currentVisual);
        }

        if (frozenFriesSceneVisual != null)
            frozenFriesSceneVisual.SetActive(false);

        if (cookedFriesSceneVisual != null)
            cookedFriesSceneVisual.SetActive(false);
    }
}
