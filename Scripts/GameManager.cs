using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int selectedCarIndex = 0; // the choosen car

    public static string sceneToLoad;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public static void LoadScene(string scenceName)
    {
        sceneToLoad = scenceName;
        SceneManager.LoadScene("LoaderScene");
    }
}
