using UnityEngine;
using System.Collections.Generic;

public class SlideGateGadget : PlayerGadget
{
    
    public SlideGateData myStats;
    
    private Queue<GameObject> activePads = new Queue<GameObject>();
    private float nextFireTime = 0f;

    public override void InitializeGadget(GadgetData data)
    {
        myStats = (SlideGateData)data; 
    }
    
    public override void ActivateGadget()
    {
        if (Time.time < nextFireTime) return;

        // 1. Get the exact mouse position using the New Input System
        Vector2 mouseScreenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        
        // Convert that screen position into the 2D game world
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f; // Lock it strictly to 2D!

        // 2. Calculate the direction from the Player to the Mouse
        Vector2 aimDir = (mouseWorldPos - transform.position).normalized;

        // 3. Calculate Rotation: Make the speed bump sit perpendicular to our mouse aim
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        Quaternion padRotation = Quaternion.Euler(0, 0, angle - 90f);

        // 4. Spawn from Object Pool
        GameObject deployedPad = ObjectPoolManager.Instance.SpawnObject(myStats.slidePadPrefab, transform.position, padRotation);

        if (deployedPad != null)
        {
            // Update the trap's internal boost force to match our ScriptableObject stats
            SlideGateInteractable gateScript = deployedPad.GetComponent<SlideGateInteractable>();
            if (gateScript != null) 
            {
                gateScript.boostForce = myStats.boostForce; 
                gateScript.knockbackDuration = myStats.enemyKnockbackDuration;
            }

            // 5. Queue Management
            activePads.Enqueue(deployedPad);

            PlayerController pc = GetComponent<PlayerController>();
            int bonusLimit = pc != null ? pc.bonusGadgetDeployLimit : 0;
            int totalMaxPads = myStats.maxActivePads + bonusLimit;

            if (activePads.Count > totalMaxPads)
            {
                GameObject oldestPad = activePads.Dequeue();
                ObjectPoolManager.Instance.ReturnObject(oldestPad);
            }
            // 6. Start Cooldown
            nextFireTime = Time.time + GetEffectiveCooldown(myStats.cooldownTime);
        }
    }

    public override void DeactivateGadget()
    {
        
    }

    private void Update()
    {
        if (myStats == null) return; // Safety check

        // Are we currently on cooldown?
        if (Time.time < nextFireTime)
        {
            float effectiveCD = GetEffectiveCooldown(myStats.cooldownTime);
            float timeRemaining = nextFireTime - Time.time;
            float timePassed = effectiveCD - timeRemaining;
            
            // Push the 0-to-1 decimal to the UI
            float fillPercentage = timePassed / effectiveCD;
            
            if (EquipmentUI.instance != null)
            {
                EquipmentUI.instance.UpdateGadgetCooldownUI(fillPercentage);
            }
        }
        else
        {
            // Cooldown finished, ensure UI is locked at 100%
            if (EquipmentUI.instance != null)
            {
                EquipmentUI.instance.UpdateGadgetCooldownUI(1f);
            }
        }
    }
    
}

    

