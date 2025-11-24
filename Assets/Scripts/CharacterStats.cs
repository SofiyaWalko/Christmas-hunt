using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    //События
    public static event Action<int, int> OnHealthChanged;
    public static event Action<float, float> OnStaminaChanged; 

    [Header("Primary Stats")]
    public int strength = 10;

    [Header("Derived Stats")]
    public int maxHealth;
    public int currentHealth { get; private set; }

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina { get; private set; }
    public float staminaRegenRate = 10f; 

    private void Awake()
    {
        
        maxHealth = strength * 10;
        currentHealth = maxHealth;

        currentStamina = maxStamina;
    }

    private void Start()
    {
        // Сообщаем UI начальные значения
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    //HEALTH
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"{transform.name} takes {damage} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log(transform.name + " died.");
        
    }

    //STAMINA
    public bool UseStamina(float amount)
    {
        if (currentStamina < amount)
            return false; // недостаточно стамины

        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        return true;
    }

    public void RegenerateStamina(float amount)
    {
        currentStamina += amount * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
}
