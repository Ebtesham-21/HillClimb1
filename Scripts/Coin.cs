using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 50;
    public float moveUpSpeed = 5f;
    public float fadeOutTime = 0.5f;

    private bool isCollected = false;
    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that entered the trigger is the player's car
        // We'll assume any object with a CarController is the player.
        if (!isCollected && other.GetComponentInParent<CarController>() != null)
        {
            Collect();
        }
    }

    private void Collect()
    {
        isCollected = true;

        // Add coins via the GameManager
        GameManager.Instance.AddCoins(value);

        // Optional: Add sound effect here
        // AudioManager.Instance.PlayCoinSound();
        // --- NEW: Also tell the session manager for the end-of-run stats ---
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.AddRunCoins(value);
        }
        // Start the visual "pop up and fade" animation
        StartCoroutine(AnimateAndDestroy());
    }

    private System.Collections.IEnumerator AnimateAndDestroy()
    {
        // Disable the collider so it can't be collected twice
        GetComponent<Collider2D>().enabled = false;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * 2f; // Move up 2 units
        float elapsedTime = 0f;

        while (elapsedTime < fadeOutTime)
        {
            // Move the coin up
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / fadeOutTime);

            // Fade the coin out
            Color c = spriteRenderer.color;
            c.a = 1.0f - (elapsedTime / fadeOutTime);
            spriteRenderer.color = c;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destroy the coin object when the animation is finished
        Destroy(gameObject);
    }
}