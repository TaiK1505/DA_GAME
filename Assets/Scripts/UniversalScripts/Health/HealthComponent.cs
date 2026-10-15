using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float CurrentHealth { get; private set; }

    [Header("HyperShield Stats")]
    public float maxHyperShield = 25f;
    [field: SerializeField] public float CurrentHyperShield { get; private set; }
    
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
        // 1. TRIGGER VISUAL FEEDBACK IMMEDIATELY FOR ANY HIT
        HitFlash hitFlash = GetComponent<HitFlash>();
        if (hitFlash != null) hitFlash.Flash();

        if (gameObject.CompareTag("Player"))
        {
            if (GameFeelManager.Instance != null)
            {
                GameFeelManager.Instance.TriggerPlayerDamageFeel();
            }
        }

        // 2. HYPERSHIELD INTERCEPTS DAMAGE FIRST
        if (CurrentHyperShield > 0)
        {
            float leftoverDamage = damageAmount - CurrentHyperShield;
            CurrentHyperShield -= damageAmount;
            
            if (CurrentHyperShield < 0) CurrentHyperShield = 0;
            
            // If the shield absorbed all of the hit, stop here (health is safe)
            if (leftoverDamage <= 0) 
            {
                Debug.Log(gameObject.name + " shield absorbed the hit! Shield remaining: " + CurrentHyperShield);
                return; 
            }
            
            // Otherwise, pass the remaining damage to base health
            damageAmount = leftoverDamage;
        }

        // 3. NORMAL HEALTH REDUCTION
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
        
        if (CurrentHealth > maxHealth)
        {
            CurrentHealth = maxHealth;
        }
        
        Debug.Log(gameObject.name + " healed! Current HP: " + CurrentHealth);
    }

    // ---> NEW: Methods to handle adding and losing the overshield <---
    public void AddOverhealth(float amount)
    {
        CurrentHyperShield += amount;
        if (CurrentHyperShield > maxHyperShield)
        {
            CurrentHyperShield = maxHyperShield;
        }
    }

    public void DecayOverhealth(float amount)
    {
        if (CurrentHyperShield > 0)
        {
            CurrentHyperShield -= amount;
            if (CurrentHyperShield < 0) CurrentHyperShield = 0;
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died!");
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}