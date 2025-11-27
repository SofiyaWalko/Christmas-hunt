using UnityEngine;

public class StatPickup : MonoBehaviour, IInteractable
{
    public ItemData statItem;

    public string GetInteractText()
    {
        if (statItem == null) return null;
        return "Взять " + statItem.itemName;
    }

    public void Interact()
    {
        if (statItem == null) return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("Нет InventoryManager в сцене.");
            return;
        }

        // Добавляем только в stat слот (AddStat сам добавит копию в основной инвентарь)
        bool addedToStat = InventoryManager.Instance.AddStat(statItem);
        Debug.Log($"StatPickup: AddStat returned {addedToStat}");
        if (addedToStat)
        {
            Destroy(gameObject);
        }
    }
}
