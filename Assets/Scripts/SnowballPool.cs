using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object Pool для снежков.
/// Переиспользует объекты снежков вместо их создания и уничтожения,
/// что повышает производительность игры.
/// </summary>
public class SnowballPool : MonoBehaviour
{
    public static SnowballPool Instance { get; private set; }

    [Header("Pool Settings")]
    [Tooltip("Префаб снежка")]
    public GameObject snowballPrefab;

    [Tooltip("Начальный размер пула (сколько снежков создать сразу)")]
    public int initialPoolSize = 20;

    [Tooltip("Максимальный размер пула")]
    public int maxPoolSize = 50;

    // Очередь доступных снежков
    private Queue<Snowball> availableSnowballs = new Queue<Snowball>();
    
    // Множество активных снежков (для отслеживания)
    private HashSet<Snowball> activeSnowballs = new HashSet<Snowball>();

    private void Awake()
    {
        // Реализуем Singleton паттерн
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Инициализируем пул
        InitializePool();
    }

    /// <summary>
    /// Инициализирует пул, создавая начальное количество снежков
    /// </summary>
    private void InitializePool()
    {
        if (snowballPrefab == null)
        {
            Debug.LogError("SnowballPool: Snowball prefab не назначен!");
            return;
        }

        // Создаем контейнер для организации иерархии
        GameObject poolContainer = new GameObject("SnowballPool");
        poolContainer.transform.SetParent(transform);

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateSnowballInstance(poolContainer.transform);
        }

        Debug.Log($"SnowballPool инициализирован. Создано {initialPoolSize} снежков.");
    }

    /// <summary>
    /// Создает новый экземпляр снежка и добавляет его в пул
    /// </summary>
    private void CreateSnowballInstance(Transform parent)
    {
        GameObject snowballObj = Instantiate(snowballPrefab, parent);
        snowballObj.name = "Snowball (Pooled)";

        Snowball snowball = snowballObj.GetComponent<Snowball>();
        if (snowball != null)
        {
            snowball.SetPool(this);
            snowballObj.SetActive(false);
            availableSnowballs.Enqueue(snowball);
        }
        else
        {
            Debug.LogError("Префаб снежка не содержит компонент Snowball!");
            Destroy(snowballObj);
        }
    }

    /// <summary>
    /// Получает снежок из пула или создает новый, если нет доступных
    /// </summary>
    public Snowball GetSnowball(Vector3 position)
    {
        Snowball snowball;

        if (availableSnowballs.Count > 0)
        {
            snowball = availableSnowballs.Dequeue();
        }
        else
        {
            // Если пул исчерпан, создаем новый снежок (если не превышен максимум)
            if (activeSnowballs.Count < maxPoolSize)
            {
                CreateSnowballInstance(transform);
                snowball = availableSnowballs.Dequeue();
            }
            else
            {
                Debug.LogWarning("SnowballPool: Достигнут максимальный размер пула!");
                return null;
            }
        }

        // Полностью восстанавливаем снежок перед использованием
        snowball.gameObject.SetActive(true);
        snowball.transform.position = position;
        snowball.transform.rotation = Quaternion.identity;
        
        // Очищаем физику
        Rigidbody rb = snowball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Добавляем в активные снежки
        activeSnowballs.Add(snowball);

        return snowball;
    }

    /// <summary>
    /// Возвращает снежок в пул
    /// </summary>
    public void ReturnSnowball(Snowball snowball)
    {
        if (snowball == null)
            return;

        // Убедиться, что снежок был в активных
        if (!activeSnowballs.Contains(snowball))
            return;

        // Деактивируем снежок
        snowball.gameObject.SetActive(false);

        // Удаляем из активных
        activeSnowballs.Remove(snowball);

        // Очищаем состояние перед возвратом в пул
        Rigidbody rb = snowball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Добавляем обратно в пул
        availableSnowballs.Enqueue(snowball);
    }

    /// <summary>
    /// Получает информацию о состоянии пула
    /// </summary>
    public string GetPoolStats()
    {
        return $"Доступных: {availableSnowballs.Count} | Активных: {activeSnowballs.Count} | Всего: {availableSnowballs.Count + activeSnowballs.Count}";
    }


}
