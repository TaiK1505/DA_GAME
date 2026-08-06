using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image healthBarFill;

    [Header("Player")]
    public HealthComponent playerHealth;
    
    
    // Update is called once per frame
    void Update()
    {
        if (playerHealth == null || healthBarFill == null) return;

        // Makes current health into a percentage 
        float healthPercentage = (float)playerHealth.CurrentHealth / (float)playerHealth.maxHealth;

    
        healthBarFill.fillAmount = healthPercentage;
    }
}
