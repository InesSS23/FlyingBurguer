using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [Header("prefab")]
    [SerializeField] private GameObject customerPrefab;

    [Header("pontos")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private CustomerServicePoint[] servicePoints;
    [SerializeField] private Transform[] exitPoints;

    [Header("tempo de spawn")]
    [SerializeField] private float minSpawnTime = 8f;
    [SerializeField] private float maxSpawnTime = 15f;

    private List<BurgerOrder> possibleOrders = new List<BurgerOrder>();

    void Start()
    {
        CriarPedidosPossiveis();
        StartCoroutine(SpawnLoop());
    }

    private void CriarPedidosPossiveis()
    {
        possibleOrders.Add(new BurgerOrder("Burger simples", new List<string> { "Bread", "CookedMeat" }));
        possibleOrders.Add(new BurgerOrder("Cheeseburger", new List<string> { "Bread", "CookedMeat", "Cheese" }));
        possibleOrders.Add(new BurgerOrder("Burger alface", new List<string> { "Bread", "CookedMeat", "Lettuce" }));
        possibleOrders.Add(new BurgerOrder("Burger completo", new List<string> { "Bread", "CookedMeat", "Cheese", "Lettuce" }));
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
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

        if (customerPrefab == null)
        {
            Debug.Log("falta ligar customer prefab");
            return;
        }

        if (spawnPoints.Length == 0 || exitPoints.Length == 0)
        {
            Debug.Log("faltam spawn points ou exit points");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Transform exitPoint = exitPoints[Random.Range(0, exitPoints.Length)];
        BurgerOrder order = possibleOrders[Random.Range(0, possibleOrders.Count)];

        GameObject customerObject = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);

        CustomerNPC customer = customerObject.GetComponent<CustomerNPC>();

        if (customer == null)
        {
            Debug.Log("o prefab do cliente n tem CustomerNPC");
            Destroy(customerObject);
            return;
        }

        freePoint.SetCustomer(customer);
        customer.SetupCustomer(freePoint.transform, exitPoint, order, freePoint);
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