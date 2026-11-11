using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    public LevelingManager levelManager;
    public MasterHubUI hubUI;

    [Header("The Master Pool")]
    [Tooltip("Drag all of your UpgradeData ScriptableObjects in here!")]
    public List<UpgradeData> allAvailableUpgrades = new List<UpgradeData>();

    [Header("UI Card Slots")]
    [Tooltip("Drag your 3 Upgrade_Card objects from Cards_Container in here!")]
    public UpgradeSlotUI[] cardSlots;

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
        List<UpgradeData> tempPool = new List<UpgradeData>(allAvailableUpgrades);

        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (tempPool.Count > 0)
            {
                int randomIndex = Random.Range(0, tempPool.Count);
                UpgradeData chosenUpgrade = tempPool[randomIndex];

                cardSlots[i].SetUpgradeData(chosenUpgrade);
                tempPool.RemoveAt(randomIndex);
            }
            else
            {
                cardSlots[i].SetUpgradeData(null);
            }
        }
    }

    public void OnUpgradePurchased(UpgradeSlotUI purchasedSlot)
    {
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
        if ( isHandLocked && levelManager != null && levelManager.bankedSkillPoints > 0)
        {
            RollRandomUpgrades();
        }
    }
}
