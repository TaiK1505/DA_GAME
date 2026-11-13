using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeSlotUI : MonoBehaviour
{
    [Header("Data References")]
    public UpgradeData upgradeData;
    public LevelingManager levelManager;
    public PlayerStats playerStats;
    public PlayerController playerController; 
    public MasterHubUI hubUI; 
    public UpgradeManager upgradeManager; 

    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;

    private Button buyButton;

    private void Awake()
    {
        buyButton = GetComponent<Button>();
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(AttemptPurchase);
        }
    }

    // ---> NEW: Allows UpgradeManager to deal a random ScriptableObject to this card <---
    public void SetUpgradeData(UpgradeData newUpgrade)
    {
        upgradeData = newUpgrade;

        if (buyButton == null) buyButton = GetComponent<Button>();

        if (upgradeData == null)
        {
            gameObject.SetActive(false); // Hide card if loot pool is empty
            return;
        }

        gameObject.SetActive(true);
        if (buyButton != null) buyButton.interactable = true;

        if (nameText != null) nameText.text = upgradeData.upgradeName;
        if (descriptionText != null) descriptionText.text = upgradeData.description;
        if (costText != null) costText.text = "Cost: " + upgradeData.skillPointCost;
    }

    public void AttemptPurchase()
    {
        if (upgradeData == null) return;

        if (levelManager.bankedSkillPoints >= upgradeData.skillPointCost)
        {
            levelManager.bankedSkillPoints -= upgradeData.skillPointCost;
            if (hubUI != null) hubUI.RefreshCurrencyUI();

            if (upgradeData.category == UpgradeData.UpgradeCategory.StatBoost)
            {
                ApplyStatBoost();
            }
            else if (upgradeData.category == UpgradeData.UpgradeCategory.MechanicUnlock)
            {
                ApplyMechanicUnlock();
            }

            // Lock THIS card as OWNED
            if (buyButton != null) buyButton.interactable = false;
            if (costText != null) costText.text = "OWNED";

            // Tell the UpgradeManager to lock the OTHER two cards
            if (upgradeManager != null) upgradeManager.OnUpgradePurchased(this);

            // Auto-close the Hub menu after buying (delete or comment out if you want it to stay open so you see it gray out!)
            if (hubUI != null) hubUI.SetHubState(false);
        }
        else
        {
            Debug.Log("Not enough Skill Points!");
        }
    }

    // ---> NEW: Gray out the cards you didn't pick <---
    public void LockCard()
    {
        if (buyButton == null) buyButton = GetComponent<Button>();
        if (buyButton != null) buyButton.interactable = false;
        if (costText != null) costText.text = "LOCKED";
    }

    private void ApplyStatBoost()
    {
        float oldMaxHealthBonus = 0f;

        // 1. If this upgrade builds off a lower-tier prerequisite, remove the old prerequisite's stats first!
        if (upgradeData.prerequisiteUpgrade != null)
        {
            // Check if the old prerequisite gave MaxHealth so we only heal the DIFFERENCE
            foreach (StatModifier oldMod in upgradeData.prerequisiteUpgrade.statModifiers)
            {
                if (oldMod.statType == StatModifier.StatType.MaxHealth)
                {
                    oldMaxHealthBonus += oldMod.multiplier;
                }
            }

            playerStats.RemoveModifiersFromSource(upgradeData.prerequisiteUpgrade);
        }

        // 2. Apply the new Tier's stats and stamp them with this upgradeData as their source
        foreach (StatModifier mod in upgradeData.statModifiers)
        {
            StatModifier permanentTicket = new StatModifier(
                mod.statType, 
                mod.multiplier, 
                0f,
                upgradeData // Stamp the ticket with this UpgradeData!
            );
            playerStats.AddModifier(permanentTicket);

            if (mod.statType == StatModifier.StatType.MaxHealth)
            {
                HealthComponent hp = playerStats.GetComponent<HealthComponent>();
                if (hp != null)
                {
                    // If replacing Tier 1 MaxHealth (+20%) with Tier 2 MaxHealth (+50%), only heal the net +30% difference!
                    float netMultiplier = Mathf.Max(0f, mod.multiplier - oldMaxHealthBonus);
                    float bonusHP = playerStats.baseMaxHealth * netMultiplier;
                    if (bonusHP > 0f) hp.Heal(bonusHP);
                }
            }
        }
    }

    private void ApplyMechanicUnlock()
    {
        if (playerController == null) return;

        switch (upgradeData.mechanicToUnlock)
        {
            case UpgradeData.MechanicType.ExtraWallBoost:
                playerController.maxWallBoosts++;
                playerController.currentWallBoosts++; // Immediately give them the new charge ready to use!
                Debug.Log("Extra Wall Boost Unlocked! Max is now: " + playerController.maxWallBoosts);
                break;

            case UpgradeData.MechanicType.ExtraGadgetDeployLimit:
                playerController.bonusGadgetDeployLimit++;
                Debug.Log("Extra Gadget Capacity Unlocked! Bonus deployables: +" + playerController.bonusGadgetDeployLimit);
                break;

        }
    }
}