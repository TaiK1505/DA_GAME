using UnityEngine;

public class HealthDrop : Pickup
{
    [Header("Health Settings")]
    public float healAmount = 20f;

    protected override void OnCollected(GameObject player)
    {
        HealthComponent health = player.GetComponent<HealthComponent>();
        
        // Only consume the drop if the player actually needs healing
        if (health != null && health.CurrentHealth < health.maxHealth)
        {
            health.Heal(healAmount);
            
            // TODO: Trigger GameFeelManager for a heal sound/flash here
            
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.ReturnObject(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
