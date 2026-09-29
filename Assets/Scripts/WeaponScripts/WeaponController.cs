using UnityEngine;

public class WeaponController : MonoBehaviour
{
    
    [Header("Aiming Components")]
    public Transform weaponPivot;
    public SpriteRenderer playerSprite;
    public SpriteRenderer currentGunSprite;


    private WeaponDirectionalSprites currentWeaponDirection;

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

    public void UpdateWeaponSprite(SpriteRenderer newSprite, WeaponDirectionalSprites weaponDirection)
    {
        currentGunSprite = newSprite;
        currentWeaponDirection = weaponDirection;
    }

    private void HandleAiming()
    {
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldMousePosition.z = 0f;
        Vector3 aimDirection = (worldMousePosition - transform.position).normalized;
        
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        weaponPivot.eulerAngles = new Vector3(0, 0, angle);

        if (currentGunSprite != null && currentWeaponDirection != null)
        {
            // 1. SWAP SPRITE & SLIDE FIREPOINT
            if (angle > 45 && angle <= 135) 
            {
                currentGunSprite.sprite = currentWeaponDirection.upSprite;
                currentWeaponDirection.firePoint.localPosition = currentWeaponDirection.upFireOffset;
            } 
            else if (angle < -45 && angle >= -135) 
            {
                currentGunSprite.sprite = currentWeaponDirection.downSprite;
                currentWeaponDirection.firePoint.localPosition = currentWeaponDirection.downFireOffset;
            } 
            else 
            {
                currentGunSprite.sprite = currentWeaponDirection.sideSprite;
                currentWeaponDirection.firePoint.localPosition = currentWeaponDirection.sideFireOffset;
            }

            float horizontalRadius = 0.3f; // Distance from center when aiming Left/Right
        float verticalRadius = 0.2f;   // Distance from center when aiming Up/Down

        // Mathf.Cos is 1 when horizontal (Right/Left), and 0 when vertical (Up/Down)
        float anglePercentage = Mathf.Abs(Mathf.Cos(angle * Mathf.Deg2Rad));

        // Smoothly blend between the vertical and horizontal distances
        float dynamicRadius = Mathf.Lerp(verticalRadius, horizontalRadius, anglePercentage);

        // Push/pull the child weapon along its local X-axis
        currentGunSprite.transform.localPosition = new Vector3(dynamicRadius, 0, 0);

            // 2. THE FLIP FIX 
            float flipY = (worldMousePosition.x < transform.position.x) ? -1f : 1f;
            currentGunSprite.transform.localScale = new Vector3(1f, flipY, 1f);

            // 3. THE LAYER FIX 
            if (angle > 20 && angle < 160) 
            {
                currentGunSprite.sortingOrder = 0; 
            } 
            else 
            {
                currentGunSprite.sortingOrder = 10; 
            }
        }
    }

   
}
