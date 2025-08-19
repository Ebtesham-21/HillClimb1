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
            Debug.LogError
        }
    }


}
