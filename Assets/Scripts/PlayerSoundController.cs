using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(SurfaceDetector))]
public class PlayerSoundController : MonoBehaviour
{
    [Header("Default Audio Clips")]
    public AudioClip footstepClip;
    public AudioClip jumpClip;

    [Header("Settings")]
    public float stepInterval = 0.5f;

    private AudioSource _audioSource;
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    
    private SurfaceDetector _surfaceDetector;
    private SurfaceMaterial _currentMaterial;

    private float _stepTimer;
    private bool _isMoving;
    private bool _wasMoving;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _playerInput = GetComponent<PlayerInput>();
        _surfaceDetector = GetComponent<SurfaceDetector>();

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
            if (!_wasMoving)
            {
                PlayFootstep();
                _stepTimer = 0;
            }
            else
            {
                _stepTimer += Time.deltaTime;
                if (_stepTimer >= stepInterval)
                {
                    PlayFootstep();
                    _stepTimer = 0;
                }
            }
        }
        else
        {
            _stepTimer = 0;
            
            if (_wasMoving)
            {
                _audioSource.Stop();
            }
        }

        _wasMoving = _isMoving;
    }

    private void PlayFootstep()
    {
        AudioClip clipToPlay = footstepClip;
        float pitchMin = 0.9f;
        float pitchMax = 1.1f;

        // Используем звуки материала если доступны
        if (_currentMaterial != null)
        {
            AudioClip materialClip = _currentMaterial.GetRandomFootstepClip();
            if (materialClip != null)
            {
                clipToPlay = materialClip;
                pitchMin = _currentMaterial.footstepPitchMin;
                pitchMax = _currentMaterial.footstepPitchMax;
            }
        }

        if (clipToPlay == null)
            return;

        _audioSource.pitch = Random.Range(pitchMin, pitchMax);
        float volume = AudioManager.Instance != null ? AudioManager.Instance.GetSFXVolume() : 1f;
        
        // Применяем множитель громкости материала
        if (_currentMaterial != null)
        {
            volume *= _currentMaterial.volumeMultiplier;
        }
        
        _audioSource.volume = volume;
        _audioSource.PlayOneShot(clipToPlay);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        AudioClip clipToPlay = jumpClip;

        // Используем звук прыжка материала если доступен
        if (_currentMaterial != null && _currentMaterial.jumpClip != null)
        {
            clipToPlay = _currentMaterial.jumpClip;
        }

        if (clipToPlay == null)
            return;

        _audioSource.pitch = 1f;
        float volume = AudioManager.Instance != null ? AudioManager.Instance.GetSFXVolume() : 1f;
        
        // Применяем множитель громкости материала
        if (_currentMaterial != null)
        {
            volume *= _currentMaterial.volumeMultiplier;
        }
        
        _audioSource.volume = volume;
        _audioSource.PlayOneShot(clipToPlay);
    }

    /// <summary>
    /// Устанавливает текущий материал поверхности
    /// </summary>
    public void SetCurrentMaterial(SurfaceMaterial material)
    {
        _currentMaterial = material;
    }
}
