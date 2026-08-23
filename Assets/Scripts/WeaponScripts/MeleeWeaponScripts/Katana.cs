using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Katana : MonoBehaviour
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

    private void Start()
    {
        playerRb = GetComponentInParent<Rigidbody2D>();
    }

    public void Swing()
    {
        if (isThrown || weaponData == null || Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + weaponData.attackCooldown;

        bool isLunging = weaponData.canLunge && playerRb != null && TryLunge();

        if (!isLunging)
        {
            ExecuteSlash();
        }
    }

    private bool TryLunge()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // Scan the massive execution area
        float maxPossibleRange = weaponData.lungeDistance * weaponData.executionRangeMultiplier;
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(transform.position, maxPossibleRange, enemyLayers);
        
        Transform bestTarget = null;
        float bestTargetScore = Mathf.Infinity; // Changed from 'closestToMouse' to a generic 'Score'

        Vector2 playerPos = playerRb.transform.position;
        Vector2 aimDirection = (mousePos - playerPos).normalized;

        foreach (Collider2D enemy in potentialTargets)
        {
            Vector2 dirToEnemy = ((Vector2)enemy.transform.position - playerPos).normalized;
            if (Vector2.Angle(aimDirection, dirToEnemy) > 75f) continue; 

            float distToMouse = Vector2.Distance(mousePos, enemy.transform.position);
            
            // The "Reasonable Range": You still have to aim within 4 units of the target
            if (distToMouse > 4f) continue; 

            // --- THE STUN MAGNETISM ---
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            bool isStunned = (enemyAI != null && enemyAI.isStunned);

            // If they are stunned, subtract 100 from their score so they ALWAYS win the priority check!
            float score = isStunned ? (distToMouse - 100f) : distToMouse;

            if (score < bestTargetScore)
            {
                bestTargetScore = score;
                bestTarget = enemy.transform;
            }
        }

        if (bestTarget != null)
        {
            EnemyAI enemyAI = bestTarget.GetComponent<EnemyAI>();
            bool targetIsStunned = (enemyAI != null && enemyAI.isStunned);

            float currentAllowedRange = targetIsStunned ? 
                (weaponData.lungeDistance * weaponData.executionRangeMultiplier) : 
                weaponData.lungeDistance;

            float distToPlayer = Vector2.Distance(playerPos, bestTarget.position);

            if (distToPlayer > weaponData.attackRange * 0.8f && distToPlayer <= currentAllowedRange)
            {
                StartCoroutine(LungeRoutine(bestTarget, targetIsStunned));
                return true; 
            }
        }

        return false; 
    }

    private IEnumerator LungeRoutine(Transform target, bool isStunned)
    {
        Vector2 startPos = playerRb.position;
        Vector2 targetPos = target.position; 
        Vector2 direction = (targetPos - startPos).normalized;
        Vector2 destination = targetPos - (direction * (weaponData.attackRange * 0.5f));

        float dashDuration = isStunned ? weaponData.executionDuration : weaponData.lungeDuration;
        float elapsed = 0f;

        playerRb.linearVelocity = Vector2.zero;

        while (elapsed < dashDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / dashDuration;
            playerRb.MovePosition(Vector2.Lerp(startPos, destination, t * (2f - t)));
            yield return new WaitForFixedUpdate(); 
        }

        playerRb.MovePosition(destination); 
        ExecuteSlash();
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
            // 1. Ask the Object Pool for the slash!
            GameObject slash = ObjectPoolManager.Instance.SpawnObject(weaponData.slashVFXPrefab, attackPoint.position, attackPoint.rotation);
            
            // 2. Parent it to the attackPoint so it perfectly follows the player's arm as they move!
            // (The PooledVFX script will safely un-parent it when it dies)
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

        // --- PHASE 1: FLY OUT ---
        while (Vector2.Distance(activeDummy.transform.position, targetPos) > 0.2f)
        {
            activeDummy.transform.position = Vector2.MoveTowards(activeDummy.transform.position, targetPos, weaponData.throwSpeed * Time.fixedDeltaTime);
            activeDummy.transform.Rotate(0, 0, -weaponData.throwSpinSpeed * Time.fixedDeltaTime);
            
            // IF IT HITS, STOP DEAD!
            if (DamageEnemiesInFlight(activeDummy.transform.position, false, alreadyHit)) break;
            
            yield return new WaitForFixedUpdate();
        }

        // --- PHASE 2: RETURN TO SENDER ---
        while (Vector2.Distance(activeDummy.transform.position, transform.position) > 0.2f)
        {
            activeDummy.transform.position = Vector2.MoveTowards(activeDummy.transform.position, transform.position, weaponData.throwSpeed * Time.fixedDeltaTime);
            activeDummy.transform.Rotate(0, 0, -weaponData.throwSpinSpeed * Time.fixedDeltaTime);
            
            // PIERCES AND DAMAGES EVERYTHING ON THE WAY BACK!
            DamageEnemiesInFlight(activeDummy.transform.position, true, alreadyHit);
            
            yield return new WaitForFixedUpdate();
        }

        // 3. RETURN TO OBJECT POOL
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

    // --- WEAPON SWAP TELEPORT (SAFETY NET) ---
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