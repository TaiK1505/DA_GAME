using UnityEngine;

public class GadgetPedestal : MonoBehaviour, IInteractable
{
    [Header("Gadget")]
    public GadgetData gadgetToEquip; 
    
    [Tooltip("The name of the script")]
    public string gadgetScriptName;

    public void Interact(GameObject player)
    {
        if (gadgetToEquip == null) return;

        PlayerController pc = player.GetComponent<PlayerController>();

        if (pc != null)
        {
            pc.EquipGadget(gadgetToEquip, gadgetScriptName);
            Debug.Log("Player picked up: " + gadgetToEquip.name);
        }
    }


}
