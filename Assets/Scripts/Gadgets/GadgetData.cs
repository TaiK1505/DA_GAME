using UnityEngine;

public class GadgetData : ScriptableObject
{
    [Header("Gadget ID")]
    public string gadgetName;
    public string description;
    public Sprite icon;

    [Header("Universal Stats")]
    public float cooldownTime;
}
