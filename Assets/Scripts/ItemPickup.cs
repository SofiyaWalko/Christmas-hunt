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

                if (InventoryUIController.Instance != null)
                {
                    InventoryUIController.Instance.ShowNotification(
                        "Вы разблокировали двойной прыжок (двойное нажатие на пробел) и можете бросаться снежками (нажатие на ЛКМ)!",
                        $"Вы подобрали {itemData.itemName}!",
                        10f
                    );
                }

                StartCoroutine(ShowVictoryNotificationDelayed());
                return;
            }
        }

        if (itemData.itemName == "Последний подарок")
        {
            StartCoroutine(LoadMainMenuDelayed());
            return;
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

        yield return new WaitForSeconds(3f);
        InventoryUIController.Instance.ShowVictoryNotification(
            "Ура вы прошли первый уровень!",
            "Победа!",
            "Location_2"
        );
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator LoadMainMenuDelayed()
    {
        MarkAsCollected();
        GetComponent<Collider>().enabled = false;
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = false;

        if (InventoryUIController.Instance != null)
        {
            InventoryUIController.Instance.ShowNotification("Ура вы победили", "Победа!");
        }

        yield return new WaitForSeconds(3f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
