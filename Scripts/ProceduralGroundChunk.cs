using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ProceduralGroundChunk : MonoBehaviour
{

    [HideInInspector] public int segments = 60;
    [HideInInspector] public float step = 1f;
    [HideInInspector] public float noiseScale = 0.05f;
    [HideInInspector] public float amplitude = 6f;
    [HideInInspector] public float baseY = 0f;
    [HideInInspector] public float bottomY = -20f;
    [HideInInspector] public float seed = 1234f;
    [HideInInspector] public float startXWorld = 0f;
    [HideInInspector] public float uvTilesX = 4f;


    private Mesh mesh;
    private MeshFilter mf;
    private EdgeCollider2D edge;

    public float EndXWorld => startXWorld + segments * step;


    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        edge = GetComponent<EdgeCollider2D>();
        mesh = new Mesh { name = "GroundChunk" };
        mf.sharedMesh = mesh;
    }

    public void Build()
    {
        int topCount = segments + 1;
        int vertCount = topCount * 2;

        var vertices = new Vector3[vertCount];
        var uvs = new Vector2[vertCount];
        var tris = new int[segments * 6];
        var edgePoints = new Vector2[topCount];

        for (int i = 0; i <= segments; i++)
        {
            float xLocal = i * step;
            float xWorld = startXWorld + xLocal;

            float yTop = baseY + Mathf.PerlinNoise((xWorld + seed) * noiseScale, 0f) * amplitude;

            vertices[i] = new Vector3(xLocal, yTop, 0f);

            vertices[i + topCount] = new Vector3(xLocal, bottomY, 0f);

            float u = (float)i / segments * uvTilesX;
            uvs[i] = new Vector2(u, 1f);
            uvs[i + topCount] = new Vector2(u, 0f);

            edgePoints[i] = new Vector2(xLocal, yTop);

        }

        int t = 0;
        for (int i = 0; i < segments; i++)
        {
            int a = i;
            int b = i + topCount;
            int c = i + 1;
            int d = i + topCount + 1;

            tris[t++] = a; tris[t++] = b; tris[t++] = c;
            tris[t++] = c; tris[t++] = b; tris[t++] = d;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        edge.useAdjacentStartPoint = false;
        edge.useAdjacentEndPoint = false;
        edge.points = edgePoints;

        transform.position = new Vector3(startXWorld, 0f, transform.position.z);
        transform.localScale = Vector3.one;
    }

    
}
