using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeWeaponController : MonoBehaviour
{
    [Header("Weapon Data")]
    public MeleeWeaponData weaponData;

    [Header("Setup")]
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public LayerMask obstacleLayers;

    [Header("Throw Setup")]
    public bool isThrown = false;
    private GameObject activeDummy; // Tracks the pooled dummy so we can delete it if you swap weapons!

    private float nextAttackTime = 0f;
    private float nextThrowTime = 0f;
    private Rigidbody2D playerRb; 

    private PlayerController playerController;
    private Coroutine currentStepCoroutine;

    private void Start()
    {
        playerRb = GetComponentInParent<Rigidbody2D>();
        playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        // 1. Safety check!
        if (weaponData == null || !weaponData.canThrow) return;

        // 2. Are we currently on cooldown?
        if (Time.time < nextThrowTime)
        {
            float timeRemaining = nextThrowTime - Time.time;
            
            // This calculates how much of the cooldown has already finished
            float timePassed = weaponData.throwCooldown - timeRemaining;
            
            // Convert to a 0-to-1 decimal and push it to the UI!
            float fillPercentage = timePassed / weaponData.throwCooldown;
            
            if (EquipmentUI.instance != null)
            {
                EquipmentUI.instance.UpdateAltFireUI(fillPercentage);
            }
        }
        else
        {
            // Cooldown is completely finished, make sure the bar is totally full and ready!
            if (EquipmentUI.instance != null)
            {
                EquipmentUI.instance.UpdateAltFireUI(1f);
            }
        }
    }

    public void Swing()
    {
        if (isThrown || weaponData == null || Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + weaponData.attackCooldown;

        bool isExecuting = weaponData.canLunge && playerRb != null && TryLunge();

        if (!isExecuting)
        {
            // ---> THE SPAM CLICK FIX <---
            // Stop the previous step if we swing again quickly so they never overlap!
            if (currentStepCoroutine != null) StopCoroutine(currentStepCoroutine);
            
            currentStepCoroutine = StartCoroutine(AttackStepRoutine());
            ExecuteSlash();
        }
    }

    private bool TryLunge()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        float maxPossibleRange = weaponData.executionLungeDistance;
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(transform.position, maxPossibleRange, enemyLayers);
        
        Transform bestTarget = null;
        float bestTargetScore = Mathf.Infinity; 

        // ---> THE MICRO-STEP NUKE <---
        // We now completely ignore all enemies unless they are STUNNED.
        foreach (Collider2D enemy in potentialTargets)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            
            // If they aren't stunned, skip them entirely! No more micro-stepping!
            if (enemyAI == null || !enemyAI.isStunned) continue; 

            float distToMouse = Vector2.Distance(mousePos, enemy.transform.position);
            
            if (distToMouse > 4f) continue; 

            if (distToMouse < bestTargetScore)
            {
                bestTargetScore = distToMouse;
                bestTarget = enemy.transform;
            }
        }

        if (bestTarget != null)
        {
            float distToPlayer = Vector2.Distance(playerRb.position, bestTarget.position);

            // Just check if they are within our new fixed execution range!
            if (distToPlayer <= weaponData.executionLungeDistance)
            {
                StartCoroutine(LungeRoutine(bestTarget, true));
                return true; 
            }
        }

        return false; 
    }

    private IEnumerator AttackStepRoutine()
    {
        if (playerController == null) playerController = GetComponentInParent<PlayerController>();
        if (playerController != null) playerController.canMove = false; // LOCK WASD!

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2 startPos = playerRb.position;
        Vector2 direction = (mousePos - startPos).normalized;

       
        float stepDistance = weaponData.attackStepDistance; 
        float stepDuration = weaponData.attackStepDuration;
        float elapsed = 0f;

        
        float startingSpeed = stepDistance / stepDuration;

        while (elapsed < stepDuration)
        {
            elapsed += Time.fixedDeltaTime;
            //  drain the speed from max down to zero to simulate friction
            float currentSpeed = Mathf.Lerp(startingSpeed, 0f, elapsed / stepDuration);
            playerRb.linearVelocity = direction * currentSpeed;
            
            yield return new WaitForFixedUpdate();
        }

        playerRb.linearVelocity = Vector2.zero;
        if (playerController != null) playerController.canMove = true; // UNLOCK WASD!
    }

    private IEnumerator LungeRoutine(Transform target, bool isStunned)
    {
        // LOCK MOVEMENT
        if (playerController == null) playerController = GetComponentInParent<PlayerController>();
        if (playerController != null) playerController.canMove = false;

        Vector2 startPos = playerRb.position;
        
        // Find the enemy's physical hitbox
        Collider2D targetCollider = target.GetComponent<Collider2D>();

        // Find the exact outer edge 
        Vector2 targetEdge = targetCollider != null ? targetCollider.ClosestPoint(startPos) : (Vector2)target.position;
        Vector2 direction = (targetEdge - startPos).normalized;

        // Stop away from their physical edge. 
        Vector2 destination = targetEdge - (direction * 0.5f);

        float dashDuration = weaponData.executionDuration;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / dashDuration;
            
            //  calculate the sweet ease-out curve position...
            Vector2 nextPos = Vector2.Lerp(startPos, destination, t * (2f - t));
            
            // ---> THE SPEEDOMETER FIX <---
            playerRb.linearVelocity = (nextPos - playerRb.position) / Time.fixedDeltaTime;
            
            yield return new WaitForFixedUpdate(); 
        }

        // Stop dead and execute the slash!
        playerRb.linearVelocity = Vector2.zero; 
        ExecuteSlash();

        //UNLOCK MOVEMENT
        if (playerController != null) playerController.canMove = true;
    }

    private void ExecuteSlash()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, weaponData.attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null) damageable.TakeDamage(weaponData.damage);
        }

        if (weaponData.slashVFXPrefab != null)
        {
            // Ask the Object Pool for the slash!
            GameObject slash = ObjectPoolManager.Instance.SpawnObject(weaponData.slashVFXPrefab, attackPoint.position, attackPoint.rotation);

            slash.transform.SetParent(attackPoint);
        }
    }

    public void Throw()
    {
        if (!weaponData.canThrow || isThrown || Time.time < nextThrowTime) return;
        
        nextThrowTime = Time.time + weaponData.throwCooldown;
        
        StartCoroutine(BoomerangRoutine());
    }

    private IEnumerator BoomerangRoutine()
    {
        isThrown = true;

        SpriteRenderer realSprite = GetComponentInChildren<SpriteRenderer>();
        if (realSprite != null) realSprite.enabled = false;

        // 1. SPAWN FROM OBJECT POOL
        activeDummy = ObjectPoolManager.Instance.SpawnObject(weaponData.dummyPrefab, transform.position, transform.rotation);
        
        SpriteRenderer dummyRenderer = activeDummy.GetComponent<SpriteRenderer>();
        if (dummyRenderer == null) dummyRenderer = activeDummy.AddComponent<SpriteRenderer>();

        if (realSprite != null)
        {
            dummyRenderer.sprite = realSprite.sprite;
            dummyRenderer.color = realSprite.color;
            dummyRenderer.sortingLayerID = realSprite.sortingLayerID;
            dummyRenderer.sortingOrder = realSprite.sortingOrder;
            activeDummy.transform.localScale = realSprite.transform.lossyScale; 
        }

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2 startPos = transform.position;
        Vector2 direction = (mousePos - startPos).normalized;
        
       float desiredDistance = weaponData.maxThrowDistance;
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, desiredDistance, obstacleLayers);
        float actualThrowDistance = hit.collider != null ? Mathf.Max(hit.distance - 0.5f, 0.5f) : desiredDistance;

        Vector2 targetPos = startPos + (direction * actualThrowDistance);
        HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();

        // FLY OUT
        while (Vector2.Distance(activeDummy.transform.position, targetPos) > 0.2f)
        {
            activeDummy.transform.position = Vector2.MoveTowards(activeDummy.transform.position, targetPos, weaponData.throwSpeed * Time.fixedDeltaTime);
            activeDummy.transform.Rotate(0, 0, -weaponData.throwSpinSpeed * Time.fixedDeltaTime);
            
            // IF IT HITS, STOP 
            if (DamageEnemiesInFlight(activeDummy.transform.position, false, alreadyHit)) break;
            
            yield return new WaitForFixedUpdate();
        }

        // RETURN TO SENDER
        while (Vector2.Distance(activeDummy.transform.position, transform.position) > 0.2f)
        {
            activeDummy.transform.position = Vector2.MoveTowards(activeDummy.transform.position, transform.position, weaponData.throwSpeed * Time.fixedDeltaTime);
            activeDummy.transform.Rotate(0, 0, -weaponData.throwSpinSpeed * Time.fixedDeltaTime);
            
            // PIERCES AND DAMAGES EVERYTHING ON THE WAY BACK!
            DamageEnemiesInFlight(activeDummy.transform.position, true, alreadyHit);
            
            yield return new WaitForFixedUpdate();
        }

        // RETURN TO OBJECT POOL
        ObjectPoolManager.Instance.ReturnObject(activeDummy); 
        if (realSprite != null) realSprite.enabled = true;
        
        isThrown = false;
        activeDummy = null;
    }

    private bool DamageEnemiesInFlight(Vector2 currentHitboxPos, bool isReturning, HashSet<Collider2D> alreadyHit)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(currentHitboxPos, weaponData.throwHitboxRadius, enemyLayers);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            // Ignore anyone already on the hit list!
            if (alreadyHit.Contains(enemy)) continue;

            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(weaponData.throwDamage); 
                alreadyHit.Add(enemy); 
            }

            // Stun and Stop only happens on the way OUT
            if (!isReturning)
            {
                EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                if (enemyAI != null) enemyAI.Stun(weaponData.stunDuration);
                
                return true; 
            }
        }
        return false;
    }

    // WEAPON SWAP TELEPORT
    private void OnDisable()
    {
        if (isThrown)
        {
            // Instantly send the dummy back to the pool!
            if (activeDummy != null)
            {
                ObjectPoolManager.Instance.ReturnObject(activeDummy);
                activeDummy = null;
            }

            // Visually restore the sword to the player's hip
            SpriteRenderer realSprite = GetComponentInChildren<SpriteRenderer>();
            if (realSprite != null) realSprite.enabled = true;
            
            isThrown = false;
        }
    }
}