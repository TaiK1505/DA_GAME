using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Character Stats")]
    public float baseMovementSpeed = 8f; 
    public float baseDamageMultiplier = 1f;  // 1f = 100% normal damage
    public float baseDamageReduction = 0f;   // 0f = 0% reduction
    public float baseDodgeChance = 0f;       
    public float baseFrictionMultiplier = 1f; 
    
    [Header("New Combat Stats")]
    public float baseCritChance = 0.05f; // 5% base crit
    public float baseCritDamage = 1.5f;  // 150% damage on crit
    public float baseLifeSteal = 0f;     // 0% base lifesteal
    public float baseMaxHealth = 100f;
    public float baseAttackSpeedMultiplier = 1f;

    [Header("Cooldown & Rate Stats")]
    public float baseReloadSpeedMultiplier = 1f;       // 1f = 100% normal reload speed
    public float baseWallBoostRechargeMultiplier = 1f; // 1f = 100% normal recharge rate
    public float baseAltFireCooldownReduction = 0f;    // 0f = 0% cooldown reduction
    public float baseGadgetCooldownReduction = 0f;
    public float baseDashCooldownReduction = 0f;

    // The List of active stat modifiers currently affecting the player
    private List<StatModifier> activeModifiers = new List<StatModifier>();
    
    public void AddModifier(StatModifier modifier)
    {
        if (modifier.duration > 0)
        {
            modifier.expirationTime = Time.time + modifier.duration;
        }
        
        activeModifiers.Add(modifier);
        Debug.Log("Added Modifier: " + modifier.statType + " (" + modifier.multiplier + ")");
    }

    public void RemoveModifiersFromSource(UpgradeData sourceToRemove)
    {
        if (sourceToRemove == null) return;

        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            if (activeModifiers[i].sourceUpgrade == sourceToRemove)
            {
                Debug.Log("Replaced lower-tier modifier: " + activeModifiers[i].statType + " from " + sourceToRemove.upgradeName);
                activeModifiers.RemoveAt(i);
            }
        }
    }

    private void Update()
    {
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            if (activeModifiers[i].duration > 0 && Time.time >= activeModifiers[i].expirationTime)
            {
                Debug.Log("Modifier Expired: " + activeModifiers[i].statType);
                activeModifiers.RemoveAt(i); 
            }
        }
    }

    // --- CALCULATORS ---

    public float GetCurrentSpeed()
    {
        float finalSpeed = baseMovementSpeed;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.MovementSpeed)
                finalSpeed += (baseMovementSpeed * mod.multiplier);
        }
        return finalSpeed;
    }

    public float GetCurrentDamageMultiplier()
    {
        float finalMultiplier = baseDamageMultiplier;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.AttackDamage)
                finalMultiplier += mod.multiplier; 
        }
        return Mathf.Max(finalMultiplier, 0.1f); 
    }

    public float GetCurrentDamageReduction()
    {
        float finalReduction = baseDamageReduction;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.DamageReduction)
                finalReduction += mod.multiplier;
        }
        return Mathf.Clamp(finalReduction, 0f, 0.9f); 
    }

    public float GetCurrentDodgeChance()
    {
        float finalDodge = baseDodgeChance;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.DodgeChance)
                finalDodge += mod.multiplier;
        }
        return Mathf.Clamp(finalDodge, 0f, 0.8f); 
    }

    public float GetCurrentFrictionMultiplier()
    {
        float finalMult = baseFrictionMultiplier;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.Friction)
                finalMult += mod.multiplier;
        }
        return Mathf.Max(finalMult, 0.1f); 
    }

    public float GetCurrentCritChance()
    {
        float finalCrit = baseCritChance;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.CritChance)
                finalCrit += mod.multiplier;
        }
        return Mathf.Clamp(finalCrit, 0f, 1f); 
    }

    public float GetCurrentCritDamage()
    {
        float finalCritDmg = baseCritDamage;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.CritDamage)
                finalCritDmg += mod.multiplier;
        }
        return finalCritDmg;
    }

    public float GetCurrentLifeSteal()
    {
        float finalLifeSteal = baseLifeSteal;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.LifeSteal)
                finalLifeSteal += mod.multiplier;
        }
        return Mathf.Clamp(finalLifeSteal, 0f, 1f); 
    }

    public float GetCurrentMaxHealth()
    {
        float finalMaxHealth = baseMaxHealth;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.MaxHealth)
            {
                // If the upgrade is 0.2f, you gain +20% extra Max Health!
                finalMaxHealth += (baseMaxHealth * mod.multiplier);
            }
        }
        return finalMaxHealth;
    }

    public float GetCurrentAttackSpeedMultiplier()
    {
        float finalAttackSpeed = baseAttackSpeedMultiplier;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.AttackSpeed)
                finalAttackSpeed += mod.multiplier;
        }
        return Mathf.Max(finalAttackSpeed, 0.1f); // Never drop below 10% speed
    }

    public float GetCurrentReloadSpeedMultiplier()
    {
        float finalReload = baseReloadSpeedMultiplier;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.ReloadSpeed)
                finalReload += mod.multiplier;
        }
        return Mathf.Max(finalReload, 0.1f);
    }

    public float GetCurrentWallBoostRechargeMultiplier()
    {
        float finalRecharge = baseWallBoostRechargeMultiplier;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.WallBoostRecharge)
                finalRecharge += mod.multiplier;
        }
        return Mathf.Max(finalRecharge, 0.1f);
    }

    public float GetCurrentAltFireCooldownReduction()
    {
        float finalCDR = baseAltFireCooldownReduction;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.AltFireCooldown)
                finalCDR += mod.multiplier;
        }
        return Mathf.Clamp(finalCDR, 0f, 0.8f); // Max 80% cooldown reduction
    }

    public float GetCurrentGadgetCooldownReduction()
    {
        float finalCDR = baseGadgetCooldownReduction;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.GadgetCooldown)
                finalCDR += mod.multiplier;
        }
        return Mathf.Clamp(finalCDR, 0f, 0.8f); // Max 80% cooldown reduction
    }
    
    public float GetCurrentDashCooldownReduction()
    {
        float finalCDR = baseDashCooldownReduction;
        foreach (StatModifier mod in activeModifiers)
        {
            if (mod.statType == StatModifier.StatType.DashCooldown)
                finalCDR += mod.multiplier;
        }
        return Mathf.Clamp(finalCDR, 0f, 0.8f); // Max 80% cooldown reduction
    }


}