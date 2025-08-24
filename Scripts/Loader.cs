using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    public Slider loadingBar;

    void Start()
    {
        // When this scene starts, begin loading the scene stored in our GameManager
        // If it's the very first time launching the game, load the Menu.
        string sceneName = string.IsNullOrEmpty(GameManager.sceneToLoad) ? "MenuScene" : GameManager.sceneToLoad;
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            // The operation.progress value goes from 0.0 to 0.9.
            // We clamp it to make the loading bar fill up nicely.
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingBar.value = progress;
            
            yield return null; // Wait for the next frame
        }
    }
}