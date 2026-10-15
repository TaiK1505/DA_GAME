using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image healthBarFill;
    public Image overshieldBarFill; // ---> NEW: Slot for the overshield bar

    [Header("Player")]
    public HealthComponent playerHealth;
    
    // Update is called once per frame
    void Update()
    {
        if (playerHealth == null) return;

        // 1. Update Base Health Bar
        if (healthBarFill != null && playerHealth.maxHealth > 0)
        {
            float healthPercentage = playerHealth.CurrentHealth / playerHealth.maxHealth;
            healthBarFill.fillAmount = healthPercentage;
        }

        // 2. Update Overshield Bar
        if (overshieldBarFill != null && playerHealth.maxHyperShield > 0)
        {
            float overshieldPercentage = playerHealth.CurrentHyperShield / playerHealth.maxHyperShield;
            overshieldBarFill.fillAmount = overshieldPercentage;
        }
    }
}