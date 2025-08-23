using System.Collections;
using UnityEngine;

public class DayNightController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The material that has the day/night textures and fade property.")]
    public Material skyMaterial;

    [Header("Timings & Dynamics")]
    [Tooltip("How long, in seconds, the fade transition takes.")]
    public float transitionDuration = 5.0f;
    [Tooltip("The MINIMUM and MAXIMUM time a day or night cycle will last.")]
    public Vector2 timeBetweenChanges = new Vector2(30f, 60f); // <-- UPDATED DEFAULT VALUES

    private bool isDay;
    private float timer;
    private float nextChangeTime;

    // --- THIS IS THE MAINLY MODIFIED METHOD ---
    void Start()
    {
        if (skyMaterial == null)
        {
            Debug.LogError("Sky Material not assigned on DayNightController!");
            enabled = false;
            return;
        }

        // --- NEW: Randomize the starting state ---
        // Random.value returns a float between 0.0 and 1.0. 
        // This gives us a 50/50 chance for isDay to be true or false.
        isDay = Random.value > 0.5f;

        // --- NEW: Set the initial sky instantly based on the random state ---
        // We use a ternary operator here: (condition) ? (value if true) : (value if false)
        float initialFade = isDay ? 0f : 1f; // If it's day, start at 0 (day texture). If night, start at 1 (night texture).
        skyMaterial.SetFloat("_Fade", initialFade);
        
        // Log the starting state for easy debugging
        Debug.Log(isDay ? "Game starts at DAY." : "Game starts at NIGHT.");

        // Set the timer for the *first* transition
        SetNextChangeTime();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextChangeTime)
        {
            StartCoroutine(FadeSky());
            timer = -99999; // Prevent re-triggering during fade
        }
    }

    void SetNextChangeTime()
    {
        // This existing logic already handles random duration, so it's perfect.
        nextChangeTime = Random.Range(timeBetweenChanges.x, timeBetweenChanges.y);
        timer = 0f; // Reset the timer
        Debug.Log($"Next sky change in {nextChangeTime:F1} seconds.");
    }

    private IEnumerator FadeSky()
    {
        // Flip the state for the transition
        isDay = !isDay; 
        Debug.Log(isDay ? "Fading to DAY." : "Fading to NIGHT.");

        float startValue = isDay ? 1f : 0f;
        float endValue = isDay ? 0f : 1f;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float currentFade = Mathf.Lerp(startValue, endValue, elapsedTime / transitionDuration);
            skyMaterial.SetFloat("_Fade", currentFade);
            yield return null;
        }

        skyMaterial.SetFloat("_Fade", endValue);
        
        // Set the timer for the next cycle *after* this one has finished fading
        SetNextChangeTime();
    }
}