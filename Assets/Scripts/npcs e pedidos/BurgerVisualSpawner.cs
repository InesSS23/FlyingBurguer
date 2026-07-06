using System.Collections.Generic;
using UnityEngine;

public class BurgerVisualSpawner : MonoBehaviour
{
    [Header("ponto onde aparece o hamburger")]
    [SerializeField] private Transform visualPoint;

    [Header("prefabs visuais")]
    [SerializeField] private GameObject trayVisualPrefab;
    [SerializeField] private GameObject breadVisualPrefab;
    [SerializeField] private GameObject cookedMeatVisualPrefab;
    [SerializeField] private GameObject cheeseVisualPrefab;
    [SerializeField] private GameObject lettuceVisualPrefab;
    [SerializeField] private GameObject tomatoVisualPrefab;
    [SerializeField] private GameObject pepperVisualPrefab;
    [SerializeField] private GameObject cookedFriesVisualPrefab;
    [SerializeField] private GameObject drinkVisualPrefab;

    [Header("posicoes no tabuleiro")]
    [SerializeField] private Vector3 trayVisualPosition = new Vector3(0.15f, -0.1f, 0.5f);
    [SerializeField] private Vector3 trayItemsOffset = new Vector3(0.15f, -0.1f, 0.5f);
    [SerializeField] private Vector3 burgerOnTrayPosition = new Vector3(0f, 0.1f, 0.15f);
    [SerializeField] private Vector3 friesOnTrayPosition = new Vector3(-1f, 0.4f, -0.8f);
    [SerializeField] private Vector3 drinkOnTrayPosition = new Vector3(1f, 0.1f, -0.6f);
    [SerializeField] private Vector3 trayVisualRotation = new Vector3(-90f, 0f, 0f);

    [Header("altura base das camadas")]
    [SerializeField] private float layerHeight = 0.14f;

    [Header("ajuste individual dos ingredientes")]
    [SerializeField] private float breadExtraY = -0.02f;
    [SerializeField] private float cookedMeatExtraY = 0f;
    [SerializeField] private float cheeseExtraY = 0.04f;
    [SerializeField] private float lettuceExtraY = 0.05f;
    [SerializeField] private float tomatoExtraY = 0.03f;
    [SerializeField] private float pepperExtraY = 0.04f;
    [SerializeField] private float friesExtraY = 0f;
    [SerializeField] private float drinkExtraY = 0f;

    private List<GameObject> spawnedVisuals = new List<GameObject>();

    public void ShowBurger(List<string> burger)
    {
        ClearVisuals();

        if (visualPoint == null)
        {
            Debug.Log("falta visual point para mostrar burger");
            return;
        }

        if (burger == null || burger.Count == 0)
        {
            Debug.Log("burger vazio, n tenho o que mostrar");
            return;
        }

        for (int i = 0; i < burger.Count; i++)
        {
            CriarVisual(burger[i], i, Vector3.zero);
        }
    }

    public void ShowTray(MealTray tray)
    {
        ClearVisuals();

        if (visualPoint == null)
        {
            Debug.Log("falta visual point para mostrar tabuleiro");
            return;
        }

        if (tray == null || tray.IsEmpty())
        {
            Debug.Log("tabuleiro vazio, n tenho o que mostrar");
            return;
        }

        Debug.Log("visual do tabuleiro recebido - burger: " + burgerText(tray.GetBurgerCopy()) + " | batatas: " + tray.hasFries + " | bebida: " + tray.hasDrink);

        if (trayVisualPrefab != null)
        {
            CriarVisualDireto(trayVisualPrefab, trayVisualPosition, Quaternion.Euler(trayVisualRotation));
        }

        List<string> burger = tray.GetBurgerCopy();

        for (int i = 0; i < burger.Count; i++)
        {
            CriarVisual(burger[i], i, trayItemsOffset + burgerOnTrayPosition);
        }

        if (tray.hasFries)
        {
            CriarVisual("CookedFries", 0, trayItemsOffset + friesOnTrayPosition);
        }

        if (tray.hasDrink)
        {
            CriarVisual("Drink", 0, trayItemsOffset + drinkOnTrayPosition);
        }
    }

    private void CriarVisual(string item, int index, Vector3 basePosition)
    {
        GameObject prefab = GetPrefab(item);

        if (prefab == null)
        {
            Debug.Log("n tenho visual para: " + item);
            return;
        }

        float finalY = index * layerHeight + GetExtraY(item);
        Vector3 finalPosition = basePosition + new Vector3(0f, finalY, 0f);

        CriarVisualDireto(prefab, finalPosition, Quaternion.identity);
    }

    private void CriarVisualDireto(GameObject prefab, Vector3 localPosition, Quaternion localRotation)
    {
        if (prefab == null)
            return;

        GameObject visual = Instantiate(prefab, visualPoint);
        visual.transform.localPosition = localPosition;
        visual.transform.localRotation = localRotation;

        spawnedVisuals.Add(visual);
    }

    public void ClearVisuals()
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

    private float GetExtraY(string item)
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

        if (item == "CookedFries")
            return friesExtraY;

        if (item == "Drink")
            return drinkExtraY;

        return 0f;
    }

    private GameObject GetPrefab(string item)
    {
        if (item == "Bread")
            return breadVisualPrefab;

        if (item == "CookedMeat")
            return cookedMeatVisualPrefab;

        if (item == "Cheese")
            return cheeseVisualPrefab;

        if (item == "Lettuce")
            return lettuceVisualPrefab;

        if (item == "Tomato")
            return tomatoVisualPrefab;

        if (item == "Pepper")
            return pepperVisualPrefab;

        if (item == "CookedFries")
            return cookedFriesVisualPrefab;

        if (item == "Drink")
            return drinkVisualPrefab;

        return null;
    }

    private string burgerText(List<string> burger)
    {
        if (burger == null || burger.Count == 0)
            return "vazio";

        string text = "";

        for (int i = 0; i < burger.Count; i++)
        {
            text += burger[i];

            if (i < burger.Count - 1)
                text += " + ";
        }

        return text;
    }
}
