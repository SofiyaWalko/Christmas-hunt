using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    public ItemData itemData;

    public string GetInteractText()
    {
        return "Подобрать " + itemData.itemName;
    }

    public void Interact()
    {
        // Обращаемся к синглтону InventoryManager
        bool wasPickedUp = InventoryManager.Instance.AddItem(itemData);

        if (wasPickedUp)
        {
            if (itemData.itemName == "Главный подарок")
            {
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    player.UnlockDoubleJump();
                }
                else
                    Debug.LogWarning("PlayerController не найден в сцене.");
            }
        }

        if (itemData.itemName == "Последний подарок")
        {
            InventoryUIController.Instance.ShowNotification("Ура вы победили", "Победа!");
        }
        Destroy(gameObject);
    }
}
