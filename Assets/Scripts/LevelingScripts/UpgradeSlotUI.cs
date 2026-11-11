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
            else if (upgradeData.category == UpgradeData.UpgradeCategory.MechanicUnlock) // Matches your UpgradeData enum
            {
                ApplyMechanicUnlock();
            }

            buyButton.interactable = false;
            if (costText != null) costText.text = "OWNED";

            // Tell the UpgradeManager we bought an upgrade (optional: re-roll or lock)
            if (upgradeManager != null) upgradeManager.OnUpgradePurchased(upgradeData);
        }
        else
        {
            Debug.Log("Not enough Skill Points!");
        }
    }

    private void ApplyStatBoost()
    {
        StatModifier permanentTicket = new StatModifier(
            upgradeData.statToBoost, 
            upgradeData.boostMultiplier, 
            0f 
        );
        playerStats.AddModifier(permanentTicket);

        // If it's a MaxHealth upgrade, also heal the player by the bonus amount!
        if (upgradeData.statToBoost == StatModifier.StatType.MaxHealth)
        {
            HealthComponent hp = playerStats.GetComponent<HealthComponent>();
            if (hp != null)
            {
                float bonusHP = playerStats.baseMaxHealth * upgradeData.boostMultiplier;
                hp.Heal(bonusHP);
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
                break;
            case UpgradeData.MechanicType.ExtraDash:
                // playerController.maxDashes++;
                break;
        }
    }
}