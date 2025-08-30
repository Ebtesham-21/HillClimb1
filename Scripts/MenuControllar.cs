using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Needed for the message coroutine

public class MenuController : MonoBehaviour
{
    [Header("Data Source")]
    [Tooltip("The ScriptableObject that holds the list of all cars.")]
    public CarDatabase carDatabase;

    [Header("Scene References")]
    [Tooltip("An empty GameObject where the car preview will be spawned.")]
    public Transform carSpawnPoint;
    [Tooltip("The text element to display the current car's name.")]
    public TextMeshProUGUI carNameText;

    [Header("UI Elements")]
    [Tooltip("The main button for selecting or buying a car.")]
    public Button selectButton;
    [Tooltip("The text on the main button (e.g., 'SELECT' or 'BUY').")]
    public TextMeshProUGUI selectButtonText;
    [Tooltip("The GameObject with the lock icon image.")]
    public GameObject lockIcon;
    [Tooltip("The text element that appears when the player can't afford a car.")]
    public TextMeshProUGUI notEnoughCoinsText;

    [Header("Menu Display Settings")]
    [Tooltip("The scale to apply to the car model when it's displayed in the menu.")]
     public Vector3 targetFrameSize = new Vector3(5f, 3f, 1f);

    private int currentCarIndex = 0;
    private GameObject currentCarInstance;

    void Start()
    {
        // This ensures the default car is always unlocked the very first time the game is played.
        if (!PlayerPrefs.HasKey("CarUnlocked_0"))
        {
            PlayerPrefs.SetInt("CarUnlocked_0", 1);
            PlayerPrefs.Save();
        }
        
        notEnoughCoinsText.gameObject.SetActive(false); // Make sure the error message is hidden at start
        SwitchCar(0); // Display the very first car
    }

    public void NextCar()
    {
        SwitchCar(1);
    }

    public void PreviousCar()
    {
        SwitchCar(-1);
    }

    void SwitchCar(int direction)
    {
        currentCarIndex += direction;

        // Loop the index back around if it goes out of bounds
        if (currentCarIndex < 0) currentCarIndex = carDatabase.allCars.Length - 1;
        if (currentCarIndex >= carDatabase.allCars.Length) currentCarIndex = 0;

        // Destroy the previous car preview if it exists
        if (currentCarInstance != null)
        {
            Destroy(currentCarInstance);
        }

        // Get the data for the current car from our central database
        CarData currentCarData = carDatabase.allCars[currentCarIndex];

        // --- Spawn and Configure the Preview Car ---
        currentCarInstance = Instantiate(currentCarData.carPrefab, carSpawnPoint.position, carSpawnPoint.rotation);
        carNameText.text = currentCarData.carName;
        // --- AUTO-SCALING LOGIC (RESTORED) ---
        AutoScaleToShowcase(currentCarInstance);

        // --- Disable All Unnecessary Components for the Menu ---
        DisableCarComponents(currentCarInstance);

        // Update the button to show "SELECT" or "BUY (price)"
        UpdateSelectButton();
    }
    

     void AutoScaleToShowcase(GameObject carInstance)
    {
        // Get the combined bounds of all renderers in the car prefab.
        // This gives us the total visual size of the car.
        var renderers = carInstance.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0) return; // No renderers found, can't scale

        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }

        // The actual size of the car in world units
        Vector3 carSize = combinedBounds.size;

        // If carSize is zero on any axis, we can't divide by it.
        if (carSize.x == 0 || carSize.y == 0) return;

        // Calculate the scale factor needed for each axis to fit inside the frame
        float scaleX = targetFrameSize.x / carSize.x;
        float scaleY = targetFrameSize.y / carSize.y;
        
        // Use the SMALLER of the two scale factors to ensure the whole car fits
        // without being stretched or squashed.
        float finalScale = Mathf.Min(scaleX, scaleY);

        // Apply the calculated uniform scale
        carInstance.transform.localScale = new Vector3(finalScale, finalScale, finalScale);
    }

    void DisableCarComponents(GameObject carInstance)
    {
        // 1. Disable the controller script(s) so the car doesn't respond to input
        var carController = carInstance.GetComponent<CarController>();
        if (carController != null) carController.enabled = false;

        var multiWheelController = carInstance.GetComponent<MultiWheelCarController>();
        if (multiWheelController != null) multiWheelController.enabled = false;
        
        // 2. Disable all physics by making rigidbodies kinematic
        Rigidbody2D[] allRigidbodies = carInstance.GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rb in allRigidbodies)
        {
            rb.isKinematic = true; // Stops all physics forces, including gravity
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // 3. Disable the smoke particle effect
        ParticleSystem smokeEffect = carInstance.GetComponentInChildren<ParticleSystem>();
        if (smokeEffect != null)
        {
            smokeEffect.gameObject.SetActive(false);
        }
    }

    void UpdateSelectButton()
    {
        CarData currentCarData = carDatabase.allCars[currentCarIndex];
        bool isUnlocked = GameManager.Instance.IsCarUnlocked(currentCarIndex) || currentCarData.isUnlockedByDefault;

        if (isUnlocked)
        {
            lockIcon.SetActive(false);
            selectButtonText.text = "SELECT";
        }
        else
        {
            lockIcon.SetActive(true);
            selectButtonText.text = $"BUY ({currentCarData.price})";
        }
    }

    public void OnSelectButtonPressed()
    {
        CarData currentCarData = carDatabase.allCars[currentCarIndex];
        bool isUnlocked = GameManager.Instance.IsCarUnlocked(currentCarIndex) || currentCarData.isUnlockedByDefault;

        if (isUnlocked)
        {
            // If unlocked, select the car and start the game
            GameManager.Instance.selectedCarIndex = currentCarIndex;
            GameManager.LoadScene("GameScene");
        }
        else
        {
            // If locked, try to buy the car
            bool purchaseSuccessful = GameManager.Instance.TryPurchaseCar(currentCarData, currentCarIndex);
            if (purchaseSuccessful)
            {
                // If purchase works, update the button to show "SELECT"
                UpdateSelectButton();
            }
            else
            {
                // If not enough coins, show the error message
                StartCoroutine(ShowNotEnoughCoinsMessage());
            }
        }
    }

    private IEnumerator ShowNotEnoughCoinsMessage()
    {
        notEnoughCoinsText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f); // Show the message for 2 seconds
        notEnoughCoinsText.gameObject.SetActive(false);
    }
}