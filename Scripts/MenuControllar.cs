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



    [Header("Car Preview Settings")]
    [Tooltip("The maximum bounding box size (W,H,D) that cars will be scaled to fit inside the menu frame.")]
    public Vector3 targetFrameSize = new Vector3(5f, 2f, 2f);



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

    // Spawn new car
    currentCarInstance = Instantiate(allCars[currentCarIndex].carPrefab, carSpawnPoint.position, carSpawnPoint.rotation);
    carNameText.text = allCars[currentCarIndex].carName;

    // -------------------
    // AUTO SCALE TO FRAME
    // -------------------
    // Desired bounding box size (frame size)
    Vector3 targetFrameSize = new Vector3(5f, 2f, 2f); 
    // ^ Adjust this to match your UI "frame" size (X=width, Y=height, Z=depth)

    // Get combined bounds of all renderers in the car prefab
    SpriteRenderer[] renderers = currentCarInstance.GetComponentsInChildren<SpriteRenderer>();
    if (renderers.Length > 0)
    {
        Bounds combinedBounds = renderers[0].bounds;
        foreach (SpriteRenderer r in renderers)
        {
            combinedBounds.Encapsulate(r.bounds);
        }

        // Car size
        Vector3 carSize = combinedBounds.size;

        // Scale factor = min of (target size / car size) for each axis
        float scaleX = targetFrameSize.x / carSize.x;
        float scaleY = targetFrameSize.y / carSize.y;
        float scaleZ = targetFrameSize.z / carSize.z;

        float finalScale = Mathf.Min(scaleX, scaleY, scaleZ);

        currentCarInstance.transform.localScale = Vector3.one * finalScale;
    }
    else
    {
        // fallback: use default scale
        currentCarInstance.transform.localScale = menuCarScale;
    }

    // Disable physics
    Rigidbody2D[] allRigridbodies = currentCarInstance.GetComponentsInChildren<Rigidbody2D>();
    foreach (Rigidbody2D rb in allRigridbodies)
    {
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    // Disable controller
    CarController newCarController = currentCarInstance.GetComponent<CarController>();
    if (newCarController != null)
    {
        newCarController.enabled = false;
    }

    // Disable smoke
    ParticleSystem smokeEffect = currentCarInstance.GetComponentInChildren<ParticleSystem>();
    if (smokeEffect != null)
    {
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