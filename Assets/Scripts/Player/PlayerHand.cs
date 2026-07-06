using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [Header("item atual")]
    [SerializeField] private string currentItem = "";

    [Header("hamburger na mao")]
    [SerializeField] private List<string> currentBurger = new List<string>();

    [Header("visual na mao")]
    [SerializeField] private Transform handVisualPoint;
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
    [SerializeField] private float emptyCupExtraY = 0f;
    [SerializeField] private float drinkExtraY = 0f;

    private List<GameObject> spawnedVisuals = new List<GameObject>();

    public bool HasItem()
    {
        return currentItem != "";
    }

    public bool HasBurger()
    {
        return currentBurger.Count > 0;
    }

    public bool IsEmpty()
    {
        return !HasItem() && !HasBurger();
    }

    public string GetCurrentItem()
    {
        return currentItem;
    }

    public List<string> GetBurgerCopy()
    {
        return new List<string>(currentBurger);
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

        currentItem = "";
        currentBurger.Clear();

        LimparVisualDaMao();
    }

    private void AtualizarVisualDaMao()
    {
        LimparVisualDaMao();

        if (handVisualPoint == null)
        {
            Debug.Log("n tens HandVisualPoint ligado no PlayerHand");
            return;
        }

        if (HasItem())
        {
            CriarVisual(currentItem, 0);
        }
        else if (HasBurger())
        {
            for (int i = 0; i < currentBurger.Count; i++)
            {
                CriarVisual(currentBurger[i], i);
            }
        }
    }

    private void CriarVisual(string item, int index)
    {
        GameObject prefab = BuscarPrefab(item);

        if (prefab == null)
        {
            Debug.Log("n tenho visual para: " + item);
            return;
        }

        GameObject visual = Instantiate(prefab, handVisualPoint);

        float finalY = index * layerHeight + BuscarExtraY(item);

        visual.transform.localPosition = new Vector3(0f, finalY, 0f);
        visual.transform.localRotation = Quaternion.identity;

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
