using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class StatefulAI : MonoBehaviour, IInteractable
{
    public enum AIState
    {
        Idle,
        Patrol,
        Chase,
        Alert,
        Interact,
    }

    public enum NPCType
    {
        Friendly,
        Hostile,
    }

    [Header("NPC Type")]
    [Tooltip("Тип NPC: дружелюбный или враждебный.")]
    public NPCType npcType = NPCType.Friendly;
    
    [Header("Save System")]
    public string id;

    private AIState currentState;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;

    [ContextMenu("Generate ID")]
    public void GenerateId()
    {
        id = System.Guid.NewGuid().ToString();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(id))
        {
            GenerateId();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        else
        {
            StatefulAI[] npcs = FindObjectsOfType<StatefulAI>();
            foreach (var npc in npcs)
            {
                if (npc != this && npc.id == id)
                {
                    GenerateId();
                    UnityEditor.EditorUtility.SetDirty(this);
                    return;
                }
            }
        }
#endif
    }

    private void Reset()
    {
        GenerateId();
    }

    [Header("Detection Settings")]
    public float awarenessRange = 5f;
    public float interactionDistance = 2.5f;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float waitTimeAtPoint = 3f;

    [Header("Rotation Settings")]
    [Tooltip("Скорость поворота NPC при слежении за игроком.")]
    public float rotationSpeed = 5f;

    [Header("Health Settings")]
    [Tooltip("Максимальное здоровье NPC")]
    public int maxHealth = 50;
    private int currentHealth;

    [Tooltip("Компонент полоски здоровья (опционально)")]
    public NPCHealthBar healthBar;

    [Header("Attack Settings (Hostile Only)")]
    [Tooltip("Префаб снежка для стрельбы")]
    public GameObject snowballPrefab;

    [Tooltip("Точка спавна снежка (обычно перед NPC)")]
    public Transform shootPoint;

    [Tooltip("Интервал между выстрелами в секундах")]
    public float shootingInterval = 2f;

    [Tooltip("Скорость полета снежка")]
    public float snowballSpeed = 15f;
    private float lastShootTime = -999f; // Время последнего выстрела

    private int currentPatrolIndex = 0;
    private float waitTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Start()
    {
        // Инициализируем здоровье
        currentHealth = maxHealth;

        // Load saved data if available
        if (SaveManager.Instance != null)
        {
            NPCSaveData data = SaveManager.Instance.GetNPCData(id);
            if (data != null)
            {
                if (data.isDead)
                {
                    Destroy(gameObject);
                    return;
                }
                
                currentHealth = data.currentHealth;
                
                if (agent != null)
                {
                    agent.Warp(new Vector3(data.positionX, data.positionY, data.positionZ));
                }
                else
                {
                    transform.position = new Vector3(data.positionX, data.positionY, data.positionZ);
                }
            }
        }

        // Инициализируем полоску здоровья если есть
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        // Начинаем с состояния ожидания, чтобы сразу выбрать первую точку
        ChangeState(AIState.Idle);
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    private void Update()
    {
        switch (currentState)
        {
            case AIState.Idle:
                UpdateIdle();
                break;
            case AIState.Patrol:
                UpdatePatrol();
                break;
            case AIState.Chase:
                UpdateChase();
                break;
            case AIState.Alert:
                UpdateAlert();
                break;
            case AIState.Interact:
                UpdateInteract();
                break;
        }
        // Синхронизируем анимацию со скоростью агента
        animator.SetFloat("Speed", agent.velocity.magnitude / agent.speed, 0.1f, Time.deltaTime);
    }

    #region State Logic
    private void UpdateIdle()
    {
        //ПРОВЕРКА ОБНАРУЖЕНИЯ ИГРОКА
        if (IsPlayerInRange(awarenessRange))
        {
            if (npcType == NPCType.Friendly)
                ChangeState(AIState.Alert);
            else
                ChangeState(AIState.Chase);
            return;
        }

        // Если игрока нет, ждем определенное время, затем идем к следующей точке
        waitTimer += Time.deltaTime;
        if (waitTimer >= waitTimeAtPoint)
        {
            GoToNextPatrolPoint();
            ChangeState(AIState.Patrol);
        }
    }

    private void UpdatePatrol()
    {
        if (IsPlayerInRange(awarenessRange))
        {
            if (npcType == NPCType.Friendly)
                ChangeState(AIState.Alert);
            else
                ChangeState(AIState.Chase);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            ChangeState(AIState.Idle);
        }
    }

    private void UpdateChase()
    {
        if (!IsPlayerInRange(awarenessRange))
        {
            ChangeState(AIState.Idle);
            return;
        }

        agent.SetDestination(player.position); // Постоянное обновление цели

        if (IsPlayerInRange(interactionDistance))
        {
            //Переходим в Alert - готовность к взаимодействию
            ChangeState(AIState.Alert);
        }

        // ── СТРЕЛЬБА СНЕЖКАМИ (ТОЛЬКО ДЛЯ HOSTILE) ──
        // Враждебные NPC атакуют игрока во время преследования
        if (npcType == NPCType.Hostile)
        {
            TryShootSnowball();
        }
    }

    private void UpdateAlert()
    {
        LookAtPlayer(); // Плавно поворачиваемся к игроку

        if (!IsPlayerInRange(awarenessRange))
        {
            ChangeState(AIState.Idle);
            return;
        }

        if (!IsPlayerInRange(interactionDistance) && npcType == NPCType.Hostile)
        {
            // Игрок в зоне видимости, но вышел из зоны взаимодействия
            // Враждебные NPC не позволят убежать - начинают преследование снова!
            ChangeState(AIState.Chase); // ← Возврат в Chase из Alert!

            // ПОЧЕМУ ТАК:
            // - Дружелюбные NPC (Friendly) остаются в Alert и просто наблюдают
            // - Враждебные NPC (Hostile) активно преследуют убегающего игрока
        }

        // ── СТРЕЛЬБА СНЕЖКАМИ (ТОЛЬКО ДЛЯ HOSTILE) ──
        // Враждебные NPC атакуют игрока даже в состоянии Alert
        if (npcType == NPCType.Hostile)
        {
            TryShootSnowball();
        }
    }

    private void UpdateInteract()
    {
        LookAtPlayer();

        if (!IsPlayerInRange(interactionDistance))
        {
            if (npcType == NPCType.Hostile && IsPlayerInRange(awarenessRange))
                ChangeState(AIState.Chase);
            else
                ChangeState(AIState.Idle);
        }
    }
    #endregion


    #region State Changes
    private void ChangeState(AIState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        waitTimer = 0f; // Сбрасываем таймер ожидания

        // Настраиваем NavMeshAgent в зависимости от состояния
        switch (newState)
        {
            case AIState.Idle:
                agent.isStopped = true;
                break;

            case AIState.Patrol:
                agent.isStopped = false;
                break;

            case AIState.Chase:
                agent.isStopped = false;

                // Сразу начинаем преследование - устанавливаем цель на игрока
                if (player != null)
                    agent.SetDestination(player.position);
                break;

            case AIState.Alert:
            case AIState.Interact:
                agent.isStopped = true;
                break;
        }
    }
    #endregion


    #region Helpers
    private void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        // Формула: (текущий_индекс + 1) % количество_точек
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    private bool IsPlayerInRange(float range)
    {
        if (player == null)
            return false;

        // Vector3.Distance() вычисляет Евклидово расстояние между двумя точками
        // Формула: √((x2-x1)² + (y2-y1)² + (z2-z1)²)

        return Vector3.Distance(transform.position, player.position) <= range;
    }

    private void LookAtPlayer()
    {
        if (player == null)
            return;

        Vector3 direction = (player.position - transform.position).normalized;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Slerp = Spherical Linear Interpolation (сферическая линейная интерполяция)
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
    #endregion


    #region Interaction
    public void Interact()
    {
        if (currentState == AIState.Alert && IsPlayerInRange(interactionDistance))
        {
            ChangeState(AIState.Interact);

            GetComponent<DialogueTrigger>()?.Interact();
        }
    }

    public string GetInteractText()
    {
        // Показываем подсказку
        return (currentState == AIState.Alert && IsPlayerInRange(interactionDistance))
            ? "Поговорить"
            : string.Empty;
    }

    public void EndInteraction()
    {
        if (IsPlayerInRange(interactionDistance))
        {
            ChangeState(AIState.Alert);
        }
        else if (IsPlayerInRange(awarenessRange))
        {
            if (npcType == NPCType.Hostile)
                ChangeState(AIState.Chase);
            else
                ChangeState(AIState.Alert);
        }
        else
        {
            ChangeState(AIState.Idle);
        }
    }
    #endregion


    #region Health System
    /// <summary>
    /// NPC получает урон
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"{transform.name} получил {damage} урона. Осталось здоровья: {currentHealth}");

        // Обновляем полоску здоровья
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Смерть NPC
    /// </summary>
    private void Die()
    {
        Debug.Log($"{transform.name} умер!");

        if (SaveManager.Instance != null && !string.IsNullOrEmpty(id))
        {
            NPCSaveData data = new NPCSaveData
            {
                id = this.id,
                positionX = transform.position.x,
                positionY = transform.position.y,
                positionZ = transform.position.z,
                currentHealth = 0,
                isDead = true
            };
            SaveManager.Instance.UpdateNPCData(this.id, data);
        }

        // Можно добавить анимацию смерти, звук и т.д.
        // animator.SetTrigger("Death");

        // Уничтожаем объект NPC
        Destroy(gameObject);
    }
    #endregion


    #region Combat System
    /// <summary>
    /// Попытка выстрелить снежком (только для Hostile NPC)
    /// </summary>
    private void TryShootSnowball()
    {
        // Проверяем, прошло ли достаточно времени с последнего выстрела
        if (Time.time - lastShootTime < shootingInterval)
            return;

        // Проверяем наличие префаба снежка
        if (snowballPrefab == null)
        {
            Debug.LogWarning($"{transform.name}: Snowball prefab не назначен!");
            return;
        }

        // Проверяем наличие точки спавна
        if (shootPoint == null)
        {
            Debug.LogWarning($"{transform.name}: Shoot point не назначен!");
            return;
        }

        // Проверяем наличие игрока
        if (player == null)
            return;

        // Вычисляем направление к игроку
        Vector3 directionToPlayer = (player.position - shootPoint.position).normalized;

        // Создаем снежок
        GameObject snowballObj = Instantiate(
            snowballPrefab,
            shootPoint.position,
            Quaternion.identity
        );

        // Получаем компонент Snowball и запускаем его
        Snowball snowball = snowballObj.GetComponent<Snowball>();
        if (snowball != null)
        {
            // Устанавливаем тип стрелка - NPC
            snowball.SetShooterType(Snowball.ShooterType.NPC);

            // Запускаем снежок
            snowball.Launch(directionToPlayer, snowballSpeed);
        }
        else
        {
            Debug.LogError($"Префаб снежка не содержит компонент Snowball!");
        }

        // Обновляем время последнего выстрела
        lastShootTime = Time.time;

        Debug.Log($"{transform.name} выстрелил снежком!");
    }
    #endregion
}
