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

        nextAttackTime = Time.time + weaponData.attackCooldown;

        bool isLunging = false;
        
        if (weaponData.canLunge && playerRb != null)
        {
            isLunging = TryLunge();
        }

        if (!isLunging)
        {
            ExecuteSlash();
        }
    }

    private bool TryLunge()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // Scan huge area to account for potential Executions!
        float maxPossibleRange = weaponData.lungeDistance * weaponData.executionRangeMultiplier;
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(transform.position, maxPossibleRange, enemyLayers);
        
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
            // --- THE EXECUTION CHECK ---
            IStunnable stunnableTarget = bestTarget.GetComponent<IStunnable>();
            bool targetIsStunned = (stunnableTarget != null && stunnableTarget.IsCurrentlyStunned());

            // Dynamically scale the allowed dash range!
            float currentAllowedRange = targetIsStunned ? 
                (weaponData.lungeDistance * weaponData.executionRangeMultiplier) : 
                weaponData.lungeDistance;

            float distToPlayer = Vector2.Distance(playerPos, bestTarget.position);

            if (distToPlayer > weaponData.attackRange * 0.8f && distToPlayer <= currentAllowedRange)
            {
                // Pass the boolean into the routine so it knows how fast to dash!
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

        // --- DYNAMIC DURATION ---
        float dashDuration = isStunned ? weaponData.executionDuration : weaponData.lungeDuration;
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

            // Draw the massive Execution range in blue!
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, weaponData.lungeDistance * weaponData.executionRangeMultiplier);
        }
    }

    public void Throw()
    {
        if (!weaponData.canThrow || isThrown) return;
        StartCoroutine(BoomerangRoutine());
    }

    private IEnumerator BoomerangRoutine()
    {
        isThrown = true;

        SpriteRenderer realSprite = GetComponentInChildren<SpriteRenderer>();
        if (realSprite != null) realSprite.enabled = false;

        GameObject dummy = new GameObject("SpinningDummy");
        
        if (realSprite != null) {
            dummy.transform.position = realSprite.transform.position;
        } else {
            dummy.transform.position = transform.position;
        }
        
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
            dummy.transform.position = Vector2.MoveTowards(dummy.transform.position, targetPos, weaponData.throwSpeed * Time.fixedDeltaTime);
            dummy.transform.Rotate(0, 0, -weaponData.throwSpinSpeed * Time.fixedDeltaTime);
            
            DamageEnemiesInFlight(dummy.transform.position);
            yield return new WaitForFixedUpdate();
        }

        // --- PHASE 2: RETURN TO SENDER ---
        while (Vector2.Distance(dummy.transform.position, transform.position) > 0.2f)
        {
            dummy.transform.position = Vector2.MoveTowards(dummy.transform.position, transform.position, weaponData.throwSpeed * Time.fixedDeltaTime);
            dummy.transform.Rotate(0, 0, -weaponData.throwSpinSpeed * Time.fixedDeltaTime);
            
            DamageEnemiesInFlight(dummy.transform.position);
            yield return new WaitForFixedUpdate();
        }

        Destroy(dummy);
        if (realSprite != null) realSprite.enabled = true;
        
        isThrown = false;
    }

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

            // --- THE STUN TRIGGER ---
            IStunnable stunnable = enemy.GetComponent<IStunnable>();
            if (stunnable != null)
            {
                stunnable.Stun(weaponData.stunDuration);
            }
        }
    }
}