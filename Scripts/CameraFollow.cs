using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraFollow : MonoBehaviour
{
    [Header("Target to follow")]
    public Transform target; // the car

    [Header("Offset from target")]
    public Vector3 offset = new Vector3(-6f, 2f, -10f);

    [Header("Follow settings")]
    public float smoothSpeed = 5f;


    void LateUpdate()
    {
        if (target == null) return;


        // Position set
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            offset.z
        );


        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

    }
}
