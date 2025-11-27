using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public event Action OnInventoryChanged;

    public List<InventorySlot> slots = new List<InventorySlot>();
    public int inventorySize = 12;

    // Специальный слот для подарка (всегда один)
    public InventorySlot rewardSlot;

    // Специальный слот для stat (всегда один)
    public InventorySlot statSlot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public bool AddItem(ItemData item)
    {
        if (item.isStackable)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.item == item && slot.quantity < item.maxStackSize)
                {
                    slot.AddQuantity(1);
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        if (slots.Count < inventorySize)
        {
            slots.Add(new InventorySlot(item, 1));
            OnInventoryChanged?.Invoke();
            return true;
        }

        // Инвентарь полон
        return false;
    }

    // Добавляет предмет в специальный слот reward. Возвращает true если успешно.
    public bool AddReward(ItemData item)
    {
        if (rewardSlot != null && rewardSlot.item == item)
        {
            // Увеличиваем количество в reward слоте вне зависимости от isStackable
            rewardSlot.AddQuantity(1);
            OnInventoryChanged?.Invoke();
            return true;
        }

        // если слот пуст — создаём
        if (rewardSlot == null)
        {
            rewardSlot = new InventorySlot(item, 1);
            OnInventoryChanged?.Invoke();
            return true;
        }

        return false;
    }

    // Удаляет/забирает предмет из reward слота
    public void TakeReward()
    {
        rewardSlot = null;
        OnInventoryChanged?.Invoke();
    }

    public bool HasReward()
    {
        return rewardSlot != null;
    }

    // Добавляет предмет в специальный stat слот. Также пытается добавить копию в общий инвентарь.
    public bool AddStat(ItemData item)
    {
        if (statSlot != null)
        {
            // statSlot exists — debug info removed
        }

        // Сначала попытка добавить в statSlot (если тот же предмет — инкремент)
        if (statSlot != null && statSlot.item == item)
        {
            // Увеличиваем количество в stat слоте вне зависимости от isStackable
            statSlot.AddQuantity(1);
            OnInventoryChanged?.Invoke();
            // также добавим в общий инвентарь
            AddItem(item);
            return true;
        }
        if (statSlot == null)
        {
            statSlot = new InventorySlot(item, 1);
            // created new statSlot
            OnInventoryChanged?.Invoke();
            AddItem(item);
            return true;
        }

        return false;
    }

    public void TakeStat()
    {
        statSlot = null;
        OnInventoryChanged?.Invoke();
    }

    public bool HasStat()
    {
        return statSlot != null;
    }

    public void RemoveItem(ItemData item)
    {
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i].item == item)
            {
                slots[i].RemoveQuantity(1);

                if (slots[i].quantity <= 0)
                {
                    slots.RemoveAt(i);
                }

                OnInventoryChanged?.Invoke();

                return;
            }
        }

        Debug.LogWarning("Попытка удалить предмет, которого нет в инвентаре: " + item.name);
    }
}
