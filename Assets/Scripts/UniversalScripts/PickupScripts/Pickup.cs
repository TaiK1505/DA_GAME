using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [Header("Magnetism Settings")]
    public bool isMagnetic = false; 
    public float magneticRange = 4f;
    public float magneticSpeed = 20f;
    
    protected Transform playerTransform;
    protected bool isFollowingPlayer = false;
    protected bool isRoomClearVacuumActive = false;

    protected virtual void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    protected virtual void OnEnable()
    {
        isFollowingPlayer = false;
        isRoomClearVacuumActive = false;
    }

    protected virtual void Update()
    {
        if (playerTransform == null) return;

        if (isRoomClearVacuumActive)
        {
            isFollowingPlayer = true;
            magneticSpeed += Time.deltaTime * 15f; 
        }
        else if (isMagnetic)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance <= magneticRange)
            {
                isFollowingPlayer = true;
            }
        }

        if (isFollowingPlayer)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, magneticSpeed * Time.deltaTime);
        }
    }

    public void TriggerRoomClearVacuum()
    {
        isRoomClearVacuumActive = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnCollected(collision.gameObject);
        }
    }

    protected abstract void OnCollected(GameObject player);
}
