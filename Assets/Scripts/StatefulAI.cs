using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class StatefulAI : MonoBehaviour, IInteractable
{
    public enum AIState
    {
        Idle, // Ожидание - NPC стоит на месте
        Patrol, // Патрулирование - NPC ходит между точками
        Chase, // ← НОВОЕ! Преследование - NPC активно следует за игроком
        Alert, // Внимание - NPC заметил игрока и готов к взаимодействию
        Interact, // Взаимодействие - NPC в диалоге с игроком
    }

    public enum NPCType
    {
        Friendly, // Дружелюбный - останавливается и ждет, не преследует
        Hostile, // ← НОВОЕ! Враждебный - активно преследует игрока
    }

    [Header("NPC Type")]
    [Tooltip("Тип NPC: дружелюбный или враждебный.")]
    public NPCType npcType = NPCType.Friendly; // ← Определяет, будет ли NPC преследовать игрока
    private AIState currentState;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;

    [Header("Detection Settings")]
    public float awarenessRange = 5f;
    public float interactionDistance = 2.5f;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float waitTimeAtPoint = 3f;

    [Header("Rotation Settings")]
    [Tooltip("Скорость поворота NPC при слежении за игроком.")]
    public float rotationSpeed = 5f;

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
        // Начинаем с состояния ожидания, чтобы сразу выбрать первую точку
        ChangeState(AIState.Idle);
    }

    private void Update()
    {
        // Выполняем логику, соответствующую текущему состоянию
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
        // ── ПРОВЕРКА ОБНАРУЖЕНИЯ ИГРОКА ──
        // Если игрок в зоне видимости (awarenessRange), реагируем на него
        if (IsPlayerInRange(awarenessRange))
        {
            if (npcType == NPCType.Friendly)
                // Дружелюбный NPC просто останавливается и смотрит на игрока
                ChangeState(AIState.Alert);
            else
                // Враждебный NPC начинает ПРЕСЛЕДОВАНИЕ
                ChangeState(AIState.Chase); // ← Переход в новое состояние Chase!
            return;
        }

        // ── ЛОГИКА ОЖИДАНИЯ ──
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
        // ── ПРОВЕРКА ОБНАРУЖЕНИЯ ИГРОКА ──
        // Даже во время патруля проверяем, не появился ли игрок
        if (IsPlayerInRange(awarenessRange))
        {
            // ← КЛЮЧЕВАЯ ЛОГИКА: Та же логика, что и в Idle

            if (npcType == NPCType.Friendly)
                // Дружелюбный NPC останавливает патруль и наблюдает
                ChangeState(AIState.Alert);
            else
                // Враждебный NPC прерывает патруль и начинает ПРЕСЛЕДОВАНИЕ
                ChangeState(AIState.Chase); // ← Переход в Chase из Patrol!
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Дошли - переходим в Idle, чтобы подождать перед следующей точкой
            ChangeState(AIState.Idle);
        }
    }

    private void UpdateChase()
    {
        // ── ПРОВЕРКА 1: Игрок вышел из зоны видимости? ──
        // Если игрок ушел слишком далеко, прекращаем преследование
        if (!IsPlayerInRange(awarenessRange))
        {
            // Теряем игрока из виду - возвращаемся к обычному поведению
            ChangeState(AIState.Idle);
            return;
        }

        // ── АКТИВНОЕ ПРЕСЛЕДОВАНИЕ ──
        // КАЖДЫЙ КАДР обновляем путь к игроку (так как игрок постоянно движется)
        // Это ключевое отличие от Patrol, где цель статична
        agent.SetDestination(player.position); // ← Постоянное обновление цели!

        // ── ПРОВЕРКА 2: Догнали игрока? ──
        // Если расстояние стало меньше interactionDistance
        if (IsPlayerInRange(interactionDistance))
        {
            // Догнали! Переходим в Alert - готовность к взаимодействию
            // NPC останавливается и поворачивается к игроку
            ChangeState(AIState.Alert);
        }
    }

    private void UpdateAlert()
    {
        // ── ВСЕГДА СМОТРИМ НА ИГРОКА ──
        LookAtPlayer(); // Плавно поворачиваемся к игроку

        // ── ПРОВЕРКА 1: Игрок ушел совсем далеко? ──
        if (!IsPlayerInRange(awarenessRange))
        {
            // Потеряли игрока из виду полностью - возвращаемся к патрулю
            ChangeState(AIState.Idle);
            return;
        }

        // ── ПРОВЕРКА 2: Игрок начал убегать? ──
        // ← КЛЮЧЕВОЕ ИЗМЕНЕНИЕ! Проверяем, не отошел ли игрок
        if (!IsPlayerInRange(interactionDistance) && npcType == NPCType.Hostile)
        {
            // Игрок в зоне видимости, но вышел из зоны взаимодействия
            // Враждебные NPC не позволят убежать - начинают преследование снова!
            ChangeState(AIState.Chase); // ← Возврат в Chase из Alert!

            // ПОЧЕМУ ТАК:
            // - Дружелюбные NPC (Friendly) остаются в Alert и просто наблюдают
            // - Враждебные NPC (Hostile) активно преследуют убегающего игрока
        }
        // Если дружелюбный NPC - он просто остается в Alert и наблюдает за игроком
    }

    private void UpdateInteract()
    {
        // ── ПРОДОЛЖАЕМ СМОТРЕТЬ НА ИГРОКА ──
        LookAtPlayer();

        // ── ПРОВЕРКА: Игрок убежал во время диалога? ──
        if (!IsPlayerInRange(interactionDistance))
        {
            // ← ЛОГИКА РЕАКЦИИ НА ПОБЕГ

            // Враждебные преследуют даже если игрок убегает во время диалога
            if (npcType == NPCType.Hostile && IsPlayerInRange(awarenessRange))
                ChangeState(AIState.Chase);
            else
                // Дружелюбные или если игрок ушел совсем далеко - возврат к патрулю
                ChangeState(AIState.Idle);
        }
    }
    #endregion


    #region State Changes
    private void ChangeState(AIState newState)
    {
        // ── ЗАЩИТА ОТ ДУБЛИРОВАНИЯ ──
        // Если уже в этом состоянии, не делаем ничего
        if (currentState == newState)
            return;

        // ── СМЕНА СОСТОЯНИЯ ──
        currentState = newState;
        waitTimer = 0f; // Сбрасываем таймер ожидания

        // ── ЛОГИКА ВХОДА В НОВОЕ СОСТОЯНИЕ ──
        // Настраиваем NavMeshAgent в зависимости от состояния
        switch (newState)
        {
            case AIState.Idle:
                // В режиме ожидания - останавливаем движение
                agent.isStopped = true;
                break;

            case AIState.Patrol:
                // Начинаем патрулирование - разрешаем движение
                agent.isStopped = false;
                break;

            case AIState.Chase:
                // ← НОВАЯ ЛОГИКА ДЛЯ CHASE!
                agent.isStopped = false; // Разрешаем движение

                // Сразу начинаем преследование - устанавливаем цель на игрока
                if (player != null)
                    agent.SetDestination(player.position);

                // ПОЧЕМУ ТАК: Устанавливаем начальную цель сразу при входе в состояние,
                // затем UpdateChase() будет обновлять её каждый кадр
                break;

            case AIState.Alert:
            case AIState.Interact:
                // В этих состояниях NPC стоит на месте и смотрит на игрока
                agent.isStopped = true;
                break;
        }
    }
    #endregion


    #region Helpers
    private void GoToNextPatrolPoint()
    {
        // ── ПРОВЕРКА НАЛИЧИЯ ТОЧЕК ──
        // Если точки патруля не заданы, выходим из метода
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        // ── ЦИКЛИЧЕСКИЙ ВЫБОР СЛЕДУЮЩЕЙ ТОЧКИ ──
        // Оператор % (остаток от деления) создает циклический обход:
        // Если точек 3: 0 → 1 → 2 → 0 → 1 → 2 → ...
        // Формула: (текущий_индекс + 1) % количество_точек
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

        // ── УСТАНОВКА ЦЕЛИ НАВИГАЦИИ ──
        // NavMeshAgent автоматически построит путь к указанной позиции
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        // ПРИМЕЧАНИЕ: SetDestination вызывается ОДИН РАЗ для статичной точки
        // В отличие от Chase, где цель обновляется каждый кадр
    }

    private bool IsPlayerInRange(float range)
    {
        // ── ПРОВЕРКА СУЩЕСТВОВАНИЯ ИГРОКА ──
        // Если игрок не найден (null), возвращаем false
        // Это может произойти если у игрока нет тега "Player"
        if (player == null)
            return false;

        // ── ВЫЧИСЛЕНИЕ РАССТОЯНИЯ ──
        // Vector3.Distance() вычисляет Евклидово расстояние между двумя точками
        // Формула: √((x2-x1)² + (y2-y1)² + (z2-z1)²)
        // Сравниваем с заданным радиусом
        return Vector3.Distance(transform.position, player.position) <= range;

        // ПОЧЕМУ НЕ sqrMagnitude:
        // Хотя sqrMagnitude быстрее, Distance() более читаем и понятен
        // Для AI логика производительность не критична
    }

    private void LookAtPlayer()
    {
        // ── ПРОВЕРКА СУЩЕСТВОВАНИЯ ИГРОКА ──
        if (player == null)
            return;

        // ── ВЫЧИСЛЕНИЕ НАПРАВЛЕНИЯ К ИГРОКУ ──
        // Normalized возвращает вектор длиной 1 (единичный вектор направления)
        Vector3 direction = (player.position - transform.position).normalized;

        // ── ИГНОРИРОВАНИЕ ВЕРТИКАЛЬНОЙ ОСИ ──
        // Устанавливаем Y в 0, чтобы NPC не наклонялся вверх/вниз
        // NPC поворачивается только по горизонтали (как человек)
        direction.y = 0f; // Поворачиваемся только по горизонтали

        // ── ПРОВЕРКА НА НУЛЕВОЙ ВЕКТОР ──
        // Если направление почти нулевое (NPC стоит прямо над/под игроком)
        // sqrMagnitude - квадрат длины вектора (быстрее чем magnitude)
        if (direction.sqrMagnitude < 0.0001f)
            return;

        // ── СОЗДАНИЕ ЦЕЛЕВОГО ПОВОРОТА ──
        // LookRotation создает Quaternion (поворот), смотрящий в заданном направлении
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // ── ПЛАВНАЯ ИНТЕРПОЛЯЦИЯ ПОВОРОТА ──
        // Slerp = Spherical Linear Interpolation (сферическая линейная интерполяция)
        // Плавно переходит от текущего поворота к целевому
        // rotationSpeed * Time.deltaTime = скорость поворота, независимая от FPS
        transform.rotation = Quaternion.Slerp(
            transform.rotation, // Текущий поворот
            targetRotation, // Целевой поворот (к игроку)
            rotationSpeed * Time.deltaTime // Скорость интерполяции (0-1)
        );

        // ПОЧЕМУ Slerp А НЕ Lerp:
        // Slerp обеспечивает равномерную скорость вращения по дуге
        // Lerp может давать неравномерное вращение для больших углов

        // ПОЧЕМУ Time.deltaTime:
        // Делает поворот независимым от частоты кадров (FPS)
        // При 60 FPS и 30 FPS поворот займет одинаковое время
    }
    #endregion


    #region Interaction
    public void Interact()
    {
        // ── ПРОВЕРКА УСЛОВИЙ ВЗАИМОДЕЙСТВИЯ ──
        // Можно взаимодействовать только если:
        // 1. NPC в состоянии Alert (заметил игрока и готов)
        // 2. Игрок достаточно близко (в зоне interactionDistance)
        if (currentState == AIState.Alert && IsPlayerInRange(interactionDistance))
        {
            // Переходим в режим взаимодействия
            ChangeState(AIState.Interact);

            // Запускаем диалог (если есть компонент DialogueTrigger)
            GetComponent<DialogueTrigger>()?.Interact();
        }
    }

    public string GetInteractText()
    {
        // Показываем подсказку только если взаимодействие возможно
        return (currentState == AIState.Alert && IsPlayerInRange(interactionDistance))
            ? "Поговорить"
            : string.Empty;
    }

    public void EndInteraction()
    {
        // ── РЕШАЕМ, ЧТО ДЕЛАТЬ ПОСЛЕ ДИАЛОГА ──

        // СЛУЧАЙ 1: Игрок все еще рядом (в зоне взаимодействия)
        if (IsPlayerInRange(interactionDistance))
        {
            // Возвращаемся в Alert - NPC готов продолжить общение
            ChangeState(AIState.Alert);
        }
        // СЛУЧАЙ 2: Игрок отошел, но в зоне видимости
        else if (IsPlayerInRange(awarenessRange))
        {
            // ← КЛЮЧЕВАЯ ЛОГИКА ПОСЛЕ ДИАЛОГА

            if (npcType == NPCType.Hostile)
                // Враждебные NPC не дадут просто уйти - преследуют!
                ChangeState(AIState.Chase);
            else
                // Дружелюбные NPC продолжают наблюдать
                ChangeState(AIState.Alert);

            // ПОЧЕМУ ТАК:
            // - Friendly NPC не навязчивы - позволяют игроку уйти
            // - Hostile NPC настойчивы - начинают преследование
        }
        // СЛУЧАЙ 3: Игрок ушел далеко (вне зоны видимости)
        else
        {
            // Игрок ушел совсем - возвращаемся к обычному поведению
            ChangeState(AIState.Idle);
        }
    }
    #endregion
}
