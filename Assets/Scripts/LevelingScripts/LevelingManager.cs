using UnityEngine;

public class LevelingManager : MonoBehaviour
{
    [Header("Level Stats")]
    public int playerLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 50;

    [Header("Rewards")]
    public int bankedSkillPoints = 0;

    [Header("Visuals")]
    public GameObject scannerVisual;

    // Called by the XP orbs you pick up
    public void AddXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"Gained {amount} XP. Total: {currentXP}/{xpToNextLevel}");

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
        
        // TODO: Update HUD XP Bar in the next step
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel;
        playerLevel++;
        bankedSkillPoints++;

        // Scale the requirement for the next level (e.g., 50 -> 75 -> 112)
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);

        Debug.Log($"LEVEL UP! You are now Level {playerLevel}. Skill Points: {bankedSkillPoints}");

        if (scannerVisual != null) 
        {
            scannerVisual.SetActive(true);
        }

        // Safety check: If a massive XP drop leveled you up twice, loop it!
        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }
}
