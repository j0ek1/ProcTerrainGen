using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHeightRandom : TerrainHeightGen // Inherits from terrain height gen script
{
    [SerializeField] float heightChange;
    public override void Execute(int mapResolution, float[,] heightMap, Vector3 heightmapScale, int[,] biomeMap = null, int biomeIndex = -1, BiomeGen biome = null)
    {
        for (int y = 0; y < mapResolution; y++)
        {
            for (int x = 0; x < mapResolution; x++)
            {
                if (biomeIndex >= 0 && biomeMap[x, y] != biomeIndex) // Skip if this is not the correct biome
                {
                    continue;
                }
                float newHeight = heightMap[x, y] + (Random.Range(-heightChange, heightChange) / heightmapScale.y); // Calculate random height variation
                heightMap[x, y] = Mathf.Lerp(heightMap[x, y], newHeight, Strength); // Linearly interpolate based on strength and apply to terrain
            }
        }
    }
}
