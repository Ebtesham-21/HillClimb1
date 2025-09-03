using UnityEngine;

[System.Serializable]
public class SpawnableObject
{
    public GameObject prefab;
    [Tooltip("Compared to other objects, a higher weight means this is more likely to spawn.")]
    public float weight;
    public float heightOffset = 1.0f;
}