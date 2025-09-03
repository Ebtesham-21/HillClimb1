using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGroundChunk : MonoBehaviour
{
    // --- Public properties set by GroundStreamer ---
    [HideInInspector] public int segments;
    [HideInInspector] public float step;
    [HideInInspector] public float startXWorld;
    [HideInInspector] public float startingFlatZoneLength; // <-- ADD THIS LINE
    [HideInInspector] public float seed;
    [HideInInspector] public BiomeProfile biome;
    [HideInInspector] public List<SpawnableObject> spawnableObjects;
    [HideInInspector] public float masterSpawnChance;
    [HideInInspector] public int spawnInterval;
    
    // --- Private component references ---
    private MeshFilter roadMF, stoneMF;
    private MeshRenderer roadMR, stoneMR;
    [SerializeField] private PolygonCollider2D polyCollider;
    private List<Vector3> potentialSpawnPoints = new List<Vector3>();

    // --- Pooled Arrays ---
    private Vector3[] roadVertices;
    private Vector2[] roadUVs;
    private int[] roadTris;
    private Vector3[] stoneVertices;
    private Vector2[] stoneUVs;
    private int[] stoneTris;
    private Vector2[] colliderPoints;

    public float EndXWorld => startXWorld + segments * step;

    void Awake()
    {
        // Get all necessary components for this chunk instance
        Transform roadGO = new GameObject("RoadMesh").transform;
        roadGO.parent = transform;
        roadGO.localPosition = Vector3.zero;
        roadMF = roadGO.gameObject.AddComponent<MeshFilter>();
        roadMR = roadGO.gameObject.AddComponent<MeshRenderer>();

        Transform stoneGO = new GameObject("StoneMesh").transform;
        stoneGO.parent = transform;
        stoneGO.localPosition = Vector3.zero;
        stoneMF = stoneGO.gameObject.AddComponent<MeshFilter>();
        stoneMR = stoneGO.gameObject.AddComponent<MeshRenderer>();

        polyCollider = GetComponent<PolygonCollider2D>();
        if (polyCollider == null)
        {
            polyCollider = gameObject.AddComponent<PolygonCollider2D>();
        }
    }

    // GroundStreamer calls this AFTER setting 'segments'.
    public void Initialize()
    {
        int vertexCount = segments + 1;
        if (segments <= 0) return; // Safety check

        roadVertices = new Vector3[vertexCount * 2];
        roadUVs = new Vector2[vertexCount * 2];
        roadTris = new int[segments * 6];
        stoneVertices = new Vector3[vertexCount * 2];
        stoneUVs = new Vector2[vertexCount * 2];
        stoneTris = new int[segments * 6];
        colliderPoints = new Vector2[vertexCount * 2];
    }

    public IEnumerator BuildRoutine()
    {
        potentialSpawnPoints.Clear();
        if (roadVertices == null) { yield break; }
        if (biome == null) { yield break; }

        int vertexCount = segments + 1;
        float roadHeight = 0.2f;

        // Step 1: Calculate vertex positions and identify potential spawn points
        for (int i = 0; i <= segments; i++)
        {
            float xLocal = i * step;
            float yTop = CalculateYTop(xLocal);

            if (i > 0 && i < segments && i % spawnInterval == 0)
            {
                potentialSpawnPoints.Add(new Vector3(xLocal, yTop, 0));
            }

            Vector3 roadTopVertex = new Vector3(xLocal, yTop, 0f);
            Vector3 roadBottomVertex = new Vector3(xLocal, yTop - roadHeight, 0f);
            roadVertices[i] = roadTopVertex;
            roadVertices[i + vertexCount] = roadBottomVertex;
            stoneVertices[i] = roadBottomVertex;
            stoneVertices[i + vertexCount] = new Vector3(xLocal, yTop - 30f, 0f); // Use yTop for depth
            
            // UVs
            float u = (float)i / segments * 4f; // Example UV tiling
            roadUVs[i] = new Vector2(u, 1f);
            roadUVs[i + vertexCount] = new Vector2(u, 0f);
            stoneUVs[i] = new Vector2(roadBottomVertex.x / 5f, roadBottomVertex.y / 5f);
            stoneUVs[i + vertexCount] = new Vector2(stoneVertices[i + vertexCount].x / 5f, stoneVertices[i + vertexCount].y / 5f);
        }

        yield return null; // Pause frame

        // Step 2: Generate Triangles
        int t = 0;
        for (int i = 0; i < segments; i++)
        {
            int a = i, b = i + vertexCount, c = i + 1, d = i + vertexCount + 1;
            roadTris[t] = a; roadTris[t + 1] = b; roadTris[t + 2] = c;
            roadTris[t + 3] = c; roadTris[t + 4] = b; roadTris[t + 5] = d;
            stoneTris[t] = a; stoneTris[t + 1] = b; stoneTris[t + 2] = c;
            stoneTris[t + 3] = c; stoneTris[t + 4] = b; stoneTris[t + 5] = d;
            t += 6;
        }

        yield return null; // Pause frame

        // Step 3: Build Meshes, set final chunk position
        Mesh roadMesh = new Mesh { name = "RoadProcMesh", vertices = roadVertices, triangles = roadTris, uv = roadUVs };
        roadMesh.RecalculateNormals();
        roadMF.mesh = roadMesh;

        Mesh stoneMesh = new Mesh { name = "StoneProcMesh", vertices = stoneVertices, triangles = stoneTris, uv = stoneUVs };
        stoneMesh.RecalculateNormals();
        stoneMF.mesh = stoneMesh;

        int pointIndex = 0;
        for (int i = 0; i < vertexCount; i++) { colliderPoints[pointIndex] = roadVertices[i]; pointIndex++; }
        for (int i = vertexCount - 1; i >= 0; i--) { colliderPoints[pointIndex] = roadVertices[i + vertexCount]; pointIndex++; }
        polyCollider.points = colliderPoints;
        
        roadMR.material = biome.roadMaterial;
        stoneMR.material = biome.stoneMaterial;

        transform.position = new Vector3(startXWorld, 0, 0);
        transform.localScale = Vector3.one;

        // Step 4: Now that the chunk is in its final position, spawn the objects
        SpawnObjects();
    }
    
    private float CalculateYTop(float xLocal)
    {
        float xWorld = startXWorld + xLocal;
        float baseYNoise = Mathf.PerlinNoise(xWorld * biome.metaNoiseScale, 100f);
        float currentBaseY = Mathf.Lerp(biome.baseYRange.x, biome.baseYRange.y, baseYNoise);
        float amplitudeNoise = Mathf.PerlinNoise(xWorld * biome.metaNoiseScale, 200f);
        float currentAmplitude = Mathf.Lerp(biome.amplitudeRange.x, biome.amplitudeRange.y, amplitudeNoise);

        // --- NEW: STARTING SAFE ZONE LOGIC ---
        // Check if the current world position is within the flat zone.
        if (xWorld < startingFlatZoneLength)
        {
            // If we are in the flat zone, we calculate a "fade-in" multiplier.
            // This multiplier will go smoothly from 0.0 (at the start) to 1.0 (at the end of the zone).
            float amplitudeMultiplier = Mathf.InverseLerp(0, startingFlatZoneLength, xWorld);
            
            // We apply this multiplier to the hill height.
            currentAmplitude *= amplitudeMultiplier;
        }
        // --- END OF NEW LOGIC ---
        float scaleNoise = Mathf.PerlinNoise(xWorld * biome.metaNoiseScale, 300f);
        float currentNoiseScale = Mathf.Lerp(biome.noiseScaleRange.x, biome.noiseScaleRange.y, scaleNoise);
        float mainTerrainNoise = Mathf.PerlinNoise(xWorld * currentNoiseScale, 0f);
        return currentBaseY + mainTerrainNoise * currentAmplitude;
    }
    
    private void SpawnObjects()
    {
        foreach (Vector3 localSpawnPoint in potentialSpawnPoints)
        {
            if (Random.value < masterSpawnChance)
            {
                SpawnRandomObjectFromList(localSpawnPoint);
            }
        }
    }

    private void SpawnRandomObjectFromList(Vector3 localPosition)
    {
        if (spawnableObjects == null || spawnableObjects.Count == 0) return;

        float totalWeight = 0;
        foreach (var obj in spawnableObjects) { totalWeight += obj.weight; }

        float randomValue = Random.Range(0, totalWeight);

        foreach (var obj in spawnableObjects)
        {
            if (randomValue <= obj.weight)
            {
                Vector3 worldPos = transform.TransformPoint(localPosition + new Vector3(0, obj.heightOffset, 0));
                ObjectPooler.Instance.SpawnFromPool(obj.prefab.tag, worldPos, Quaternion.identity);
                return;
            }
            randomValue -= obj.weight;
        }
    }
}