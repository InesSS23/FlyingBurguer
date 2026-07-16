using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [Header("prefabs dos clientes")]
    [SerializeField] private GameObject[] customerPrefabs;

    [Header("prefab animacao tucano")]
    [SerializeField] private GameObject tucanoAnimationPrefab;

    [Header("prefab animacao arara")]
    [SerializeField] private GameObject araraAnimationPrefab;

    [Header("prefab animacao gaivota")]
    [SerializeField] private GameObject gaivotaAnimationPrefab;

    [Header("prefab animacao coruja")]
    [SerializeField] private GameObject corujaAnimationPrefab;

    [Header("pontos")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private CustomerServicePoint[] servicePoints;
    [SerializeField] private Transform[] exitPoints;

    [Header("tempo de spawn")]
    [SerializeField] private float minSpawnTime = 8f;
    [SerializeField] private float maxSpawnTime = 15f;
    [SerializeField] private float spawnCheckRadius = 0.6f;
    [SerializeField] private LayerMask customerLayerMask;

    [Header("dificuldade dos clientes")]
    [SerializeField] private float customerPatienceTime = 25f;
    [SerializeField] private int missedCustomerPenalty = 5;
    [SerializeField] private DayManager dayManager;

    [Header("controlo")]
    [SerializeField] private bool startSpawningAutomatically = false;

    private List<BurgerOrder> possibleOrders = new List<BurgerOrder>();
    private Coroutine spawnCoroutine;
    private bool isSpawning = false;

    private LevelConfig activeConfig;
    private bool levelConfigApplied = false;

    void Start()
    {
        if (!levelConfigApplied && dayManager != null && dayManager.GetLevelConfig() != null)
        {
            ApplyLevelConfig(dayManager.GetLevelConfig());
        }

        if (!levelConfigApplied)
        {
            CriarPedidosPossiveis();
        }

        if (startSpawningAutomatically)
        {
            StartSpawning();
        }
    }

    public void ApplyLevelConfig(LevelConfig config)
    {
        if (config == null)
            return;

        activeConfig = config;

        minSpawnTime = Mathf.Max(0.1f, config.minSpawnTime);
        maxSpawnTime = Mathf.Max(minSpawnTime, config.maxSpawnTime);
        customerPatienceTime = Mathf.Max(1f, config.customerPatienceTime);
        missedCustomerPenalty = Mathf.Max(0, config.missedCustomerPenalty);

        CriarPedidosPossiveis();

        levelConfigApplied = true;

        Debug.Log("CustomerManager configurado para o dia " + config.dayNumber);
    }

    private bool IsSpawnPointFree(Transform point)
    {
        int mask = customerLayerMask.value == 0 ? ~0 : customerLayerMask.value;

        Collider[] hits = Physics.OverlapSphere(
            point.position,
            spawnCheckRadius,
            mask,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].GetComponentInParent<CustomerNPC>() != null)
            {
                return false;
            }
        }

        return true;
    }

    private void CriarPedidosPossiveis()
    {
        possibleOrders.Clear();

        if (activeConfig == null)
        {
            CriarPedidosPadrao();
        }
        else
        {
            CriarPedidosAPartirDoLevelConfig(activeConfig);
        }

        if (possibleOrders.Count == 0)
        {
            possibleOrders.Add(new BurgerOrder(
                "Burger simples",
                new List<string> { "Bread", "CookedMeat", "Bread" }
            ));
        }

        Debug.Log("Pedidos possiveis criados: " + possibleOrders.Count);
    }

    private void CriarPedidosPadrao()
    {
        AdicionarPedidoComVariacoes(new BurgerOrder(
            "Burger simples",
            new List<string> { "Bread", "CookedMeat", "Bread" }
        ));

        AdicionarPedidoComVariacoes(new BurgerOrder(
            "Cheeseburger",
            new List<string> { "Bread", "CookedMeat", "Cheese", "Bread" }
        ));

        AdicionarPedidoComVariacoes(new BurgerOrder(
            "Burger alface",
            new List<string> { "Bread", "CookedMeat", "Lettuce", "Bread" }
        ));

        AdicionarPedidoComVariacoes(new BurgerOrder(
            "Burger tomate",
            new List<string> { "Bread", "CookedMeat", "Tomato", "Bread" }
        ));

        AdicionarPedidoComVariacoes(new BurgerOrder(
            "Burger pimento",
            new List<string> { "Bread", "CookedMeat", "Pepper", "Bread" }
        ));

        AdicionarPedidoComVariacoes(new BurgerOrder(
            "Burger queijo e tomate",
            new List<string> { "Bread", "CookedMeat", "Cheese", "Tomato", "Bread" }
        ));

        AdicionarPedidoComVariacoes(new BurgerOrder(
            "Burger queijo e alface",
            new List<string> { "Bread", "CookedMeat", "Cheese", "Lettuce", "Bread" }
        ));

        AdicionarPedidoComVariacoes(new BurgerOrder(
            "Burger fresco",
            new List<string> { "Bread", "CookedMeat", "Lettuce", "Tomato", "Bread" }
        ));

        AdicionarPedidoComVariacoes(new BurgerOrder(
            "Burger picante",
            new List<string> { "Bread", "CookedMeat", "Cheese", "Pepper", "Bread" }
        ));
    }

    private void CriarPedidosAPartirDoLevelConfig(LevelConfig config)
    {
        List<string> extras = new List<string>();

        if (config.allowCheese)
            extras.Add("Cheese");

        if (config.allowLettuce)
            extras.Add("Lettuce");

        if (config.allowTomato)
            extras.Add("Tomato");

        if (config.allowPepper)
            extras.Add("Pepper");

        int maxExtras = Mathf.Clamp(config.maxExtraIngredients, 0, 3);

        AdicionarPedidoComVariacoes(CriarPedido("Burger simples"));

        if (maxExtras >= 1)
        {
            for (int i = 0; i < extras.Count; i++)
            {
                string extra = extras[i];
                string nome = "Burger " + NomeIngrediente(extra);

                AdicionarPedidoComVariacoes(CriarPedido(nome, extra));
            }
        }

        if (maxExtras >= 2)
        {
            for (int i = 0; i < extras.Count; i++)
            {
                for (int j = i + 1; j < extras.Count; j++)
                {
                    string extraA = extras[i];
                    string extraB = extras[j];

                    string nome = "Burger " + NomeIngrediente(extraA) + " e " + NomeIngrediente(extraB);

                    AdicionarPedidoComVariacoes(CriarPedido(nome, extraA, extraB));
                }
            }
        }

        if (maxExtras >= 3)
        {
            for (int i = 0; i < extras.Count; i++)
            {
                for (int j = i + 1; j < extras.Count; j++)
                {
                    for (int k = j + 1; k < extras.Count; k++)
                    {
                        string extraA = extras[i];
                        string extraB = extras[j];
                        string extraC = extras[k];

                        string nome = "Burger "
                            + NomeIngrediente(extraA)
                            + ", "
                            + NomeIngrediente(extraB)
                            + " e "
                            + NomeIngrediente(extraC);

                        AdicionarPedidoComVariacoes(CriarPedido(nome, extraA, extraB, extraC));
                    }
                }
            }
        }
    }

    private BurgerOrder CriarPedido(string nome, params string[] extras)
    {
        List<string> ingredientes = new List<string>();

        ingredientes.Add("Bread");
        ingredientes.Add("CookedMeat");

        if (extras != null)
        {
            for (int i = 0; i < extras.Length; i++)
            {
                ingredientes.Add(extras[i]);
            }
        }

        ingredientes.Add("Bread");

        return new BurgerOrder(nome, ingredientes);
    }

    private string NomeIngrediente(string ingredient)
    {
        switch (ingredient)
        {
            case "Cheese":
                return "queijo";

            case "Lettuce":
                return "alface";

            case "Tomato":
                return "tomate";

            case "Pepper":
                return "pimento";

            default:
                return ingredient;
        }
    }

    private void AdicionarPedidoComVariacoes(BurgerOrder baseOrder)
    {
        possibleOrders.Add(new BurgerOrder(
            baseOrder.orderName,
            new List<string>(baseOrder.ingredients),
            false,
            false
        ));

        bool allowFries = activeConfig == null || activeConfig.allowFries;
        bool allowDrink = activeConfig == null || activeConfig.allowDrink;

        if (allowFries)
        {
            possibleOrders.Add(new BurgerOrder(
                baseOrder.orderName + " com batatas",
                new List<string>(baseOrder.ingredients),
                true,
                false
            ));
        }

        if (allowDrink)
        {
            possibleOrders.Add(new BurgerOrder(
                baseOrder.orderName + " com bebida",
                new List<string>(baseOrder.ingredients),
                false,
                true
            ));
        }

        if (allowFries && allowDrink)
        {
            possibleOrders.Add(new BurgerOrder(
                "Menu " + baseOrder.orderName,
                new List<string>(baseOrder.ingredients),
                true,
                true
            ));
        }
    }

    public void StartSpawning()
    {
        if (isSpawning)
            return;

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnLoop());

        Debug.Log("clientes comecaram a aparecer");
    }

    public void StopSpawning()
    {
        if (!isSpawning)
            return;

        isSpawning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        Debug.Log("clientes pararam de aparecer");
    }

    private IEnumerator SpawnLoop()
    {
        while (isSpawning)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);

            yield return new WaitForSeconds(waitTime);

            TrySpawnCustomer();
        }
    }

    private void TrySpawnCustomer()
    {
        CustomerServicePoint freePoint = GetFreeServicePoint();

        if (freePoint == null)
        {
            Debug.Log("todos os lugares estao ocupados");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.Log("faltam spawn points");
            return;
        }

        if (exitPoints == null || exitPoints.Length == 0)
        {
            Debug.Log("faltam exit points");
            return;
        }

        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.Log("falta ligar os prefabs");
            return;
        }

        if (possibleOrders == null || possibleOrders.Count == 0)
        {
            CriarPedidosPossiveis();
        }

        if (possibleOrders == null || possibleOrders.Count == 0)
        {
            Debug.Log("nao existem pedidos possiveis");
            return;
        }

        GameObject selectedCustomerPrefab = GetRandomCustomerPrefab();

        if (selectedCustomerPrefab == null)
        {
            Debug.Log("prefab vazio");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        if (!IsSpawnPointFree(spawnPoint))
        {
            Debug.Log("spawn point ocupado");
            return;
        }

        Transform exitPoint = exitPoints[Random.Range(0, exitPoints.Length)];
        BurgerOrder baseOrder = possibleOrders[Random.Range(0, possibleOrders.Count)];

        // clona o pedido para cada cliente ter o seu proprio numero (possibleOrders e uma lista partilhada e reutilizada)
        // o numero do pedido e o numero do posto de servico (1, 2 ou 3), ha no maximo 3 pontos de servico
        BurgerOrder order = new BurgerOrder(
            baseOrder.orderName,
            new List<string>(baseOrder.ingredients),
            baseOrder.wantsFries,
            baseOrder.wantsDrink
        );
        order.orderNumber = freePoint.GetOrderSlotIndex() + 1;

        Debug.Log("PREFAB SELECIONADO: [" + selectedCustomerPrefab.name + "]");

        GameObject customerObject = Instantiate(
            selectedCustomerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        CustomerNPC customer = customerObject.GetComponent<CustomerNPC>();

        if (customer == null)
        {
            Debug.Log("sem CustomerNPC");
            Destroy(customerObject);
            return;
        }

        Debug.Log("IsTucano: " + customer.IsTucano() + " | IsArara: " + customer.IsArara() + " | IsGaivota: " + customer.IsGaivota() + " | IsCoruja: " + customer.IsCoruja());

        if (tucanoAnimationPrefab != null && customer.IsTucano())
        {
            GameObject tucanoObject = Instantiate(
                tucanoAnimationPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            tucanoObject.transform.SetParent(customerObject.transform);
            tucanoObject.transform.localPosition = Vector3.zero;
            tucanoObject.transform.localScale = Vector3.one;

            Camera tucanoCamera = tucanoObject.GetComponentInChildren<Camera>();
            if (tucanoCamera != null)
            {
                tucanoCamera.gameObject.SetActive(false);
            }

            Light tucanoLight = tucanoObject.GetComponentInChildren<Light>();
            if (tucanoLight != null)
            {
                tucanoLight.gameObject.SetActive(false);
            }

            Animator anim = tucanoObject.GetComponentInChildren<Animator>();
            Debug.Log("TUCANO ANIM OBJECT: " + (anim == null ? "NULL" : anim.gameObject.name));

            customer.SetTucanoAnimator(anim);
            customer.SetBirdVisualTransform(tucanoObject.transform);
        }
        else if (araraAnimationPrefab != null && customer.IsArara())
        {
            GameObject araraObject = Instantiate(
                araraAnimationPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            araraObject.transform.SetParent(customerObject.transform);
            araraObject.transform.localPosition = Vector3.zero;
            araraObject.transform.localScale = Vector3.one;

            Camera araraCamera = araraObject.GetComponentInChildren<Camera>();
            if (araraCamera != null)
            {
                araraCamera.gameObject.SetActive(false);
            }

            Light araraLight = araraObject.GetComponentInChildren<Light>();
            if (araraLight != null)
            {
                araraLight.gameObject.SetActive(false);
            }

            Animator anim = araraObject.GetComponentInChildren<Animator>();
            Debug.Log("ARARA ANIM OBJECT: " + (anim == null ? "NULL" : anim.gameObject.name));

            customer.SetAraraAnimator(anim);
            customer.SetBirdVisualTransform(araraObject.transform);
        }
        else if (gaivotaAnimationPrefab != null && customer.IsGaivota())
        {
            GameObject gaivotaObject = Instantiate(
                gaivotaAnimationPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            gaivotaObject.transform.SetParent(customerObject.transform);
            gaivotaObject.transform.localPosition = Vector3.zero;
            gaivotaObject.transform.localScale = Vector3.one;

            Camera gaivotaCamera = gaivotaObject.GetComponentInChildren<Camera>();
            if (gaivotaCamera != null)
            {
                gaivotaCamera.gameObject.SetActive(false);
            }

            Light gaivotaLight = gaivotaObject.GetComponentInChildren<Light>();
            if (gaivotaLight != null)
            {
                gaivotaLight.gameObject.SetActive(false);
            }

            Animator anim = gaivotaObject.GetComponentInChildren<Animator>();
            Debug.Log("GAIVOTA ANIM OBJECT: " + (anim == null ? "NULL" : anim.gameObject.name));

            customer.SetGaivotaAnimator(anim);
            customer.SetBirdVisualTransform(gaivotaObject.transform);
        }
        else if (corujaAnimationPrefab != null && customer.IsCoruja())
        {
            GameObject corujaObject = Instantiate(
                corujaAnimationPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            corujaObject.transform.SetParent(customerObject.transform);
            corujaObject.transform.localPosition = Vector3.zero;
            corujaObject.transform.localScale = Vector3.one;

            Camera corujaCamera = corujaObject.GetComponentInChildren<Camera>();
            if (corujaCamera != null)
            {
                corujaCamera.gameObject.SetActive(false);
            }

            Light corujaLight = corujaObject.GetComponentInChildren<Light>();
            if (corujaLight != null)
            {
                corujaLight.gameObject.SetActive(false);
            }

            Animator anim = corujaObject.GetComponentInChildren<Animator>();
            Debug.Log("CORUJA ANIM OBJECT: " + (anim == null ? "NULL" : anim.gameObject.name));

            customer.SetCorujaAnimator(anim);
            customer.SetBirdVisualTransform(corujaObject.transform);
        }

        freePoint.SetCustomer(customer);

        customer.SetupCustomer(
            freePoint.transform,
            exitPoint,
            order,
            freePoint,
            customerPatienceTime,
            this
        );
    }

    private GameObject GetRandomCustomerPrefab()
    {
        int randomIndex = Random.Range(0, customerPrefabs.Length);

        return customerPrefabs[randomIndex];
    }

    private CustomerServicePoint GetFreeServicePoint()
    {
        for (int i = 0; i < servicePoints.Length; i++)
        {
            if (servicePoints[i] != null && servicePoints[i].IsFree())
            {
                return servicePoints[i];
            }
        }

        return null;
    }

    public bool TryServeBurgerToCustomer(List<string> burger)
    {
        if (burger == null || burger.Count == 0)
        {
            Debug.Log("burger vazio");
            return false;
        }

        Debug.Log("tentando entregar hamburger: " + BurgerToText(burger));

        for (int i = 0; i < servicePoints.Length; i++)
        {
            if (servicePoints[i] == null)
                continue;

            CustomerNPC customer = servicePoints[i].GetCurrentCustomer();

            if (customer == null)
            {
                Debug.Log("service point " + i + " sem cliente");
                continue;
            }

            BurgerOrder order = customer.GetCurrentOrder();
            string orderText = order != null ? order.GetOrderText() : "sem pedido";

            Debug.Log("service point " + i + " tem cliente com pedido: " + orderText);

            if (customer.CanReceiveBurger(burger))
            {
                customer.ReceiveCorrectBurger(burger);
                return true;
            }
        }

        Debug.Log("nenhum cliente quer este hamburger");
        return false;
    }

    public bool TryServeTrayToCustomer(MealTray tray)
    {
        if (tray == null || !tray.HasBurger())
        {
            Debug.Log("tabuleiro vazio ou sem hamburger");
            return false;
        }

        Debug.Log("tentando entregar tabuleiro: " + TrayToText(tray));

        for (int i = 0; i < servicePoints.Length; i++)
        {
            if (servicePoints[i] == null)
                continue;

            CustomerNPC customer = servicePoints[i].GetCurrentCustomer();

            if (customer == null)
            {
                Debug.Log("service point " + i + " sem cliente");
                continue;
            }

            BurgerOrder order = customer.GetCurrentOrder();
            string orderText = order != null ? order.GetOrderText() : "sem pedido";

            Debug.Log("service point " + i + " tem cliente com pedido: " + orderText);

            if (customer.CanReceiveTray(tray))
            {
                customer.ReceiveCorrectTray(tray);
                return true;
            }
        }

        Debug.Log("nenhum cliente quer este tabuleiro");
        return false;
    }

    private string BurgerToText(List<string> burger)
    {
        string text = "";

        for (int i = 0; i < burger.Count; i++)
        {
            text += burger[i];

            if (i < burger.Count - 1)
            {
                text += " + ";
            }
        }

        return text;
    }

    private string TrayToText(MealTray tray)
    {
        string text = BurgerToText(tray.GetBurgerCopy());

        if (tray.hasFries)
            text += " + Batatas";

        if (tray.hasDrink)
            text += " + Bebida";

        return text;
    }

    public void NotifyCustomerLeftImpatient(CustomerNPC customer)
    {
        if (dayManager != null && missedCustomerPenalty > 0)
        {
            dayManager.AddScore(-missedCustomerPenalty);
        }

        Debug.Log("cliente perdeu a paciencia e foi embora");
    }
}