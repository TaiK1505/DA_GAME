using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    
    [Header("Targeting")]
    public Transform player;
    
    private Camera cam;

    [Header("Camera Feel")]
    [Range(0f, 1f)]
    public float mouseWeight = 0.3f; // 0.3 means camera moves 30% towards the mouse
    public float smoothSpeed = 10f;
    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(mouseScreenPosition);

        
        Vector3 focalPoint = player.position + (mouseWorldPosition - player.position) * mouseWeight;

        
        Vector3 targetPosition = new Vector3(focalPoint.x, focalPoint.y, -10f);

        
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
