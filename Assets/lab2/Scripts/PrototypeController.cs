using UnityEngine;
using UnityEngine.InputSystem;

public class PrototypeController : MonoBehaviour {

    [Header("Movement Settings")]
    
    public float jumpForce = 5f;
    public float moveSpeed = 5f;

    private Rigidbody rb;
    private PlayerControls playerControls;
    private Vector2 moveInput;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        playerControls = new PlayerControls();

        playerControls.Gameplay.Jump.performed += context => Jump();
    }

    private void OnEnable() {
        playerControls.Gameplay.Enable();
    }

    private void OnDisable() {
        playerControls.Gameplay.Disable();
    }

    private void Update() {
        moveInput = playerControls.Gameplay.Move.ReadValue<Vector2>();

    }

    private void FixedUpdate() {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
    }

    private void Jump() {
        if (Physics.Raycast(transform.position, Vector3.down, 1.1f)) {        
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log("Jump action performed!");
        }
    }


}
