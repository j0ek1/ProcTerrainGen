using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHeightGen : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] protected float Strength = 1f;

    public virtual void Execute(int mapResolution, float[,] heightMap, Vector3 heightmapScale, int[,] biomeMap = null, int biomeIndex = -1, BiomeGen biome = null)
    {
        Debug.LogError("no implementation of Execute func for " + gameObject.name);
    }
}
