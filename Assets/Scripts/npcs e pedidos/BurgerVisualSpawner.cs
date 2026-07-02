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

    [Header("altura das camadas")]
    [SerializeField] private float layerHeight = 0.14f;

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
            GameObject prefab = GetPrefab(burger[i]);

            if (prefab == null)
            {
                Debug.Log("n tenho visual para: " + burger[i]);
                continue;
            }

            GameObject visual = Instantiate(prefab, visualPoint);

            visual.transform.localPosition = new Vector3(0, i * layerHeight, 0);
            visual.transform.localRotation = Quaternion.identity;

            spawnedVisuals.Add(visual);
        }
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
