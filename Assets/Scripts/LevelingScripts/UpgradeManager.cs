using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    public LevelingManager levelManager;
    public MasterHubUI hubUI;

    [Header("The Master Pool")]
    [Tooltip("Drag all of your UpgradeData ScriptableObjects (Tier 1, 2, and 3) in here!")]
    public List<UpgradeData> allAvailableUpgrades = new List<UpgradeData>();

    [Header("UI Card Slots")]
    [Tooltip("Drag your 3 Upgrade_Card objects from Cards_Container in here!")]
    public UpgradeSlotUI[] cardSlots;

    [Header("Floor & Tier Weighting")]
    [Tooltip("Tracks which floor the player is on. Increase this when transitioning to a new floor!")]
    public int currentFloor = 1;

    [Tooltip("How much more likely a Tier 2/3 upgrade is to spawn if you already own its prerequisite! (e.g., 3 = 3x more likely)")]
    public float prerequisiteOwnedMultiplier = 3.5f;

    [Tooltip("Tracks all upgrades purchased this run (plus any that were replaced by higher tiers).")]
    public List<UpgradeData> purchasedUpgrades = new List<UpgradeData>();
    private HashSet<UpgradeData> replacedPrerequisites = new HashSet<UpgradeData>();

    // Tracks if the current 3 cards are locked out from a completed purchase
    private bool isHandLocked = false;

    private void Start()
    {
        RollRandomUpgrades();
    }

    public void RollRandomUpgrades()
    {
        if (allAvailableUpgrades.Count == 0) return;

        isHandLocked = false;

        // 1. Build a candidate pool of valid upgrades the player DOES NOT already own (or hasn't already upgraded past)
        List<UpgradeData> candidatePool = new List<UpgradeData>();
        foreach (UpgradeData upg in allAvailableUpgrades)
        {
            if (upg == null) continue;

            // Don't offer an upgrade they already own, or a Tier 1 upgrade they already replaced with Tier 2!
            if (purchasedUpgrades.Contains(upg) || replacedPrerequisites.Contains(upg))
                continue;

            // Only include if its tier weight on the current floor is > 0
            if (GetUpgradeWeight(upg) > 0f)
            {
                candidatePool.Add(upg);
            }
        }

        // 2. Roll 3 unique cards using weighted probabilities
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (candidatePool.Count > 0)
            {
                UpgradeData chosenUpgrade = PickWeightedUpgrade(candidatePool);
                cardSlots[i].SetUpgradeData(chosenUpgrade);
                candidatePool.Remove(chosenUpgrade); // Prevent duplicates in the same 3-card hand!
            }
            else
            {
                cardSlots[i].SetUpgradeData(null);
            }
        }
    }

    private UpgradeData PickWeightedUpgrade(List<UpgradeData> pool)
    {
        float totalWeight = 0f;
        foreach (UpgradeData upg in pool)
        {
            totalWeight += GetUpgradeWeight(upg);
        }

        float randomRoll = Random.Range(0f, totalWeight);
        float currentSum = 0f;

        foreach (UpgradeData upg in pool)
        {
            currentSum += GetUpgradeWeight(upg);
            if (randomRoll <= currentSum)
            {
                return upg;
            }
        }

        return pool[pool.Count - 1];
    }

    private float GetUpgradeWeight(UpgradeData upg)
    {
        float baseWeight = 0f;

        // Floor-based Tier probabilities (Floor 1 has 0% Tier 3; higher floors increase Tier 2 & 3!)
        if (currentFloor <= 1)
        {
            if (upg.tier == 1) baseWeight = 85f;
            else if (upg.tier == 2) baseWeight = 15f;
            else if (upg.tier >= 3) baseWeight = 0f; // 0% chance for Tier 3 on Floor 1
        }
        else if (currentFloor == 2)
        {
            if (upg.tier == 1) baseWeight = 55f;
            else if (upg.tier == 2) baseWeight = 35f;
            else if (upg.tier >= 3) baseWeight = 10f;
        }
        else // Floor 3+
        {
            if (upg.tier == 1) baseWeight = 25f;
            else if (upg.tier == 2) baseWeight = 50f;
            else if (upg.tier >= 3) baseWeight = 25f;
        }

        // SYNERGY BOOST: If this upgrade builds off a lower-tier upgrade AND we currently own that prerequisite, boost its chance!
        if (upg.prerequisiteUpgrade != null && purchasedUpgrades.Contains(upg.prerequisiteUpgrade))
        {
            baseWeight *= prerequisiteOwnedMultiplier;
        }

        return baseWeight;
    }

    public void OnUpgradePurchased(UpgradeSlotUI purchasedSlot)
    {
        if (purchasedSlot != null && purchasedSlot.upgradeData != null)
        {
            UpgradeData bought = purchasedSlot.upgradeData;

            // Record that we now own this upgrade
            if (!purchasedUpgrades.Contains(bought))
            {
                purchasedUpgrades.Add(bought);
            }

            // If this upgrade replaced a lower-tier prerequisite, move the prerequisite to the replaced list!
            if (bought.prerequisiteUpgrade != null)
            {
                purchasedUpgrades.Remove(bought.prerequisiteUpgrade);
                replacedPrerequisites.Add(bought.prerequisiteUpgrade);
            }
        }

        // 1. Check if the player STILL has skill points left to spend!
        if (levelManager != null && levelManager.bankedSkillPoints > 0)
        {
            // Immediately deal 3 fresh cards and keep the menu open so they can spend their next point!
            RollRandomUpgrades();
        }
        else
        {
            // 2. Out of points! Lock the other two cards and close the Hub
            isHandLocked = true;
            foreach (UpgradeSlotUI slot in cardSlots)
            {
                if (slot != null && slot != purchasedSlot)
                {
                    slot.LockCard();
                }
            }

            if (hubUI != null) hubUI.SetHubState(false);
        }
    }

    // Safety net: Call this when opening the Hub or Leveling Up
    public void CheckAndRefreshIfCanAfford()
    {
        if (isHandLocked && levelManager != null && levelManager.bankedSkillPoints > 0)
        {
            RollRandomUpgrades();
        }
    }
}
