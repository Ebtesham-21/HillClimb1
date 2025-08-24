using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use this if you have TextMeshPro for better text

public class MenuController : MonoBehaviour
{
    public CarData[] allCars; // Assign all your CarData assets here
    public Transform carSpawnPoint; // An empty GameObject where the car will appear
    public TextMeshProUGUI carNameText; // The text to display the name

    private int currentCarIndex = 0;
    private GameObject currentCarInstance;

    void Start()
    {
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