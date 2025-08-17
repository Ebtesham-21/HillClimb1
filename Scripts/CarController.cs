using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Wheel Transforms (visuals only)")]
    public Transform wheelFL;
    public Transform wheelFR;

    [Header("Wheel Colliders")]
    public Collider2D wheelFLCollider;
    public Collider2D wheelFRCollider;

    [Header("Car Settings")]
    public float motorForce = 2000f;     // Force applied to move car
    public float tiltTorque = 300f;      // Torque for tilting in air
    public float wheelRotateSpeed = 20f; // Visual wheel rotation multiplier

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Auto-find colliders if not assigned
        if (wheelFLCollider == null)
            wheelFLCollider = transform.Find("WheelFL").GetComponent<Collider2D>();
        if (wheelFRCollider == null)
            wheelFRCollider = transform.Find("WheelFR").GetComponent<Collider2D>();

        // Auto-find wheel transforms if not assigned
        if (wheelFL == null)
            wheelFL = transform.Find("WheelFL");
        if (wheelFR == null)
            wheelFR = transform.Find("WheelFR");
    }

    void FixedUpdate()
    {
        float move = Input.GetAxis("Horizontal"); // -1 for A, +1 for D

        // Move car body
        rb.AddForce(Vector2.right * move * motorForce * Time.fixedDeltaTime);

        float tiltAmount = 50f;

        if (move > 0)
        {
            rb.AddTorque(-tiltAmount * Time.fixedDeltaTime);
        }

        else if (move < 0)
        {
            rb.AddTorque(tiltAmount * Time.fixedDeltaTime);
        }

        // Rotate wheels visually
        wheelFL.Rotate(0, 0, -move * wheelRotateSpeed);
        wheelFR.Rotate(0, 0, -move * wheelRotateSpeed);

        //  Clamp car rotation so it doesn't flip too much
        float maxTilt = 30f;
        float clampedZ = Mathf.Clamp(rb.rotation, -maxTilt, maxTilt);
        rb.rotation = clampedZ;

    }

    bool IsGrounded()
    {
        // Returns true if either wheel touches ground
        return wheelFLCollider.IsTouchingLayers() || wheelFRCollider.IsTouchingLayers();
    }
}
