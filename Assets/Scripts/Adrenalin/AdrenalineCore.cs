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

    [Header("Ghost Trail VFX")]
    public float ghostSpawnRate = 0.1f;
    public float ghostFadeDuration = 0.5f;
    public Color ghostColor = new Color(1f, 1f, 1f, 0.5f);

    private SpriteRenderer playerSprite;
    
    
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
        playerSprite = GetComponentInChildren<SpriteRenderer>();

        controls.Player.Adrenalin.performed += ctx => AttemptAdrenaline();
    }

    private void OnEnable() { controls.Enable(); }
    private void OnDisable() { controls.Disable(); }

    private void AttemptAdrenaline()
    {
        // Calculate exactly how long the ability lasts right now
        float finalDuration = baseDuration + bonusDuration;
        
        // Total time before we can use it again = Active Time + Cooldown Time
        float totalLockoutTime = finalDuration + adrenalineCooldown;

        if (Time.time >= lastAdrenalineTime + totalLockoutTime)
        {
            ActivateAdrenaline();
        }
        else
        {
            float timeLeft = (lastAdrenalineTime + totalLockoutTime) - Time.time;
            Debug.Log("Adrenaline is active/on cooldown! Wait " + timeLeft.ToString("F1") + " seconds.");
        }
    }

    private void ActivateAdrenaline()
    {
        if (currentClass == null) return; 

        lastAdrenalineTime = Time.time;

        float finalDuration = baseDuration + bonusDuration;

        Debug.Log($"ADRENALINE INJECTED: {currentClass.className} for {finalDuration} seconds!");

        StartCoroutine(GhostRoutine(finalDuration));

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

    public float GetAdrenalineFillPercentage()
    {
        float timeSinceActivation = Time.time - lastAdrenalineTime;
        float finalDuration = baseDuration + bonusDuration;
        float totalCycleTime = finalDuration + adrenalineCooldown;

        // PHASE 1: Ready to use (Both duration AND cooldown are fully finished)
        if (timeSinceActivation >= totalCycleTime)
        {
            return 1f; // 1 = 100% Full Green Bar
        }

        // PHASE 2: Active and Draining (e.g. The 3 seconds you are buffed)
        if (timeSinceActivation <= finalDuration)
        {
            // This math drains it from 1 down to 0 over the duration
            return 1f - (timeSinceActivation / finalDuration);
        }

        // PHASE 3: Duration ended, now we are on Cooldown and Refilling
        // We subtract the duration to find out exactly how many seconds we've been recharging
        float timeSpentRecharging = timeSinceActivation - finalDuration;
        
        // This math fills it from 0 back up to 1 over the cooldown length!
        return timeSpentRecharging / adrenalineCooldown;
    }

    private System.Collections.IEnumerator GhostRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            SpawnGhostFrame();
            yield return new WaitForSeconds(ghostSpawnRate);
            elapsed += ghostSpawnRate;
        }
    }

    private void SpawnGhostFrame()
    {
        if (playerSprite == null || playerSprite.sprite == null) return;

        // 1. Create a clone object
        GameObject ghost = new GameObject("AdrenalineGhost");
        ghost.transform.position = playerSprite.transform.position;
        ghost.transform.rotation = playerSprite.transform.rotation;
        ghost.transform.localScale = playerSprite.transform.lossyScale;

        // 2. Copy the exact animation frame
        SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();
        sr.sprite = playerSprite.sprite;
        sr.color = ghostColor;
        sr.sortingLayerID = playerSprite.sortingLayerID;
        sr.sortingOrder = playerSprite.sortingOrder - 1; // Draw behind player

        // 3. Start the fade out on a standalone Coroutine
        StartCoroutine(FadeOutAndDestroy(sr, ghost));
    }

    private System.Collections.IEnumerator FadeOutAndDestroy(SpriteRenderer sr, GameObject ghost)
    {
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < ghostFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / ghostFadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(ghost);
    }
}
