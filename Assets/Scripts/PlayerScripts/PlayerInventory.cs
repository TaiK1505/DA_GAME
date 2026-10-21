using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [Header("Wallet")]
    public int currentCurrency = 0;

    [Header("UI Setup")]
    public TextMeshProUGUI currencyText;

    private void Start()
    {
        UpdateUI();
    }
    
    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
        Debug.Log($"Picked up {amount} coin(s)! Total Wallet: {currentCurrency}");
        UpdateUI();
    }

    
    public bool TrySpendCurrency(int cost)
    {
        if (currentCurrency >= cost)
        {
            currentCurrency -= cost;
            Debug.Log($"Purchase successful! Spent {cost}. Remaining: {currentCurrency}");
            
          
            
            return true; // The player had enough money, approve the transaction
        }
        else
        {
            Debug.Log("Not enough currency to buy this!");
            return false; // Transaction denied
        }
    }

    private void UpdateUI()
    {
        if (currencyText != null)
        {
            currencyText.text = currentCurrency.ToString(); 
        }
    }
}
