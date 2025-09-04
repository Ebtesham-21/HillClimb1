using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System; // Required for GC.Collect

public class Loader : MonoBehaviour
{
    public Slider loadingBar;

    void Start()
    {
        // Start the full, controlled loading coroutine
        StartCoroutine(LoadSceneWithCleanup());
    }

    IEnumerator LoadSceneWithCleanup()
    {
        // STEP 1: Wait one frame to ensure the old scene has been fully destroyed.
        yield return null; 

        // STEP 2: Manually and forcefully run the Garbage Collector.
        // This is a heavy operation, but we are doing it here on a blank loading screen
        // so the player doesn't notice the performance hit.
        Debug.Log("Forcing Garbage Collection...");
        GC.Collect();
        Debug.Log("Garbage Collection complete.");

        // STEP 3: Wait for any unused assets to be unloaded from memory.
        // This cleans up textures, meshes, etc., from the previous scene.
        Debug.Log("Unloading unused assets...");
        yield return Resources.UnloadUnusedAssets();
        Debug.Log("Unused assets unloaded.");

        // STEP 4: Now that memory is clean, start loading the NEW scene.
        string sceneName = string.IsNullOrEmpty(GameManager.sceneToLoad) ? "MenuScene" : GameManager.sceneToLoad;
        Debug.Log($"Starting to load scene: {sceneName}");
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingBar.value = progress;
            yield return null;
        }
    }
}