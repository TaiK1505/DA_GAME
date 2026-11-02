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

    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;

    private Button buyButton;

    private void Start()
    {
        // 1. Auto-wire the button so you don't have to use the Inspector OnClick event
        buyButton = GetComponent<Button>();
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(AttemptPurchase);
        }

        // 2. Populate the UI text from your ScriptableObject
        if (upgradeData == null) return;
        if (nameText != null) nameText.text = upgradeData.upgradeName;
        if (descriptionText != null) descriptionText.text = upgradeData.description;
        if (costText != null) costText.text = "Cost: " + upgradeData.skillPointCost;
    }

    public void AttemptPurchase()
    {
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

            buyButton.interactable = false;
            if (costText != null) costText.text = "OWNED";
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
    }

    private void ApplyMechanicUnlock()
    {
        switch (upgradeData.mechanicToUnlock)
        {
            case UpgradeData.MechanicType.ExtraWallBoost:
                Debug.Log("Extra Wall Boost Unlocked!");
                break;
            case UpgradeData.MechanicType.ExtraDash:
                Debug.Log("Extra Dash Unlocked!");
                break;
        }
    }
}