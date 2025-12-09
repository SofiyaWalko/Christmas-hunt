using UnityEngine;

public abstract class PickupBase : MonoBehaviour
{
    public string id;

    [ContextMenu("Generate ID")]
    public void GenerateId()
    {
        id = System.Guid.NewGuid().ToString();
    }

    protected virtual void OnValidate()
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(id))
        {
            GenerateId();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        else
        {
            PickupBase[] pickups = FindObjectsOfType<PickupBase>();
            foreach (var pickup in pickups)
            {
                if (pickup != this && pickup.id == id)
                {
                    GenerateId();
                    UnityEditor.EditorUtility.SetDirty(this);
                    return;
                }
            }
        }
#endif
    }

    protected virtual void Reset()
    {
        GenerateId();
    }

    protected virtual void Start()
    {
        if (Application.isPlaying)
        {
            if (SaveManager.Instance != null && SaveManager.Instance.IsCollected(id))
            {
                Destroy(gameObject);
            }
        }
    }
    
    protected void MarkAsCollected()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.MarkAsCollected(id);
        }
    }
}
