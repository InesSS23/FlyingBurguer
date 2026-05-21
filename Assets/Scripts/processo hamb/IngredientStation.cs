using UnityEngine;

public class IngredientStation : MonoBehaviour, IInteractable
{
    [Header("ingrediente desta estação")]
    [SerializeField] private string ingredientName;

    public void Interact()
    {
        PlayerHand playerHand = FindFirstObjectByType<PlayerHand>();

        if (playerHand == null)
        {
            Debug.Log("n encontrei a mao do player");
            return;
        }

        playerHand.TrySetItem(ingredientName);
    }
}