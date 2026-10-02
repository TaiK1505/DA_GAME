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

    [Header("Swing Tuning")]
    public float standardSwingAngle = 75f; 
    public float doubleSwingAngle = 60f;
    public float finisherSwingAngle = 110f; 

    [Header("Throw Setup")]
    public bool isThrown = false;
    private GameObject activeDummy; 

    [Header("Combo System")]
    private int currentComboStep = 0;
    private float lastSwingTime = 0f;
    public float comboResetWindow = 1.2f;

    private float nextAttackTime = 0f;
    private float nextThrowTime = 0f;
    
    // ---> NEW: Locks your inputs while a swing is active <---
    private bool isSwinging = false; 

    private Rigidbody2D playerRb; 
    private PlayerController playerController;
    private Coroutine currentStepCoroutine;
    private WeaponController aimingScript; 

    private void Start()
    {
        playerRb = GetComponentInParent<Rigidbody2D>();
        playerController = GetComponentInParent<PlayerController>();
        aimingScript = GetComponentInParent<WeaponController>(); 
    }

    private void Update()
    {
        if (weaponData == null || !weaponData.canThrow) return;

        if (Time.time < nextThrowTime)
        {
            float timeRemaining = nextThrowTime - Time.time;
            float timePassed = weaponData.throwCooldown - timeRemaining;
            float fillPercentage = timePassed / weaponData.throwCooldown;
            
            if (EquipmentUI.instance != null) EquipmentUI.instance.UpdateAltFireUI(fillPercentage);
        }
        else
        {
            if (EquipmentUI.instance != null) EquipmentUI.instance.UpdateAltFireUI(1f);
        }
    }

    public void Swing()
    {
        // ---> NEW: Blocks the click if a swing is currently happening <---
        if (isThrown || isSwinging || weaponData == null || Time.time < nextAttackTime) return;
        
        nextAttackTime = Time.time + weaponData.attackCooldown;

        if (Time.time > lastSwingTime + comboResetWindow) currentComboStep = 0;
        
        currentComboStep++;
        lastSwingTime = Time.time;
        if (currentComboStep > 3) currentComboStep = 1; 

        if (aimingScript != null) aimingScript.isAttacking = true;

        Transform lungeTarget = (weaponData.canLunge && playerRb != null) ? GetLungeTarget() : null;

        if (currentStepCoroutine != null) StopCoroutine(currentStepCoroutine);

        if (lungeTarget != null)
        {
            currentStepCoroutine = StartCoroutine(LungeRoutine(lungeTarget, true, currentComboStep));
        }
        else
        {
            currentStepCoroutine = StartCoroutine(AttackStepRoutine(currentComboStep));
        }
    }

    private Transform GetLungeTarget()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        float maxPossibleRange = weaponData.executionLungeDistance;
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(transform.position, maxPossibleRange, enemyLayers);
        
        Transform bestTarget = null;
        float bestTargetScore = Mathf.Infinity; 

        foreach (Collider2D enemy in potentialTargets)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
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
            if (distToPlayer <= weaponData.executionLungeDistance)
            {
                return bestTarget;
            }
        }
        return null;
    }

    // ==========================================
    // ---> NEW LINGERING HITBOX METHODS <---
    // ==========================================

    private void SpawnSlashVFX(int comboStep, Vector2 attackCenter, Quaternion attackRotation)
    {
        if (weaponData.comboVFXPrefabs != null && weaponData.comboVFXPrefabs.Length > 0)
        {
            int vfxIndex = Mathf.Clamp(comboStep - 1, 0, weaponData.comboVFXPrefabs.Length - 1);
            GameObject selectedVFX = weaponData.comboVFXPrefabs[vfxIndex];

            if (selectedVFX != null)
            {
                GameObject slash = ObjectPoolManager.Instance.SpawnObject(selectedVFX, attackCenter, attackRotation);
                
                if (playerRb != null) slash.transform.SetParent(playerRb.transform);
                else slash.transform.SetParent(null);
            }
        }
    }

    private void DamageEnemiesInSwing(HashSet<Collider2D> alreadyHit, Vector2 currentAttackCenter)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(currentAttackCenter, weaponData.attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (alreadyHit.Contains(enemy)) continue;

            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null) 
            {
                damageable.TakeDamage(weaponData.damage);
                alreadyHit.Add(enemy);
            }
        }
    }

    // ==========================================

    private IEnumerator AttackStepRoutine(int comboStep)
    {
        isSwinging = true; 
        if (playerController != null) playerController.canMove = false; 

        float startSwing = standardSwingAngle; 
        float endSwing = -standardSwingAngle;   

        if (comboStep == 2) 
        { 
            startSwing = -standardSwingAngle;  
            endSwing = standardSwingAngle;   
        }
        else if (comboStep == 3) 
        { 
            startSwing = finisherSwingAngle; 
            endSwing = -finisherSwingAngle; 
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 startPos = playerRb.position;
        Vector2 direction = (mousePos - startPos).normalized;

        float attackAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion attackRotation = Quaternion.Euler(0, 0, attackAngle);
        float distance = Vector2.Distance(transform.position, attackPoint.position);
        Vector2 attackCenter = (Vector2)transform.position + (direction * distance);

        // ---> 1. Spawn VFX and calculate offset <---
        SpawnSlashVFX(comboStep, attackCenter, attackRotation);
        HashSet<Collider2D> enemiesHit = new HashSet<Collider2D>();
        Vector2 attackOffset = attackCenter - playerRb.position;

        float stepDuration = 0.2f; // Fallback
        if (weaponData.comboStepDurations != null && weaponData.comboStepDurations.Length > 0)
        {
            int index = Mathf.Clamp(comboStep - 1, 0, weaponData.comboStepDurations.Length - 1);
            stepDuration = weaponData.comboStepDurations[index];
        }
        
        float elapsed = 0f;
        float startingSpeed = weaponData.attackStepDistance / stepDuration;

        while (elapsed < stepDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / stepDuration;
            
            playerRb.linearVelocity = direction * Mathf.Lerp(startingSpeed, 0f, t);

            // ---> 2. Constantly damage enemies with the moving offset <---
            DamageEnemiesInSwing(enemiesHit, playerRb.position + attackOffset);

           if (aimingScript != null)
            {
                float easeT = 1f - Mathf.Pow(1f - t, 3f); 
                aimingScript.swingOffset = Mathf.Lerp(startSwing, endSwing, easeT);
            }
            yield return new WaitForFixedUpdate();
        }

        playerRb.linearVelocity = Vector2.zero;
        if (playerController != null) playerController.canMove = true; 
        
        if (aimingScript != null) 
        {
            aimingScript.swingOffset = 0f;
            aimingScript.isAttacking = false;
        }
        
        isSwinging = false; 
    }

    private IEnumerator LungeRoutine(Transform target, bool isStunned, int comboStep)
    {
        isSwinging = true;
        if (playerController != null) playerController.canMove = false;

        float startSwing = standardSwingAngle; 
        float endSwing = -standardSwingAngle; 
        
        if (comboStep == 2) 
        { 
            startSwing = standardSwingAngle * 0.8f; 
            endSwing = -standardSwingAngle * 0.8f; 
        }
        else if (comboStep == 3) 
        { 
            startSwing = -finisherSwingAngle; 
            endSwing = finisherSwingAngle; 
        }

        Vector2 startPos = playerRb.position;
        Collider2D targetCollider = target.GetComponent<Collider2D>();
        Vector2 targetEdge = targetCollider != null ? targetCollider.ClosestPoint(startPos) : (Vector2)target.position;
        Vector2 direction = (targetEdge - startPos).normalized;

        float attackAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion attackRotation = Quaternion.Euler(0, 0, attackAngle);
        float distance = Vector2.Distance(transform.position, attackPoint.position);
        Vector2 attackCenter = (Vector2)transform.position + (direction * distance);

        // ---> 1. Spawn VFX and calculate offset for the lunge <---
        SpawnSlashVFX(comboStep, attackCenter, attackRotation);
        HashSet<Collider2D> enemiesHitThisLunge = new HashSet<Collider2D>();
        Vector2 attackOffset = attackCenter - playerRb.position;

        float baseDuration = weaponData.executionDuration;
        float dashDuration = (comboStep == 2) ? baseDuration * 1.5f : baseDuration;

        Vector2 destination = targetEdge - (direction * 0.5f);
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / dashDuration;
            
            Vector2 nextPos = Vector2.Lerp(startPos, destination, t * (2f - t));
            playerRb.linearVelocity = (nextPos - playerRb.position) / Time.fixedDeltaTime;

            // ---> 2. Constantly damage enemies with the moving offset <---
            DamageEnemiesInSwing(enemiesHitThisLunge, playerRb.position + attackOffset);

            if (aimingScript != null)
            {
                if (comboStep == 2)
                {
                    float doubleT = (t <= 0.5f) ? (t * 2f) : ((t - 0.5f) * 2f);
                    float easeT = 1f - Mathf.Pow(1f - doubleT, 3f);
                    
                    if (t <= 0.5f) aimingScript.swingOffset = Mathf.Lerp(startSwing, endSwing, easeT);
                    else aimingScript.swingOffset = Mathf.Lerp(endSwing, startSwing, easeT);
                }
                else
                {
                    float easeT = 1f - Mathf.Pow(1f - t, 3f); 
                    aimingScript.swingOffset = Mathf.Lerp(startSwing, endSwing, easeT);
                }
            }
            yield return new WaitForFixedUpdate(); 
        }

        playerRb.linearVelocity = Vector2.zero; 
        if (playerController != null) playerController.canMove = true;
        
        if (aimingScript != null) 
        {
            aimingScript.swingOffset = 0f;
            aimingScript.isAttacking = false;
        }

        isSwinging = false;
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

        activeDummy = ObjectPoolManager.Instance.SpawnObject(weaponData.dummyPrefab, transform.position, transform.rotation);
        
        SpriteRenderer dummyRenderer = activeDummy.GetComponent<SpriteRenderer>();
        if (dummyRenderer == null) dummyRenderer = activeDummy.AddComponent<SpriteRenderer>();

        if (realSprite != null)
        {
           dummyRenderer.sprite = (weaponData.thrownSprite != null) ? weaponData.thrownSprite : realSprite.sprite;
            dummyRenderer.color = realSprite.color;
            dummyRenderer.sortingLayerID = realSprite.sortingLayerID;
            dummyRenderer.sortingOrder = realSprite.sortingOrder;
            activeDummy.transform.localScale = realSprite.transform.lossyScale; 
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 startPos = transform.position;
        Vector2 direction = (mousePos - startPos).normalized;
        
        float desiredDistance = weaponData.maxThrowDistance;
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, desiredDistance, obstacleLayers);
        float actualThrowDistance = hit.collider != null ? Mathf.Max(hit.distance - 0.5f, 0.5f) : desiredDistance;

        Vector2 targetPos = startPos + (direction * actualThrowDistance);
        HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();

        while (Vector2.Distance(activeDummy.transform.position, targetPos) > 0.2f)
        {
            activeDummy.transform.position = Vector2.MoveTowards(activeDummy.transform.position, targetPos, weaponData.throwSpeed * Time.fixedDeltaTime);
            activeDummy.transform.Rotate(0, 0, -weaponData.throwSpinSpeed * Time.fixedDeltaTime);
            if (DamageEnemiesInFlight(activeDummy.transform.position, false, alreadyHit)) break;
            yield return new WaitForFixedUpdate();
        }

        while (Vector2.Distance(activeDummy.transform.position, transform.position) > 0.2f)
        {
            activeDummy.transform.position = Vector2.MoveTowards(activeDummy.transform.position, transform.position, weaponData.throwSpeed * Time.fixedDeltaTime);
            activeDummy.transform.Rotate(0, 0, -weaponData.throwSpinSpeed * Time.fixedDeltaTime);
            DamageEnemiesInFlight(activeDummy.transform.position, true, alreadyHit);
            yield return new WaitForFixedUpdate();
        }

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
            if (alreadyHit.Contains(enemy)) continue;

            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(weaponData.throwDamage); 
                alreadyHit.Add(enemy); 
            }

            if (!isReturning)
            {
                EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                if (enemyAI != null) enemyAI.Stun(weaponData.stunDuration);
                return true; 
            }
        }
        return false;
    }

    private void OnDisable()
    {
        isSwinging = false; // Safety reset
        if (aimingScript != null) aimingScript.isAttacking = false;
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
        if (playerController != null) playerController.canMove = true;

        if (isThrown)
        {
            if (activeDummy != null)
            {
                ObjectPoolManager.Instance.ReturnObject(activeDummy);
                activeDummy = null;
            }

            SpriteRenderer realSprite = GetComponentInChildren<SpriteRenderer>();
            if (realSprite != null) realSprite.enabled = true;
            isThrown = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null || weaponData == null) return;

        // Draws a red wireframe circle in the Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, weaponData.attackRange);
    }
}