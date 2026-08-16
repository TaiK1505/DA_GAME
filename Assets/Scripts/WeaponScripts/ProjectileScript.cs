using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    
    [Header("Flight Stats")]
    public float speed = 25f;
    public float lifetime = 3f;

    [Header("Collision Settings")]
    [Tooltip("Select the layers this bullet is allowed to hit (e.g., Enemy, Environment, Wall)")]
    public LayerMask hitLayers; // This tells the invisible laser what it's allowed to poke!

    [HideInInspector] public float damage; 

    // We completely deleted the Rigidbody2D variable!

    private void OnEnable()
    {
        // No more physics velocity needed! We handle it in Update now.
        Invoke(nameof(Deactivate), lifetime);
    }

    private void OnDisable()
    {
        // Safety cleanup for our Object Pool
        CancelInvoke();
    }

    private void Update()
    {
        // 1. Calculate the exact distance for this single frame
        float distanceThisFrame = speed * Time.deltaTime;

        // 2. Shoot the invisible laser forward by exactly that distance
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, distanceThisFrame, hitLayers);

        // 3. Did the laser hit something?
        if (hit.collider != null)
        {
            // We hit something! Grab the health component (just like your old collision code)
            IDamageable damageableTarget = hit.collider.GetComponent<IDamageable>();

            if (damageableTarget != null)
            {
                damageableTarget.TakeDamage(damage);
            }   

            Debug.Log("Bullet hit: " + hit.collider.gameObject.name);

            // Optional AAA Polish: Snap the bullet perfectly to the wall before it vanishes
            transform.position = hit.point;

            // Deactivate immediately
            Deactivate();
            
            return; // CRITICAL: This stops the code so the bullet doesn't keep moving this frame!
        }

        // 4. If the laser hit absolutely nothing, it is safe to take the step forward!
        transform.Translate(Vector3.right * distanceThisFrame);
    }

    private void Deactivate()
    {
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}
