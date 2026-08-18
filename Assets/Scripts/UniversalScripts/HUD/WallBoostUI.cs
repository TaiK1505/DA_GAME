using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WallBoostUI : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public GameObject chargePrefab; // Drag your ChargeSegment_BG prefab here!

    private List<Image> chargeFills = new List<Image>();

    void Start()
    {
        // Auto-find the player if you forget to drag them in
        if (player == null) player = FindFirstObjectByType<PlayerController>();

        InitializeUI();
    }

    public void InitializeUI()
    {
        // Wipe the container clean
        foreach (Transform child in transform) { Destroy(child.gameObject); }
        chargeFills.Clear();

        // Spawn exactly as many boxes as the player has Max Wall Boosts!
        for (int i = 0; i < player.maxWallBoosts; i++)
        {
            GameObject newCharge = Instantiate(chargePrefab, transform);
            
            // Grab the "Fill" image child object so we can control it later
            Image fillImage = newCharge.transform.Find("ChargeSegementFill").GetComponent<Image>();
            chargeFills.Add(fillImage);
        }
    }

    void Update()
    {
        if (player == null) return;

        if (chargeFills.Count != player.maxWallBoosts)
        {
            InitializeUI();
        }

        for (int i = 0; i < chargeFills.Count; i++)
        {
            if (i < player.currentWallBoosts)
            {
                // This charge is fully ready to use
                chargeFills[i].fillAmount = 1f;
            }
            else if (i == player.currentWallBoosts)
            {
                // This is the EXACT charge that is currently recharging on the conveyor belt!
                chargeFills[i].fillAmount = player.boostRechargeTimer / player.boostRechargeTime;
            }
            else
            {
                // This charge is empty and waiting in line
                chargeFills[i].fillAmount = 0f;
            }
        }
    }
}
