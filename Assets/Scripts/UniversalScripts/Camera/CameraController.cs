using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // ---> NEW: The Singleton so HealthComponent can talk to it <---
    public static CameraController Instance { get; private set; }

    [Header("Targeting")]
    public Transform player;
    private Camera cam;

    [Header("Camera Feel")]
    [Range(0f, 1f)]
    public float mouseWeight = 0.3f; // 0.3 means camera moves 30% towards the mouse
    public float smoothSpeed = 10f;

    [Header("Screen Shake")]
    private float shakeTimer;
    private float shakeMagnitude;
    
    // Tracks the "smooth" movement separately so the shake doesn't cause camera drift!
    private Vector3 currentBasePosition;

    void Awake()
    {
        // Setup the Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        cam = GetComponent<Camera>();
    }

    void Start()
    {
        // Initialize our base position
        currentBasePosition = transform.position;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // 1. Calculate the Target Position based on Mouse Weight
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(mouseScreenPosition);
        
        Vector3 focalPoint = player.position + (mouseWorldPosition - player.position) * mouseWeight;
        Vector3 targetPosition = new Vector3(focalPoint.x, focalPoint.y, -10f);

        // 2. Smoothly move our BASE position (ignoring any active shake)
        currentBasePosition = Vector3.Lerp(currentBasePosition, targetPosition, smoothSpeed * Time.deltaTime);

        // 3. Calculate the active Screen Shake Offset
        Vector3 currentShakeOffset = Vector3.zero;
        if (shakeTimer > 0)
        {
            // Pick a random point inside a circle for chaotic vibration
            Vector2 randomPoint = Random.insideUnitCircle * shakeMagnitude;
            currentShakeOffset = new Vector3(randomPoint.x, randomPoint.y, 0f);
            
            // Using unscaledDeltaTime so the shake works even during Room-Clear Slow-Mo!
            shakeTimer -= Time.unscaledDeltaTime; 
        }

        // 4. Apply both together to the actual camera
        transform.position = currentBasePosition + currentShakeOffset;
    }

    // ---> NEW: The method called by HealthComponent <---
    public void TriggerShake(float duration, float magnitude)
    {
        shakeTimer = duration;
        shakeMagnitude = magnitude;
    }
}
