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
        //gameObject.SetActive(false);
        Destroy(gameObject);
    }
}

}
