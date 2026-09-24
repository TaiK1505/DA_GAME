using UnityEngine;

public class PooledVFX : MonoBehaviour
{
    public float lifetime = 0.15f;
    private bool isReturning = false;

    private void OnEnable()
    {
        isReturning = false;
        // The millisecond it wakes up from the pool, start the countdown!
        Invoke(nameof(Deactivate), lifetime);
    }

    private void OnDisable()
    {
        CancelInvoke();
        
        // AAA TRICK FIX: We CANNOT use transform.SetParent() here because the 
        // hierarchy is locking. Instead, just return it to the pool so it isn't lost.
        if (!isReturning && ObjectPoolManager.Instance != null)
        {
            isReturning = true;
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        }
    }

    private void Deactivate()
    {
        if (isReturning) return;
        isReturning = true;

        // It is safe to unparent here because we are disabling it manually 
        // via the Invoke timer, not interrupting a hierarchy change.
        transform.SetParent(null);
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}
