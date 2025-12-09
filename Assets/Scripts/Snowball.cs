using UnityEngine;

/// <summary>
/// Скрипт снежка, выпускаемого игроком или враждебными NPC.
/// Летит по прямой линии, наносит урон при попадании.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Snowball : MonoBehaviour
{
    public enum ShooterType
    {
        Player,  // Снежок от игрока - наносит урон NPC
        NPC      // Снежок от NPC - наносит урон игроку
    }

    [Header("Shooter Settings")]
    [Tooltip("Кто выпустил снежок")]
    public ShooterType shooterType = ShooterType.NPC;

    [Header("Damage Settings")]
    [Tooltip("Урон, наносимый при попадании")]
    public int damage = 10;

    [Header("Lifetime Settings")]
    [Tooltip("Время жизни снежка в секундах")]
    public float lifetime = 5f;

    private Rigidbody rb;
    private float spawnTime;
    private SnowballPool pool; // Ссылка на пул объектов

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;
    }

    private void Start()
    {
        // Отключаем гравитацию для прямолинейного полета
        rb.useGravity = false;
    }

    /// <summary>
    /// Запуск снежка в заданном направлении с заданной скоростью
    /// </summary>
    public void Launch(Vector3 direction, float speed)
    {
        // ВАЖНО: сбрасываем время спавна при каждом запуске
        spawnTime = Time.time;
        
        // Устанавливаем скорость в направлении полета
        rb.linearVelocity = direction.normalized * speed;
    }

    /// <summary>
    /// Установка типа стрелка
    /// </summary>
    public void SetShooterType(ShooterType type)
    {
        shooterType = type;
    }

    /// <summary>
    /// Установка пула объектов
    /// </summary>
    public void SetPool(SnowballPool snowballPool)
    {
        pool = snowballPool;
    }

    private void Update()
    {
        // Проверяем, не истекло ли время жизни
        if (Time.time - spawnTime >= lifetime)
        {
            ReturnToPool();
        }
    }

    /// <summary>
    /// Возвращает снежок в пул вместо уничтожения
    /// </summary>
    private void ReturnToPool()
    {
        if (pool != null)
        {
            // Полностью очищаем состояние снежка перед возвратом в пул
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            pool.ReturnSnowball(this);
        }
        else
        {
            // Если нет пула, уничтожаем объект (fallback)
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Логика попадания зависит от типа стрелка
        if (shooterType == ShooterType.NPC)
        {
            // Снежок от NPC - проверяем попадание в игрока
            if (other.CompareTag("Player"))
            {
                CharacterStats playerStats = other.GetComponent<CharacterStats>();
                
                if (playerStats != null)
                {
                    playerStats.TakeDamage(damage);
                    Debug.Log($"Snowball hit player! Dealt {damage} damage.");
                }

                // Возвращаем снежок в пул
                ReturnToPool();
            }
        }
        else if (shooterType == ShooterType.Player)
        {
            // Снежок от игрока - проверяем попадание в NPC
            StatefulAI npcAI = other.GetComponent<StatefulAI>();
            
            if (npcAI != null)
            {
                npcAI.TakeDamage(damage);
                Debug.Log($"Snowball hit NPC! Dealt {damage} damage.");

                // Возвращаем снежок в пул
                ReturnToPool();
            }
        }
    }
}
