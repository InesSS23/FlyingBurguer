using UnityEngine;

public class GrillStation : MonoBehaviour, IInteractable
{
    [Header("configuração")]
    [SerializeField] private string rawMeatName = "RawMeat";
    [SerializeField] private string cookedMeatName = "CookedMeat";

    public void Interact()
    {
        PlayerHand playerHand = FindFirstObjectByType<PlayerHand>();

        if (playerHand == null)
        {
            Debug.Log("n encontrei a mao do player");
            return;
        }

        if (!playerHand.HasItem())
        {
            Debug.Log("n tens nada na mao para cozinhar");
            return;
        }

        string item = playerHand.GetCurrentItem();

        if (item != rawMeatName)
        {
            Debug.Log("isto n pode ir para a grelha: " + item);
            return;
        }

        playerHand.ClearItem();
        playerHand.TrySetItem(cookedMeatName);

        Debug.Log("carne cozinhada: " + cookedMeatName);
    }
}