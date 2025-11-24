using UnityEngine;


public class DamageZone : MonoBehaviour
{
    public int damage = 10;
    
    private void OnTriggerEnter(Collider other)    {
        
        CharacterStats stats = other.GetComponent<CharacterStats>();
        if (stats != null)
        {
            stats.TakeDamage(damage);
        }
    }
}
