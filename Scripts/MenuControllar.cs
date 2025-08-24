using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use this if you have TextMeshPro for better text

public class MenuController : MonoBehaviour
{
    public CarData[] allCars; // Assign all your CarData assets here
    public Transform carSpawnPoint; // An empty GameObject where the car will appear
    public TextMeshProUGUI carNameText; // The text to display the name

    [Header("Menu Display Settings")] // A new header for clarity
    [Tooltip("The scale to apply to the car model when displayed in the menu.")]
    public Vector3 menuCarScale = new Vector3(2f, 2f, 1f); // Make it twice as big by default

    // You probably have these UI scripts in your menu scene too.
// If not, you can skip adding them.
    private SpeedometerUI speedometer;
    private FuelMeterUI fuelMeter;
    private PedalControllerUI pedalController;



    private int currentCarIndex = 0;
    private GameObject currentCarInstance;

    void Start()
    {
         // --- NEW: Find UI controllers ---
    // We can do this since the menu UI is persistent
    speedometer = FindObjectOfType<SpeedometerUI>();
    fuelMeter = FindObjectOfType<FuelMeterUI>();
    pedalController = FindObjectOfType<PedalControllerUI>();
        // Spawn the first car
        SwitchCar(0);
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

    if (currentCarIndex < 0) currentCarIndex = allCars.Length - 1;
    if (currentCarIndex >= allCars.Length) currentCarIndex = 0;

    if (currentCarInstance != null)
    {
        Destroy(currentCarInstance);
    }

    // Spawn the new car model
    currentCarInstance = Instantiate(allCars[currentCarIndex].carPrefab, carSpawnPoint.position, carSpawnPoint.rotation);
    carNameText.text = allCars[currentCarIndex].carName;
    
    currentCarInstance.transform.localScale = menuCarScale;

    // --- NEW: Assign the NEW, LIVE reference ---
    CarController newCarController = currentCarInstance.GetComponent<CarController>();
    if (speedometer != null) speedometer.carController = newCarController;
    if (fuelMeter != null) fuelMeter.carController = newCarController;
    if (pedalController != null) pedalController.carController = newCarController;

    // --- NEW: Disable all physics components on the spawned car ---
        // This stops it from falling or reacting to physics in the menu.
        Rigidbody2D[] allRigidbodies = currentCarInstance.GetComponentsInChildren<Rigidbody2D>();
    foreach (Rigidbody2D rb in allRigidbodies)
    {
        rb.simulated = false; // 'simulated = false' is the best way to turn off physics
    }

    // Also disable the main CarController script itself, as we don't need it in the menu
    CarController carController = currentCarInstance.GetComponent<CarController>();
    if (carController != null)
    {
        carController.enabled = false;
    }
}

    public void StartGame()
    {
        // Tell the GameManager which car was selected
        GameManager.Instance.selectedCarIndex = currentCarIndex;
        
        // Use our static method to go to the LoaderScene, which will then load the GameScene
        GameManager.LoadScene("GameScene");
    }
}