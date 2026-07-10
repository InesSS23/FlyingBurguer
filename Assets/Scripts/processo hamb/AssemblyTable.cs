using System.Collections.Generic;
using UnityEngine;

public class AssemblyTable : MonoBehaviour, IInteractable
{
    [Header("pedido atual")]
    [SerializeField] private MealTray currentTray = new MealTray();

    [Header("visual do pedido")]
    [SerializeField] private Transform burgerVisualPoint;
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
    [SerializeField] private Vector3 trayVisualPosition = Vector3.zero;
    [SerializeField] private Vector3 trayItemsOffset = Vector3.zero;
    [SerializeField] private Vector3 burgerOnTrayPosition = new Vector3(0f, 0.08f, -0.05f);
    [SerializeField] private Vector3 friesOnTrayPosition = new Vector3(-0.7f, 0.08f, 0.05f);
    [SerializeField] private Vector3 drinkOnTrayPosition = new Vector3(0.7f, 0.08f, 0.05f);
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

    private void Awake()
    {
        GarantirTabuleiro();
    }

    public void Interact()
    {
        GarantirTabuleiro();

        PlayerHand playerHand = FindFirstObjectByType<PlayerHand>();

        if (playerHand == null)
        {
            Debug.Log("n encontrei a mao do player");
            return;
        }

        if (playerHand.HasTray())
        {
            ColocarTabuleiroNaMesa(playerHand);
            return;
        }

        if (playerHand.HasBurger())
        {
            ColocarHamburgerAntigoNaMesa(playerHand);
            return;
        }

        if (playerHand.HasItem())
        {
            AdicionarItemAoPedido(playerHand);
            return;
        }

        if (!currentTray.IsEmpty())
        {
            PegarTabuleiro(playerHand);
            return;
        }

        Debug.Log("n tens nada na mao e a mesa esta vazia");
    }

    private void AdicionarItemAoPedido(PlayerHand playerHand)
    {
        string item = playerHand.GetCurrentItem();

        if (item == "RawMeat")
        {
            Debug.Log("n podes meter carne crua no pedido");
            return;
        }

        if (item == "FrozenFries")
        {
            Debug.Log("as batatas ainda estao congeladas");
            return;
        }

        if (item == "EmptyCup")
        {
            Debug.Log("tens de encher o copo antes de o meter no tabuleiro");
            return;
        }

        if (item == "CookedFries")
        {
            AdicionarBatatas(playerHand);
            return;
        }

        if (item == "Drink")
        {
            AdicionarBebida(playerHand);
            return;
        }

        currentTray.burger.Add(item);
        RecriarVisualDoPedido();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAddBurgerIngredientSFX();
        }

        Debug.Log("adicionei ao hamburger: " + item);

        playerHand.ClearHand();

        MostrarPedidoAtual();
    }

    private void AdicionarBatatas(PlayerHand playerHand)
    {
        if (currentTray.hasFries)
        {
            Debug.Log("o tabuleiro ja tem batatas");
            return;
        }

        currentTray.hasFries = true;
        RecriarVisualDoPedido();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlaceObjectSFX();
        }

        playerHand.ClearHand();

        Debug.Log("adicionei batatas ao tabuleiro");
    }

    private void AdicionarBebida(PlayerHand playerHand)
    {
        if (currentTray.hasDrink)
        {
            Debug.Log("o tabuleiro ja tem bebida");
            return;
        }

        currentTray.hasDrink = true;
        RecriarVisualDoPedido();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlaceObjectSFX();
        }

        playerHand.ClearHand();

        Debug.Log("adicionei bebida ao tabuleiro");
    }

    private void PegarTabuleiro(PlayerHand playerHand)
    {
        if (!playerHand.IsEmpty())
        {
            Debug.Log("a tua mao n esta vazia");
            return;
        }

        if (!playerHand.TrySetTray(currentTray))
            return;

        ClearTray();

        Debug.Log("peguei no tabuleiro da mesa");
    }

    private void ColocarTabuleiroNaMesa(PlayerHand playerHand)
    {
        if (!currentTray.IsEmpty())
        {
            Debug.Log("a mesa ja tem um pedido");
            return;
        }

        currentTray = playerHand.GetTrayCopy();

        playerHand.ClearHand();

        RecriarVisualDoPedido();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlaceObjectSFX();
        }

        Debug.Log("voltei a meter o tabuleiro na mesa");
    }

    private void ColocarHamburgerAntigoNaMesa(PlayerHand playerHand)
    {
        if (!currentTray.IsEmpty())
        {
            Debug.Log("a mesa ja tem um pedido");
            return;
        }

        currentTray.burger = playerHand.GetBurgerCopy();

        playerHand.ClearHand();

        RecriarVisualDoPedido();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlaceObjectSFX();
        }

        Debug.Log("voltei a meter o hamburger na mesa");
    }

    private void RecriarVisualDoPedido()
    {
        GarantirTabuleiro();

        LimparVisuais();

        if (burgerVisualPoint == null)
        {
            Debug.Log("n tens BurgerVisualPoint ligado");
            return;
        }

        if (trayVisualPrefab != null && !currentTray.IsEmpty())
        {
            CriarVisual(trayVisualPrefab, trayVisualPosition, Quaternion.Euler(trayVisualRotation));
        }

        for (int i = 0; i < currentTray.burger.Count; i++)
        {
            CriarVisualDoIngrediente(currentTray.burger[i], i, trayItemsOffset + burgerOnTrayPosition);
        }

        if (currentTray.hasFries)
        {
            CriarVisualDoIngrediente("CookedFries", 0, trayItemsOffset + friesOnTrayPosition);
        }

        if (currentTray.hasDrink)
        {
            CriarVisualDoIngrediente("Drink", 0, trayItemsOffset + drinkOnTrayPosition);
        }
    }

    private void CriarVisualDoIngrediente(string item, int index, Vector3 basePosition)
    {
        GameObject prefab = BuscarPrefab(item);

        if (prefab == null)
        {
            Debug.Log("n tenho visual para: " + item);
            return;
        }

        float finalY = index * layerHeight + BuscarExtraY(item);
        Vector3 finalPosition = basePosition + new Vector3(0f, finalY, 0f);

        CriarVisual(prefab, finalPosition);
    }

    private void CriarVisual(GameObject prefab, Vector3 localPosition)
    {
        CriarVisual(prefab, localPosition, Quaternion.identity);
    }

    private void CriarVisual(GameObject prefab, Vector3 localPosition, Quaternion localRotation)
    {
        GameObject visual = Instantiate(prefab, burgerVisualPoint);
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

        if (item == "CookedFries")
            return friesExtraY;

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

    private void MostrarPedidoAtual()
    {
        string pedidoText = "pedido atual: ";

        for (int i = 0; i < currentTray.burger.Count; i++)
        {
            pedidoText += currentTray.burger[i];

            if (i < currentTray.burger.Count - 1)
            {
                pedidoText += " + ";
            }
        }

        if (currentTray.hasFries)
            pedidoText += " + Batatas";

        if (currentTray.hasDrink)
            pedidoText += " + Bebida";

        Debug.Log(pedidoText);
    }

    public List<string> GetBurger()
    {
        return currentTray.GetBurgerCopy();
    }

    public MealTray GetTray()
    {
        return currentTray.Copy();
    }

    public void ClearBurger()
    {
        ClearTray();
    }

    public void ClearTray()
    {
        GarantirTabuleiro();

        currentTray.Clear();
        LimparVisuais();
        Debug.Log("pedido limpo da mesa");
    }

    private void GarantirTabuleiro()
    {
        if (currentTray == null)
            currentTray = new MealTray();

        if (currentTray.burger == null)
            currentTray.burger = new List<string>();
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