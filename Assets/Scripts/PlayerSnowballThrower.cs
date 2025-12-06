using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Скрипт для стрельбы снежками игроком.
/// Использует новую Input System и интегрируется с PlayerControls.
/// Привязан к действию "Catch" (ЛКМ).
/// </summary>
public class PlayerSnowballThrower : MonoBehaviour
{
    [Header("Snowball Settings")]
    [Tooltip("Префаб снежка для стрельбы")]
    public GameObject snowballPrefab;
    
    [Header("Shooting Settings")]
    [Tooltip("Точка спавна снежка (обычно камера игрока)")]
    public Transform shootPoint;
    
    [Tooltip("Скорость полета снежка")]
    public float snowballSpeed = 20f;
    
    [Tooltip("Интервал между выстрелами в секундах")]
    public float shootingCooldown = 0.5f;
    
    private float lastShootTime = -999f;
    private Camera playerCamera;
    private PlayerControls controls;

    private void Awake()
    {
        // Создаем экземпляр PlayerControls
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        // Включаем Input Actions
        controls.Enable();
        
        // Подписываемся на действие "Catch" (ЛКМ)
        controls.Gameplay.Catch.performed += OnCatchPerformed;
    }

    private void OnDisable()
    {
        // Отписываемся от действия
        controls.Gameplay.Catch.performed -= OnCatchPerformed;
        
        // Отключаем Input Actions
        controls.Disable();
    }

    private void Start()
    {
        // Получаем камеру игрока
        playerCamera = Camera.main;
        
        // Если точка спавна не назначена, используем камеру
        if (shootPoint == null && playerCamera != null)
        {
            shootPoint = playerCamera.transform;
        }
    }

    /// <summary>
    /// Обработчик нажатия ЛКМ (действие Catch)
    /// </summary>
    private void OnCatchPerformed(InputAction.CallbackContext context)
    {
        TryShootSnowball();
    }

    /// <summary>
    /// Попытка выстрелить снежком
    /// </summary>
    private void TryShootSnowball()
    {
        // Проверяем кулдаун
        if (Time.time - lastShootTime < shootingCooldown)
            return;

        // Проверяем наличие префаба
        if (snowballPrefab == null)
        {
            Debug.LogWarning("PlayerSnowballThrower: Snowball prefab не назначен!");
            return;
        }

        // Проверяем наличие точки спавна
        if (shootPoint == null)
        {
            Debug.LogWarning("PlayerSnowballThrower: Shoot point не назначен!");
            return;
        }

        // Вычисляем направление выстрела (от камеры вперед)
        Vector3 shootDirection = shootPoint.forward;

        // Создаем снежок
        GameObject snowballObj = Instantiate(snowballPrefab, shootPoint.position, Quaternion.identity);
        
        // Получаем компонент Snowball и настраиваем его
        Snowball snowball = snowballObj.GetComponent<Snowball>();
        if (snowball != null)
        {
            // Устанавливаем тип стрелка - игрок
            snowball.SetShooterType(Snowball.ShooterType.Player);
            
            // Запускаем снежок
            snowball.Launch(shootDirection, snowballSpeed);
        }
        else
        {
            Debug.LogError("Префаб снежка не содержит компонент Snowball!");
            Destroy(snowballObj);
            return;
        }

        // Обновляем время последнего выстрела
        lastShootTime = Time.time;

        Debug.Log("Игрок выстрелил снежком!");
    }
}
