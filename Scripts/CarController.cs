using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Wheel Transforms (visuals only)")]
    public Transform wheelFL;
    public Transform wheelFR;

    [Header("Car body Tilt Only")]
    public Transform carBody; //only body tilts at z axis 

    [Header("Wheel Colliders")]
    public Collider2D wheelFLCollider;
    public Collider2D wheelFRCollider;

    [Header("Car tilt Settings")]
    public float motorForce = 2000f;
    public float wheelRotateSpeed = 20f;

    public float tiltOnGround = 15f;
    public float tiltInAir = 50f;
    public float tiltSmooth = 5f;
    public float maxTilt = 30f;

    private Rigidbody2D rb;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Auto-find wheel colliders
        if (wheelFLCollider == null)
            wheelFLCollider = transform.Find("WheelFL").GetComponent<Collider2D>();
        if (wheelFRCollider == null)
            wheelFRCollider = transform.Find("wheelFR").GetComponent<Collider2D>();

        // Auto find wheel transformers

        if (wheelFL == null)
            wheelFL = transform.Find("WheelFL");
        if (wheelFR == null)
            wheelFR = transform.Find("WheelFR");


        // Auto find car body
        if (carBody == null)
            carBody = transform.Find("Body");


    }


    void FixedUpdate()
    {
        float move = Input.GetAxis("Horizontal");

        rb.AddForce(Vector2.right * move * motorForce * Time.fixedDeltaTime);

        wheelFL.Rotate(0, 0, -move * wheelRotateSpeed);
        wheelFR.Rotate(0, 0, -move * wheelRotateSpeed);

          // 3️⃣ Tilt car body
        if (IsGrounded())
        {
            float targetTilt = 0f;
            if (move > 0) targetTilt = -tiltOnGround; // tilt left on accelerator
            else if (move < 0) targetTilt = tiltOnGround; // tilt right on brake

            carBody.localEulerAngles = new Vector3(0, 0,
                Mathf.LerpAngle(carBody.localEulerAngles.z, targetTilt, Time.fixedDeltaTime * tiltSmooth));
        }
        else
        {
            // In air, apply torque to body visually (optional)
            float bodyTilt = carBody.localEulerAngles.z;
            bodyTilt -= move * tiltInAir * Time.fixedDeltaTime;
            bodyTilt = Mathf.Clamp(bodyTilt, -maxTilt, maxTilt);
            carBody.localEulerAngles = new Vector3(0, 0, bodyTilt);
        }
    }

    bool IsGrounded()
    {
        return wheelFLCollider.IsTouchingLayers() || wheelFRCollider.IsTouchingLayers();
    }
}


    