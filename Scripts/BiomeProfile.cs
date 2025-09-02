using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "NewBiomeProfile", menuName = "Procedural/Biome Profile")]
public class BiomeProfile : ScriptableObject
{
    [Header("Biome Identification")]
    public string biomeName;

    [Header("Terrain Shape")]
    [Tooltip("The MIN and MAX average height of the terrain. Creates gradual slopes over the biome.")]
    public Vector2 baseYRange = new Vector2(0f, 2f);

    [Tooltip("The MIN and MAX height variation of the hills. Creates quiet flat areas and dramatic mountain areas.")]
    public Vector2 amplitudeRange = new Vector2(2f, 10f);

    [Tooltip("The MIN and MAX frequency of hills. Creates areas with tight, bumpy hills and areas with long, rolling hills.")]
    public Vector2 noiseScaleRange = new Vector2(0.04f, 0.1f);

    [Header("Parameter Variation")]
    [Tooltip("How gradually the parameters (amplitude, frequency) change. Smaller value = slower, longer transitions.")]
    public float metaNoiseScale = 0.001f;

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
