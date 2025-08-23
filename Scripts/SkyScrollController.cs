using UnityEngine;

public class SkyScrollController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The transform of the car to track its position.")]
    public Transform carTransform;
    [Tooltip("The sky material that we will be scrolling.")]
    public Material skyMaterial;

    [Header("Scrolling Settings")]
    [Tooltip("Controls how fast the sky scrolls relative to the car's movement. A smaller value makes it look more distant.")]
    [Range(0f, 1f)]
    public float parallaxFactor = 0.05f;

    // We store the initial offset to handle material properties correctly
    private Vector2 initialOffset;

    void Start()
    {
        if (carTransform == null || skyMaterial == null)
        {
            Debug.LogError("Car Transform or Sky Material is not assigned in the SkyScrollController!");
            enabled = false; // Disable the script if references are missing
            return;
        }

        // Store the initial offset of the material if it has one
        initialOffset = skyMaterial.mainTextureOffset;
    }

    // Using LateUpdate ensures that the car has finished its movement for the frame
    void LateUpdate()
{
    float offsetX = (initialOffset.x + carTransform.position.x * parallaxFactor) % 1f;
    Vector2 newOffset = new Vector2(offsetX, initialOffset.y);

    // Apply the new offset to BOTH texture properties in the material
    skyMaterial.SetTextureOffset("_MainTex", newOffset);
    skyMaterial.SetTextureOffset("_SecondTex", newOffset);
}

void OnDisable()
{
    if (skyMaterial != null)
    {
        // Reset both texture offsets when the game stops
        skyMaterial.SetTextureOffset("_MainTex", initialOffset);
        skyMaterial.SetTextureOffset("_SecondTex", initialOffset);
    }
}
}