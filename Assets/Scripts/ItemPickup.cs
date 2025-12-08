using UnityEngine;

public class ItemPickup : PickupBase, IInteractable
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
                StartCoroutine(ShowVictoryNotificationDelayed());
                return;
            }
        }

        if (itemData.itemName == "Последний подарок")
        {
            InventoryUIController.Instance.ShowNotification("Ура вы победили", "Победа!");
        }
        
        MarkAsCollected();
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator ShowVictoryNotificationDelayed()
    {
        MarkAsCollected();
        GetComponent<Collider>().enabled = false;
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = false;

        yield return new WaitForSeconds(5f);
        InventoryUIController.Instance.ShowVictoryNotification(
            "Ура вы прошли первый уровень!",
            "Победа!",
            "Location_1"
        );
        Destroy(gameObject);
    }
}
