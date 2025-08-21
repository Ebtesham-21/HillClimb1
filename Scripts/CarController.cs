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

    [Header("Movement Settings")]
    [Tooltip("The main force applied to move the car forward/backward.")]
    public float motorForce = 2000f;
    [Tooltip("How fast the wheel visuals spin.")]
    public float wheelRotateSpeed = 20f;

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
    private float horizontalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

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
        horizontalInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        // Apply movement force
        rb.AddForce(transform.right * horizontalInput * motorForce * Time.fixedDeltaTime);

        // Rotate wheel visuals
        wheelFL.Rotate(0, 0, -rb.velocity.magnitude * Mathf.Sign(horizontalInput) * 0.5f);
        wheelFR.Rotate(0, 0, -rb.velocity.magnitude * Mathf.Sign(horizontalInput) * 0.5f);


        // Check if the car is on the ground
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
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            // Tilt based on input direction
            targetBodyTilt = -horizontalInput * bodyTiltOnGround;
        }

        // Smoothly apply the local tilt to the car body
        carBody.localEulerAngles = new Vector3(0, 0,
            Mathf.LerpAngle(carBody.localEulerAngles.z, targetBodyTilt, Time.fixedDeltaTime * tiltSmoothSpeed));
    }

    void HandleAirControl()
    {
        // --- 1. Apply torque to rotate the ENTIRE CAR in the air ---
        rb.AddTorque(-horizontalInput * airControlTorque * Time.fixedDeltaTime);

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