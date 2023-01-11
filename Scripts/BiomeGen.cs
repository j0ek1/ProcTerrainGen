using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BiomeTexture
{
    public string uniqueID;
    public Texture2D diffuse;
    public Texture2D normalMap;
}

[CreateAssetMenu(fileName = "BiomeGen", menuName = "Procedural Generation/BiomeGen")]
public class BiomeGen : ScriptableObject
{
    public string biomeName;

    [Range(0f, 1f)] public float minIntensity = 0.5f;
    [Range(0f, 1f)] public float maxIntensity = 1f;

    // The lower the decay rate, the larger each biome can spread
    [Range(0f, 1f)] public float minDecayRate = 0.05f;
    [Range(0f, 1f)] public float maxDecayRate = 0.1f;

    // If map resolution is 1000, biome has min decay rate (0.01) and max intensity (1) then biome could spread to 100 cells

    public GameObject heightGen; // To be able to add the height mod prefab to each biome

    public GameObject terrainPainter;

    public List<BiomeTexture> textures;
}