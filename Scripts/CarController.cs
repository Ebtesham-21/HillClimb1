using UnityEngine;

public class CarController : MonoBehaviour
{
    // --- REFERENCES ---
    [Header("References")]
    public Transform carBody;
    public Transform wheelFL; // Still needed for the raycast
    public Transform wheelFR; // Still needed for the raycast
    public Collider2D wheelFLCollider;
    public Collider2D wheelFRCollider;
    public ParticleSystem exhaustSmoke; 


    
 
    [Header("Effects")]
    [Tooltip("How long after the car starts moving before smoke appears.")]
    public float smokeStartDelay = 2f;

    private float timeSpentMoving = 0f;
    private bool isSmokeActive = false;
  

    // --- NEW: References for the new physics model ---
    [Header("Physics References")]
    public WheelJoint2D backWheelJoint;
    public WheelJoint2D frontWheelJoint;
    private JointMotor2D motor; // We will configure and reuse this

    // --- FUEL SYSTEM (No changes needed) ---
    [Header("Fuel System")]
    public float maxFuel = 100f;
    public float fuelConsumptionRate = 2.5f;
    public float CurrentFuel { get; private set; }
    public bool HasFuel { get; private set; }

    // --- MOVEMENT SETTINGS (Modified) ---
    [Header("Movement Settings")]
    [Tooltip("The speed of the motor on the wheels.")]
    public float motorSpeed = 2500f;
    [Tooltip("The maximum torque the motor can apply to the wheels.")]
    public float maxMotorTorque = 1000f;
    public float CurrentForwardSpeed { get; private set; } // We still calculate this for the UI

    // --- AIR CONTROL (No changes needed) ---
    [Header("Air Control")]
    public float airControlTorque = 300f;

    // --- VISUALS & GENERAL SETTINGS (Modified) ---
    [Header("General Settings")]
    public float bodyTiltOnGround = 15f;
    public float tiltSmoothSpeed = 5f;
    public LayerMask groundLayer;
    
    private Rigidbody2D rb; // This now refers to the car BODY's rigidbody
    public float HorizontalInput { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Gets the Rigidbody2D of the car body
        CurrentFuel = maxFuel;
        HasFuel = true;

        // Initialize the motor struct
        motor = new JointMotor2D();
    }

    void Update()
    {
        HorizontalInput = Input.GetAxis("Horizontal");
        ConsumeFuel();
        HandleSmokeEffect();
        
        // VISUAL wheel rotation is now handled automatically by the wheel's Rigidbody2D.
        // The old HandleVisualWheelRotation() method can be deleted.
    }

    private void HandleSmokeEffect()
{
    // Safety check first
    if (exhaustSmoke == null) return;

    // Condition 1: Is the player trying to move forward?
    bool isAccelerating = HorizontalInput > 0.1f && IsGrounded() && HasFuel;

    if (isAccelerating)
    {
        // If accelerating, increase the timer.
        timeSpentMoving += Time.deltaTime;
    }
    else
    {
        // If not accelerating, reset the timer.
        timeSpentMoving = 0f;
    }

    // Condition 2: Have we been accelerating long enough?
    bool shouldSmokeBeOn = timeSpentMoving >= smokeStartDelay;

    // --- NEW, SIMPLIFIED LOGIC ---
    // Now we manage the state directly.

    if (shouldSmokeBeOn && !exhaustSmoke.isPlaying)
    {
        // If the smoke SHOULD be on, but it's not currently playing, PLAY it.
        exhaustSmoke.Play();
    }
    else if (!shouldSmokeBeOn && exhaustSmoke.isPlaying)
    {
        // If the smoke SHOULD be off, but it IS currently playing, STOP it.
        exhaustSmoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}

    void ConsumeFuel()
    {
        // This logic is perfect, but we'll use our new IsGrounded() check.
        // We only consume fuel if ACCELERATING FORWARD.
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

    // --- In CarController.cs ---

    // ... (place this method below ConsumeFuel() or wherever you like) ...

    public void AddFuel(float amount)
    {
        CurrentFuel += amount;
        
        // Clamp the fuel so it doesn't go over the maximum
        if (CurrentFuel > maxFuel)
        {
            CurrentFuel = maxFuel;
        }

        // If we were out of fuel, this makes the car drivable again
        if (CurrentFuel > 0)
        {
            HasFuel = true;
        }

        Debug.Log("Added " + amount + " fuel. Current fuel: " + CurrentFuel);
    }

    void FixedUpdate()
    {

        Debug.Log("IsGrounded check: " + IsGrounded() + " | HasFuel check: " + HasFuel); // <-- ADD THIS LINE
        // We still calculate this for the speedometer UI.
        CurrentForwardSpeed = Vector2.Dot(rb.velocity, transform.right);

        // --- NEW MOVEMENT LOGIC using WheelJoint Motors ---
        if (HasFuel && IsGrounded())
        {
            // Apply motor torque based on player input
            motor.motorSpeed = -HorizontalInput * motorSpeed;
            motor.maxMotorTorque = maxMotorTorque;
        }
        else
        {
            // No fuel or in the air, turn off the motor
            motor.motorSpeed = 0;
            motor.maxMotorTorque = maxMotorTorque; // Can keep torque to allow for braking
        }

        // Apply the motor configuration to both wheels
        backWheelJoint.motor = motor;
        frontWheelJoint.motor = motor;

        // --- MODIFIED Ground/Air Control ---
        if (IsGrounded())
        {
            // The physical ground alignment is now automatic thanks to the joints.
            // We only need to handle the VISUAL body tilt here.
            float targetBodyTilt = -HorizontalInput * bodyTiltOnGround;
            carBody.localEulerAngles = new Vector3(0, 0,
                Mathf.LerpAngle(carBody.localEulerAngles.z, targetBodyTilt, Time.fixedDeltaTime * tiltSmoothSpeed));
        }
        else
        {
            // Air control logic is still perfect
            HandleAirControl();
        }
    }

    void HandleAirControl()
    {
        rb.AddTorque(HorizontalInput * airControlTorque * Time.fixedDeltaTime);
        // Reset the visual tilt when in the air
        carBody.localEulerAngles = new Vector3(0, 0,
            Mathf.LerpAngle(carBody.localEulerAngles.z, 0, Time.fixedDeltaTime * tiltSmoothSpeed));
    }

    bool IsGrounded()
    {
        // This check is still valid and works perfectly.
        return wheelFLCollider.IsTouchingLayers(groundLayer) || wheelFRCollider.IsTouchingLayers(groundLayer);
    }
}