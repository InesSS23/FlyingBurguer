using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : MonoBehaviour, IInteractable
{
    [Header("mesa de montagem")]
    [SerializeField] private AssemblyTable assemblyTable;

    public void Interact()
    {
        if (assemblyTable == null)
        {
            Debug.Log("n tenho a mesa de montagem ligada no delivery");
            return;
        }

        List<string> burger = assemblyTable.GetBurger();

        if (burger == null || burger.Count == 0)
        {
            Debug.Log("n tens hamburger para entregar");
            return;
        }

        string burgerText = "hamburger entregue: ";

        for (int i = 0; i < burger.Count; i++)
        {
            burgerText += burger[i];

            if (i < burger.Count - 1)
            {
                burgerText += " + ";
            }
        }

        Debug.Log(burgerText);

        // por agora limpa a mesa depois de entregar
        assemblyTable.ClearBurger();
    }
}