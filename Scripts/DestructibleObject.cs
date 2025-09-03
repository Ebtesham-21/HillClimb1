using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [Header("Destruction Settings")]
    public float health = 50f;
    public ParticleSystem destructionEffect;
    
    private float initialHealth;

    void Awake()
    {
        // Store the starting health so we can reset it
        initialHealth = health;
    }

        private void OnCollisionEnter2D(Collision2D collision)
    {
        // --- THE FIX ---
        // First, try to find a standard CarController.
        CarController standardCar = collision.collider.GetComponentInParent<CarController>();
        if (standardCar != null)
        {
            // If we found one, handle the collision and we're done.
            HandleCarCollision(collision);
            return;
        }

        // If we didn't find a standard car, try to find a multi-wheel car.
        MultiWheelCarController multiWheelCar = collision.collider.GetComponentInParent<MultiWheelCarController>();
        if (multiWheelCar != null)
        {
            // If we found one of these, handle the collision.
            HandleCarCollision(collision);
        }
    }

    private void HandleCarCollision(Collision2D collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;
        TakeDamage(impactForce * 5f);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            DestroyObject();
        }
    }

    private void DestroyObject()
    {
        if (destructionEffect != null)
        {
            destructionEffect.transform.SetParent(null);
            destructionEffect.Play();
            Destroy(destructionEffect.gameObject, destructionEffect.main.duration);
        }
        
        // Return this object to the pool instead of destroying it
        ObjectPooler.Instance.ReturnToPool(gameObject.tag, gameObject);
    }

    // This method is called by the ObjectPooler when the object is reused.
    public void ResetState()
    {
        // Restore health to its original value
        health = initialHealth;
    }
}