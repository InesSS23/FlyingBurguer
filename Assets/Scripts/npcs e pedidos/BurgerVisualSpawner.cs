using System.Collections.Generic;
using UnityEngine;

public class BurgerVisualSpawner : MonoBehaviour
{
    [Header("ponto onde aparece o hamburger")]
    [SerializeField] private Transform visualPoint;

    [Header("prefabs visuais")]
    [SerializeField] private GameObject breadVisualPrefab;
    [SerializeField] private GameObject cookedMeatVisualPrefab;
    [SerializeField] private GameObject cheeseVisualPrefab;
    [SerializeField] private GameObject lettuceVisualPrefab;
    [SerializeField] private GameObject tomatoVisualPrefab;
    [SerializeField] private GameObject pepperVisualPrefab;

    [Header("altura base das camadas")]
    [SerializeField] private float layerHeight = 0.14f;

    [Header("ajuste individual dos ingredientes")]
    [SerializeField] private float breadExtraY = 0f;
    [SerializeField] private float cookedMeatExtraY = 0f;
    [SerializeField] private float cheeseExtraY = 0.04f;
    [SerializeField] private float lettuceExtraY = 0.05f;
    [SerializeField] private float tomatoExtraY = 0.03f;
    [SerializeField] private float pepperExtraY = 0.04f;

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
            CriarVisual(burger[i], i);
        }
    }

    private void CriarVisual(string item, int index)
    {
        GameObject prefab = GetPrefab(item);

        if (prefab == null)
        {
            Debug.Log("n tenho visual para: " + item);
            return;
        }

        GameObject visual = Instantiate(prefab, visualPoint);

        float finalY = index * layerHeight + GetExtraY(item);

        visual.transform.localPosition = new Vector3(0f, finalY, 0f);
        visual.transform.localRotation = Quaternion.identity;

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

        return null;
    }
}