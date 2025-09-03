using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGroundChunk : MonoBehaviour
{
    // --- Public properties set by GroundStreamer ---
    [HideInInspector] public int segments;
    [HideInInspector] public float step;
    [HideInInspector] public float startXWorld;
    [HideInInspector] public float seed;
    [HideInInspector] public float worldVerticalOffset;
    [HideInInspector] public GameObject coinPrefab;
    [HideInInspector] public float coinSpawnChance;
    [HideInInspector] public float coinHeightOffset;
    [HideInInspector] public GameObject fuelCanPrefab;
    [HideInInspector] public float fuelCanSpawnChance;
    [HideInInspector] public float fuelCanHeightOffset;
    [HideInInspector] public BiomeProfile biome;
    [HideInInspector] public float bottomY = -10f;
    [HideInInspector] public float uvTilesX = 4f;
    
    [Tooltip("Controls the size of the tiled stone texture. Smaller value = larger texture.")]
    public float stoneUvScale = 5f;
    [SerializeField] private float offsetY = -1f;

    // --- Private component references ---
    private MeshFilter roadMF, stoneMF;
    private MeshRenderer roadMR, stoneMR;
    [SerializeField] private PolygonCollider2D polyCollider;

    // --- Pooled Arrays (declared at the class level for reuse) ---
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
        // Get all necessary components
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

    // GroundStreamer will call this AFTER setting 'segments'.
    public void Initialize()
    {
        Debug.Log($"Chunk Initialize START for {gameObject.name}. Segments: {segments}");
        int vertexCount = segments + 1;
        if (segments <= 0)
        {
            // --- DEBUG LOG 2 ---
            Debug.LogError($"CRITICAL ERROR: Segments is {segments}! Cannot create arrays. Check GroundStreamer.", this.gameObject);
            return;
        }
        roadVertices = new Vector3[vertexCount * 2];
        roadUVs = new Vector2[vertexCount * 2];
        roadTris = new int[segments * 6];
        stoneVertices = new Vector3[vertexCount * 2];
        stoneUVs = new Vector2[vertexCount * 2];
        stoneTris = new int[segments * 6];
        colliderPoints = new Vector2[vertexCount * 2];
         Debug.Log($"Chunk Initialize COMPLETE for {gameObject.name}. roadVertices array size: {roadVertices.Length}");
    }

        public IEnumerator BuildRoutine()
    {
        // --- DEBUG LOG 4 ---
        Debug.Log($"BuildRoutine START for {gameObject.name}. Segments: {segments}, Step: {step}, StartX: {startXWorld}");

        if (roadVertices == null || roadTris == null)
        {
            // --- DEBUG LOG 5 ---
            Debug.LogError($"CRITICAL ERROR: Data arrays are NULL in BuildRoutine for {gameObject.name}. The Initialize() method was likely not called or failed. Aborting build for this chunk.", this.gameObject);
            yield break; // Stop this coroutine immediately.
        }
        
        if (biome == null)
        {
            Debug.LogError($"CRITICAL ERROR: Biome is NULL for {gameObject.name}. Check the GroundStreamer.", this.gameObject);
            yield break;
        }

        // --- Shared variables ---
        int vertexCount = segments + 1;
        float roadHeight = 0.2f;

        // --- Generate all vertex data ---
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

            // Object Spawning Logic...
            if (i > 0 && i < segments && i % 5 == 0)
            {
                if (Random.value < fuelCanSpawnChance)
                {
                    // Calculate the final world position directly, without TransformPoint
                    Vector3 worldPos = new Vector3(startXWorld + xLocal, yTop + fuelCanHeightOffset, 0);
                    ObjectPooler.Instance.SpawnFromPool("FuelCan", worldPos, Quaternion.identity);
                }
                else if (Random.value < coinSpawnChance)
                {
                    // Calculate the final world position directly here as well
                    Vector3 worldPos = new Vector3(startXWorld + xLocal, yTop + coinHeightOffset, 0);
                    ObjectPooler.Instance.SpawnFromPool("Coin", worldPos, Quaternion.identity);
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
        }
        
        // --- DEBUG LOG 5.5 ---
        Debug.Log($"Vertex generation loop COMPLETE for {gameObject.name}. First vertex Y position: {roadVertices[0].y}");

        // --- PAUSE EXECUTION ---
        yield return null;

        // --- Generate Triangles ---
        int t = 0;
        for (int i = 0; i < segments; i++)
        {
            int a = i; int b = i + vertexCount; int c = i + 1; int d = i + vertexCount + 1;
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
        roadMesh.name = "RoadProceduralMesh"; // Give the mesh a name for debugging
        roadMesh.vertices = roadVertices;
        roadMesh.triangles = roadTris;
        roadMesh.uv = roadUVs;
        roadMesh.RecalculateNormals();
        roadMF.mesh = roadMesh;

        Mesh stoneMesh = new Mesh();
        stoneMesh.name = "StoneProceduralMesh";
        stoneMesh.vertices = stoneVertices;
        stoneMesh.triangles = stoneTris;
        stoneMesh.uv = stoneUVs;
        stoneMesh.RecalculateNormals();
        stoneMF.mesh = stoneMesh;

        int pointIndex = 0;
        for (int i = 0; i < vertexCount; i++) { colliderPoints[pointIndex] = roadVertices[i]; pointIndex++; }
        for (int i = vertexCount - 1; i >= 0; i--) { colliderPoints[pointIndex] = roadVertices[i + vertexCount]; pointIndex++; }
        polyCollider.points = colliderPoints;

        // --- DEBUG LOG 6 ---
        if (biome.roadMaterial != null)
        {
             roadMR.material = biome.roadMaterial;
             Debug.Log($"Assigned Road Material '{biome.roadMaterial.name}' to {gameObject.name}");
        } else {
            Debug.LogError($"CRITICAL ERROR: Road Material is NULL in biome '{biome.name}'!", this.gameObject);
        }

        if (biome.stoneMaterial != null)
        {
            stoneMR.material = biome.stoneMaterial;
        } else {
            Debug.LogError($"CRITICAL ERROR: Stone Material is NULL in biome '{biome.name}'!", this.gameObject);
        }

        transform.position = new Vector3(startXWorld, offsetY, transform.position.z);
        transform.localScale = Vector3.one;
        
        // --- DEBUG LOG 7 ---
        Debug.Log($"BuildRoutine COMPLETE for {gameObject.name}. Mesh assigned with {roadMesh.vertexCount} vertices. Final position set to {transform.position}");
    }
}