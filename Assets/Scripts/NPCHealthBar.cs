using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управление полоской здоровья над NPC.
/// Автоматически обновляется и поворачивается к камере.
/// </summary>
public class NPCHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Slider для отображения здоровья")]
    public Slider healthSlider;
    
    [Tooltip("Image компонент для изменения цвета полоски")]
    public Image fillImage;

    [Header("Settings")]
    [Tooltip("Скрывать полоску при полном здоровье")]
    public bool hideWhenFull = true;
    
    [Tooltip("Высота над NPC в юнитах")]
    public float heightOffset = 2f;

    [Header("Color Settings")]
    [Tooltip("Цвет при полном здоровье")]
    public Color fullHealthColor = Color.green;
    
    [Tooltip("Цвет при среднем здоровье")]
    public Color midHealthColor = Color.yellow;
    
    [Tooltip("Цвет при низком здоровье")]
    public Color lowHealthColor = Color.red;
    
    [Tooltip("Порог среднего здоровья (0-1)")]
    [Range(0f, 1f)]
    public float midHealthThreshold = 0.5f;
    
    [Tooltip("Порог низкого здоровья (0-1)")]
    [Range(0f, 1f)]
    public float lowHealthThreshold = 0.25f;

    private Camera mainCamera;
    private Canvas canvas;

    private void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponent<Canvas>();
        
        // Инициализируем полоску как полную
        if (healthSlider != null)
        {
            healthSlider.value = 1f;
        }
        
        // Скрываем если нужно
        if (hideWhenFull)
        {
            SetVisible(false);
        }
    }

    private void LateUpdate()
    {
        // Billboard эффект - всегда смотрим на камеру
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                           mainCamera.transform.rotation * Vector3.up);
        }
    }

    /// <summary>
    /// Обновление полоски здоровья
    /// </summary>
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthSlider == null)
            return;

        // Вычисляем процент здоровья
        float healthPercent = (float)currentHealth / maxHealth;
        healthSlider.value = healthPercent;

        // Обновляем цвет в зависимости от уровня здоровья
        UpdateColor(healthPercent);

        // Показываем/скрываем полоску
        if (hideWhenFull)
        {
            SetVisible(healthPercent < 1f);
        }
        else
        {
            SetVisible(true);
        }
    }

    /// <summary>
    /// Обновление цвета полоски
    /// </summary>
    private void UpdateColor(float healthPercent)
    {
        if (fillImage == null)
            return;

        if (healthPercent <= lowHealthThreshold)
        {
            // Низкое здоровье - красный
            fillImage.color = lowHealthColor;
        }
        else if (healthPercent <= midHealthThreshold)
        {
            // Среднее здоровье - желтый
            fillImage.color = midHealthColor;
        }
        else
        {
            // Полное здоровье - зеленый
            fillImage.color = fullHealthColor;
        }
    }

    /// <summary>
    /// Показать/скрыть полоску
    /// </summary>
    private void SetVisible(bool visible)
    {
        if (canvas != null)
        {
            canvas.enabled = visible;
        }
    }
}
