using UnityEngine;

public class IngredientStation : MonoBehaviour, IInteractable
{
    [Header("ingrediente desta esta��o")]
    [SerializeField] private string ingredientName;

    private LevelConfig levelConfig;

    private void Start()
    {
        DayManager dayManager = FindFirstObjectByType<DayManager>();

        if (dayManager != null)
        {
            levelConfig = dayManager.GetLevelConfig();
        }
    }

    private bool IsUnlocked()
    {
        if (levelConfig == null)
            return true;

        switch (ingredientName)
        {
            case "Tomato":
                return levelConfig.allowTomato;
            case "Pepper":
                return levelConfig.allowPepper;
            case "FrozenFries":
                return levelConfig.allowFries;
            case "EmptyCup":
                return levelConfig.allowDrink;
            default:
                return true;
        }
    }

    public void Interact()
    {
        if (!IsUnlocked())
        {
            if (GameplayHUDPolish.Instance != null)
            {
                GameplayHUDPolish.Instance.ShowFeedback("Ainda não desbloqueaste este ingrediente.");
            }

            return;
        }

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