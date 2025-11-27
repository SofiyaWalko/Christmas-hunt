using UnityEngine;

public class PlayerFallHandler : MonoBehaviour
{
    public float fallThreshold = -200f;
    private CharacterStats characterStats;

    private void Start()
    {
        characterStats = GetComponent<CharacterStats>();
        if (characterStats == null)
        {
            Debug.LogError("PlayerFallHandler: CharacterStats component not found on player!");
        }
    }

    private void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            KillPlayer();
        }
    }

    private void KillPlayer()
    {
        if (characterStats != null && characterStats.currentHealth > 0)
        {
            // Deal massive damage to ensure death
            characterStats.TakeDamage(characterStats.maxHealth + 999);
        }
    }
}
