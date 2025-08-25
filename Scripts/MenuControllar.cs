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

    // --- (Your existing reference assignment code is here, which is fine) ---
    CarController newCarController = currentCarInstance.GetComponent<CarController>();
    // ...

    // --- (Your existing physics disabling code is here, which is fine) ---
    Rigidbody2D[] allRigridbodies = currentCarInstance.GetComponentsInChildren<Rigidbody2D>();
        // ...
     foreach (Rigidbody2D rb in allRigridbodies)
    {
        // Make the Rigidbody kinematic - it will no longer be affected by gravity or forces.
        rb.isKinematic = true;

        // Also stop its velocity just in case it had any from the spawn frame.
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
    
    // Disable the CarController script
        if (newCarController != null)
        {
            newCarController.enabled = false;
        }
    
    // --- NEW CODE TO DISABLE SMOKE ---
    // Find the ParticleSystem component on the newly spawned car instance.
    // We use GetComponentInChildren because the smoke might be on a child object.
    ParticleSystem smokeEffect = currentCarInstance.GetComponentInChildren<ParticleSystem>();
    if (smokeEffect != null)
    {
        // Deactivate the entire GameObject that the particle system is attached to.
        // This is the cleanest and most reliable way to ensure it's completely off.
        smokeEffect.gameObject.SetActive(false);
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