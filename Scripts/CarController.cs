using UnityEngine;

public class CarController : MonoBehaviour
{
    // --- (Keep all your existing Headers and Variables here, they are fine) ---
    // [Header("References")] ...
    // [Header("Fuel System")] ...
    // [Header("Movement Settings")] ...
    // [Header("Ground Control")] ...
    // [Header("Air Control")] ...
    // [Header("General Settings")] ...

    // --- (Declare all your variables as before) ---
    [Header("References")]
    public Transform carBody;
    public Transform wheelFL;
    public Transform wheelFR;
    public Collider2D wheelFLCollider;
    public Collider2D wheelFRCollider;

    [Header("Fuel System")]
    public float maxFuel = 100f;
    public float fuelConsumptionRate = 2.5f;
    public float CurrentFuel { get; private set; }
    public bool HasFuel { get; private set; }

    [Header("Movement Settings")]
    public float maxSpeed = 50f;
    public float accelerationRate = 50f;
    public float decelerationRate = 100f;
    public float brakeForce = 300f;
    public float CurrentForwardSpeed { get; private set; }

    [Header("Ground Control")]
    public float groundAlignmentSpeed = 5f;
    public float bodyTiltOnGround = 15f;

    [Header("Air Control")]
    public float airControlTorque = 50f;

    [Header("General Settings")]
    public float tiltSmoothSpeed = 5f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    public float HorizontalInput { get; private set; }
    
    // --- NEW: For Visual Wheel Rotation ---
    private float wheelAngle = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        CurrentFuel = maxFuel;
        HasFuel = true;

        if (wheelFLCollider == null) wheelFLCollider = transform.Find("WheelFL").GetComponent<Collider2D>();
        if (wheelFRCollider == null) wheelFRCollider = transform.Find("WheelFR").GetComponent<Collider2D>();
        if (wheelFL == null) wheelFL = transform.Find("WheelFL");
        if (wheelFR == null) wheelFR = transform.Find("WheelFR");
        if (carBody == null) carBody = transform.Find("Body");
    }

    void Update()
    {
        HorizontalInput = Input.GetAxis("Horizontal");
        ConsumeFuel();
        
        // --- NEW: Handle VISUAL wheel rotation in Update ---
        // This is now decoupled from physics and will always be smooth.
        HandleVisualWheelRotation();
    }
    
    // --- (The ConsumeFuel method is fine, no changes needed) ---
    void ConsumeFuel()
    {
        if (HasFuel && HorizontalInput > 0.1f && IsGrounded())
        {
            CurrentFuel -= fuelConsumptionRate * Time.deltaTime;
            if (CurrentFuel <= 0)
            {
                CurrentFuel = 0;
                HasFuel = false;
            }
        }
    }

    void FixedUpdate()
    {
        // --- (The Movement Logic is fine, no changes needed) ---
        CurrentForwardSpeed = Vector2.Dot(rb.velocity, transform.right);
        float targetSpeed = HorizontalInput * maxSpeed;
        float speedDifference = targetSpeed - CurrentForwardSpeed;
        
        float acceleration = 0f;
        if (Mathf.Abs(HorizontalInput) > 0.1f && Mathf.Sign(HorizontalInput) == Mathf.Sign(CurrentForwardSpeed))
            acceleration = accelerationRate;
        else if (Mathf.Abs(HorizontalInput) > 0.1f && CurrentForwardSpeed != 0)
            acceleration = brakeForce;
        else
            acceleration = decelerationRate;

        if (IsGrounded() && HasFuel)
        {
            float movementForce = speedDifference * acceleration;
            rb.AddForce(transform.right * movementForce * Time.fixedDeltaTime);
        }
        
        // --- (Handle Ground/Air Control) ---
        if (IsGrounded())
        {
            HandleGroundControl(); // <-- This method is now completely rewritten
        }
        else
        {
            HandleAirControl(); // <-- This method is fine
        }
    }
    
    // --- NEW: Rewritten Method for Smooth Ground Alignment ---
    void HandleGroundControl()
    {
        // --- 1. Cast two rays, one from each wheel ---
        RaycastHit2D hitFront = Physics2D.Raycast(wheelFL.position, -transform.up, 2f, groundLayer);
        RaycastHit2D hitRear = Physics2D.Raycast(wheelFR.position, -transform.up, 2f, groundLayer);
        
        // --- 2. Calculate the average normal vector of the ground ---
        // This gives us the true "average slope" between the two wheels.
        Vector2 averageNormal = (hitFront.normal + hitRear.normal).normalized;

        // --- 3. Calculate the target angle from the average normal ---
        // We only proceed if at least one wheel is touching the ground.
        if (hitFront.collider != null || hitRear.collider != null)
        {
            float targetAngle = Vector2.SignedAngle(Vector2.up, averageNormal);
            
            // --- 4. Smoothly rotate the Rigidbody to match the average angle ---
            // This will feel much more stable and removes the "seesaw" effect.
            float newRotation = Mathf.LerpAngle(rb.rotation, targetAngle, groundAlignmentSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }

        // --- 5. Tilt the body visually (this logic is still good) ---
        float targetBodyTilt = -HorizontalInput * bodyTiltOnGround;
        carBody.localEulerAngles = new Vector3(0, 0,
            Mathf.LerpAngle(carBody.localEulerAngles.z, targetBodyTilt, Time.fixedDeltaTime * tiltSmoothSpeed));
    }
    
    // --- NEW: Method for Visual Wheel Rotation ---
    void HandleVisualWheelRotation()
    {
        // Calculate how much to rotate based on the car's actual forward speed
        float rotationAmount = CurrentForwardSpeed * 360f / (2f * Mathf.PI * 0.5f); // Approximation using wheel circumference
        
        // Add this frame's rotation to our total angle
        wheelAngle += rotationAmount * Time.deltaTime;
        
        // Apply the rotation directly to the localEulerAngles.
        // This is not affected by the parent's physics rotation.
        wheelFL.localEulerAngles = new Vector3(0, 0, -wheelAngle);
        wheelFR.localEulerAngles = new Vector3(0, 0, -wheelAngle);
    }
    
    // --- (Air Control and IsGrounded methods are fine, no changes needed) ---
    void HandleAirControl()
    {
        rb.AddTorque(-HorizontalInput * airControlTorque * Time.fixedDeltaTime);
        carBody.localEulerAngles = new Vector3(0, 0, 
            Mathf.LerpAngle(carBody.localEulerAngles.z, 0, Time.fixedDeltaTime * tiltSmoothSpeed));
    }

    bool IsGrounded()
    {
        return wheelFLCollider.IsTouchingLayers(groundLayer) || wheelFRCollider.IsTouchingLayers(groundLayer);
    }
}