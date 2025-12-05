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
        
        ChangeState(AIState.Idle);
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
        
        if (IsPlayerInRange(awarenessRange))       {
            

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
            ChangeState(AIState.Chase);            
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
}
