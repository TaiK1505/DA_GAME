using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public enum UpgradeCategory { StatBoost, MechanicUnlock }
    public enum MechanicType { ExtraWallBoost, ExtraDash, ExtraGadgetDeployLimit }

    [Header("UI Info")]
    public string upgradeName;
    [TextArea] public string description;
    public int skillPointCost = 1;

    [Header("Tier & Progression")]
    [Range(1, 3)] public int tier = 1;
    public UpgradeData prerequisiteUpgrade;

    [Header("Upgrade Routing")]
    public UpgradeCategory category;

    [Header("Stat Boosts (Can add 1 or multiple for Tier 2/3!)")]
    public List<StatModifier> statModifiers = new List<StatModifier>();

    [Header("Mechanic Unlock")]
    public MechanicType mechanicToUnlock;
}