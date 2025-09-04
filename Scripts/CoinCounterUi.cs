using UnityEngine;
using TMPro;

public class CoinCounterUI : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    private int lastDisplayedCoins = -1; // Cache variable

    void Start()
    {
        // Make sure the text is correct when the scene starts
        UpdateCoinText();
    }
    
    // Update is fine here since it's just checking a value, not running complex logic.
    void Update()
    {
         if (GameManager.Instance != null)
        {
            int currentCoins = GameManager.Instance.totalCoins;
            // Only update the text if the coin value has actually changed.
            if (currentCoins != lastDisplayedCoins)
            {
                coinText.text = currentCoins.ToString();
                lastDisplayedCoins = currentCoins;
            }
        }
    }

    void UpdateCoinText()
    {
        if (GameManager.Instance != null)
        {
            string currentText = GameManager.Instance.totalCoins.ToString();
            // Only update the text if it has actually changed, for a tiny performance boost.
            if (coinText.text != currentText)
            {
                coinText.text = currentText;
            }
        }
    }
}