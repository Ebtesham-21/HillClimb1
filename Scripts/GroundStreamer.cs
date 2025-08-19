using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundStreamer : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public ProceduralGroundChunk chunkPrefab;

    [Header("Noise Settings")]
    public float seed = 1234f;
    public float noiseScale = 0.05f;
    public float amplitude = 6f;
    public float baseY = 0f;

    [Header("Chunk Settings")]
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

        // Start spawning around the player
        float startX = Mathf.Floor(player.position.x / ChunkWorldLength) * ChunkWorldLength - chunksBehind * ChunkWorldLength;
        nextSpawnX = startX;

        int total = chunksBehind + chunksAhead + 1;
        for (int i = 0; i < total; i++) SpawnNextChunk();
    }

    void Update()
    {
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

    void SpawnNextChunk()
    {
        var chunk = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);

        chunk.segments = segmentsPerChunk;
        chunk.step = step;
         chunk.noiseScale   = noiseScale;
        chunk.amplitude    = amplitude;
        chunk.baseY        = baseY;
        chunk.bottomY      = bottomY;
        chunk.seed         = seed;
        chunk.startXWorld  = nextSpawnX;
        chunk.uvTilesX     = uvTilesX;

        chunk.Build();

        active.AddLast(chunk);
        nextSpawnX += ChunkWorldLength;

    }


}
