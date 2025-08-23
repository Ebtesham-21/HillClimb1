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
            }

    // You can call this from an editor script or a manager to test generation
    public void Build()
    {
        // --- Shared variables ---
        int vertexCount = segments + 1;
        float roadHeight = 0.2f;

        // --- Data arrays for Road Mesh ---
        Vector3[] roadVertices = new Vector3[vertexCount * 2];
        Vector2[] roadUVs = new Vector2[vertexCount * 2];
        int[] roadTris = new int[segments * 6];
        

        // --- Data arrays for Stone Mesh ---
        Vector3[] stoneVertices = new Vector3[vertexCount * 2];
        Vector2[] stoneUVs = new Vector2[vertexCount * 2];
        int[] stoneTris = new int[segments * 6];

        // --- Single loop to generate all vertex data ---
        for (int i = 0; i <= segments; i++)
        {
            float xLocal = i * step;
            float xWorld = startXWorld + xLocal;
            // This is the new, corrected line
            // --- DYNAMIC TERRAIN PARAMETER CALCULATION ---
            // We use different large offsets for each meta-noise sample to ensure they are unique patterns.

            // 1. Calculate the current Base Y using a low-frequency noise.
            float baseYNoise = Mathf.PerlinNoise((xWorld + seed) * biome.metaNoiseScale, 100f);
            float currentBaseY = Mathf.Lerp(biome.baseYRange.x, biome.baseYRange.y, baseYNoise);

            // 2. Calculate the current Amplitude.
            float amplitudeNoise = Mathf.PerlinNoise((xWorld + seed) * biome.metaNoiseScale, 200f);
            float currentAmplitude = Mathf.Lerp(biome.amplitudeRange.x, biome.amplitudeRange.y, amplitudeNoise);

            // 3. Calculate the current Noise Scale (frequency).
            float scaleNoise = Mathf.PerlinNoise((xWorld + seed) * biome.metaNoiseScale, 300f);
            float currentNoiseScale = Mathf.Lerp(biome.noiseScaleRange.x, biome.noiseScaleRange.y, scaleNoise);

            // 4. Finally, calculate the main terrain height using these dynamic, ever-changing parameters.
            float mainTerrainNoise = Mathf.PerlinNoise((xWorld + seed) * currentNoiseScale, 0f);
            float yTop = worldVerticalOffset + currentBaseY + mainTerrainNoise * currentAmplitude;
            // --- Road Vertices ---
            Vector3 roadTopVertex = new Vector3(xLocal, yTop, 0f);
            Vector3 roadBottomVertex = new Vector3(xLocal, yTop - roadHeight, 0f);
            roadVertices[i] = roadTopVertex;
            roadVertices[i + vertexCount] = roadBottomVertex;

            // --- Stone Vertices ---
            // The stone's top vertex is the same as the road's bottom vertex
            stoneVertices[i] = roadBottomVertex;
            // The stone's bottom vertex is at a fixed y-position
            stoneVertices[i + vertexCount] = new Vector3(xLocal, bottomY, 0f);

            // --- Road UVs ---
            float u = (float)i / segments * uvTilesX;
            roadUVs[i] = new Vector2(u, 1f);
            roadUVs[i + vertexCount] = new Vector2(u, 0f);

            // --- Stone UVs (for Tiling Effect) ---
            // We use the local position of the vertices to create a tiling effect
            stoneUVs[i] = new Vector2(roadBottomVertex.x / stoneUvScale, roadBottomVertex.y / stoneUvScale);
            stoneUVs[i + vertexCount] = new Vector2(stoneVertices[i + vertexCount].x / stoneUvScale, stoneVertices[i + vertexCount].y / stoneUvScale);

            
        }

        // --- Generate Triangles for Both Meshes ---
        int t = 0;
        for (int i = 0; i < segments; i++)
        {
            int a = i;
            int b = i + vertexCount;
            int c = i + 1;
            int d = i + vertexCount + 1;

            // Triangle 1
            roadTris[t] = a;
            roadTris[t + 1] = b;
            roadTris[t + 2] = c;
            // Triangle 2
            roadTris[t + 3] = c;
            roadTris[t + 4] = b;
            roadTris[t + 5] = d;
            
            // The triangle structure is identical for the stone mesh
            stoneTris[t] = a;
            stoneTris[t + 1] = b;
            stoneTris[t + 2] = c;
            stoneTris[t + 3] = c;
            stoneTris[t + 4] = b;
            stoneTris[t + 5] = d;
            
            t += 6;
        }
        
        // --- Build Polygon Collider for the Road ---
// The polygon needs points for the top surface and the bottom surface to form a closed shape.
Vector2[] colliderPoints = new Vector2[vertexCount * 2];
int pointIndex = 0;

// First, trace the top of the road from left to right
for (int i = 0; i < vertexCount; i++)
{
    colliderPoints[pointIndex] = roadVertices[i];
    pointIndex++;
}

// Second, trace the bottom of the road from RIGHT to LEFT to complete the loop
for (int i = vertexCount - 1; i >= 0; i--)
{
    colliderPoints[pointIndex] = roadVertices[i + vertexCount];
    pointIndex++;
}

// Assign the completed shape to the PolygonCollider2D
polyCollider.points = colliderPoints;

       // --- Assign Materials from Biome ---
        roadMR.material = biome.roadMaterial;
stoneMR.material = biome.stoneMaterial;

// --- Build Road Mesh ---
Mesh roadMesh = new Mesh();
// ... (rest of the method is the same)
        roadMesh.vertices = roadVertices;
        roadMesh.triangles = roadTris;
        roadMesh.uv = roadUVs;
        roadMesh.RecalculateNormals();
        roadMF.mesh = roadMesh;
        
        // --- Build Stone Mesh ---
        Mesh stoneMesh = new Mesh();
        stoneMesh.vertices = stoneVertices;
        stoneMesh.triangles = stoneTris;
        stoneMesh.uv = stoneUVs;
        stoneMesh.RecalculateNormals();
        stoneMF.mesh = stoneMesh;

      

        // --- Position the Chunk ---
        transform.position = new Vector3(startXWorld, offsetY, transform.position.z);
        transform.localScale = Vector3.one;
    }
}