using UnityEngine;
[CreateAssetMenu(fileName = "NewCarData", menuName = "Car Data")]
public class CarData : ScriptableObject
{
    public GameObject carPrefab;
    public string carName;
    // Add other stats like speed, handling, etc. here later

    public int price; // <-- NEW: The cost of the car in coins

    [Tooltip("Is this car unlocked by default? (Should only be true for the very first car)")]
    public bool isUnlockedByDefault = false; // <-- NEW
}