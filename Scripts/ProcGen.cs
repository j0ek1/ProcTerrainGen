using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BiomeConfig // Class containing different biomes
{
    public BiomeGen biome;
    [Range(0f, 1f)] public float weighting = 1f;
}

[CreateAssetMenu(fileName = "ProcGen", menuName = "Procedural Generation/ProcGen")]
public class ProcGen : ScriptableObject
{
    public List<BiomeConfig> biomes;

    [Range(0f, 1f)] public float biomeSeedPointDensity = 0.1f;

    public int mapRes = 128;

    public int biomeCount => biomes.Count; // How many biomes there are

    public GameObject InitialHeightGen;
    public GameObject HeightPostProcessing;

    public GameObject PainterPostProcessing;

    public float totalWeighting
    {
        get // Get sum of weights to normalize later
        {
            float sum = 0f;
            foreach(var config in biomes)
            {
                sum += config.weighting;
            }
            return sum;
        }
    }
}
