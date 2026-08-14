using UnityEngine;

public class BurstGrenade : MonoBehaviour
{
    private BurstGrenadeData myStats;
    private Vector2 targetPos;
    private bool hasLanded = false;

    // We pass the data in from the player gadget
    public void InitializeMine(BurstGrenadeData mineData, Vector2 landingSpot)
    {
        myStats = mineData;
        targetPos = landingSpot;
        hasLanded = false;
    }

    private void Update()
    {
        // 1. If it hasn't landed yet, fly through the air!
        if (!hasLanded)
        {
            // MoveTowards smoothly slides an object from point A to point B
            transform.position = Vector2.MoveTowards(transform.position, targetPos, myStats.throwSpeed * Time.deltaTime);

            // Did we reach the target spot?
            if (Vector2.Distance(transform.position, targetPos) < 0.05f)
            {
                hasLanded = true;
                // Start the fuse!
                Invoke("Explode", myStats.detonationDelay);
            }
        }
    }

    private void Explode()
    {
        Collider2D[] objectsInBlast = Physics2D.OverlapCircleAll(transform.position, myStats.blastRadius);

        foreach (Collider2D hit in objectsInBlast)
        {
            Vector2 blastDirection = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;

            if (hit.CompareTag("Enemy"))
            {
                EnemyAI enemy = hit.GetComponent<EnemyAI>();
                if (enemy != null) enemy.ApplyKnockback(blastDirection * myStats.blastForce, myStats.enemyKnockbackDuration);
            }
            else if (hit.CompareTag("Player"))
            {
                PlayerController player = hit.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.currentState = PlayerController.State.Sliding;
                    player.slideDirection = blastDirection;
                    player.currentSlideSpeed = myStats.blastForce;
                    player.activeBoostFriction = myStats.burstGrenadeBoostFriction; 
                }
            }
        }

        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // If we have slotted a Stat Card into the Gadget Arm...
        if (myStats != null)
        {
            // 1. Draw a YELLOW circle to show exactly how far we can throw it
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, myStats.maxThrowRange);

            // 2. Draw a RED circle around the player just to visualize how big the explosion is!
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, myStats.blastRadius);
        }
    }

}
