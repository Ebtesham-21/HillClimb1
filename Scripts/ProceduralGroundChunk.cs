using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGroundChunk : MonoBehaviour
{

    // These settings are controlled by the GroundStreamer
    [HideInInspector] public int segments;
    [HideInInspector] public float step;
    [HideInInspector] public float startXWorld;
    [HideInInspector] public float seed;

    [HideInInspector] public float worldVerticalOffset; // <-- ADD THIS LINE
    [HideInInspector] public GameObject coinPrefab;
    [HideInInspector] public float coinSpawnChance;
    [HideInInspector] public float coinHeightOffset;

    // --- NEW: Fuel Can Spawning Variables ---
    [HideInInspector] public GameObject fuelCanPrefab;
    [HideInInspector] public float fuelCanSpawnChance;
    [HideInInspector] public float fuelCanHeightOffset;



    // These settings come from the BiomeProfile
    [HideInInspector] public BiomeProfile biome;

    // These are general chunk settings
    [HideInInspector] public float bottomY = -10f;
    [HideInInspector] public float uvTilesX = 4f;
    [Tooltip("Controls the size of the tiled stone texture. Smaller value = larger texture.")]
    public float stoneUvScale = 5f;
    [SerializeField] private float offsetY = -1f;

    private MeshFilter roadMF, stoneMF;
    private MeshRenderer roadMR, stoneMR;
    // This is the new, corrected line
    [SerializeField] private PolygonCollider2D polyCollider;

    public float EndXWorld => startXWorld + segments * step;

    void Awake()
    {
        // Create child objects for meshes
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


        // New, improved code
        // Try to get the component that's already on the object
        polyCollider = GetComponent<PolygonCollider2D>();
        // If it doesn't exist (returns null), then add it.
        if (polyCollider == null)
        {
            polyCollider = gameObject.AddComponent<PolygonCollider2D>();
        }

        int vertexCount = segments + 1; 
    }

    // You can call this from an editor script or a manager to test generation
    // You can call this from the GroundStreamer to start the build process
    public IEnumerator BuildRoutine()
{
    // --- Shared variables ---
    int vertexCount = segments + 1;
    float roadHeight = 0.2f;

    // --- Data arrays ---
    Vector3[] roadVertices = new Vector3[vertexCount * 2];
    Vector2[] roadUVs = new Vector2[vertexCount * 2];
    int[] roadTris = new int[segments * 6];
    Vector3[] stoneVertices = new Vector3[vertexCount * 2];
    Vector2[] stoneUVs = new Vector2[vertexCount * 2];
    int[] stoneTris = new int[segments * 6];
    Vector2[] colliderPoints = new Vector2[vertexCount * 2];
    
     if (roadVertices == null || roadVertices.Length != vertexCount * 2)
        {
            roadVertices = new Vector3[vertexCount * 2];
            roadUVs = new Vector2[vertexCount * 2];
            stoneVertices = new Vector3[vertexCount * 2];
            stoneUVs = new Vector2[vertexCount * 2];
            colliderPoints = new Vector2[vertexCount * 2];
        }
        if (roadTris == null || roadTris.Length != segments * 6)
        {
            roadTris = new int[segments * 6];
            stoneTris = new int[segments * 6];
        }

    // --- Generate all vertex data AND SPAWN OBJECTS---
        for (int i = 0; i <= segments; i++)
        {
            float xLocal = i * step;
            float xWorld = startXWorld + xLocal;

            // Dynamic parameter calculation...
            float baseYNoise = Mathf.PerlinNoise((xWorld + seed) * biome.metaNoiseScale, 100f);
            float currentBaseY = Mathf.Lerp(biome.baseYRange.x, biome.baseYRange.y, baseYNoise);
            float amplitudeNoise = Mathf.PerlinNoise((xWorld + seed) * biome.metaNoiseScale, 200f);
            float currentAmplitude = Mathf.Lerp(biome.amplitudeRange.x, biome.amplitudeRange.y, amplitudeNoise);
            float scaleNoise = Mathf.PerlinNoise((xWorld + seed) * biome.metaNoiseScale, 300f);
            float currentNoiseScale = Mathf.Lerp(biome.noiseScaleRange.x, biome.noiseScaleRange.y, scaleNoise);
            float mainTerrainNoise = Mathf.PerlinNoise((xWorld + seed) * currentNoiseScale, 0f);
            float yTop = worldVerticalOffset + currentBaseY + mainTerrainNoise * currentAmplitude;

            // --- Object Spawning Logic ---
            // Check if we should spawn an object at this point.
            if (i > 0 && i < segments && i % 5 == 0) // We'll check every 5th segment
            {
                // First, try to spawn a fuel can (rarer)
                if (Random.value < fuelCanSpawnChance)
                {
                    Vector3 canPos = new Vector3(xLocal, yTop + fuelCanHeightOffset, 0);
                    // We use transform.TransformPoint to convert the local position to a world position for the new object
                    Instantiate(fuelCanPrefab, transform.TransformPoint(canPos), Quaternion.identity, transform);
                }
                // If we didn't spawn a fuel can, then try for a coin
                else if (Random.value < coinSpawnChance)
                {
                    Vector3 coinPos = new Vector3(xLocal, yTop + coinHeightOffset, 0);
                    Instantiate(coinPrefab, transform.TransformPoint(coinPos), Quaternion.identity, transform);
                }
            }

            // Vertex calculations...
            Vector3 roadTopVertex = new Vector3(xLocal, yTop, 0f);
            Vector3 roadBottomVertex = new Vector3(xLocal, yTop - roadHeight, 0f);
            roadVertices[i] = roadTopVertex;
            roadVertices[i + vertexCount] = roadBottomVertex;
            stoneVertices[i] = roadBottomVertex;
            stoneVertices[i + vertexCount] = new Vector3(xLocal, bottomY, 0f);

            // UVs...
            float u = (float)i / segments * uvTilesX;
            roadUVs[i] = new Vector2(u, 1f);
            roadUVs[i + vertexCount] = new Vector2(u, 0f);
            stoneUVs[i] = new Vector2(roadBottomVertex.x / stoneUvScale, roadBottomVertex.y / stoneUvScale);
            stoneUVs[i + vertexCount] = new Vector2(stoneVertices[i + vertexCount].x / stoneUvScale, stoneVertices[i + vertexCount].y / stoneUvScale);
        } // <-- THIS IS THE CORRECT PLACE FOR THE LOOP TO END

    // --- PAUSE EXECUTION ---
    yield return null;

    // --- Generate Triangles ---
    int t = 0;
    for (int i = 0; i < segments; i++)
    {
        int a = i; int b = i + vertexCount; int c = i + 1; int d = i + vertexCount + 1;
        // Corrected triangle winding order for 3D shaders
        roadTris[t] = a; roadTris[t + 1] = b; roadTris[t + 2] = c;
        roadTris[t + 3] = d; roadTris[t + 4] = b; roadTris[t + 5] = c;
        stoneTris[t] = a; stoneTris[t + 1] = b; stoneTris[t + 2] = c;
        stoneTris[t + 3] = d; stoneTris[t + 4] = b; stoneTris[t + 5] = c;
        t += 6;
    }

    // --- PAUSE EXECUTION AGAIN ---
    yield return null;

    // --- Build Meshes and Collider ---
    Mesh roadMesh = new Mesh();
    roadMesh.vertices = roadVertices;
    roadMesh.triangles = roadTris;
    roadMesh.uv = roadUVs;
    roadMesh.RecalculateNormals();
    roadMF.mesh = roadMesh;

    Mesh stoneMesh = new Mesh();
    stoneMesh.vertices = stoneVertices;
    stoneMesh.triangles = stoneTris;
    stoneMesh.uv = stoneUVs;
    stoneMesh.RecalculateNormals();
    stoneMF.mesh = stoneMesh;

    int pointIndex = 0;
    for (int i = 0; i < vertexCount; i++) { colliderPoints[pointIndex] = roadVertices[i]; pointIndex++; }
    for (int i = vertexCount - 1; i >= 0; i--) { colliderPoints[pointIndex] = roadVertices[i + vertexCount]; pointIndex++; }
    polyCollider.points = colliderPoints;

    roadMR.material = biome.roadMaterial;
    stoneMR.material = biome.stoneMaterial;

    transform.position = new Vector3(startXWorld, offsetY, transform.position.z);
    transform.localScale = Vector3.one;
}
}