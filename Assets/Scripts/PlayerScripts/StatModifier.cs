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
        MaxHealth       
    }
    public StatType statType;
    public float multiplier;
    public float duration;
    [HideInInspector] public float expirationTime;

    public StatModifier(StatType type, float mult, float time)
    {
        statType = type;
        multiplier = mult;
        duration = time;
    }

}
