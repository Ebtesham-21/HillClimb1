using UnityEngine;
using UnityEngine.UI;

public class FuelMeterUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The car controller script to get fuel data from.")]
    public CarController carController;
    [Tooltip("The RectTransform of the needle image.")]
    public RectTransform needleTransform;

    [Header("Fuel Meter Angles")]
    [Tooltip("The rotation of the needle when the tank is full.")]
    public float fullAngle = 0f;
    [Tooltip("The rotation of the needle when the tank is empty.")]
    public float emptyAngle = 90f; // Positive for clockwise rotation

    void Update()
    {
        if (carController == null || needleTransform == null)
        {
            return; // Don't do anything if references aren't set
        }

        // Calculate the current fuel percentage (from 0.0 to 1.0)
        float fuelPercentage = carController.CurrentFuel / carController.maxFuel;

        // Map this percentage to our angle range
        // We use LerpUnclamped so the needle can go a tiny bit past the mark for a nice feel
        float targetAngle = Mathf.LerpUnclamped(emptyAngle, fullAngle, fuelPercentage);

        // Apply the rotation to the needle with smoothing
        float smoothedAngle = Mathf.LerpAngle(needleTransform.localEulerAngles.z, targetAngle, Time.deltaTime * 5f);
        needleTransform.localEulerAngles = new Vector3(0, 0, smoothedAngle);
    }
}