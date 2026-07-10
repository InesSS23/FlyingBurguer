using UnityEngine;

public class TrashBin : MonoBehaviour, IInteractable
{
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
            Debug.Log("n tens nada para deitar fora");
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTrashSFX();
        }

        playerHand.ClearHand();

        Debug.Log("deitei fora o que tinha na mao");
    }
}