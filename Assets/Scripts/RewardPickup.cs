using UnityEngine;

public class RewardPickup : PickupBase
{
    public ItemData rewardItem;

    protected override void Reset()
    {
        base.Reset();
        // Попробуем сделать коллайдер триггером по умолчанию
        var col = GetComponent<Collider>();
        if (col == null)
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
        else
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (rewardItem == null)
            return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("Нет InventoryManager в сцене.");
            return;
        }

        bool added = InventoryManager.Instance.AddReward(rewardItem);

        if (added)
        {
            Debug.Log($"Награда {rewardItem.itemName} подобрана!");
            MarkAsCollected();
            Destroy(gameObject);
        }
    }
}
