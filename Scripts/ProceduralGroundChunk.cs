using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGroundChunk : MonoBehaviour
{
    [HideInInspector] public int segments = 60;
    [HideInInspector] public float step = 1f;
    [HideInInspector] public float noiseScale = 0.05f;
    [HideInInspector] public float amplitude = 6f;
    [HideInInspector] public float baseY = 0f;
    [HideInInspector] public float bottomY = -3f;
    [HideInInspector] public float seed = 1234f;
    [HideInInspector] public float startXWorld = 0f;
    [HideInInspector] public float uvTilesX = 4f;

    [SerializeField] private float offsetY = -1f;

    // Materials
    public Material roadMaterial;
    public Material stoneMaterial;

    private MeshFilter roadMF, stoneMF;
    private MeshRenderer roadMR, stoneMR;
    private EdgeCollider2D edge;

    public float EndXWorld => startXWorld + segments * step;

    void Awake()
    {
        // Create child objects
        Transform roadGO = new GameObject("RoadMesh").transform;
        roadGO.parent = transform;
        roadGO.localPosition = Vector3.zero;

        roadMF = roadGO.gameObject.AddComponent<MeshFilter>();
        roadMR = roadGO.gameObject.AddComponent<MeshRenderer>();
        roadMR.material = roadMaterial;

        Transform stoneGO = new GameObject("StoneMesh").transform;
        stoneGO.parent = transform;
        stoneGO.localPosition = Vector3.zero;

        stoneMF = stoneGO.gameObject.AddComponent<MeshFilter>();
        stoneMR = stoneGO.gameObject.AddComponent<MeshRenderer>();
        stoneMR.material = stoneMaterial;

        // EdgeCollider for car to ride on road
        edge = gameObject.AddComponent<EdgeCollider2D>();
    }

    void Start()
    {
        Build();
    }

    public void Build()
    {
        int topCount = segments + 1;

        // --- Road Mesh ---
        Mesh roadMesh = new Mesh();
        Vector3[] roadVertices = new Vector3[topCount * 2];
        Vector2[] roadUVs = new Vector2[topCount * 2];
        int[] roadTris = new int[segments * 6];
        Vector2[] edgePoints = new Vector2[topCount];

        for (int i = 0; i <= segments; i++)
        {
            float xLocal = i * step;
            float xWorld = startXWorld + xLocal;
            float yTop = baseY + Mathf.PerlinNoise((xWorld + seed) * noiseScale, 0f) * amplitude;

            float roadHeight = 0.2f; // <--- very thin stroke
            roadVertices[i] = new Vector3(xLocal, yTop, 0f);
            roadVertices[i + topCount] = new Vector3(xLocal, yTop - roadHeight, 0f); // small thickness

            float u = (float)i / segments * uvTilesX;
            roadUVs[i] = new Vector2(u, 1f);
            roadUVs[i + topCount] = new Vector2(u, 0f);

            edgePoints[i] = new Vector2(xLocal, yTop);
        }

        int t = 0;
        for (int i = 0; i < segments; i++)
        {
            int a = i;
            int b = i + topCount;
            int c = i + 1;
            int d = i + topCount + 1;

            roadTris[t++] = a; roadTris[t++] = b; roadTris[t++] = c;
            roadTris[t++] = c; roadTris[t++] = b; roadTris[t++] = d;
        }

        roadMesh.vertices = roadVertices;
        roadMesh.triangles = roadTris;
        roadMesh.uv = roadUVs;
        roadMesh.RecalculateBounds();
        roadMesh.RecalculateNormals();

        roadMF.mesh = roadMesh;

        // --- Stone Mesh (flat rectangle under road) ---
        Mesh stoneMesh = new Mesh();
        Vector3[] stoneVertices = new Vector3[4]; // rectangle
        Vector2[] stoneUVs = new Vector2[4];
        int[] stoneTris = new int[6];

        float width = segments * step;
        float height = baseY - bottomY;

        stoneVertices[0] = new Vector3(0, bottomY, 0);
        stoneVertices[1] = new Vector3(width, bottomY, 0);
        stoneVertices[2] = new Vector3(0, baseY, 0);
        stoneVertices[3] = new Vector3(width, baseY, 0);

        stoneUVs[0] = new Vector2(0, 0);
        stoneUVs[1] = new Vector2(1, 0);
        stoneUVs[2] = new Vector2(0, 1);
        stoneUVs[3] = new Vector2(1, 1);

        stoneTris[0] = 0; stoneTris[1] = 2; stoneTris[2] = 1;
        stoneTris[3] = 1; stoneTris[4] = 2; stoneTris[5] = 3;

        stoneMesh.vertices = stoneVertices;
        stoneMesh.triangles = stoneTris;
        stoneMesh.uv = stoneUVs;
        stoneMesh.RecalculateBounds();
        stoneMesh.RecalculateNormals();

        stoneMF.mesh = stoneMesh;

        // --- Edge Collider ---
        edge.useAdjacentStartPoint = false;
        edge.useAdjacentEndPoint = false;
        edge.points = edgePoints;

        transform.position = new Vector3(startXWorld, offsetY, transform.position.z);
        transform.localScale = Vector3.one;
    }
}
