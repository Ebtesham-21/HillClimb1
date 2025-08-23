using UnityEngine;
using UnityEngine.UI;

public class SpeedometerUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The car controller script to get speed from.")]
    public CarController carController;
    [Tooltip("The RectTransform of the needle image.")]
    public RectTransform needleTransform;

    [Header("Speedometer Settings")]
    [Tooltip("The speed that corresponds to the minimum angle (e.g., 0).")]
    public float minSpeed = 0f;
    [Tooltip("The speed that corresponds to the maximum angle (e.g., 200).")]
    public float maxSpeed = 200f; // This is for display, can be different from the car's actual maxSpeed

    [Tooltip("The rotation of the needle at minimum speed.")]
    public float minAngle = 0f;
    [Tooltip("The rotation of the needle at maximum speed.")]
    public float maxAngle = -180f; // Negative because UI rotation is often clockwise

    void Update()
    {
        if (carController == null || needleTransform == null)
        {
            // Don't do anything if references are not set
            return;
        }

        // Get the car's current forward speed (absolute value for display)
        float speed = Mathf.Abs(carController.CurrentForwardSpeed);

        // Clamp the speed to our display range to prevent the needle from going too far
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        
        // Calculate how far we are through the speed range (0.0 to 1.0)
        float speedPercentage = (speed - minSpeed) / (maxSpeed - minSpeed);

        // Map this percentage to our angle range
        float targetAngle = Mathf.Lerp(minAngle, maxAngle, speedPercentage);

        // Apply the rotation to the needle
        // We use LerpAngle for a slightly smoother needle movement
        float smoothedAngle = Mathf.LerpAngle(needleTransform.localEulerAngles.z, targetAngle, Time.deltaTime * 10f);
        needleTransform.localEulerAngles = new Vector3(0, 0, smoothedAngle);
    }
}
