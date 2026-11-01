using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Upgrade Data")]
public class UpgradeData : ScriptableObject
{
   public enum UpgradeCategory { StatBoost, MechanicUnlock }
    public enum MechanicType { ExtraWallBoost, ExtraDash }

    [Header("UI Info")]
    public string upgradeName;
    public string description;
    public int skillPointCost = 1;

    [Header("Upgrade Routing")]
    public UpgradeCategory category;

    [Header("Stat Boost")]
    public StatModifier.StatType statToBoost; 
    public float boostMultiplier; 

    [Header("Mechanic Unlock")]
    public MechanicType mechanicToUnlock;
}