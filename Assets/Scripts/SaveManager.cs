using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class InventoryItemSaveData
{
    public string itemName;
    public int quantity;
}

[System.Serializable]
public class SaveData
{
    public string saveName;
    public string timestamp;
    public string sceneName;
    
    public float positionX;
    public float positionY;
    public float positionZ;
    
    public int currentHealth;
    public float currentStamina;
    public bool doubleJumpUnlocked;
    
    public List<InventoryItemSaveData> inventoryItems = new List<InventoryItemSaveData>();
    public InventoryItemSaveData rewardItem;
    public InventoryItemSaveData statItem;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();
        
        // 1. Player Stats & Position
        PlayerController player = FindObjectOfType<PlayerController>();
        CharacterStats stats = FindObjectOfType<CharacterStats>();
        
        if (player != null)
        {
            data.positionX = player.transform.position.x;
            data.positionY = player.transform.position.y;
            data.positionZ = player.transform.position.z;
            data.doubleJumpUnlocked = player.IsDoubleJumpUnlocked();
        }
        
        if (stats != null)
        {
            data.currentHealth = stats.currentHealth;
            data.currentStamina = stats.currentStamina;
        }

        data.sceneName = SceneManager.GetActiveScene().name;
        data.timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.saveName = "Save_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 2. Inventory
        if (InventoryManager.Instance != null)
        {
            foreach (var slot in InventoryManager.Instance.slots)
            {
                if (slot.item != null)
                {
                    data.inventoryItems.Add(new InventoryItemSaveData 
                    { 
                        itemName = slot.item.itemName, 
                        quantity = slot.quantity 
                    });
                }
            }

            if (InventoryManager.Instance.rewardSlot != null && InventoryManager.Instance.rewardSlot.item != null)
            {
                data.rewardItem = new InventoryItemSaveData
                {
                    itemName = InventoryManager.Instance.rewardSlot.item.itemName,
                    quantity = InventoryManager.Instance.rewardSlot.quantity
                };
            }

            if (InventoryManager.Instance.statSlot != null && InventoryManager.Instance.statSlot.item != null)
            {
                data.statItem = new InventoryItemSaveData
                {
                    itemName = InventoryManager.Instance.statSlot.item.itemName,
                    quantity = InventoryManager.Instance.statSlot.quantity
                };
            }
        }

        // Serialize and Write
        string json = JsonUtility.ToJson(data, true);
        string filename = data.saveName + ".json";
        string path = Path.Combine(Application.persistentDataPath, filename);
        
        File.WriteAllText(path, json);
        Debug.Log("Game Saved to: " + path);
    }

    public void LoadGame(string saveFileName)
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        if (!File.Exists(path))
        {
            Debug.LogError("Save file not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        StartCoroutine(LoadGameRoutine(data));
    }

    private IEnumerator LoadGameRoutine(SaveData data)
    {
        // 1. Load Scene
        if (SceneManager.GetActiveScene().name != data.sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(data.sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        // Wait a frame to ensure Start methods run
        yield return null;

        // 2. Restore Player State
        PlayerController player = FindObjectOfType<PlayerController>();
        CharacterStats stats = FindObjectOfType<CharacterStats>();

        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            
            player.transform.position = new Vector3(data.positionX, data.positionY, data.positionZ);
            player.SetDoubleJumpUnlocked(data.doubleJumpUnlocked);
            
            if (cc != null) cc.enabled = true;
        }

        if (stats != null)
        {
            stats.SetHealth(data.currentHealth);
            stats.SetStamina(data.currentStamina);
        }

        // 3. Restore Inventory
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearInventory();

            foreach (var itemData in data.inventoryItems)
            {
                ItemData item = InventoryManager.Instance.GetItemByName(itemData.itemName);
                if (item != null)
                {
                    InventoryManager.Instance.AddItem(item, itemData.quantity);
                }
                else
                {
                    Debug.LogWarning("Item not found in database: " + itemData.itemName);
                }
            }

            if (data.rewardItem != null && !string.IsNullOrEmpty(data.rewardItem.itemName))
            {
                ItemData item = InventoryManager.Instance.GetItemByName(data.rewardItem.itemName);
                if (item != null)
                {
                    InventoryManager.Instance.SetRewardSlot(item, data.rewardItem.quantity);
                }
            }

            if (data.statItem != null && !string.IsNullOrEmpty(data.statItem.itemName))
            {
                ItemData item = InventoryManager.Instance.GetItemByName(data.statItem.itemName);
                if (item != null)
                {
                    InventoryManager.Instance.SetStatSlot(item, data.statItem.quantity);
                }
            }
        }
        
        Debug.Log("Game Loaded!");
    }
    
    public List<string> GetAllSaveFiles()
    {
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.json");
        List<string> fileNames = new List<string>();
        foreach (string path in filePaths)
        {
            fileNames.Add(Path.GetFileName(path));
        }
        return fileNames;
    }

    public void DeleteSave(string saveFileName)
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted save: " + path);
        }
    }

    public void DeleteAllSaves()
    {
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.json");
        foreach (string path in filePaths)
        {
            File.Delete(path);
        }
        Debug.Log("All saves deleted.");
    }

    public SaveData GetSaveData(string saveFileName)
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        if (!File.Exists(path)) return null;
        
        try 
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch
        {
            return null;
        }
    }
}
