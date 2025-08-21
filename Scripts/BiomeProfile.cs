using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBiomeProfile", menuName = "Procedural/Biome Profile")]
public class BiomeProfile : ScriptableObject
{
    [Header("Biome Identification")]
    public string biomeName;

    [Header("Terrain Shape")]
    [Tooltip("How jagged/smooth the terrain is. Higher value = more jagged.")]
    public float noiseScale = 0.05f;
    [Tooltip("The maximum height variation of the hills")]
    public float amplitude = 6f;
    [Tooltip("The average height of the terrain")]
    public float baseY = 0f;

    [Header("Visuals")]
    [Tooltip("The material for the top layer of the ground")]
    public Material roadMaterial;
    [Tooltip("The material for the earth/stone beneath the road")]
    public Material stoneMaterial;


    // You can add more properties here later!
    // For example:
    // public Color skyColor;
    // public GameObject[] sceneryPrefabs;
    // public float scenerySpawnChance;
}
