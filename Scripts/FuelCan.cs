using UnityEngine;

public class FuelCan : MonoBehaviour
{
    [Tooltip("The amount of fuel this can restores.")]
    public float fuelAmount = 25f; // Restore 25% of a full tank by default

    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Find the CarController on the object that collided with us
        CarController car = other.GetComponentInParent<CarController>();

        if (!isCollected && car != null)
        {
            // We found the car! Call its AddFuel method.
            car.AddFuel(fuelAmount);

            isCollected = true;

            // Optional: Play a fuel pickup sound effect here
            // AudioManager.Instance.PlayFuelSound();

            // Destroy the fuel can immediately after it's collected
            ObjectPooler.Instance.ReturnToPool(gameObject.tag, gameObject);
        }
    }

    public void ResetState()
{
    isCollected = false;
    // Also re-enable the collider if you disabled it
    GetComponent<Collider2D>().enabled = true; 
}
}