using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    
    [Header("Flight Stats")]
    public float speed = 25f;
    public float lifetime = 3f;

    [Header("Shotgun Physics (Drag)")]
    [Tooltip("How quickly the bullet loses speed. 0 = never slows down. 5+ = heavy shotgun drag.")]
    public float drag = 0f; 

    [Header("Collision Settings")]
    [Tooltip("Select the layers this bullet is allowed to hit (e.g., Enemy, Environment, Wall)")]
    public LayerMask hitLayers; 

    [HideInInspector] public float damage; 

    // ---> NEW: We need to cache the speed for the Object Pool! <---
    private float currentSpeed;

    private void OnEnable()
    {
        // 1. Reset the speed back to maximum every time it comes out of the pool
        currentSpeed = speed; 
        
        Invoke(nameof(Deactivate), lifetime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Update()
    {
        // ---> NEW: Exponential Drag <---
        // Lerping toward 0 means it loses speed quickly at first, then coasts smoothly
        if (drag > 0f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, drag * Time.deltaTime);
        }

        // 1. Calculate the exact distance for this single frame using the new currentSpeed
        float distanceThisFrame = currentSpeed * Time.deltaTime;

        // 2. Shoot the invisible laser forward by exactly that distance
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, distanceThisFrame, hitLayers);

        // 3. Did the laser hit something?
        if (hit.collider != null)
        {
            IDamageable damageableTarget = hit.collider.GetComponent<IDamageable>();

            if (damageableTarget != null)
            {
                damageableTarget.TakeDamage(damage);
            }   

            // Optional AAA Polish: Snap the bullet perfectly to the wall before it vanishes
            transform.position = hit.point;

            // Deactivate immediately
            Deactivate();
            
            return; 
        }

        // 4. If the laser hit absolutely nothing, it is safe to take the step forward!
        transform.Translate(Vector3.right * distanceThisFrame);
    }

    private void Deactivate()
    {
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}
