using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Character Stats")]
    public float baseMovementSpeed = 8f; 
    public float baseDamageMultiplier = 1f;  // 1f = 100% normal damage
    public float baseDamageReduction = 0f;   // 0f = 0% reduction
    public float baseDodgeChance = 0f;       // 0f = 0% chance to dodge
    public float baseFrictionMultiplier = 1f; // 1f = 100% normal friction

    // The List of active stat modifiers currently affecting the player
    private List<StatModifier> activeModifiers = new List<StatModifier>();
    
    public void AddModifier(StatModifier modifier)
    {
        // If it's a temporary buff, calculate exactly when it should expire
        if (modifier.duration > 0)
        {
            modifier.expirationTime = Time.time + modifier.duration;
        }
        
        activeModifiers.Add(modifier);
        Debug.Log("Added Modifier: " + modifier.statType + " (" + modifier.multiplier + ")");
    }

    private void Update()
    {
        // Check our tickets every frame. Have any of them expired?
        // (We loop backwards so deleting an item doesn't break the list!)
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            if (activeModifiers[i].duration > 0 && Time.time >= activeModifiers[i].expirationTime)
            {
                Debug.Log("Modifier Expired: " + activeModifiers[i].statType);
                activeModifiers.RemoveAt(i); // Rip up the ticket!
            }
        }
    }

    // --- HOW THE REST OF THE GAME GETS THE FINAL CALCULATED NUMBERS ---

    public float GetCurrentSpeed()
    {
        float finalSpeed = baseMovementSpeed;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.MovementSpeed)
            {
                // If Adrenaline gives 0.5f, we ADD 50% of the base speed!
                finalSpeed += (baseMovementSpeed * mod.multiplier);
            }
        }
        return finalSpeed;
    }

    public float GetCurrentDamageMultiplier()
    {
        float finalMultiplier = baseDamageMultiplier;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.AttackDamage)
            {
                finalMultiplier += mod.multiplier; 
            }
        }
        // Bottom out at 0.1f so a heavy debuff doesn't accidentally heal enemies!
        return Mathf.Max(finalMultiplier, 0.1f); 
    }

    public float GetCurrentDamageReduction()
    {
        float finalReduction = baseDamageReduction;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.DamageReduction)
            {
                finalReduction += mod.multiplier;
            }
        }
        // Cap it at 0.9f (90%) so you can never be 100% immortal
        return Mathf.Clamp(finalReduction, 0f, 0.9f); 
    }

    public float GetCurrentDodgeChance()
    {
        float finalDodge = baseDodgeChance;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.DodgeChance)
            {
                finalDodge += mod.multiplier;
            }
        }
        // Cap it at 0.8f (80%) so the player is never untouchable
        return Mathf.Clamp(finalDodge, 0f, 0.8f); 
    }

    public float GetCurrentFrictionMultiplier()
    {
        float finalMult = baseFrictionMultiplier;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.Friction)
            {
                // A ticket with -0.5f will reduce friction by 50%
                finalMult += mod.multiplier;
            }
        }
        // Don't let friction drop below 10%, otherwise you will slide on ice forever!
        return Mathf.Max(finalMult, 0.1f); 
    }
}
