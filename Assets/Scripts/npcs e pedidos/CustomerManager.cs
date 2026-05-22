using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [Header("prefabs dos clientes")]
    [SerializeField] private GameObject[] customerPrefabs;

    [Header("pontos")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private CustomerServicePoint[] servicePoints;
    [SerializeField] private Transform[] exitPoints;

    [Header("tempo de spawn")]
    [SerializeField] private float minSpawnTime = 8f;
    [SerializeField] private float maxSpawnTime = 15f;

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

    private void CriarPedidosPossiveis()
    {
        possibleOrders.Add(new BurgerOrder("Burger simples", new List<string>
        {
            "Bread", "CookedMeat", "Bread"
        }));

        possibleOrders.Add(new BurgerOrder("Cheeseburger", new List<string>
        {
            "Bread", "CookedMeat", "Cheese", "Bread"
        }));

        possibleOrders.Add(new BurgerOrder("Burger alface", new List<string>
        {
            "Bread", "CookedMeat", "Lettuce", "Bread"
        }));

        possibleOrders.Add(new BurgerOrder("Burger completo", new List<string>
        {
            "Bread", "CookedMeat", "Cheese", "Lettuce", "Bread"
        }));
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
            Debug.Log("todos os lugares estao ocupados, n spawna cliente");
            return;
        }

        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.Log("falta ligar os prefabs dos clientes");
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

        GameObject selectedCustomerPrefab = GetRandomCustomerPrefab();

        if (selectedCustomerPrefab == null)
        {
            Debug.Log("algum prefab de cliente está vazio no array");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Transform exitPoint = exitPoints[Random.Range(0, exitPoints.Length)];
        BurgerOrder order = possibleOrders[Random.Range(0, possibleOrders.Count)];

        GameObject customerObject = Instantiate(
            selectedCustomerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        CustomerNPC customer = customerObject.GetComponent<CustomerNPC>();

        if (customer == null)
        {
            Debug.Log("o prefab do cliente n tem CustomerNPC");
            Destroy(customerObject);
            return;
        }

        freePoint.SetCustomer(customer);
        customer.SetupCustomer(freePoint.transform, exitPoint, order, freePoint);

        OrderHUDManager hud = FindFirstObjectByType<OrderHUDManager>();

        if (hud != null)
        {
            hud.SetOrder(freePoint.GetOrderSlotIndex(), order);
        }
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

        for (int i = 0; i < servicePoints.Length; i++)
        {
            if (servicePoints[i] == null)
                continue;

            CustomerNPC customer = servicePoints[i].GetCurrentCustomer();

            if (customer == null)
                continue;

            if (customer.CanReceiveBurger(burger))
            {
                customer.ReceiveCorrectBurger(burger);
                return true;
            }
        }

        Debug.Log("nenhum cliente quer este hamburger");
        return false;
    }
}