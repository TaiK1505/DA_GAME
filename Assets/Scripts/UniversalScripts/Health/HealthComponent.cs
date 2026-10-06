using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float CurrentHealth { get; private set; }
    
    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void InitializeHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        CurrentHealth = newMaxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        CurrentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage! Current HP: " + CurrentHealth);

        HitFlash hitFlash = GetComponent<HitFlash>();
        if (hitFlash != null) hitFlash.Flash();

        // ---> CLEANED UP: Just tells the GameFeelManager what happened <---
        if (gameObject.CompareTag("Player"))
        {
            if (GameFeelManager.Instance != null)
            {
                GameFeelManager.Instance.TriggerPlayerDamageFeel();
            }
        }

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        CurrentHealth += healAmount;
        
        if (CurrentHealth > maxHealth)
        {
            CurrentHealth = maxHealth;
        }
        
        Debug.Log(gameObject.name + " healed! Current HP: " + CurrentHealth);
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died!");
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}