using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : MonoBehaviour, IInteractable
{
    [Header("hamburger no balcao")]
    [SerializeField] private List<string> deliveredBurger = new List<string>();

    [Header("visual")]
    [SerializeField] private Transform deliveryVisualPoint;
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

        if (playerHand.HasBurger())
        {
            ColocarHamburgerNoBalcao(playerHand);
            return;
        }

        if (playerHand.IsEmpty() && deliveredBurger.Count > 0)
        {
            PegarHamburgerDoBalcao(playerHand);
            return;
        }

        if (playerHand.HasItem())
        {
            Debug.Log("n podes entregar so um ingrediente");
            return;
        }

        Debug.Log("n tens hamburger para entregar");
    }

    private void ColocarHamburgerNoBalcao(PlayerHand playerHand)
    {
        if (deliveredBurger.Count > 0)
        {
            Debug.Log("ja existe um hamburger no balcao");
            return;
        }

        deliveredBurger = playerHand.GetBurgerCopy();

        playerHand.ClearHand();

        RecriarVisual();

        Debug.Log("hamburger colocado no balcao");
    }

    private void PegarHamburgerDoBalcao(PlayerHand playerHand)
    {
        playerHand.TrySetBurger(deliveredBurger);

        LimparBalcao();

        Debug.Log("peguei no hamburger do balcao");
    }

    public List<string> GetDeliveredBurger()
    {
        return deliveredBurger;
    }

    public void LimparBalcao()
    {
        deliveredBurger.Clear();

        for (int i = 0; i < spawnedVisuals.Count; i++)
        {
            if (spawnedVisuals[i] != null)
            {
                Destroy(spawnedVisuals[i]);
            }
        }

        spawnedVisuals.Clear();
    }

    private void RecriarVisual()
    {
        LimparVisuais();

        for (int i = 0; i < deliveredBurger.Count; i++)
        {
            CriarVisual(deliveredBurger[i], i);
        }
    }

    private void CriarVisual(string item, int index)
    {
        if (deliveryVisualPoint == null)
        {
            Debug.Log("falta DeliveryVisualPoint");
            return;
        }

        GameObject prefab = BuscarPrefab(item);

        if (prefab == null)
        {
            Debug.Log("n tenho visual para: " + item);
            return;
        }

        GameObject visual = Instantiate(prefab, deliveryVisualPoint);
        visual.transform.localPosition = new Vector3(0, index * layerHeight, 0);
        visual.transform.localRotation = Quaternion.identity;

        spawnedVisuals.Add(visual);
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

        return null;
    }

    private void LimparVisuais()
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