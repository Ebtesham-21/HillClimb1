using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int selectedCarIndex = 0;   // the choosen car
    public int totalCoins = 0;

    public static string sceneToLoad;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCoins();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoins(int amount)
    {
        totalCoins += amount;
        SaveCoins();
        Debug.Log("Added " + amount + "coins.Total" + totalCoins);
    }
    

    



    // --- In GameManager.cs ---

    // ... (your existing coin and scene logic is here) ...

    // --- NEW: Car Unlock Logic ---

    public bool IsCarUnlocked(int carIndex)
    {
        // PlayerPrefs stores data as key-value pairs. We'll use a key like "CarUnlocked_1"
        // The '1' at the end of GetInt is the default value if the key doesn't exist.
        // 1 means true (unlocked), 0 means false (locked).
        if (PlayerPrefs.GetInt($"CarUnlocked_{carIndex}", 0) == 1)
        {
            return true;
        }
        return false;
    }

public void UnlockCar(int carIndex)
{
    // Set the value for this car's key to 1 (true)
    PlayerPrefs.SetInt($"CarUnlocked_{carIndex}", 1);
    PlayerPrefs.Save();
}

public bool TryPurchaseCar(CarData carToBuy, int carIndex)
{
    if (totalCoins >= carToBuy.price)
    {
        // Player has enough coins
        totalCoins -= carToBuy.price; // Deduct the price
        UnlockCar(carIndex);
        SaveCoins(); // Save the new coin total
        
        Debug.Log($"Purchased {carToBuy.carName}!");
        return true; // Purchase was successful
    }
    else
    {
        // Not enough coins
        Debug.Log($"Not enough coins to buy {carToBuy.carName}.");
        return false; // Purchase failed
    }
}

    public void SaveCoins()
    {
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();
    }

    public void LoadCoins()
    {
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
    }



    public void GoToMenu()
    {
        Time.timeScale = 1f; // Unpause the game before changing scene
        LoadScene("MenuScene");
    }

public int GetUpgradeLevel(int carIndex, string statName)
    {
        return PlayerPrefs.GetInt($"Car_{carIndex}_{statName}Level", 0); // Default level is 0
    }

    public void SetUpgradeLevel(int carIndex, string statName, int level)
    {
        PlayerPrefs.SetInt($"Car_{carIndex}_{statName}Level", level);
    }

 public int GetUpgradeCost(int currentLevel)
    {
        return 500 * (int)Math.Pow(2, currentLevel);
    }

    public bool TryPurchaseUpgrade(int carIndex, string statName)
    {
        int currentLevel = GetUpgradeLevel(carIndex, statName);
        int cost = GetUpgradeCost(currentLevel);

        if (totalCoins >= cost)
        {
            totalCoins -= cost;
            SetUpgradeLevel(carIndex, statName, currentLevel + 1);
            SaveCoins();
            Debug.Log($"Upgraded {statName} for car {carIndex} to level {currentLevel + 1}");
            return true;
        }
        else
        {
            Debug.Log($"Not enough coins for {statName} upgrade.");
            return false;
        }
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        LoadScene("GameScene");
    }


    public static void LoadScene(string scenceName)
    {
        sceneToLoad = scenceName;
        SceneManager.LoadScene("LoaderScene");
    }
}
