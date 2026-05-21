using System.Collections.Generic;
using UnityEngine;

public class AssemblyTable : MonoBehaviour, IInteractable
{
    [Header("hamburger atual")]
    [SerializeField] private List<string> currentBurger = new List<string>();

    [Header("visual do hamburger")]
    [SerializeField] private Transform burgerVisualPoint;
    [SerializeField] private GameObject breadVisualPrefab;
    [SerializeField] private GameObject cookedMeatVisualPrefab;
    [SerializeField] private GameObject cheeseVisualPrefab;
    [SerializeField] private GameObject lettuceVisualPrefab;
    [SerializeField] private float layerHeight = 0.08f;

    private List<GameObject> spawnedVisuals = new List<GameObject>();

    public void Interact()
    {
        PlayerHand playerHand = FindFirstObjectByType<PlayerHand>();

        if (playerHand == null)
        {
            Debug.Log("n encontrei a mao do player");
            return;
        }

        if (!playerHand.HasItem())
        {
            Debug.Log("n tens nada na mao para meter no hamburger");
            return;
        }

        string item = playerHand.GetCurrentItem();

        if (item == "RawMeat")
        {
            Debug.Log("n podes meter carne crua no hamburger, cozinha primeiro");
            return;
        }

        currentBurger.Add(item);
        Debug.Log("adicionei ao hamburger: " + item);

        CriarVisualDoIngrediente(item);

        playerHand.ClearItem();

        MostrarBurgerAtual();
    }

    private void CriarVisualDoIngrediente(string item)
    {
        if (burgerVisualPoint == null)
        {
            Debug.Log("n tens BurgerVisualPoint ligado na AssemblyTable");
            return;
        }

        GameObject prefab = null;

        if (item == "Bread")
        {
            prefab = breadVisualPrefab;
        }
        else if (item == "CookedMeat")
        {
            prefab = cookedMeatVisualPrefab;
        }
        else if (item == "Cheese")
        {
            prefab = cheeseVisualPrefab;
        }
        else if (item == "Lettuce")
        {
            prefab = lettuceVisualPrefab;
        }

        if (prefab == null)
        {
            Debug.Log("n tenho visual para este ingrediente: " + item);
            return;
        }

        Vector3 spawnPosition = burgerVisualPoint.position;
        spawnPosition.y += spawnedVisuals.Count * layerHeight;

        GameObject visual = Instantiate(prefab, spawnPosition, burgerVisualPoint.rotation);
        spawnedVisuals.Add(visual);
    }

    private void MostrarBurgerAtual()
    {
        string burgerText = "hamburger atual: ";

        for (int i = 0; i < currentBurger.Count; i++)
        {
            burgerText += currentBurger[i];

            if (i < currentBurger.Count - 1)
            {
                burgerText += " + ";
            }
        }

        Debug.Log(burgerText);
    }

    public List<string> GetBurger()
    {
        return currentBurger;
    }

    public void ClearBurger()
    {
        currentBurger.Clear();

        for (int i = 0; i < spawnedVisuals.Count; i++)
        {
            if (spawnedVisuals[i] != null)
            {
                Destroy(spawnedVisuals[i]);
            }
        }

        spawnedVisuals.Clear();

        Debug.Log("hamburger limpo");
    }
}