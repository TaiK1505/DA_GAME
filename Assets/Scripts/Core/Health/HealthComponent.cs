using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{

    [Header("Health Stats")]
    public float maxHealth = 100f;

    public float CurrentHealth { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        // When the object spawns, fill its health bar
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        CurrentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage! Current HP: " + CurrentHealth);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        CurrentHealth += healAmount;
        
        // This  ensures an item can never heal past Max Health
        if (CurrentHealth > maxHealth)
        {
            CurrentHealth = maxHealth;
        }
        
        Debug.Log(gameObject.name + " healed! Current HP: " + CurrentHealth);
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died!");
        // For now, just turn the object off. We will add death animations/pooling later.
        gameObject.SetActive(false);
    }
}
