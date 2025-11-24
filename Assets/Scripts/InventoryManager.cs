using System;
using System.Collections.Generic;
using UnityEngine;


public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    public event Action OnInventoryChanged; 


    public List<InventorySlot> slots = new List<InventorySlot>();
    public int inventorySize = 12;


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

        
        Debug.Log("Инвентарь полон!");
        return false;
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
