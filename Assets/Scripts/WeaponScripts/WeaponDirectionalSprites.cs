using UnityEngine;

public class WeaponDirectionalSprites : MonoBehaviour
{
    public Sprite sideSprite;
    public Sprite upSprite;
    public Sprite downSprite;

    public Transform firePoint; // Drag this weapon's FirePoint here in the Inspector!
    public Vector2 sideFireOffset; 
    public Vector2 upFireOffset;
    public Vector2 downFireOffset;
}
