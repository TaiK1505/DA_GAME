using UnityEngine;

[System.Serializable] 
public class StatModifier
{
    public enum StatType 
    { 
        MovementSpeed, 
        DamageReduction, 
        AttackDamage, 
        AttackSpeed, 
        DodgeChance, 
        Friction,       
        CritChance,     
        CritDamage,     
        LifeSteal,      
        MaxHealth,      
        ReloadSpeed,        
        AltFireCooldown,    
        WallBoostRecharge,  
        GadgetCooldown,
        DashCooldown, 
    }
    public StatType statType;
    public float multiplier;
    public float duration;
    [HideInInspector] public float expirationTime;
    [HideInInspector] public UpgradeData sourceUpgrade;

    public StatModifier(StatType type, float mult, float time, UpgradeData source = null)
    {
        statType = type;
        multiplier = mult;
        duration = time;
        sourceUpgrade = source;
    }

}
