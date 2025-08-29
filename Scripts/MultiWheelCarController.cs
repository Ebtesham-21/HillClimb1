using UnityEngine;
using System.Collections.Generic; // IMPORTANT: Required to use Lists

// This is a new, separate script for vehicles with more than 2 wheels.
public class MultiWheelCarController : MonoBehaviour
{
    // --- REFERENCES ---
    [Header("References")]
    public Transform carBody;
    public ParticleSystem exhaustSmoke;

    // --- DYNAMIC WHEEL REFERENCES using Lists ---
    [Header("Physics References (Multi-Wheel)")]
    [Tooltip("Add all BACK wheel joints to this list. These provide the main driving force.")]
    public List<WheelJoint2D> backWheelJoints;
    [Tooltip("Add all FRONT wheel joints to this list. They will also receive motor force.")]
    public List<WheelJoint2D> frontWheelJoints;
    [Tooltip("Add the COLLIDERS of ALL wheels to this list for the IsGrounded() check.")]
    public List<Collider2D> allWheelColliders;
    
    private JointMotor2D motor;

    // --- FUEL SYSTEM (Identical to original script) ---
    [Header("Fuel System")]
    public float maxFuel = 100f;
    public float fuelConsumptionRate = 2.5f;
    public float CurrentFuel { get; private set; }
    public bool HasFuel { get; private set; }

    // --- MOVEMENT SETTINGS (Identical to original script) ---
    [Header("Movement Settings")]
    public float motorSpeed = 2500f;
    public float maxMotorTorque = 1000f;
    public float CurrentForwardSpeed { get; private set; }

    // --- AIR CONTROL (Identical to original script) ---
    [Header("Air Control")]
    public float airControlTorque = 300f;

    // --- VISUALS & GENERAL SETTINGS (Identical to original script) ---
    [Header("General Settings")]
    public float bodyTiltOnGround = 15f;
    public float tiltSmoothSpeed = 5f;
    public LayerMask groundLayer;
    
    [Header("Effects")]
    public float smokeStartDelay = 2f;
    private float timeSpentMoving = 0f;
    
    private Rigidbody2D rb;
    public float HorizontalInput { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        CurrentFuel = maxFuel;
        HasFuel = true;
        motor = new JointMotor2D();
    }

    void Update()
    {
        HorizontalInput = Input.GetAxis("Horizontal");
        ConsumeFuel();
        HandleSmokeEffect();
    }

    void FixedUpdate()
    {
        CurrentForwardSpeed = Vector2.Dot(rb.velocity, transform.right);

        if (HasFuel && IsGrounded())
        {
            motor.motorSpeed = -HorizontalInput * motorSpeed;
            motor.maxMotorTorque = maxMotorTorque;
        }
        else
        {
            motor.motorSpeed = 0;
            motor.maxMotorTorque = maxMotorTorque;
        }

        // --- MODIFIED: Apply motor to ALL wheels in the lists ---
        foreach (WheelJoint2D joint in backWheelJoints)
        {
            joint.motor = motor;
        }
        foreach (WheelJoint2D joint in frontWheelJoints)
        {
            joint.motor = motor;
        }

        if (IsGrounded())
        {
            float targetBodyTilt = -HorizontalInput * bodyTiltOnGround;
            carBody.localEulerAngles = new Vector3(0, 0,
                Mathf.LerpAngle(carBody.localEulerAngles.z, targetBodyTilt, Time.fixedDeltaTime * tiltSmoothSpeed));
        }
        else
        {
            HandleAirControl();
        }
    }

    // --- MODIFIED: IsGrounded now checks ALL wheel colliders in the list ---
    public bool IsGrounded()
    {
        if (allWheelColliders == null || allWheelColliders.Count == 0) return false;

        foreach (Collider2D wheelCollider in allWheelColliders)
        {
            if (wheelCollider.IsTouchingLayers(groundLayer))
            {
                return true; // If any wheel is touching, we're grounded.
            }
        }
        return false; // If we looped through all and none were touching.
    }
    
    // --- The rest of the methods are identical to your original script ---

    public void AddFuel(float amount)
    {
        CurrentFuel += amount;
        if (CurrentFuel > maxFuel) CurrentFuel = maxFuel;
        if (CurrentFuel > 0) HasFuel = true;
        Debug.Log("Added " + amount + " fuel. Current fuel: " + CurrentFuel);
    }
    
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

    private void HandleSmokeEffect()
    {
        if (exhaustSmoke == null) return;
        bool isAccelerating = HorizontalInput > 0.1f && IsGrounded() && HasFuel;
        timeSpentMoving = isAccelerating ? timeSpentMoving + Time.deltaTime : 0f;
        bool shouldSmokeBeOn = timeSpentMoving >= smokeStartDelay;
        var emission = exhaustSmoke.emission;
        if (shouldSmokeBeOn && !emission.enabled) emission.enabled = true;
        else if (!shouldSmokeBeOn && emission.enabled) emission.enabled = false;
    }

    void HandleAirControl()
    {
        rb.AddTorque(HorizontalInput * airControlTorque * Time.fixedDeltaTime);
        carBody.localEulerAngles = new Vector3(0, 0,
            Mathf.LerpAngle(carBody.localEulerAngles.z, 0, Time.fixedDeltaTime * tiltSmoothSpeed));
    }
}