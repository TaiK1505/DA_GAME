using UnityEngine;

public class MasterHubUI : MonoBehaviour
{
    [Header("Master Panel")]
    public GameObject hubPanel;

    [Header("Sub-Menus (0=Map, 1=Upgrades)")]
    public GameObject[] menuTabs;
    private int currentTabIndex = 0;

    [Header("Dependencies")]
    public LevelingManager levelManager;
    public PlayerController playerController;
    public GameObject upgradeNotificationIcon; 
    public GameObject crosshairCanvas; // Disables your combat crosshair so you can click!

    public void ToggleHub()
    {
        SetHubState(!hubPanel.activeSelf);
    }

    public void SetHubState(bool isOpen)
    {
        hubPanel.SetActive(isOpen);
        
        // 1. Lock/Unlock the player's combat inputs
        if (playerController != null) 
        {
            playerController.isUIActive = isOpen;
            
            // Safety: Force the gun to stop shooting if they open the menu while holding Left Click
            if (isOpen && playerController.arsenal != null) playerController.arsenal.ReleaseTrigger();
        }

        // 2. Manage the Crosshair (Hide combat crosshair, show standard OS mouse)
        if (crosshairCanvas != null) crosshairCanvas.SetActive(!isOpen);
        Cursor.visible = isOpen;

        if (isOpen)
        {
            // 3. Context-Sensitive Routing
            bool hasPoints = levelManager != null && levelManager.bankedSkillPoints > 0;
            currentTabIndex = hasPoints ? 1 : 0; // Force Upgrades tab if points exist, else Map
            
            if (upgradeNotificationIcon != null) upgradeNotificationIcon.SetActive(hasPoints);
            
            RefreshTabs();
        }
    }

    public void CycleTab(int direction)
    {
        currentTabIndex += direction;
        
        // Loop the menu
        if (currentTabIndex >= menuTabs.Length) currentTabIndex = 0;
        else if (currentTabIndex < 0) currentTabIndex = menuTabs.Length - 1;
        
        RefreshTabs();
    }

    private void RefreshTabs()
    {
        for (int i = 0; i < menuTabs.Length; i++)
        {
            if (menuTabs[i] != null) menuTabs[i].SetActive(i == currentTabIndex);
        }
    }
}