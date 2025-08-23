using UnityEngine;
using UnityEngine.UI;

public class PedalControllerUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The car controller script to read input from.")]
    public CarController carController; // We need this to get the horizontalInput value
    [Tooltip("The RectTransform of the accelerator pedal image.")]
    public RectTransform acceleratorPedal;
    [Tooltip("The RectTransform of the brake pedal image.")]
    public RectTransform brakePedal;

    [Header("Animation Settings")]
    [Tooltip("The angle the pedal rotates to when pressed.")]
    public float pressedAngleX = -15f; // A slight tilt forward
    [Tooltip("The angle the pedal rests at when not pressed.")]
    public float releasedAngleX = 0f;
    [Tooltip("How quickly the pedal animates.")]
    public float animationSpeed = 10f;

    // We will get the input value directly from the CarController
    // This is better than getting it twice (once in the car, once here)
    // To do this, we need to make the horizontalInput in CarController accessible.

    void Update()
    {
        if (carController == null || acceleratorPedal == null || brakePedal == null)
        {
            return; // Don't run if references are missing
        }

        // Get the input value from the car controller
        float input = carController.HorizontalInput;

        // --- Animate Accelerator Pedal ---
        // It's "pressed" if input is positive (D key or right arrow)
        float targetAcceleratorAngle = (input > 0.1f) ? pressedAngleX : releasedAngleX;
        AnimatePedal(acceleratorPedal, targetAcceleratorAngle);

        // --- Animate Brake Pedal ---
        // It's "pressed" if input is negative (A key or left arrow)
        float targetBrakeAngle = (input < -0.1f) ? pressedAngleX : releasedAngleX;
        AnimatePedal(brakePedal, targetBrakeAngle);
    }

    // A helper method to handle the animation logic to avoid repeating code
    private void AnimatePedal(RectTransform pedal, float targetAngle)
    {
        // Get the current rotation
        Quaternion currentRotation = pedal.localRotation;
        // Create the target rotation
        Quaternion targetRotation = Quaternion.Euler(targetAngle, 0, 0);

        // Smoothly move from the current to the target rotation
        pedal.localRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * animationSpeed);
    }
}