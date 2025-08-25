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
