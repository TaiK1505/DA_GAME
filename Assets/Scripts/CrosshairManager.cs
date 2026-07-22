using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairManager : MonoBehaviour
{
    
    private PlayerControls controls;
    private Vector2 mousePosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        controls = new PlayerControls();
        
        // Listen to the mouse movement using our existing action
        controls.Player.Aim.performed += ctx => mousePosition = ctx.ReadValue<Vector2>();
        controls.Player.Aim.canceled += ctx => mousePosition = Vector2.zero;
        
        // Hide the default operating system cursor
        Cursor.visible = false;
        
        // Optional: Confine the cursor to the game window so players don't accidentally click out
        Cursor.lockState = CursorLockMode.Confined;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = mousePosition;
    }
}
