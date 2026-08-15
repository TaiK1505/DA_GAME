using UnityEngine;
using UnityEngine.InputSystem;

public class BurstGrenadeGadget : PlayerGadget
{
    [Header("Stats")]
    public BurstGrenadeData myStats;

    private float nextFireTime = 0f;

    public override void InitializeGadget(GadgetData data)
    {
        myStats = (BurstGrenadeData)data; 
    }
    
    public override void ActivateGadget()
    {
        if (Time.time < nextFireTime) return; 

        
        nextFireTime = Time.time + myStats.cooldownTime;
        
        // Getting the mouse position
        Vector2 screenMousePos = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(screenMousePos);
        Vector2 playerPos = transform.position;

        // the throw direction and clamp the distance!
        Vector2 throwDirection = (mousePos - playerPos).normalized;
        float distanceToMouse = Vector2.Distance(playerPos, mousePos);
        
        //caps the throw distance so it never exceeds your max range!
        float actualThrowDistance = Mathf.Min(distanceToMouse, myStats.maxThrowRange); 
        
        //the exact X/Y coordinate the mine will land on
        Vector2 targetLandingSpot = playerPos + (throwDirection * actualThrowDistance);

        
        GameObject spawnedGrenade = ObjectPoolManager.Instance.SpawnObject(myStats.grenadePrefab, playerPos, Quaternion.identity);
        
        BurstGrenade physicalScript = spawnedGrenade.GetComponent<BurstGrenade>();
        if (physicalScript != null)
        {
            physicalScript.InitializeMine(myStats, targetLandingSpot);
        }

        DeactivateGadget();
    }

    public override void DeactivateGadget()
    {
        isGadgetActive = false;
       
    }

}
