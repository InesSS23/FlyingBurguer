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
    [SerializeField] private GameObject tomatoVisualPrefab;
    [SerializeField] private GameObject pepperVisualPrefab;

    [SerializeField] private float layerHeight = 0.14f;

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
            ColocarHamburgerNaMesa(playerHand);
            return;
        }

        if (playerHand.HasItem())
        {
            AdicionarIngrediente(playerHand);
            return;
        }

        if (currentBurger.Count > 0)
        {
            PegarHamburger(playerHand);
            return;
        }

        Debug.Log("n tens nada na mao e a mesa esta vazia");
    }

    private void AdicionarIngrediente(PlayerHand playerHand)
    {
        string item = playerHand.GetCurrentItem();

        if (item == "RawMeat")
        {
            Debug.Log("n podes meter carne crua no hamburger");
            return;
        }

        currentBurger.Add(item);
        CriarVisualDoIngrediente(item);

        Debug.Log("adicionei ao hamburger: " + item);

        playerHand.ClearHand();

        MostrarBurgerAtual();
    }

    private void PegarHamburger(PlayerHand playerHand)
    {
        if (!playerHand.IsEmpty())
        {
            Debug.Log("a tua mao n esta vazia");
            return;
        }

        playerHand.TrySetBurger(currentBurger);

        ClearBurger();

        Debug.Log("peguei no hamburger da mesa");
    }

    private void ColocarHamburgerNaMesa(PlayerHand playerHand)
    {
        if (currentBurger.Count > 0)
        {
            Debug.Log("a mesa ja tem um hamburger");
            return;
        }

        currentBurger = playerHand.GetBurgerCopy();

        playerHand.ClearHand();

        RecriarVisualDoBurger();

        Debug.Log("voltei a meter o hamburger na mesa");
    }

    private void CriarVisualDoIngrediente(string item)
    {
        if (burgerVisualPoint == null)
        {
            Debug.Log("n tens BurgerVisualPoint ligado");
            return;
        }

        GameObject prefab = BuscarPrefab(item);

        if (prefab == null)
        {
            Debug.Log("n tenho visual para: " + item);
            return;
        }

        Vector3 spawnPosition = burgerVisualPoint.position;
        spawnPosition.y += spawnedVisuals.Count * layerHeight;

        GameObject visual = Instantiate(prefab, spawnPosition, burgerVisualPoint.rotation);
        spawnedVisuals.Add(visual);
    }

    private void RecriarVisualDoBurger()
    {
        LimparVisuais();

        for (int i = 0; i < currentBurger.Count; i++)
        {
            CriarVisualDoIngrediente(currentBurger[i]);
        }
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

        if (item == "Tomato")
        return tomatoVisualPrefab;

        if (item == "Pepper")
        return pepperVisualPrefab;

        return null;
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
        LimparVisuais();
        Debug.Log("hamburger limpo da mesa");
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
