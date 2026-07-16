using System.Collections;
using UnityEngine;

public class DrinkMachine : MonoBehaviour, IInteractable
{
    [Header("nomes")]
    [SerializeField] private string emptyCupName = "EmptyCup";
    [SerializeField] private string drinkName = "Drink";

    [Header("tempo")]
    [SerializeField] private float fillTime = 2f;

    [Header("visual ja na cena")]
    [SerializeField] private GameObject emptyCupSceneVisual;
    [SerializeField] private GameObject fullCupSceneVisual;

    [Header("visual por prefab")]
    [SerializeField] private Transform cupVisualPoint;
    [SerializeField] private GameObject emptyCupVisualPrefab;
    [SerializeField] private GameObject fullCupVisualPrefab;

    private bool hasCup = false;
    private bool isFilling = false;
    private bool isFilled = false;
    private float fillingProgress = 0f;

    public bool IsProcessing => isFilling;
    public float ProcessingProgress => fillingProgress;

    private GameObject currentVisual;
    private AudioSource fillingLoopSource;
    private LevelConfig levelConfig;

    private void Start()
    {
        DayManager dayManager = FindFirstObjectByType<DayManager>();

        if (dayManager != null)
        {
            levelConfig = dayManager.GetLevelConfig();
        }
    }

    private void OnDisable()
    {
        StopFillingSound();
    }

    public void Interact()
    {
        if (levelConfig != null && !levelConfig.allowDrink)
        {
            if (GameplayHUDPolish.Instance != null)
            {
                GameplayHUDPolish.Instance.ShowFeedback("Ainda não desbloqueaste as bebidas.");
            }

            return;
        }

        PlayerHand playerHand = FindFirstObjectByType<PlayerHand>();

        if (playerHand == null)
        {
            Debug.Log("n encontrei a mao do player");
            return;
        }

        if (!hasCup)
        {
            ColocarCopoNaMaquina(playerHand);
            return;
        }

        if (isFilling)
        {
            Debug.Log("a bebida ainda esta a encher");
            return;
        }

        if (isFilled)
        {
            TirarBebidaDaMaquina(playerHand);
        }
    }

    private void ColocarCopoNaMaquina(PlayerHand playerHand)
    {
        if (!playerHand.HasItem())
        {
            Debug.Log("n tens copo vazio para meter na maquina");
            return;
        }

        if (playerHand.GetCurrentItem() != emptyCupName)
        {
            Debug.Log("isto n pode ir para a maquina de bebidas: " + playerHand.GetCurrentItem());
            return;
        }

        playerHand.ClearHand();

        hasCup = true;
        isFilling = true;
        isFilled = false;
        fillingProgress = 0f;

        MostrarCopoVazio();
        PlayFillingSound();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlaceObjectSFX();
        }

        Debug.Log("copo vazio metido na maquina");

        StartCoroutine(EncherCopo());
    }

    private IEnumerator EncherCopo()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fillTime);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fillingProgress = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        fillingProgress = 1f;
        isFilling = false;
        isFilled = true;

        StopFillingSound();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayReadySFX();
        }

        MostrarCopoCheio();

        Debug.Log("bebida pronta");
    }

    private void TirarBebidaDaMaquina(PlayerHand playerHand)
    {
        if (!playerHand.IsEmpty())
        {
            Debug.Log("tens de ter a mao vazia para tirar a bebida");
            return;
        }

        if (!playerHand.TrySetItem(drinkName))
            return;

        hasCup = false;
        isFilling = false;
        isFilled = false;
        fillingProgress = 0f;

        LimparVisual();

        Debug.Log("tirei a bebida da maquina");
    }

    private void PlayFillingSound()
    {
        StopFillingSound();

        if (AudioManager.Instance != null)
        {
            fillingLoopSource = AudioManager.Instance.PlayFillDrinkLoopSFX();
        }
    }

    private void StopFillingSound()
    {
        if (AudioManager.Instance != null && fillingLoopSource != null)
        {
            AudioManager.Instance.StopLoopingSFX(fillingLoopSource);
        }

        fillingLoopSource = null;
    }

    private void MostrarCopoVazio()
    {
        MostrarVisual(emptyCupSceneVisual, emptyCupVisualPrefab);

        if (fullCupSceneVisual != null)
            fullCupSceneVisual.SetActive(false);
    }

    private void MostrarCopoCheio()
    {
        MostrarVisual(fullCupSceneVisual, fullCupVisualPrefab);

        if (emptyCupSceneVisual != null)
            emptyCupSceneVisual.SetActive(false);
    }

    private void MostrarVisual(GameObject sceneVisual, GameObject prefab)
    {
        LimparVisual();

        if (sceneVisual != null)
        {
            sceneVisual.SetActive(true);
            return;
        }

        if (prefab == null || cupVisualPoint == null)
        {
            Debug.Log("falta visual da maquina de bebidas");
            return;
        }

        currentVisual = Instantiate(prefab, cupVisualPoint);
        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;
    }

    private void LimparVisual()
    {
        if (currentVisual != null)
        {
            Destroy(currentVisual);
        }

        if (emptyCupSceneVisual != null)
            emptyCupSceneVisual.SetActive(false);

        if (fullCupSceneVisual != null)
            fullCupSceneVisual.SetActive(false);
    }
}
