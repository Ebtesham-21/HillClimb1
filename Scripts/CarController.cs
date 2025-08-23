using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]
    public Transform carBody;
    public Transform wheelFL;
    public Transform wheelFR;
    public Collider2D wheelFLCollider;
    public Collider2D wheelFRCollider;


    

    [Header("Fuel System")]
    [Tooltip("The maximum amount of fuel the car can hold.")]
    public float maxFuel = 100f;
    [Tooltip("How much fuel is consumed per second when accelerating.")]
    public float fuelConsumptionRate = 2.5f;

    // Public properties for other scripts (like our UI) to read
    public float CurrentFuel { get; private set; }
    public bool HasFuel { get; private set; }

    [Header("Movement Settings")]
    [Tooltip("The maximum speed the car can reach.")]
    public float maxSpeed = 50f;
    [Tooltip("How quickly the car reaches max speed.")]
    public float accelerationRate = 50f;
    [Tooltip("How quickly the car stops when not accelerating.")]
    public float decelerationRate = 100f;
    [Tooltip("How much force is applied when braking (going in the opposite direction).")]
    public float brakeForce = 300f;
    // Your old motorForce variable can be removed or kept for reference
    // public float motorForce = 2000f; 

    // ... (keep the other variables) ...

    // This public property will let our UI script easily read the car's speed
    public float CurrentForwardSpeed { get; private set; }

    [Header("Ground Control")]
    [Tooltip("How quickly the car aligns itself to the slope of the ground.")]
    public float groundAlignmentSpeed = 5f;
    [Tooltip("The Z-axis tilt applied to the car body when accelerating on the ground.")]
    public float bodyTiltOnGround = 15f;

    [Header("Air Control")]
    [Tooltip("The torque force applied to rotate the car in the air.")]
    public float airControlTorque = 50f;

    [Header("General Settings")]
    [Tooltip("How smoothly the visual body tilt is applied.")]
    public float tiltSmoothSpeed = 5f;
    [Tooltip("Which layers are considered 'ground' for alignment.")]
    public LayerMask groundLayer; // <-- NEW!

    private Rigidbody2D rb;
    public float HorizontalInput { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
         // --- Initialize Fuel ---
        CurrentFuel = maxFuel;
        HasFuel = true;

        // This auto-finding logic is good!
        if (wheelFLCollider == null) wheelFLCollider = transform.Find("WheelFL").GetComponent<Collider2D>();
        if (wheelFRCollider == null) wheelFRCollider = transform.Find("WheelFR").GetComponent<Collider2D>();
        if (wheelFL == null) wheelFL = transform.Find("WheelFL");
        if (wheelFR == null) wheelFR = transform.Find("WheelFR");
        if (carBody == null) carBody = transform.Find("Body");
    }

    // Get input here, so it's not missed between physics frames
    void Update()
    {
        HorizontalInput = Input.GetAxis("Horizontal");
        ConsumeFuel();
    }
    void ConsumeFuel()
    {
        // Only consume fuel if we are trying to accelerate and have fuel left
        if (HasFuel && HorizontalInput > 0.1f && IsGrounded())
        {
            CurrentFuel -= fuelConsumptionRate * Time.deltaTime;

            // Clamp the fuel to a minimum of 0
            if (CurrentFuel <= 0)
            {
                CurrentFuel = 0;
                HasFuel = false;
                Debug.Log("Out of Fuel!");
            }
        }
    }

    void FixedUpdate()
{
    // --- 1. Calculate current speed and target speed ---
    // Vector2.Dot gives us the speed along the car's forward direction (positive if forward, negative if backward)
    CurrentForwardSpeed = Vector2.Dot(rb.velocity, transform.right);

    // The speed we want to be going
    float targetSpeed = HorizontalInput * maxSpeed;

    // --- 2. Calculate the required acceleration ---
    float speedDifference = targetSpeed - CurrentForwardSpeed;

    // Determine if we are accelerating, decelerating, or braking
    float acceleration = 0f;
    if (Mathf.Abs(HorizontalInput) > 0.1f && Mathf.Sign(HorizontalInput) == Mathf.Sign(CurrentForwardSpeed))
    {
        // Player is pressing the gas in the direction of motion
        acceleration = accelerationRate;
    }
    else if (Mathf.Abs(HorizontalInput) > 0.1f && CurrentForwardSpeed != 0)
    {
        // Player is braking (pressing gas in the opposite direction of motion)
        acceleration = brakeForce;
    }
    else
    {
        // Player is coasting (no input)
        acceleration = decelerationRate;
    }

    // --- 3. Apply the force ---
    // Only apply force if the car is on the ground
    if (IsGrounded() && HasFuel)
{
    // Calculate the movement force
    float movementForce = speedDifference * acceleration;
    rb.AddForce(transform.right * movementForce * Time.fixedDeltaTime);
}

    // --- 4. Handle Visuals and Ground Control (this part remains the same) ---
    // This is a better way to calculate wheel spin based on actual velocity
    float forwardVelocity = Vector2.Dot(rb.velocity, transform.right);
    wheelFL.Rotate(0, 0, -forwardVelocity * 5f * Time.fixedDeltaTime);
    wheelFR.Rotate(0, 0, -forwardVelocity * 5f * Time.fixedDeltaTime);

    if (IsGrounded())
    {
        HandleGroundControl();
    }
    else
    {
        HandleAirControl();
    }
}

    void HandleGroundControl()
    {
        // --- 1. Align the ENTIRE CAR to the ground slope ---
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, 2f, groundLayer);

        if (hit.collider != null)
        {
            // Calculate the angle of the ground
            float targetAngle = Vector2.SignedAngle(Vector2.up, hit.normal);

            // Smoothly rotate the Rigidbody to match the ground angle
            float newRotation = Mathf.LerpAngle(rb.rotation, targetAngle, groundAlignmentSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }


        // --- 2. Tilt ONLY THE BODY for acceleration/braking effect ---
        float targetBodyTilt = 0f;
        if (Mathf.Abs(HorizontalInput) > 0.1f)
        {
            // Tilt based on input direction
            targetBodyTilt = -HorizontalInput * bodyTiltOnGround;
        }

        // Smoothly apply the local tilt to the car body
        carBody.localEulerAngles = new Vector3(0, 0,
            Mathf.LerpAngle(carBody.localEulerAngles.z, targetBodyTilt, Time.fixedDeltaTime * tiltSmoothSpeed));
    }

    void HandleAirControl()
    {
        // --- 1. Apply torque to rotate the ENTIRE CAR in the air ---
        rb.AddTorque(-HorizontalInput * airControlTorque * Time.fixedDeltaTime);

        // --- 2. Reset the visual body tilt ---
        // This ensures the body doesn't keep its acceleration tilt while airborne
        carBody.localEulerAngles = new Vector3(0, 0,
            Mathf.LerpAngle(carBody.localEulerAngles.z, 0, Time.fixedDeltaTime * tiltSmoothSpeed));
    }

    bool IsGrounded()
    {
        // Use the colliders to check for ground contact
        return wheelFLCollider.IsTouchingLayers(groundLayer) || wheelFRCollider.IsTouchingLayers(groundLayer);
    }
}