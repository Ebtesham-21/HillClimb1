using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    [Header("Game State")]
    private bool isGameOver = false;
    private bool isPaused = false;
    private CarController car;
    private MultiWheelCarController multiWheelCar; // For handling trucks
    private Transform carTransform; // A generic reference to the car's transform
    private Vector3 startPosition;

    // Getter properties to get data from whichever car controller is active
    public bool HasFuel => car != null ? car.HasFuel : multiWheelCar.HasFuel;
    public float CurrentForwardSpeed => car != null ? car.CurrentForwardSpeed : multiWheelCar.CurrentForwardSpeed;
    public bool IsGrounded() => car != null ? car.IsGrounded() : multiWheelCar.IsGrounded();


    [Header("Distance Tracking")]
    public float totalDistanceDriven = 0f;
    private int coinsCollectedThisRun = 0;

    [Header("Checkpoint System")]
    public Vector2 checkpointDistanceRange = new Vector2(500f, 2000f);
    public Vector2Int checkpointCoinRewardRange = new Vector2Int(500, 5000);
    public Vector2 checkpointTimeRewardRange = new Vector2(30f, 150f);
    private float nextCheckpointDistance;

    private GroundStreamer groundStreamer;

    [Header("Timer")]
    public float timeLeft;

    [Header("UI References")]
    public TextMeshProUGUI distanceDrivenText;
    public TextMeshProUGUI timeLeftText;
    public TextMeshProUGUI nextCheckpointText;
    public GameObject inGameMenuPanel;
    public TextMeshProUGUI finalDistanceText;
    public TextMeshProUGUI finalCoinsText;
    public TextMeshProUGUI gameOverReasonText;
    public GameObject resumeButton;

     private CrazyGamesManager crazyGamesManager;

    void Awake()
    {
        Instance = this;
        crazyGamesManager = FindObjectOfType<CrazyGamesManager>(); 
        groundStreamer = FindObjectOfType<GroundStreamer>(); 
    }

    

    void Start()
    {
        // Try to find both controller types
        car = FindObjectOfType<CarController>();
        multiWheelCar = FindObjectOfType<MultiWheelCarController>();

        // Check if we found either one
        if (car == null && multiWheelCar == null)
        {
            Debug.LogError("GameSessionManager could not find any car controller in the scene!");
            this.enabled = false;
            return;
        }

        // Get the transform from whichever controller was found
        carTransform = (car != null) ? car.transform : multiWheelCar.transform;
        startPosition = carTransform.position;
        
        inGameMenuPanel.SetActive(false);
        Time.timeScale = 1f;

        // Initialize all session variables for a clean run
        timeLeft = 0f;
        nextCheckpointDistance = 0f;
        coinsCollectedThisRun = 0;
        totalDistanceDriven = 0f;
        
        


        // --- NEW ---
        if (crazyGamesManager != null)
        {
            crazyGamesManager.StartGameplay();
        }


        GenerateNewCheckpoint();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isGameOver) return;
            if (isPaused) ResumeGame();
            else PauseGame();
        }

        if (isGameOver || isPaused || carTransform == null) return;

        // These were the missing calls
        UpdateDistance();
        UpdateTime();
        CheckForGameOver();
        CheckForCheckpoint();
        UpdateUI();
    }

    void UpdateDistance()
    {
        totalDistanceDriven = carTransform.position.x - startPosition.x;
    }

    void UpdateTime()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0) timeLeft = 0;
    }

    void CheckForGameOver()
    {
        List<string> gameOverReasons = new List<string>();

        if (!HasFuel && CurrentForwardSpeed < 0.1f) gameOverReasons.Add("Out of Fuel!");
        if (Vector3.Angle(Vector3.up, carTransform.up) > 150f && IsGrounded()) gameOverReasons.Add("Car Flipped!");
        if (timeLeft <= 0) gameOverReasons.Add("Time Out!");

        if (gameOverReasons.Count > 0)
        {
            string reasonText = string.Join("\n", gameOverReasons);
            TriggerGameOver(reasonText);
        }
    }

    void CheckForCheckpoint()
    {
        if (totalDistanceDriven >= nextCheckpointDistance)
        {
            int coinReward = Random.Range(checkpointCoinRewardRange.x, checkpointCoinRewardRange.y);
            GameManager.Instance.AddCoins(coinReward);
            GenerateNewCheckpoint();
        }
    }

    void GenerateNewCheckpoint()
    {
        float distanceToNext = Random.Range(checkpointDistanceRange.x, checkpointDistanceRange.y);
        nextCheckpointDistance += distanceToNext;
        timeLeft += Random.Range(checkpointTimeRewardRange.x, checkpointTimeRewardRange.y);
    }
    
    void UpdateUI()
    {
        distanceDrivenText.text = $"Distance Driven: {totalDistanceDriven:F0}m";
        timeLeftText.text = $"Time Left: {timeLeft:F0}s";
        float distanceToNextCheckpoint = nextCheckpointDistance - totalDistanceDriven;
        nextCheckpointText.text = $"Next Checkpoint in {distanceToNextCheckpoint:F0}m";
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        inGameMenuPanel.SetActive(true);
        gameOverReasonText.text = "PAUSED";
        finalDistanceText.gameObject.SetActive(false);
        finalCoinsText.gameObject.SetActive(false);
        resumeButton.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        inGameMenuPanel.SetActive(false);
    }
    
    public void TriggerGameOver(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;
         if(crazyGamesManager != null)
    {
        crazyGamesManager.StopGameplay();
    }
        Time.timeScale = 0f;
        inGameMenuPanel.SetActive(true);
        
        gameOverReasonText.text = reason;
        finalDistanceText.gameObject.SetActive(true);
        finalCoinsText.gameObject.SetActive(true);
        resumeButton.SetActive(false);
        
        finalDistanceText.text = $"Total Distance: {totalDistanceDriven:F0}m";
        finalCoinsText.text = $"Coins Collected: {coinsCollectedThisRun}";
    }
    
    public void AddRunCoins(int amount)
    {
        coinsCollectedThisRun += amount;
    }
}