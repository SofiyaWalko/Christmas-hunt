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
public class NPCSaveData
{
    public string id;
    public float positionX;
    public float positionY;
    public float positionZ;
    public int currentHealth;
    public bool isDead;
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

    public List<string> collectedItems = new List<string>();
    public List<NPCSaveData> npcData = new List<NPCSaveData>();
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    public bool sessionDoubleJumpUnlocked = false; // Persist across scenes in session

    private List<string> _collectedItemsSession = new List<string>();
    private Dictionary<string, NPCSaveData> _npcDataSession = new Dictionary<string, NPCSaveData>();

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
                    data.inventoryItems.Add(
                        new InventoryItemSaveData
                        {
                            itemName = slot.item.itemName,
                            quantity = slot.quantity,
                        }
                    );
                }
            }

            if (
                InventoryManager.Instance.rewardSlot != null
                && InventoryManager.Instance.rewardSlot.item != null
            )
            {
                data.rewardItem = new InventoryItemSaveData
                {
                    itemName = InventoryManager.Instance.rewardSlot.item.itemName,
                    quantity = InventoryManager.Instance.rewardSlot.quantity,
                };
            }

            if (
                InventoryManager.Instance.statSlot != null
                && InventoryManager.Instance.statSlot.item != null
            )
            {
                data.statItem = new InventoryItemSaveData
                {
                    itemName = InventoryManager.Instance.statSlot.item.itemName,
                    quantity = InventoryManager.Instance.statSlot.quantity,
                };
            }
        }

        data.collectedItems = new List<string>(_collectedItemsSession);

        // 3. NPCs - Update Session Data first
        StatefulAI[] npcs = FindObjectsOfType<StatefulAI>();
        foreach (var npc in npcs)
        {
            if (!string.IsNullOrEmpty(npc.id))
            {
                NPCSaveData npcData = new NPCSaveData
                {
                    id = npc.id,
                    positionX = npc.transform.position.x,
                    positionY = npc.transform.position.y,
                    positionZ = npc.transform.position.z,
                    currentHealth = npc.GetCurrentHealth(),
                    isDead = npc.GetCurrentHealth() <= 0,
                };

                if (_npcDataSession.ContainsKey(npc.id))
                {
                    _npcDataSession[npc.id] = npcData;
                }
                else
                {
                    _npcDataSession.Add(npc.id, npcData);
                }
            }
        }

        // Write ALL session NPC data to save file
        data.npcData = new List<NPCSaveData>(_npcDataSession.Values);

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
        // Restore collected items IMMEDIATELY
        _collectedItemsSession = new List<string>(data.collectedItems);

        // Restore NPC data to session
        _npcDataSession.Clear();
        foreach (var npc in data.npcData)
        {
            if (!_npcDataSession.ContainsKey(npc.id))
            {
                _npcDataSession.Add(npc.id, npc);
            }
        }

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

        // Explicitly destroy collected items (PickupBase covers ItemPickup, StatPickup, RewardPickup)
        PickupBase[] pickups = FindObjectsOfType<PickupBase>();
        foreach (var pickup in pickups)
        {
            if (_collectedItemsSession.Contains(pickup.id))
            {
                Destroy(pickup.gameObject);
            }
        }

        // Explicitly update NPCs
        StatefulAI[] npcs = FindObjectsOfType<StatefulAI>();
        foreach (var npc in npcs)
        {
            if (!string.IsNullOrEmpty(npc.id) && _npcDataSession.ContainsKey(npc.id))
            {
                var npcData = _npcDataSession[npc.id];
                if (npcData.isDead)
                {
                    Destroy(npc.gameObject);
                }
                else
                {
                    // Restore health and position manually if Start() missed it
                    // Note: We can't easily set health via public property if it's private,
                    // but StatefulAI.Start() should have handled it.
                    // If we need to force it, we might need a public method on StatefulAI.
                    // For now, let's assume Start() worked or we rely on this loop for destruction mainly.

                    // If we want to be sure about position:
                    UnityEngine.AI.NavMeshAgent agent =
                        npc.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null)
                    {
                        agent.Warp(
                            new Vector3(npcData.positionX, npcData.positionY, npcData.positionZ)
                        );
                    }
                    else
                    {
                        npc.transform.position = new Vector3(
                            npcData.positionX,
                            npcData.positionY,
                            npcData.positionZ
                        );
                    }
                }
            }
        }

        // 2. Restore Player State
        PlayerController player = FindObjectOfType<PlayerController>();
        CharacterStats stats = FindObjectOfType<CharacterStats>();

        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
                cc.enabled = false;

            player.transform.position = new Vector3(data.positionX, data.positionY, data.positionZ);
            player.SetDoubleJumpUnlocked(data.doubleJumpUnlocked);
            sessionDoubleJumpUnlocked = data.doubleJumpUnlocked; // Update session data

            if (cc != null)
                cc.enabled = true;
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

    public void MarkAsCollected(string id)
    {
        if (!string.IsNullOrEmpty(id) && !_collectedItemsSession.Contains(id))
        {
            _collectedItemsSession.Add(id);
        }
    }

    public bool IsCollected(string id)
    {
        return _collectedItemsSession.Contains(id);
    }

    public NPCSaveData GetNPCData(string id)
    {
        if (_npcDataSession.ContainsKey(id))
        {
            return _npcDataSession[id];
        }
        return null;
    }

    public void UpdateNPCData(string id, NPCSaveData data)
    {
        if (_npcDataSession.ContainsKey(id))
        {
            _npcDataSession[id] = data;
        }
        else
        {
            _npcDataSession.Add(id, data);
        }
    }

    public void ClearSessionData()
    {
        _collectedItemsSession.Clear();
        _npcDataSession.Clear();
        sessionDoubleJumpUnlocked = false;
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
        if (!File.Exists(path))
            return null;

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
