using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentUI : MonoBehaviour
{
    public static EquipmentUI instance; 

    [Header("UI Slots")]
    public Image weaponIcon;
    public Image gadgetIcon;
    
    [Header("Cooldown Overlays")]
    public Image gadgetOverlay; 

    [Header("Weapon Meters")]
    public Image altFireBar;

    [Header("Ammo HUD")]
    public TextMeshProUGUI ammoText;

    public GameObject altFireRoot;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void EquipGadget(Sprite gadgetSprite, Color customColor)
    {
        // 1. Set the main icon art or fallback color
        if (gadgetSprite != null)
        {
            gadgetIcon.sprite = gadgetSprite;
            gadgetIcon.color = Color.white; 
        }
        else
        {
            gadgetIcon.sprite = null;
            gadgetIcon.color = customColor; 
        }

        // 2. When first equipped, the overlay fill starts at 0 (fully revealed/ready to use!)
        if (gadgetOverlay != null)
        {
            gadgetOverlay.fillAmount = 0f;
        }
    }

    public void EquipWeapon(Sprite weaponSprite, Color customColor, bool hasAltFire)
    {
        // 1. Turn the Alt-Fire bar on or off! The Layout Group will automatically center the gun!
        if (altFireRoot != null) 
        {
            altFireRoot.SetActive(hasAltFire);
        }

        // 2. Set the color and sprite
        weaponIcon.color = customColor; 

        if (weaponSprite != null)
        {
            weaponIcon.sprite = weaponSprite;
        }
        else
        {
            weaponIcon.sprite = null;
        }
    } 

    public void UpdateGadgetCooldownUI(float fillPercentage)
    {
        if (gadgetOverlay != null)
        {
            // Because it's an overlay blocking the icon, 
            // when fill is 1 (on cooldown), the dark layer covers the icon.
            // When fill is 0 (cooldown finished), the dark layer vanishes, revealing the bright icon!
            gadgetOverlay.fillAmount = 1f - fillPercentage;
        }
    }

    public void UpdateAltFireUI(float fillPercentage)
    {
        if (altFireBar != null)
        {
            altFireBar.fillAmount = fillPercentage;
        }
    }   

    public void UpdateAmmoUI(int currentAmmo, int reserveAmmo, bool hasInfiniteReserve, bool isBottomless)
    {
        if (ammoText == null) return;

        // The Melee Fix: If we pass -1, it means we are holding the Katana. Hide the text!
        if (currentAmmo == -1) 
        {
            ammoText.text = "";
            return;
        }

        if (isBottomless)
        {
            // For laser beams or miniguns that never stop
            ammoText.text = "∞";
        }
        else if (hasInfiniteReserve)
        {
            // For the Starter Gun! 
            ammoText.text = $"{currentAmmo} / ∞";
        }
        else
        {
            // Standard Shooter Rules (e.g., 30 / 120)
            ammoText.text = $"{currentAmmo} / {reserveAmmo}";
        }
    }

}
