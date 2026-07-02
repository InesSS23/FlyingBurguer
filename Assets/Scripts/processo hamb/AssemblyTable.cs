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

    [Header("altura base das camadas")]
    [SerializeField] private float layerHeight = 0.14f;

    [Header("ajuste individual dos ingredientes")]
    [SerializeField] private float breadExtraY = -0.02f;
    [SerializeField] private float cookedMeatExtraY = 0f;
    [SerializeField] private float cheeseExtraY = 0.04f;
    [SerializeField] private float lettuceExtraY = 0.05f;
    [SerializeField] private float tomatoExtraY = 0.03f;
    [SerializeField] private float pepperExtraY = 0.04f;

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
        RecriarVisualDoBurger();

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

    private void RecriarVisualDoBurger()
    {
        LimparVisuais();

        if (burgerVisualPoint == null)
        {
            Debug.Log("n tens BurgerVisualPoint ligado");
            return;
        }

        for (int i = 0; i < currentBurger.Count; i++)
        {
            CriarVisualDoIngrediente(currentBurger[i], i);
        }
    }

    private void CriarVisualDoIngrediente(string item, int index)
    {
        GameObject prefab = BuscarPrefab(item);

        if (prefab == null)
        {
            Debug.Log("n tenho visual para: " + item);
            return;
        }

        GameObject visual = Instantiate(prefab, burgerVisualPoint);

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