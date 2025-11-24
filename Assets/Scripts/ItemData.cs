using UnityEngine;

// Атрибут CreateAssetMenu позволяет создавать экземпляры этого объекта прямо в редакторе
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string itemName;
    public string description;
    public Sprite icon;
    
    [Header("Settings")]
    public bool isStackable = false;
    public int maxStackSize = 1;
}
