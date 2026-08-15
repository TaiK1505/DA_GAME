using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class AdrenalineCore : MonoBehaviour
{
    [Header("Class Setup")]
    public PlayerClassData currentClass; 

    [Header("Adrenaline Settings")]
    public float adrenalineCooldown = 10f;
    private float lastAdrenalineTime = -100f;
    public float baseDuration = 3f; 
    
    
    [HideInInspector] public float bonusDuration = 0f; 

    [Header("In-Run Upgrades (Skill Tree)")]
    // This list starts completely empty every run! 
    public List<StatModifier> inRunUpgrades = new List<StatModifier>();

   

    private PlayerStats myStats;
    private PlayerControls controls;

    private void Awake()
    {
        myStats = GetComponent<PlayerStats>();
        
        controls = new PlayerControls();

        controls.Player.Adrenalin.performed += ctx => AttemptAdrenaline();
    }

    private void OnEnable() { controls.Enable(); }
    private void OnDisable() { controls.Disable(); }

    private void AttemptAdrenaline()
    {
        if (Time.time >= lastAdrenalineTime + adrenalineCooldown)
        {
            ActivateAdrenaline();
        }
        else
        {
            float timeLeft = (lastAdrenalineTime + adrenalineCooldown) - Time.time;
            Debug.Log("Adrenaline is on cooldown! Wait " + timeLeft.ToString("F1") + " seconds.");
        }
    }

    private void ActivateAdrenaline()
    {
        if (currentClass == null) return; 

        lastAdrenalineTime = Time.time;

        float finalDuration = baseDuration + bonusDuration;

        Debug.Log($"ADRENALINE INJECTED: {currentClass.className} for {finalDuration} seconds!");

        // --- TRAY 1: Base Class Tickets ---
        foreach (StatModifier baseTicket in currentClass.baseAdrenalineModifiers)
        {
            StatModifier newTicket = new StatModifier(
                baseTicket.statType, 
                baseTicket.multiplier, 
                finalDuration // <-- THE OVERRIDE! We ignore the SO and use the Master Timer!
            );
            myStats.AddModifier(newTicket);
        }

        // --- TRAY 2: In-Run Upgrades ---
        foreach (StatModifier upgradeTicket in inRunUpgrades)
        {
            StatModifier newTicket = new StatModifier(
                upgradeTicket.statType, 
                upgradeTicket.multiplier, 
                finalDuration // <-- OVERRIDE HERE TOO!
            );
            myStats.AddModifier(newTicket);
        }
    }
}
