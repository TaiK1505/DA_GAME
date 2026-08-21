using System.Collections;
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

    private Transform originalParent;
    private Vector3 originalLocalPos;
    
    private float nextAttackTime = 0f;
    private Rigidbody2D playerRb; 

    private void Start()
    {
        playerRb = GetComponentInParent<Rigidbody2D>();
        originalParent = transform.parent;
        originalLocalPos = transform.localPosition;
    }

    public void Swing()
    {
        if (isThrown) return;
        
        if (weaponData == null || Time.time < nextAttackTime) return;

        // 1. Set cooldown instantly so you can't spam while dashing
        nextAttackTime = Time.time + weaponData.attackCooldown;

        bool isLunging = false;
        
        // 2. Try to perform the auto-target dash
        if (weaponData.canLunge && playerRb != null)
        {
            isLunging = TryLunge();
        }

        // 3. If we DID NOT dash, slash immediately!
        if (!isLunging)
        {
            ExecuteSlash();
        }
    }

    private bool TryLunge()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(transform.position, weaponData.lungeDistance, enemyLayers);
        
        Transform bestTarget = null;
        float closestToMouse = Mathf.Infinity;

        Vector2 playerPos = playerRb.transform.position;
        Vector2 aimDirection = (mousePos - playerPos).normalized;

        foreach (Collider2D enemy in potentialTargets)
        {
            Vector2 dirToEnemy = ((Vector2)enemy.transform.position - playerPos).normalized;
            float angleToEnemy = Vector2.Angle(aimDirection, dirToEnemy);

            if (angleToEnemy > 75f) continue; 

            float distToMouse = Vector2.Distance(mousePos, enemy.transform.position);
            
            if (distToMouse > 4f) continue; 

            if (distToMouse < closestToMouse)
            {
                closestToMouse = distToMouse;
                bestTarget = enemy.transform;
            }
        }

        if (bestTarget != null)
        {
            float distToPlayer = Vector2.Distance(playerPos, bestTarget.position);

            if (distToPlayer > weaponData.attackRange * 0.8f)
            {
                StartCoroutine(LungeRoutine(bestTarget));
                return true; 
            }
        }

        return false; 
    }

    private IEnumerator LungeRoutine(Transform target)
    {
        Vector2 startPos = playerRb.position;
        Vector2 targetPos = target.position; 
        Vector2 direction = (targetPos - startPos).normalized;
        
        Vector2 destination = targetPos - (direction * (weaponData.attackRange * 0.5f));

        float dashDuration = weaponData.lungeDuration;
        float elapsed = 0f;

        playerRb.linearVelocity = Vector2.zero;

        while (elapsed < dashDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / dashDuration;
            float easeOut = t * (2f - t); 
            
            playerRb.MovePosition(Vector2.Lerp(startPos, destination, easeOut));
            
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
            if (damageable != null)
            {
                damageable.TakeDamage(weaponData.damage);
            }
        }

        if (weaponData.slashVFXPrefab != null)
        {
            GameObject slash = Instantiate(weaponData.slashVFXPrefab, attackPoint.position, attackPoint.rotation, attackPoint);
            Destroy(slash, 0.15f); 
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null && weaponData != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, weaponData.attackRange);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, weaponData.lungeDistance);
        }
    }

    public void Throw()
    {
        if (!weaponData.canThrow || isThrown)
        {
            return;
        }

        StartCoroutine(BoomerangRoutine());
    }

    private IEnumerator BoomerangRoutine()
    {
        isThrown = true;

        // 1. HIDE THE REAL SWORD
        SpriteRenderer realSprite = GetComponentInChildren<SpriteRenderer>();
        if (realSprite != null) realSprite.enabled = false;

        // 2. SPAWN THE SPINNING DUMMY 
        GameObject dummy = new GameObject("SpinningDummy");
        
        // Match the exact position of the visual blade so it spins perfectly centered
        if (realSprite != null) {
            dummy.transform.position = realSprite.transform.position;
        } else {
            dummy.transform.position = transform.position;
        }
        
        // Copy the art over to the dummy
        SpriteRenderer dummyRenderer = dummy.AddComponent<SpriteRenderer>();
        if (realSprite != null)
        {
            dummyRenderer.sprite = realSprite.sprite;
            dummyRenderer.color = realSprite.color;
            dummyRenderer.sortingLayerID = realSprite.sortingLayerID;
            dummyRenderer.sortingOrder = realSprite.sortingOrder;

            dummy.transform.localScale = realSprite.transform.lossyScale;
        }

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        
        Vector2 startPos = transform.position;
        Vector2 direction = (mousePos - startPos).normalized;
        
        float desiredDistance = Mathf.Clamp(Vector2.Distance(startPos, mousePos), 1f, weaponData.maxThrowDistance);
        
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, desiredDistance, obstacleLayers);
        float actualThrowDistance = desiredDistance;

        if (hit.collider != null)
        {
            actualThrowDistance = Mathf.Max(hit.distance - 0.5f, 0.5f); 
        }

        Vector2 targetPos = startPos + (direction * actualThrowDistance);

        // --- PHASE 1: FLY OUT ---
        while (Vector2.Distance(dummy.transform.position, targetPos) > 0.2f)
        {
            // Move the dummy out!
            dummy.transform.position = Vector2.MoveTowards(dummy.transform.position, targetPos, weaponData.throwSpeed * Time.fixedDeltaTime);
            
            // Spin the dummy like a buzzsaw! Your aiming script can't stop this!
            dummy.transform.Rotate(0, 0, -weaponData.throwSpinSpeed * Time.fixedDeltaTime);
            
            // Deal damage dynamically around the flying dummy
            DamageEnemiesInFlight(dummy.transform.position);
            
            yield return new WaitForFixedUpdate();
        }

        // --- PHASE 2: RETURN TO SENDER ---
        // It flies back to the player's current position, so you can catch it while moving
        while (Vector2.Distance(dummy.transform.position, transform.position) > 0.2f)
        {
            dummy.transform.position = Vector2.MoveTowards(dummy.transform.position, transform.position, weaponData.throwSpeed * Time.fixedDeltaTime);
            
            dummy.transform.Rotate(0, 0, -weaponData.throwSpinSpeed * Time.fixedDeltaTime);
            
            DamageEnemiesInFlight(dummy.transform.position);
            
            yield return new WaitForFixedUpdate();
        }

        // 3. CATCH IT! Delete the fake sword, show the real sword!
        Destroy(dummy);
        if (realSprite != null) realSprite.enabled = true;
        
        isThrown = false;
    }

    // UPDATED: Now requires a position so it tracks the dummy!
    private void DamageEnemiesInFlight(Vector2 currentHitboxPos)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(currentHitboxPos, weaponData.throwHitboxRadius, enemyLayers);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(weaponData.throwDamage * Time.fixedDeltaTime * 10f); 
            }
        }
    }
}
