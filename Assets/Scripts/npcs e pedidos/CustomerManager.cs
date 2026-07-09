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
    [SerializeField] private Vector3 corujaLocalPositionOffset = Vector3.zero;

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

    void Start()
    {
        CriarPedidosPossiveis();

        if (startSpawningAutomatically)
        {
            StartSpawning();
        }
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

        AdicionarPedidoComVariacoes(new BurgerOrder(
            "Burger vegetariano falso",
            new List<string> { "Bread", "CookedMeat", "Lettuce", "Pepper", "Bread" }
        ));

        AdicionarPedidoComVariacoes(new BurgerOrder(
            "Burger completo",
            new List<string> { "Bread", "CookedMeat", "Cheese", "Lettuce", "Bread" }
        ));

        AdicionarPedidoComVariacoes(new BurgerOrder(
            "Burger especial voador",
            new List<string> { "Bread", "CookedMeat", "Cheese", "Pepper", "Bread" }
        ));
    }

    private void AdicionarPedidoComVariacoes(BurgerOrder baseOrder)
    {
        possibleOrders.Add(baseOrder);

        possibleOrders.Add(new BurgerOrder(
            baseOrder.orderName + " com batatas",
            baseOrder.ingredients,
            true,
            false
        ));

        possibleOrders.Add(new BurgerOrder(
            baseOrder.orderName + " com bebida",
            baseOrder.ingredients,
            false,
            true
        ));

        possibleOrders.Add(new BurgerOrder(
            "Menu " + baseOrder.orderName,
            baseOrder.ingredients,
            true,
            true
        ));
    }

    public void StartSpawning()
    {
        if (isSpawning)
            return;

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnLoop());

        Debug.Log("clientes começaram a aparecer");
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
        BurgerOrder order = possibleOrders[Random.Range(0, possibleOrders.Count)];

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
            customer.SetVisualsActive(false, tucanoObject.transform);
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
            customer.SetVisualsActive(false, araraObject.transform);
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
            customer.SetVisualsActive(false, gaivotaObject.transform);
        }
        
        
        else if (corujaAnimationPrefab != null && customer.IsCoruja())
        {
            GameObject corujaObject = Instantiate(
                corujaAnimationPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            corujaObject.transform.SetParent(customerObject.transform);
            corujaObject.transform.localPosition = corujaLocalPositionOffset;

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
            customer.SetVisualsActive(false, corujaObject.transform);
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
