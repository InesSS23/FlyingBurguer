using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [Header("prefabs dos clientes")]
    [SerializeField] private GameObject[] customerPrefabs;

    [Header("prefab animacao tucano")]
    [SerializeField] private GameObject tucanoAnimationPrefab;

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
            StartSpawning();
    }

    private bool IsSpawnPointFree(Transform point)
    {
        int mask = customerLayerMask.value == 0 ? ~0 : customerLayerMask.value;
        Collider[] hits = Physics.OverlapSphere(point.position, spawnCheckRadius, mask, QueryTriggerInteraction.Collide);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].GetComponentInParent<CustomerNPC>() != null)
                return false;
        }

        return true;
    }

    private void CriarPedidosPossiveis()
    {
        possibleOrders.Clear();

        possibleOrders.Add(new BurgerOrder("Burger simples", new List<string> { "Bread", "CookedMeat", "Bread" }));
        possibleOrders.Add(new BurgerOrder("Cheeseburger", new List<string> { "Bread", "CookedMeat", "Cheese", "Bread" }));
        possibleOrders.Add(new BurgerOrder("Burger alface", new List<string> { "Bread", "CookedMeat", "Lettuce", "Bread" }));
        possibleOrders.Add(new BurgerOrder("Burger completo", new List<string> { "Bread", "CookedMeat", "Cheese", "Lettuce", "Bread" }));
        possibleOrders.Add(new BurgerOrder("Burger tomate", new List<string> { "Bread", "CookedMeat", "Tomato", "Bread" }));
        possibleOrders.Add(new BurgerOrder("Burger picante", new List<string> { "Bread", "CookedMeat", "Cheese", "Pepper", "Bread" }));
        possibleOrders.Add(new BurgerOrder("Burger especial", new List<string> { "Bread", "CookedMeat", "Cheese", "Lettuce", "Tomato", "Pepper", "Bread" }));
    }

    public void StartSpawning()
    {
        if (isSpawning) return;
        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnLoop());
        Debug.Log("clientes começaram a aparecer");
    }

    public void StopSpawning()
    {
        if (!isSpawning) return;
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
        if (freePoint == null) { Debug.Log("todos os lugares estao ocupados"); return; }
        if (spawnPoints == null || spawnPoints.Length == 0) { Debug.Log("faltam spawn points"); return; }
        if (exitPoints == null || exitPoints.Length == 0) { Debug.Log("faltam exit points"); return; }
        if (customerPrefabs == null || customerPrefabs.Length == 0) { Debug.Log("falta ligar os prefabs"); return; }

        GameObject selectedCustomerPrefab = GetRandomCustomerPrefab();
        if (selectedCustomerPrefab == null) { Debug.Log("prefab vazio"); return; }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        if (!IsSpawnPointFree(spawnPoint)) { Debug.Log("spawn point ocupado"); return; }

        Transform exitPoint = exitPoints[Random.Range(0, exitPoints.Length)];
        BurgerOrder order = possibleOrders[Random.Range(0, possibleOrders.Count)];

        Debug.Log("PREFAB SELECIONADO: [" + selectedCustomerPrefab.name + "]");

        GameObject customerObject = Instantiate(selectedCustomerPrefab, spawnPoint.position, spawnPoint.rotation);
        CustomerNPC customer = customerObject.GetComponent<CustomerNPC>();
        if (customer == null) { Debug.Log("sem CustomerNPC"); Destroy(customerObject); return; }

        Debug.Log("IsTucano: " + customer.IsTucano());
        if (tucanoAnimationPrefab != null && customer.IsTucano())
        {
            GameObject tucanoObject = Instantiate(tucanoAnimationPrefab, spawnPoint.position, spawnPoint.rotation);
            tucanoObject.transform.SetParent(customerObject.transform);
            tucanoObject.transform.localPosition = Vector3.zero;
            tucanoObject.transform.localScale = Vector3.one;

            Camera tucanoCamera = tucanoObject.GetComponentInChildren<Camera>();
            if (tucanoCamera != null) tucanoCamera.gameObject.SetActive(false);

            Light tucanoLight = tucanoObject.GetComponentInChildren<Light>();
            if (tucanoLight != null) tucanoLight.gameObject.SetActive(false);

            Animator anim = tucanoObject.GetComponentInChildren<Animator>();
            Debug.Log("TUCANO ANIM OBJECT: " + (anim == null ? "NULL" : anim.gameObject.name));
            customer.SetTucanoAnimator(anim);
        }

        freePoint.SetCustomer(customer);
        customer.SetupCustomer(freePoint.transform, exitPoint, order, freePoint, customerPatienceTime, this);
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
                return servicePoints[i];
        }
        return null;
    }

    public bool TryServeBurgerToCustomer(List<string> burger)
    {
        if (burger == null || burger.Count == 0) { Debug.Log("burger vazio"); return false; }

        for (int i = 0; i < servicePoints.Length; i++)
        {
            if (servicePoints[i] == null) continue;

            CustomerNPC customer = servicePoints[i].GetCurrentCustomer();
            if (customer == null) continue;

            if (customer.CanReceiveBurger(burger))
            {
                customer.ReceiveCorrectBurger(burger);
                return true;
            }
        }

        Debug.Log("nenhum cliente quer este hamburger");
        return false;
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
