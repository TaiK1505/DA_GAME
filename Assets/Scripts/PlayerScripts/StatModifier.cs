using UnityEngine;

[System.Serializable]
public class StatModifier 
{
    public enum StatType { MovementSpeed, DamageReduction, AttackDamage, AttackSpeed, DodgeChance, Friction }
    public StatType statType;
    public float multiplier;

    // How long does the ticket last (If 0, it's a permanent passive item!)
    public float duration;

    // The Unity stopwatch variable to track when this ticket expires
    [HideInInspector] public float expirationTime;

    // Constructor to easily create these tickets in code
    public StatModifier(StatType type, float mult, float time)
    {
        statType = type;
        multiplier = mult;
        duration = time;
    }
}
