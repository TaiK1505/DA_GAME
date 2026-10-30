using UnityEngine;
using System.Collections;

public class LevelUpIndicator : MonoBehaviour
{
   [Header("Scanner Settings")]
    public float scanSpeed = 15f; 
    public float topHeight = 1.2f; 
    public float bottomHeight = -0.2f; 
    public float fadeOutTime = 0.15f; // Split-second fade to prevent jarring snap

    private bool movingUp = true;
    private bool isFading = false;
    private Vector3 startingPosition;
    private SpriteRenderer[] rings;

    void Awake()
    {
        // Automatically grabs your front and back rings
        rings = GetComponentsInChildren<SpriteRenderer>();
    }

    void OnEnable()
    {
        startingPosition = transform.localPosition;
        startingPosition.y = bottomHeight;
        transform.localPosition = startingPosition;
        
        movingUp = true;
        isFading = false;

        // Reset the alpha multiplier back to normal every time it turns on
        foreach(var ring in rings)
        {
            Color c = ring.color;
            c.a = 1f; 
            ring.color = c;
        }
    }

    void Update()
    {
        if (isFading) return; // Stop moving vertically while fading out

        float direction = movingUp ? 1f : -1f;
        transform.Translate(Vector3.up * direction * scanSpeed * Time.deltaTime);

        if (movingUp && transform.localPosition.y >= topHeight)
        {
            movingUp = false;
        }
        else if (!movingUp && transform.localPosition.y <= bottomHeight)
        {
            StartCoroutine(QuickFadeOut());
        }
    }

    IEnumerator QuickFadeOut()
    {
        isFading = true;
        float elapsed = 0f;
        
        // Grab the starting alphas
        float[] startAlphas = new float[rings.Length];
        for(int i = 0; i < rings.Length; i++) startAlphas[i] = rings[i].color.a;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / fadeOutTime;

            // Quickly fade the alpha to 0
            for(int i = 0; i < rings.Length; i++)
            {
                Color c = rings[i].color;
                c.a = Mathf.Lerp(startAlphas[i], 0f, percent);
                rings[i].color = c;
            }
            yield return null;
        }
        
        gameObject.SetActive(false); // Turn it off completely once invisible
    }

}
