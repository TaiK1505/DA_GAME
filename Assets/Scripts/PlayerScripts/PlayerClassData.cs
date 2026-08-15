using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Class Data", menuName = "Class Data")]
public class PlayerClassData : ScriptableObject
{
    [Header("Class Info")]
    public string className;

    [Header("Adrenaline Base Upgrades")]
    [Tooltip("The list of tickets printed when this class hits Adrenaline!")]
    public List<StatModifier> baseAdrenalineModifiers;
}
