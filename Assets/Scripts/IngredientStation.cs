using UnityEngine;

public class IngredientStation : MonoBehaviour, IInteractable
{
    [Header("ingrediente desta caixa")]
    [SerializeField] private string ingredientName;

    public void Interact()
    {
        // por agora é só teste
        Debug.Log("apanhei ingrediente: " + ingredientName);
    }
}