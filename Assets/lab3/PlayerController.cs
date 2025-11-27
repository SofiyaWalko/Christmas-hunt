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
    private Vector3 playerVelocity; 
    private bool isGrounded; 

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float maxSlopeAngle = 45f;

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


    // <<< ADD: Переменные для платформы
    private Transform currentPlatform;
    private Vector3 lastPlatformPos;
    // >>>


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cameraTransform = Camera.main.transform;
        stats = GetComponent<CharacterStats>();
    }
    
    // Update вызывается каждый кадр
    void Update()
    {
        GroundCheck();
        HandleHorizontalMovement();
        HandleGravity();
        HandleSprint();
        HandleAnimation();
        CheckInteractionFocus();

        // <<< ADD: если не на земле — перестаём ехать с платформой
        if (!isGrounded)
        {
            if (currentPlatform != null)
            {
                TriggeredMovingPlatform platform = currentPlatform.GetComponent<TriggeredMovingPlatform>();
                if (platform != null)
                    platform.Deactivate();
            }
        
            currentPlatform = null;
        }
        // >>>
    }


    // Движение
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }
    }

    private void HandleHorizontalMovement()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (moveDirection.magnitude >= 0.1f)
        {
            Vector3 relativeMoveDirection =
                Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0) * moveDirection;
            Quaternion targetRotation = Quaternion.LookRotation(relativeMoveDirection.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Двигаем персонажа по горизонтали
            float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
            controller.Move(relativeMoveDirection.normalized * currentSpeed * Time.deltaTime);
        }
    }


    private void GroundCheck()
    {
        float sphereRadius = controller.radius;
        Vector3 sphereOrigin = transform.position + controller.center;

        if (
            Physics.SphereCast(
                sphereOrigin,
                sphereRadius,
                Vector3.down,
                out RaycastHit hit,
                (controller.height / 2f) - controller.radius + 0.1f,
                groundMask
            )
        )
        {
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

        if (
            Physics.Raycast(
                rayOrigin,
                rayDirection,
                out RaycastHit hit,
                interactionDistance,
                interactionLayer
            )
        )
        {
            newInteractable = hit.collider.GetComponent<IInteractable>();
        }

        if (currentInteractable != null)
        {
            var mb = currentInteractable as MonoBehaviour;
            if (mb == null || mb.gameObject == null || !mb.gameObject.activeInHierarchy)
            {
                currentInteractable = null;
                OnInteractableFocusChanged?.Invoke("");
            }
        }

        if (newInteractable != currentInteractable)
        {
            currentInteractable = newInteractable;

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
        if (!value.isPressed) return;

        if (currentInteractable != null)
        {
            currentInteractable.Interact();
            OnInteractableFocusChanged?.Invoke("");
            currentInteractable = null;
        }
    }


    public void OnSprint(InputValue value)
    {
        sprintButtonHeld = value.isPressed;
    }

    private void HandleSprint()
    {
        if (moveInput.magnitude <= 0.1f)
        {
            isSprinting = false;
            sprintButtonHeld = false;
            stats.RegenerateStamina(stats.staminaRegenRate);
            return;
        }

        if (sprintButtonHeld)
        {
            if (stats.UseStamina(staminaUsePerSecond * Time.deltaTime))
            {
                isSprinting = true;
            }
            else
            {
                isSprinting = false;
            }
        }
        else
        {
            isSprinting = false;
            stats.RegenerateStamina(stats.staminaRegenRate);
        }
    }


    // <<< ADD: обнаружение столкновения с платформой
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("MovingPlatform"))
        {
            TriggeredMovingPlatform platform = hit.collider.GetComponent<TriggeredMovingPlatform>();
            if (platform != null)
            {
                platform.Activate();
            }

            if (currentPlatform != hit.collider.transform)
            {
                currentPlatform = hit.collider.transform;
                lastPlatformPos = currentPlatform.position;
            }
        }
    }
    // >>>


    // <<< ADD: движение вместе с платформой
    private void LateUpdate()
    {
        if (currentPlatform != null)
        {
            Vector3 delta = currentPlatform.position - lastPlatformPos;
            controller.Move(delta);
            lastPlatformPos = currentPlatform.position;
        }
    }
    // >>>
}
