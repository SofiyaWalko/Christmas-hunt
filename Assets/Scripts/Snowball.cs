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

    private void Update()
    {
        // Проверяем, не истекло ли время жизни
        if (Time.time - spawnTime >= lifetime)
        {
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

                // Уничтожаем снежок после попадания
                Destroy(gameObject);
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

                // Уничтожаем снежок после попадания
                Destroy(gameObject);
            }
        }
    }
}
