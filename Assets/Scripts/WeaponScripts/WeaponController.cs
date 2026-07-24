using UnityEngine;

public class WeaponController : MonoBehaviour
{
    
    [Header("Aiming Components")]
    public Transform weaponPivot;
    public SpriteRenderer playerSprite;
    private SpriteRenderer currentGunSprite;

    private PlayerControls controls;
    private Vector2 mousePosition;


    private void Awake()
    {
        controls = new PlayerControls();
        
        // This tripwire constantly reads the mouse position
        controls.Player.Aim.performed += ctx => mousePosition = ctx.ReadValue<Vector2>();
        controls.Player.Aim.canceled += ctx => mousePosition = Vector2.zero;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    } 

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleAiming();
    }

     public void UpdateWeaponSprite(SpriteRenderer newSprite)
    {
        currentGunSprite = newSprite;
    }   

    private void HandleAiming()
    {
        // 1. Convert the screen mouse position into world space
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldMousePosition.z = 0f;

        // 2. Calculate the direction from the player to the mouse
        Vector3 aimDirection = (worldMousePosition - transform.position).normalized;

        // 3. Calculate the angle in degrees
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // 4. Rotate the invisible pivot
        weaponPivot.eulerAngles = new Vector3(0, 0, angle);

        // 5. The "Gungeon" Flip Logic
        // If the angle is looking left (greater than 90 or less than -90 degrees)
        if (currentGunSprite != null)
        {
            if (mousePosition.x < transform.position.x)
            {
                // A "Real" flip! Flips the art AND perfectly mirrors the FirePoint child
                currentGunSprite.transform.localScale = new Vector3(1, -1, 1);
            }
            else
            {
                // Reset to normal
                currentGunSprite.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        
        
    }

   
}
