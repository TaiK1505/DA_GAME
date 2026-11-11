using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("The Master Pool")]
    [Tooltip("Drag all of your UpgradeData ScriptableObjects in here!")]
    public List<UpgradeData> allAvailableUpgrades = new List<UpgradeData>();

    [Header("UI Card Slots")]
    [Tooltip("Drag your 3 Upgrade_Card objects from Cards_Container in here!")]
    public UpgradeSlotUI[] cardSlots;

    private void Start()
    {
        // Deal the initial 3 random cards when the run starts
        RollRandomUpgrades();
    }

    public void RollRandomUpgrades()
    {
        if (allAvailableUpgrades.Count == 0) return;

        // Create a temporary copy of the pool so we don't pick the same upgrade twice on the same screen
        List<UpgradeData> tempPool = new List<UpgradeData>(allAvailableUpgrades);

        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (tempPool.Count > 0)
            {
                int randomIndex = Random.Range(0, tempPool.Count);
                UpgradeData chosenUpgrade = tempPool[randomIndex];

                // Inject into the card UI
                cardSlots[i].SetUpgradeData(chosenUpgrade);

                // Remove from tempPool so Card 2 and Card 3 can't roll the exact same SO
                tempPool.RemoveAt(randomIndex);
            }
            else
            {
                // If you have fewer than 3 upgrades in the pool, hide the extra card slots
                cardSlots[i].SetUpgradeData(null);
            }
        }
    }

    public void OnUpgradePurchased(UpgradeData purchasedUpgrade)
    {
        // Right now, when you buy a card, it turns to "OWNED".
        // If you want all 3 cards to immediately re-roll into 3 fresh choices as soon as you buy one, 
        // just uncomment the line below!
        
        // RollRandomUpgrades();
    }
}
