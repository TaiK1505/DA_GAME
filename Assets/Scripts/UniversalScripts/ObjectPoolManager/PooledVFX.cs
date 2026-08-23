using UnityEngine;

public class PooledVFX : MonoBehaviour
{
    public float lifetime = 0.15f;

    private void OnEnable()
    {
        // The millisecond it wakes up from the pool, start the countdown!
        Invoke(nameof(Deactivate), lifetime);
    }

    private void OnDisable()
    {
        // Safety cleanup for the Object Pool
        CancelInvoke();
        
        // AAA TRICK: If the VFX was parented to the player's arm, we must un-parent it 
        // before it goes back to the Object Pool so the pool hierarchy doesn't break!
        transform.SetParent(null); 
    }

    private void Deactivate()
    {
        transform.SetParent(null);
       
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }

}
