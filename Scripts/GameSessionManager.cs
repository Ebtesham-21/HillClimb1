using UnityEngine;
using TMPro; // For the TextMeshPro UI elements
using System.Collections.Generic; 

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    [Header("UI References")]
    // ... (your other UI references are here)
  
    public TextMeshProUGUI gameOverReasonText; // <-- ADD THIS LINE

    [Header("Game State")]
    private bool isGameOver = false;
    private CarController car;
    private Vector3 startPosition;

    [Header("Distance Tracking")]
    public float totalDistanceDriven = 0f;
    private int coinsCollectedThisRun = 0;

    [Header("Checkpoint System")]
    public Vector2 checkpointDistanceRange = new Vector2(500f, 2000f);
    public Vector2Int checkpointCoinRewardRange = new Vector2Int(500, 5000);
    public Vector2 checkpointTimeRewardRange = new Vector2(30f, 150f);
    private float nextCheckpointDistance;

    [Header("Timer")]
    public float timeLeft;

    [Header("UI References")]
    public TextMeshProUGUI distanceDrivenText;
    public TextMeshProUGUI timeLeftText;
    public TextMeshProUGUI nextCheckpointText;
    public GameObject gameOverPanel; // The entire Game Over UI panel
    public TextMeshProUGUI finalDistanceText;
    public TextMeshProUGUI finalCoinsText;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Find the car in the scene once the spawner has created it
        car = FindObjectOfType<CarController>();
        if (car == null) 
        {
            Debug.LogError("GameSessionManager could not find the CarController in the scene!");
            this.enabled = false; // Disable this script to prevent errors
            return;
        }

        startPosition = car.transform.position;
        
        gameOverPanel.SetActive(false); 
        Time.timeScale = 1f;

        // --- FIX 1: Initialize values to a clean state ---
        nextCheckpointDistance = 0f; // Start distance count from 0
        timeLeft = 0f;               // Start timer from 0
        
        // Now generate the FIRST checkpoint, which will set the initial distance and time
        GenerateNewCheckpoint();
    }

    void Update()
    {
         if (isGameOver || car == null) return;  // If game is over OR car doesn't exist, stop.

        UpdateDistance();
        UpdateTime();
        CheckForGameOver();
        CheckForCheckpoint();
        UpdateUI();
    }

    void UpdateDistance()
    {
        totalDistanceDriven = car.transform.position.x - startPosition.x;
    }

    void UpdateTime()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0)
        {
            timeLeft = 0;
        }
    }

   void CheckForGameOver()
{
    // Use a list to store all the reasons the game might be over.
    List<string> gameOverReasons = new List<string>();
    
    if (car == null) {
            TriggerGameOver("CAR NOT FOUND!");
            return;
        }

    // Condition 1: Out of fuel
        if (!car.HasFuel && car.CurrentForwardSpeed < 0.1f)
        {
            gameOverReasons.Add("Out of Fuel!");
        }

    // Condition 2: Flipped over
    if (Vector3.Angle(Vector3.up, car.transform.up) > 150f && car.IsGrounded())
    {
        gameOverReasons.Add("Car Flipped!");
    }

    // Condition 3: Out of time
    if (timeLeft <= 0)
    {
        gameOverReasons.Add("Time Out!");
    }

    // If our list has one or more reasons, the game is over.
    if (gameOverReasons.Count > 0)
    {
        // Join all the reasons together with a new line character in between.
        string reasonText = string.Join("\n", gameOverReasons);
        TriggerGameOver(reasonText); // Pass the final text to the game over method
    }
}

    void CheckForCheckpoint()
    {
        if (totalDistanceDriven >= nextCheckpointDistance)
        {
            // Reached checkpoint!
            int coinReward = Random.Range(checkpointCoinRewardRange.x, checkpointCoinRewardRange.y);
            GameManager.Instance.AddCoins(coinReward);
            
            Debug.Log($"Checkpoint Reached! Rewarded {coinReward} coins.");

            GenerateNewCheckpoint();
        }
    }

    void GenerateNewCheckpoint()
    {
        float distanceToNext = Random.Range(checkpointDistanceRange.x, checkpointDistanceRange.y);
        // This is correct, we add to the previous checkpoint's location
        nextCheckpointDistance += distanceToNext;
        
        // --- FIX 2: We don't just add time, we reset and add ---
        // For the first checkpoint, timeLeft is 0, so it gets the full duration.
        // For subsequent checkpoints, this REFILLS the timer and adds the new bonus time.
        // A better approach is to just add the bonus time. Let's stick with that.
        // The previous logic was correct, the initialization in Start() was the problem.
        
        timeLeft += Random.Range(checkpointTimeRewardRange.x, checkpointTimeRewardRange.y);
    }
    
    void UpdateUI()
    {
        distanceDrivenText.text = $"Distance Driven: {totalDistanceDriven:F0}m";
        timeLeftText.text = $"Time Left: {timeLeft:F0}s";
        
        float distanceToNextCheckpoint = nextCheckpointDistance - totalDistanceDriven;
        nextCheckpointText.text = $"Next Checkpoint in {distanceToNextCheckpoint:F0}m";
    }
    
    public void TriggerGameOver(string reason)
    {
        if (isGameOver) return; // Make sure we only trigger this once

        isGameOver = true;
        Time.timeScale = 0f; // Pause the game

         gameOverReasonText.text = reason;
        
        // Update the Game Over panel with final stats
        finalDistanceText.text = $"Total Distance: {totalDistanceDriven:F0}m";
        finalCoinsText.text = $"Coins Collected: {coinsCollectedThisRun}"; // We will implement this next
        
        gameOverPanel.SetActive(true);
    }
    
    public void AddRunCoins(int amount)
    {
        coinsCollectedThisRun += amount;
    }
}