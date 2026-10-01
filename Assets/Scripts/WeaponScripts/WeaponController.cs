using UnityEngine;

public class WeaponController : MonoBehaviour
{
    
    [Header("Aiming Components")]
    public Transform weaponPivot;
    public SpriteRenderer playerSprite;
    public SpriteRenderer currentGunSprite;

    public bool isAttacking = false;
    public float swingOffset = 0f;
    
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

        // ---> NEW: THE NINJA RUN (Drags the sword behind you) <---
        // We check currentWeaponDirection to see if the isMelee box is checked
        if (currentWeaponDirection != null && currentWeaponDirection.isMelee)
    {
        if (!isAttacking)
        {
            angle -= 135f; 
            if (angle <= -180f) angle += 360f; 
            if (angle > 180f) angle -= 360f;
        }
        else
        {
            // Snaps forward and applies the physical swing offset
            angle += swingOffset; 
        }
    }

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

            // 2. ELLIPTICAL ORBIT
            float horizontalRadius = currentWeaponDirection.horizontalDistance; 
            float verticalRadius = currentWeaponDirection.verticalDistance;   

            // Mathf.Cos is 1 when horizontal (Right/Left), and 0 when vertical (Up/Down)
            float anglePercentage = Mathf.Abs(Mathf.Cos(angle * Mathf.Deg2Rad));

            // Smoothly blend between the vertical and horizontal distances
            float dynamicRadius = Mathf.Lerp(verticalRadius, horizontalRadius, anglePercentage);

            // Push/pull the child weapon along its local X-axis
            currentGunSprite.transform.localPosition = new Vector3(dynamicRadius, 0, 0);

            // 3. THE FLIP FIX 
            float flipY = (worldMousePosition.x < transform.position.x) ? -1f : 1f;
            
            // ---> NEW: Invert the flip if it's a melee weapon pointing backwards <---
            if (currentWeaponDirection.isMelee) 
            {
                flipY *= -1f; 
            }

            currentGunSprite.transform.localScale = new Vector3(1f, flipY, 1f);

            // 4. THE LAYER FIX 
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
