using UnityEngine;

public class PersistentSystems : MonoBehaviour
{
    // A static variable to ensure this bootstrapper itself is a singleton
    private static bool hasSpawned = false;

    private void Awake()
    {
        if (hasSpawned)
        {
            // If a persistent system already exists, destroy this duplicate.
            Destroy(gameObject);
            return;
        }

        // This is the first and only time this should run.
        // Make the PARENT object (and all its children managers) persistent.
        DontDestroyOnLoad(gameObject);
        hasSpawned = true;
    }
}