using UnityEngine;
using TMPro;
using System.Globalization;

public class SpeedometerScript : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI speedText;

    [Header("Player Target")]
    public Rigidbody2D playerRb;
    
    private void Update()
    {
        if (playerRb != null && speedText != null)
        {
            float currentSpeed = playerRb.linearVelocity.magnitude;
            speedText.text = "Speed : " + currentSpeed.ToString("F1", CultureInfo.InvariantCulture);
        }
    }
}
