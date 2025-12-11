using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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
        // Проверяем, что курсор не находится над UI Toolkit элементом
        if (IsPointerOverUIToolkit())
        {
            return; // Не стреляем, если курсор над UI
        }

        // Проверяем кулдаун
        if (Time.time - lastShootTime < shootingCooldown)
            return;

        // Проверяем наличие префаба (для совместимости, если пул не инициализирован)
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

        // Получаем снежок из пула или создаем новый (для совместимости)
        Snowball snowball;
        if (SnowballPool.Instance != null)
        {
            snowball = SnowballPool.Instance.GetSnowball(shootPoint.position);
        }
        else
        {
            // Fallback: создаем снежок напрямую, если пула нет
            GameObject snowballObj = Instantiate(
                snowballPrefab,
                shootPoint.position,
                Quaternion.identity
            );
            snowball = snowballObj.GetComponent<Snowball>();
        }

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
            return;
        }

        // Обновляем время последнего выстрела
        lastShootTime = Time.time;

        Debug.Log("Игрок выстрелил снежком!");
    }

    /// <summary>
    /// Проверяет, находится ли курсор мыши над UI Toolkit элементом
    /// </summary>
    private bool IsPointerOverUIToolkit()
    {
        // Находим все UIDocument компоненты в сцене
        UIDocument[] uiDocuments = FindObjectsOfType<UIDocument>();
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        foreach (UIDocument uiDoc in uiDocuments)
        {
            if (uiDoc.rootVisualElement != null && uiDoc.gameObject.activeInHierarchy)
            {
                // Проверяем, видимо ли UI (не скрыто через DisplayStyle.None)
                if (uiDoc.rootVisualElement.style.display.value == DisplayStyle.None)
                {
                    continue;
                }
                
                // Конвертируем позицию мыши в координаты UI Toolkit
                Vector2 localPoint = RuntimePanelUtils.ScreenToPanel(
                    uiDoc.rootVisualElement.panel,
                    mousePosition
                );
                
                // Проверяем, есть ли элемент под этой позицией
                VisualElement elementUnderPointer = uiDoc.rootVisualElement.panel.Pick(localPoint);
                
                // Проверяем, что элемент не только существует, но и интерактивен
                if (elementUnderPointer != null && elementUnderPointer != uiDoc.rootVisualElement)
                {
                    // Дополнительно проверяем, что элемент видимый и включенный
                    if (elementUnderPointer.style.display.value != DisplayStyle.None &&
                        elementUnderPointer.visible &&
                        elementUnderPointer.enabledSelf)
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
}
