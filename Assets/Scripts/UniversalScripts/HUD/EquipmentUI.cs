using UnityEngine;
using UnityEngine.UI;

public class EquipmentUI : MonoBehaviour
{
    public static EquipmentUI instance; 

    [Header("UI Slots")]
    public Image weaponIcon;
    public Image gadgetIcon;
    
    [Header("Cooldown Overlays")]
    public Image gadgetOverlay; // Drag your new GadgetOverlay image here!

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
}
