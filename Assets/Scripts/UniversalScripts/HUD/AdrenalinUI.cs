using UnityEngine;
using UnityEngine.UI;

public class AdrenalinUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image adrenalineFill; // Your green bar goes here

    [Header("Player Data")]
    // CHANGE 'PlayerController' to whatever the actual name of your player script is!
    public AdrenalineCore playerScript; 

    private void Update()
    {
        if (playerScript != null && adrenalineFill != null)
        {
            // We just ask the Core script what percentage the bar should be at right now!
            adrenalineFill.fillAmount = playerScript.GetAdrenalineFillPercentage();
        }
    }
}
