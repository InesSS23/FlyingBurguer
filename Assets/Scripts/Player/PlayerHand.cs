using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [Header("item atual")]
    [SerializeField] private string currentItem = "";

    public bool HasItem()
    {
        return currentItem != "";
    }

    public string GetCurrentItem()
    {
        return currentItem;
    }

    public bool TrySetItem(string itemName)
    {
        // se ja tenho alguma coisa na mao, n apanho outra
        if (HasItem())
        {
            Debug.Log("ja tenho algo na mao: " + currentItem);
            return false;
        }

        currentItem = itemName;
        Debug.Log("agora tenho na mao: " + currentItem);
        return true;
    }

    public void ClearItem()
    {
        Debug.Log("usei/larguei o item: " + currentItem);
        currentItem = "";
    }
}