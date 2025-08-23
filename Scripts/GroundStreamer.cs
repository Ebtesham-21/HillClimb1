using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundStreamer : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public ProceduralGroundChunk chunkPrefab;

    [Header("Biome Management")]
    [Tooltip("All the biomes that can be generated.")]
    public BiomeProfile[] availableBiomes;
    [Tooltip("The range in meters for how long a biome lasts before changing.")]
    public Vector2 biomeChangeDistanceRange = new Vector2(1000f, 2300f);

    private BiomeProfile currentBiome;
    private float nextBiomeChangeDistance;

[Header("Global Settings")]
public float seed = 1234f;

    [Header("Chunk Settings")]
    [Tooltip("The vertical offset for the entire generated world.")]
    public float worldVerticalOffset = 0f; // <-- ADD THIS LINE
    public int segmentsPerChunk = 60;
    public float step = 1f;
    public float bottomY = -20f;
    public float uvTilesX = 4f;
    public int chunksAhead = 4;
    public int chunksBehind = 2;


    private readonly LinkedList<ProceduralGroundChunk> active = new LinkedList<ProceduralGroundChunk>();
    private float nextSpawnX = 0f;
    private float ChunkWorldLength => segmentsPerChunk * step;

    void Start()
{
    if (!player || !chunkPrefab)
    {
        Debug.LogError("Assign player and chunk prefab on GroundStreamer");
        enabled = false;
        return;
    }

    if (availableBiomes.Length == 0)
    {
        Debug.LogError("No biomes assigned in the 'availableBiomes' array on GroundStreamer!");
        enabled = false;
        return;
    }

    // --- Biome Initialization ---
    // Start with a random biome
    currentBiome = availableBiomes[Random.Range(0, availableBiomes.Length)];
    // Set the first distance for a biome change
    nextBiomeChangeDistance = Random.Range(biomeChangeDistanceRange.x, biomeChangeDistanceRange.y);
    Debug.Log($"Starting with biome: {currentBiome.biomeName}. Next change at {nextBiomeChangeDistance}m.");


    // --- Original Spawning Logic ---
    float startX = Mathf.Floor(player.position.x / ChunkWorldLength) * ChunkWorldLength - chunksBehind * ChunkWorldLength;
    nextSpawnX = startX;

    int total = chunksBehind + chunksAhead + 1;
    for (int i = 0; i < total; i++) SpawnNextChunk();
}

    void Update()
    {
        CheckForBiomeChange();
        float needUntil = player.position.x + chunksAhead * ChunkWorldLength;
        while (active.Last == null || active.Last.Value.EndXWorld < needUntil)
            SpawnNextChunk();


        float cullBefore = player.position.x - chunksBehind * ChunkWorldLength - ChunkWorldLength * 0.5f;
        while (active.First != null && active.First.Value.EndXWorld < cullBefore)
        {
            var first = active.First.Value;
            active.RemoveFirst();
            Destroy(first.gameObject);

        }
    }


    void CheckForBiomeChange()
{
    // Check if the next chunk to be spawned is past the change threshold
    if (nextSpawnX >= nextBiomeChangeDistance)
    {
        SwitchToNewBiome();
        
        // Calculate the next change point from the current position
        float nextChange = Random.Range(biomeChangeDistanceRange.x, biomeChangeDistanceRange.y);
        nextBiomeChangeDistance += nextChange;
        
        Debug.Log($"Switching to biome: {currentBiome.biomeName}. Next change at {nextBiomeChangeDistance}m.");
    }
}

void SwitchToNewBiome()
{
    // This loop ensures we don't randomly pick the same biome again
    BiomeProfile newBiome;
    do
    {
        newBiome = availableBiomes[Random.Range(0, availableBiomes.Length)];
    } while (newBiome == currentBiome && availableBiomes.Length > 1); // Avoid infinite loop if only 1 biome exists

    currentBiome = newBiome;
}

    void SpawnNextChunk()
{
    var chunk = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);

    // --- Assign all properties to the new chunk ---

    // Assign the current biome profile. This contains materials and noise settings.
    chunk.biome = currentBiome;

    // Assign settings from the streamer
    chunk.segments = segmentsPerChunk;
    chunk.step = step;
    chunk.worldVerticalOffset = worldVerticalOffset; // <-- ADD THIS LINE
    chunk.bottomY = bottomY;
    chunk.seed = seed;
    chunk.startXWorld = nextSpawnX;
    chunk.uvTilesX = uvTilesX;

    // Now build the chunk using the assigned properties
    StartCoroutine(chunk.BuildRoutine());

    active.AddLast(chunk);
    nextSpawnX += ChunkWorldLength;
}


}
