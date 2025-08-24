using UnityEngine;
[CreateAssetMenu(fileName = "NewCarData", menuName = "Car Data")]
public class CarData : ScriptableObject
{
    public GameObject carPrefab;
    public string carName;
    // Add other stats like speed, handling, etc. here later
}