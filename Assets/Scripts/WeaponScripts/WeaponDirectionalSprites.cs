using UnityEngine;

public class WeaponDirectionalSprites : MonoBehaviour
{
    [Header("Weapon Type")]
    public bool isMelee = false;
    
    public Sprite sideSprite;
    public Sprite upSprite;
    public Sprite downSprite;

    [Header("Orbit Distances (Distance from player)")]
    public float horizontalDistance = 0.3f;
    public float verticalDistance = 0.2f;

    public Transform firePoint; // Drag this weapon's FirePoint here in the Inspector!
    public Vector2 sideFireOffset; 
    public Vector2 upFireOffset;
    public Vector2 downFireOffset;
}
