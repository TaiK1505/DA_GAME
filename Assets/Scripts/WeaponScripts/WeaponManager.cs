using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Spawning")]
    public Transform weaponPivot; // The invisible shoulder joint
    public GameObject startingWeaponPrefab; // The "box" we spawn with

    private GameObject currentWeaponObject;
    private WeaponBase currentWeaponScript; // The brain inside the box

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
        
        // When the player clicks, yell at whatever gun we are currently holding to fire!
        // The "?" ensures the game doesn't crash if our hands are empty.
        controls.Player.Fire.started += ctx => currentWeaponScript?.StartShooting();
        controls.Player.Fire.canceled += ctx => currentWeaponScript?.StopShooting();
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }
    
    private void Start()
    {
        EquipWeapon(startingWeaponPrefab);
    }

    public void EquipWeapon(GameObject newWeaponPrefab)
    {
        // 1. If we are already holding a gun, throw it away
        if (currentWeaponObject != null)
        {
            Destroy(currentWeaponObject);
        }

        // 2. Spawn the new gun EXACTLY on our shoulder pivot
        currentWeaponObject = Instantiate(newWeaponPrefab, weaponPivot.position, weaponPivot.rotation, weaponPivot);

        // 3. Find the script on the new gun so we can talk to it when we click
        currentWeaponScript = currentWeaponObject.GetComponent<WeaponBase>();
    }
}
