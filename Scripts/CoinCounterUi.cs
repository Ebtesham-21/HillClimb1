using UnityEngine;
using TMPro;

public class CoinCounterUI : MonoBehaviour
{
    public TextMeshProUGUI coinText;

    void Start()
    {
        // Make sure the text is correct when the scene starts
        UpdateCoinText();
    }
    
    // Update is fine here since it's just checking a value, not running complex logic.
    void Update()
    {
        // Constantly check if the text needs updating.
        // This is a simple way to ensure it's always correct.
        UpdateCoinText();
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