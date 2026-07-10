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

        if (playerHand.IsEmpty())
        {
            playerHand.TrySetItem(ingredientName);
            return;
        }

        if (playerHand.HasItem() && playerHand.GetCurrentItem() == ingredientName)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPickupObjectSFX();
            }

            Debug.Log("devolvi o ingrediente ao sitio: " + ingredientName);
            playerHand.ClearHand();
            return;
        }

        Debug.Log("ja tens outra coisa na mao");
    }
}