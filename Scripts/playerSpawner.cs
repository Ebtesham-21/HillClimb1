using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public CarDatabase carDatabase;
    public Transform spawnPoint;

    void Awake()
    {
        int selectedIndex = 0;
        if (GameManager.Instance != null)
        {
            selectedIndex = GameManager.Instance.selectedCarIndex;
        }
        
        // Instantiate the correct car prefab
        GameObject carInstance = Instantiate(carDatabase.allCars[selectedIndex].carPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Get the CarController component from the car we just spawned
        CarController spawnedCarController = carInstance.GetComponent<CarController>();

        // --- FIX FOR UI METERS ---
        
        // Find the SpeedometerUI in the scene
        SpeedometerUI speedometer = FindObjectOfType<SpeedometerUI>();
        if (speedometer != null)
        {
            // Assign the spawned car to its 'carController' reference
            speedometer.carController = spawnedCarController;
        }
        else { Debug.LogWarning("Could not find SpeedometerUI in the scene!"); }
        
        // Find the FuelMeterUI in the scene
        FuelMeterUI fuelMeter = FindObjectOfType<FuelMeterUI>();
        if (fuelMeter != null)
        {
            // Assign the spawned car to its 'carController' reference
            fuelMeter.carController = spawnedCarController;
        }
        else { Debug.LogWarning("Could not find FuelMeterUI in the scene!"); }
        
        // Find the PedalControllerUI in the scene
        PedalControllerUI pedalController = FindObjectOfType<PedalControllerUI>();
        if (pedalController != null)
        {
            pedalController.carController = spawnedCarController;
        }

        // --- FIX FOR CAMERA AND GROUNDSTREAMER ---
        CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = carInstance.transform;
        }
        
        GroundStreamer groundStreamer = FindObjectOfType<GroundStreamer>();
        if(groundStreamer != null)
        {
            groundStreamer.player = carInstance.transform;
        }
    }
}