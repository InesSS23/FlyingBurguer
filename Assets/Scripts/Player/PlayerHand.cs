using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [Header("item atual")]
    [SerializeField] private string currentItem = "";

    [Header("hamburger na mao")]
    [SerializeField] private List<string> currentBurger = new List<string>();

    [Header("tabuleiro na mao")]
    [SerializeField] private MealTray currentTray = new MealTray();

    [Header("visual na mao")]
    [SerializeField] private Transform handVisualPoint;
    [SerializeField] private GameObject trayVisualPrefab;
    [SerializeField] private GameObject breadVisualPrefab;
    [SerializeField] private GameObject cookedMeatVisualPrefab;
    [SerializeField] private GameObject cheeseVisualPrefab;
    [SerializeField] private GameObject lettuceVisualPrefab;
    [SerializeField] private GameObject rawMeatVisualPrefab;
    [SerializeField] private GameObject tomatoVisualPrefab;
    [SerializeField] private GameObject pepperVisualPrefab;
    [SerializeField] private GameObject frozenFriesVisualPrefab;
    [SerializeField] private GameObject cookedFriesVisualPrefab;
    [SerializeField] private GameObject emptyCupVisualPrefab;
    [SerializeField] private GameObject drinkVisualPrefab;

    [Header("posicoes no tabuleiro")]
    [SerializeField] private Vector3 trayVisualPosition = Vector3.zero;
    [SerializeField] private Vector3 trayItemsOffset = Vector3.zero;
    [SerializeField] private Vector3 burgerOnTrayPosition = new Vector3(0f, 0.08f, -0.05f);
    [SerializeField] private Vector3 friesOnTrayPosition = new Vector3(-0.7f, 0.08f, 0.05f);
    [SerializeField] private Vector3 drinkOnTrayPosition = new Vector3(0.7f, 0.08f, 0.05f);
    [SerializeField] private Vector3 trayVisualRotation = new Vector3(-90f, 0f, 0f);

    [Header("altura base das camadas")]
    [SerializeField] private float layerHeight = 0.14f;

    [Header("ajuste individual dos ingredientes")]
    [SerializeField] private float breadExtraY = 0f;
    [SerializeField] private float cookedMeatExtraY = 0f;
    [SerializeField] private float cheeseExtraY = 0.04f;
    [SerializeField] private float lettuceExtraY = 0.05f;
    [SerializeField] private float tomatoExtraY = 0.03f;
    [SerializeField] private float pepperExtraY = 0.04f;
    [SerializeField] private float frozenFriesExtraY = 0f;
    [SerializeField] private float cookedFriesExtraY = 0f;
    [SerializeField] private float emptyCupExtraY = -0.7f;
    [SerializeField] private float drinkExtraY = -0.7f;
    [SerializeField] private float trayFriesExtraY = 0f;
    [SerializeField] private float trayDrinkExtraY = 0f;

    private List<GameObject> spawnedVisuals = new List<GameObject>();

    private void Awake()
    {
        GarantirTabuleiro();
    }

    public bool HasItem()
    {
        return currentItem != "";
    }

    public bool HasBurger()
    {
        return currentBurger.Count > 0;
    }

    public bool HasTray()
    {
        GarantirTabuleiro();

        return currentTray != null && !currentTray.IsEmpty();
    }

    public bool IsEmpty()
    {
        return !HasItem() && !HasBurger() && !HasTray();
    }

    public string GetCurrentItem()
    {
        return currentItem;
    }

    public List<string> GetBurgerCopy()
    {
        return new List<string>(currentBurger);
    }

    public MealTray GetTrayCopy()
    {
        GarantirTabuleiro();

        return currentTray.Copy();
    }

    public bool TrySetItem(string itemName)
    {
        if (!IsEmpty())
        {
            Debug.Log("ja tenho algo na mao");
            return false;
        }

        currentItem = itemName;
        Debug.Log("agora tenho na mao: " + currentItem);

        AtualizarVisualDaMao();
        PlayPickupSound();

        return true;
    }

    public bool TrySetBurger(List<string> burger)
    {
        if (!IsEmpty())
        {
            Debug.Log("ja tenho algo na mao");
            return false;
        }

        if (burger == null || burger.Count == 0)
        {
            Debug.Log("esse hamburger esta vazio");
            return false;
        }

        currentBurger = new List<string>(burger);
        Debug.Log("peguei no hamburger");

        AtualizarVisualDaMao();
        PlayPickupSound();

        return true;
    }

    public bool TrySetTray(MealTray tray)
    {
        if (!IsEmpty())
        {
            Debug.Log("ja tenho algo na mao");
            return false;
        }

        if (tray == null || tray.IsEmpty())
        {
            Debug.Log("esse tabuleiro esta vazio");
            return false;
        }

        currentTray = tray.Copy();
        Debug.Log("peguei no tabuleiro");

        AtualizarVisualDaMao();
        PlayPickupSound();

        return true;
    }

    public void ClearHand()
    {
        if (HasItem())
        {
            Debug.Log("larguei/usei o item: " + currentItem);
        }

        if (HasBurger())
        {
            Debug.Log("larguei/usei o hamburger");
        }

        if (HasTray())
        {
            Debug.Log("larguei/usei o tabuleiro");
        }

        currentItem = "";
        currentBurger.Clear();

        GarantirTabuleiro();
        currentTray.Clear();

        LimparVisualDaMao();
    }

    private void PlayPickupSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPickupObjectSFX();
        }
    }

    private void AtualizarVisualDaMao()
    {
        GarantirTabuleiro();

        LimparVisualDaMao();

        if (handVisualPoint == null)
        {
            Debug.Log("n tens HandVisualPoint ligado no PlayerHand");
            return;
        }

        if (HasItem())
        {
            CriarVisual(currentItem, 0, Vector3.zero);
        }
        else if (HasBurger())
        {
            for (int i = 0; i < currentBurger.Count; i++)
            {
                CriarVisual(currentBurger[i], i, Vector3.zero);
            }
        }
        else if (HasTray())
        {
            CriarVisualDoTabuleiro();
        }
    }

    private void CriarVisualDoTabuleiro()
    {
        GarantirTabuleiro();

        if (trayVisualPrefab != null)
        {
            CriarVisualDireto(trayVisualPrefab, trayVisualPosition, Quaternion.Euler(trayVisualRotation));
        }

        for (int i = 0; i < currentTray.burger.Count; i++)
        {
            CriarVisual(currentTray.burger[i], i, trayItemsOffset + burgerOnTrayPosition);
        }

        if (currentTray.hasFries)
        {
            Vector3 position = trayItemsOffset + friesOnTrayPosition + new Vector3(0f, trayFriesExtraY, 0f);
            CriarVisualDireto(BuscarPrefab("CookedFries"), position);
        }

        if (currentTray.hasDrink)
        {
            Vector3 position = trayItemsOffset + drinkOnTrayPosition + new Vector3(0f, trayDrinkExtraY, 0f);
            CriarVisualDireto(BuscarPrefab("Drink"), position);
        }
    }

    private void CriarVisual(string item, int index, Vector3 basePosition)
    {
        GameObject prefab = BuscarPrefab(item);

        if (prefab == null)
        {
            Debug.Log("n tenho visual para: " + item);
            return;
        }

        float finalY = index * layerHeight + BuscarExtraY(item);
        Vector3 finalPosition = basePosition + new Vector3(0f, finalY, 0f);

        CriarVisualDireto(prefab, finalPosition);
    }

    private void CriarVisualDireto(GameObject prefab, Vector3 localPosition)
    {
        CriarVisualDireto(prefab, localPosition, Quaternion.identity);
    }

    private void CriarVisualDireto(GameObject prefab, Vector3 localPosition, Quaternion localRotation)
    {
        if (prefab == null)
            return;

        GameObject visual = Instantiate(prefab, handVisualPoint);

        visual.transform.localPosition = localPosition;
        visual.transform.localRotation = localRotation;

        spawnedVisuals.Add(visual);
    }

    private float BuscarExtraY(string item)
    {
        if (item == "Bread")
            return breadExtraY;

        if (item == "CookedMeat")
            return cookedMeatExtraY;

        if (item == "Cheese")
            return cheeseExtraY;

        if (item == "Lettuce")
            return lettuceExtraY;

        if (item == "Tomato")
            return tomatoExtraY;

        if (item == "Pepper")
            return pepperExtraY;

        if (item == "FrozenFries")
            return frozenFriesExtraY;

        if (item == "CookedFries")
            return cookedFriesExtraY;

        if (item == "EmptyCup")
            return emptyCupExtraY;

        if (item == "Drink")
            return drinkExtraY;

        return 0f;
    }

    private GameObject BuscarPrefab(string item)
    {
        if (item == "Bread")
            return breadVisualPrefab;

        if (item == "CookedMeat")
            return cookedMeatVisualPrefab;

        if (item == "Cheese")
            return cheeseVisualPrefab;

        if (item == "Lettuce")
            return lettuceVisualPrefab;

        if (item == "RawMeat")
            return rawMeatVisualPrefab;

        if (item == "Tomato")
            return tomatoVisualPrefab;

        if (item == "Pepper")
            return pepperVisualPrefab;

        if (item == "FrozenFries")
            return frozenFriesVisualPrefab;

        if (item == "CookedFries")
            return cookedFriesVisualPrefab;

        if (item == "EmptyCup")
            return emptyCupVisualPrefab;

        if (item == "Drink")
            return drinkVisualPrefab;

        return null;
    }

    private void GarantirTabuleiro()
    {
        if (currentTray == null)
            currentTray = new MealTray();

        if (currentTray.burger == null)
            currentTray.burger = new List<string>();
    }

    private void LimparVisualDaMao()
    {
        for (int i = 0; i < spawnedVisuals.Count; i++)
        {
            if (spawnedVisuals[i] != null)
            {
                Destroy(spawnedVisuals[i]);
            }
        }

        spawnedVisuals.Clear();
    }
}
