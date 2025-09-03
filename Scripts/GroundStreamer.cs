using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundStreamer : MonoBehaviour
{
    [Header("References")]
    public Transform player; // This will be assigned by the PlayerSpawner
    public ProceduralGroundChunk chunkPrefab;

    [Header("Biome Management")]
    public BiomeProfile[] availableBiomes;
    public Vector2 biomeChangeDistanceRange = new Vector2(1000f, 2300f);

    [Header("Chunk Settings")]
    [Tooltip("The length of the initial flat area at the start of the game.")]
    public float startingFlatZoneLength = 50f; // <-- ADD THIS LINE
    public int segmentsPerChunk = 60;
    public float step = 1f;
    public int chunksAhead = 4;
    public int chunksBehind = 2;

    [Header("Object Spawning (Unified System)")]
    public List<SpawnableObject> spawnableObjects;
    [Range(0f, 1f)] public float masterSpawnChance = 0.4f;
    public int spawnInterval = 8;


    
    // Private state variables
    private readonly LinkedList<ProceduralGroundChunk> active = new LinkedList<ProceduralGroundChunk>();
    private float nextSpawnX = 0f;
    private float ChunkWorldLength => segmentsPerChunk * step;
    private BiomeProfile currentBiome;
    private float nextBiomeChangeDistance;
    
    // This is the one and only public method to start/restart the generation.
    // It is called by the PlayerSpawner AFTER the player has been created.
    public void InitializeAndGenerate()
    {
        if (player == null || chunkPrefab == null)
        {
            Debug.LogError("GroundStreamer cannot initialize: Player or ChunkPrefab is not assigned!", this.gameObject);
            this.enabled = false;
            return;
        }

        // 1. Clean up any old chunks from a previous run
        foreach (var chunk in active)
        {
            if (chunk != null) Destroy(chunk.gameObject);
        }
        active.Clear();

        // 2. Initialize state for a new run
        currentBiome = availableBiomes[Random.Range(0, availableBiomes.Length)];
        nextBiomeChangeDistance = Random.Range(biomeChangeDistanceRange.x, biomeChangeDistanceRange.y);
        float startX = Mathf.Floor(player.position.x / ChunkWorldLength) * ChunkWorldLength - chunksBehind * ChunkWorldLength;
        nextSpawnX = startX;

        // 3. Spawn the initial set of chunks
        int total = chunksAhead + chunksBehind + 1;
        for (int i = 0; i < total; i++)
        {
            SpawnNextChunk();
        }
    }

    void Update()
    {
        if (player == null || active.Count == 0) return; // Don't run if not initialized or no player

        CheckForBiomeChange();
        
        // Spawn new chunks ahead of the player
        float needUntil = player.position.x + chunksAhead * ChunkWorldLength;
        if (active.Last.Value.EndXWorld < needUntil)
        {
            SpawnNextChunk();
        }
        
        // Cull old chunks behind the player
        float cullBefore = player.position.x - chunksBehind * ChunkWorldLength - ChunkWorldLength * 0.5f;
        while (active.First != null && active.First.Value.EndXWorld < cullBefore)
        {
            var first = active.First.Value;
            active.RemoveFirst();
            Destroy(first.gameObject);
        }
    }
    
    void SpawnNextChunk()
    {
        var chunk = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);

        // Assign all properties
        chunk.biome = currentBiome;
        chunk.segments = segmentsPerChunk;
        chunk.step = step;
        chunk.startXWorld = nextSpawnX;
        chunk.startingFlatZoneLength = startingFlatZoneLength; 
        
        // Pass the unified spawning data
        chunk.spawnableObjects = spawnableObjects;
        chunk.masterSpawnChance = masterSpawnChance;
        chunk.spawnInterval = spawnInterval;

        chunk.Initialize();
        StartCoroutine(chunk.BuildRoutine());

        active.AddLast(chunk);
        nextSpawnX += ChunkWorldLength;
    }

    void CheckForBiomeChange()
    {
        if (nextSpawnX >= nextBiomeChangeDistance)
        {
            SwitchToNewBiome();
        }
    }

    void SwitchToNewBiome()
    {
        float nextChange = Random.Range(biomeChangeDistanceRange.x, biomeChangeDistanceRange.y);
        nextBiomeChangeDistance += nextChange;

        BiomeProfile newBiome;
        do {
            newBiome = availableBiomes[Random.Range(0, availableBiomes.Length)];
        } while (newBiome == currentBiome && availableBiomes.Length > 1);
        currentBiome = newBiome;
    }
}