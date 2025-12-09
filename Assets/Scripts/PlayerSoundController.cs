using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerSoundController : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip footstepClip;
    public AudioClip jumpClip;

    [Header("Settings")]
    public float stepInterval = 0.5f;

    private AudioSource _audioSource;
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;

    private float _stepTimer;
    private bool _isMoving;
    private bool _wasMoving; // Отслеживаем изменение состояния

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _playerInput = GetComponent<PlayerInput>();

        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];
    }

    private void OnEnable()
    {
        _jumpAction.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        _jumpAction.performed -= OnJumpPerformed;
    }

    private void Update()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();
        _isMoving = input.sqrMagnitude > 0.01f;

        if (_isMoving)
        {
            _stepTimer += Time.deltaTime;
            if (_stepTimer >= stepInterval)
            {
                PlayFootstep();
                _stepTimer = 0;
            }
        }
        else
        {
            // Когда остановились, сбрасываем таймер
            _stepTimer = 0;
            
            // Если только что перестали ходить, останавливаем звук
            if (_wasMoving)
            {
                _audioSource.Stop();
            }
        }

        _wasMoving = _isMoving;
    }

    private void PlayFootstep()
    {
        _audioSource.pitch = Random.Range(0.9f, 1.1f);
        _audioSource.volume = AudioManager.Instance != null ? AudioManager.Instance.GetSFXVolume() : 1f;
        _audioSource.PlayOneShot(footstepClip);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        _audioSource.pitch = 1f;
        _audioSource.volume = AudioManager.Instance != null ? AudioManager.Instance.GetSFXVolume() : 1f;
        _audioSource.PlayOneShot(jumpClip);
    }
}
