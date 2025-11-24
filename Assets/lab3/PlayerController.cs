using UnityEngine;
using UnityEngine.InputSystem;


// Атрибут, который автоматически добавит нужные компоненты, если их нет
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    
    [Header("Movement Settings")]
    [Tooltip("Скорость движения персонажа при ходьбе.")]
    public float moveSpeed = 4.0f;
    [Tooltip("Скорость поворота персонажа.")]
    public float rotationSpeed = 10.0f;

    [Header("Gravity & Jump")]
    public float gravity = -5.0f;
    public float jumpHeight = 5f;
    private Vector3 playerVelocity; // Эта переменная будет хранить нашу вертикальную скорость
    private bool isGrounded; // Эта переменная будет показывать, на земле ли персонаж

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float maxSlopeAngle = 45f; // Максимальный угол склона, на котором персонаж может стоять

    [Header("Interaction")]    
    public float interactionDistance = 2f;    
    public LayerMask interactionLayer;    
    public Transform interactionRayPoint;

    [Header("Sprint Settings")]
    public float sprintSpeed = 7.0f;
    public float staminaUsePerSecond = 15f; 

    private bool isSprinting;
    private bool sprintButtonHeld;  

    private CharacterStats stats;




 
    private CharacterController controller;
    private Animator animator;
    private Vector2 moveInput;
    private Transform cameraTransform;
    private IInteractable currentInteractable;
    public static event System.Action<string> OnInteractableFocusChanged;


    private void Awake()
    {
        // Получаем ссылки на компоненты для производительности
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        // Находим главную камеру в сцене
        cameraTransform = Camera.main.transform;

        stats = GetComponent<CharacterStats>();
        
    }
    
    // Update вызывается каждый кадр
    void Update()
    {
        // Проверяем землю и угол склона. Этот метод теперь устанавливает isGrounded.
        GroundCheck();
        // Сначала обрабатываем горизонтальное движение (ходьба, поворот)
        HandleHorizontalMovement();
        // Затем обрабатываем вертикальное движение (гравитация, прыжок)
        HandleGravity();
        HandleSprint();
        // В конце обновляем анимацию
        HandleAnimation();
        
        CheckInteractionFocus();
    }

    
    // Этот метод вызывается компонентом Player Input при изменении действия "Move"
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        // Мы выполним прыжок, только если персонаж на земле
        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump"); // Вызываем анимацию прыжка
        }
    }



    private void HandleHorizontalMovement()
    {
    Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);


        if (moveDirection.magnitude >= 0.1f)
        {
            Vector3 relativeMoveDirection = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0) * moveDirection;
            Quaternion targetRotation = Quaternion.LookRotation(relativeMoveDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);


            // Двигаем персонажа по горизонтали
            float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
            controller.Move(relativeMoveDirection.normalized * currentSpeed * Time.deltaTime);
         }
    }

    private void GroundCheck()
    {
        // Используем SphereCast для большей надежности на краях
        float sphereRadius = controller.radius;
        Vector3 sphereOrigin = transform.position + controller.center;


        if (Physics.SphereCast(sphereOrigin, sphereRadius, Vector3.down, out RaycastHit hit, (controller.height / 2f) - controller.radius + 0.1f, groundMask))
        {
            // Vector3.Angle() вычисляет угол между двумя векторами.
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);


            // Если угол меньше допустимого, мы считаемся "на земле"
            if (slopeAngle <= maxSlopeAngle)
            {
                isGrounded = true;
                return;
            }
        }


        // Если луч ничего не нашел или угол слишком крутой, мы в воздухе.
        isGrounded = false;
    }

    private void HandleGravity()
    {
        // Логика сброса скорости теперь внутри GroundCheck, так что здесь только гравитация
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }


    private void HandleAnimation()
    {   

        float speedValue = isSprinting ? 1.0f : moveInput.magnitude * 0.5f;
        animator.SetFloat("Speed", speedValue, 0.1f, Time.deltaTime);
        animator.SetBool("IsGrounded", isGrounded);
    }
    
    
    private void CheckInteractionFocus()
    {
        Vector3 rayOrigin = interactionRayPoint.position;
        Vector3 rayDirection = cameraTransform.forward;

        IInteractable newInteractable = null;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, interactionDistance, interactionLayer))
        {
            newInteractable = hit.collider.GetComponent<IInteractable>();
        }

        // ← КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: Проверяем, жив ли currentInteractable!
        if (currentInteractable != null)
        {
            var mb = currentInteractable as MonoBehaviour;
            if (mb == null || mb.gameObject == null || !mb.gameObject.activeInHierarchy)
            {
                currentInteractable = null; // Сбрасываем "мёртвую" ссылку
                OnInteractableFocusChanged?.Invoke(""); // Очищаем подсказку
            }
        }

        // ← Безопасное сравнение
        if (newInteractable != currentInteractable)
        {
            currentInteractable = newInteractable;

            // ← Безопасно получаем текст
            string hintText = "";
            if (newInteractable != null)
            {
                var mb = newInteractable as MonoBehaviour;
                if (mb != null && mb.gameObject != null)
                {
                    hintText = newInteractable.GetInteractText();
                }
            }

            OnInteractableFocusChanged?.Invoke(hintText);
        }
    }
    

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return; // Только при нажатии (не удержании)

        // ← КЛЮЧЕВОЕ: СБРАСЫВАЕМ ФОКУС СРАЗУ!
        if (currentInteractable != null)
        {
            currentInteractable.Interact();  // Вызываем Interact() ТОЛЬКО РАЗ!
            OnInteractableFocusChanged?.Invoke("");  // Сбрасываем подсказку
            currentInteractable = null;  // ← ОЧИЩАЕМ ССЫЛКУ!
        }
    }

    public void OnSprint(InputValue value)
    {
        sprintButtonHeld = value.isPressed;
    }

        private void HandleSprint()
    {
        // Если персонаж не двигается, сбрасываем спринт полностью
        if (moveInput.magnitude <= 0.1f)
        {
            isSprinting = false;
            sprintButtonHeld = false; // ← КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: сбрасываем флаг удержания
            stats.RegenerateStamina(stats.staminaRegenRate);
            return;
        }

        // Можно спринтовать, только если есть выносливость и игрок двигается
        if (sprintButtonHeld)
        {
            // Пытаемся потратить стамину
            if (stats.UseStamina(staminaUsePerSecond * Time.deltaTime))
            {
                isSprinting = true;
            }
            else
            {
                // стамина закончилась
                isSprinting = false;
            }
        }
        else
        {
            // кнопка не зажата — восстанавливаем стамину
            isSprinting = false;
            stats.RegenerateStamina(stats.staminaRegenRate);
        }
    }






}
