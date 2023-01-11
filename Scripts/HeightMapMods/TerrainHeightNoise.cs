using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHeightNoise : TerrainHeightGen // Inherits from terrain height gen script
{  
    [SerializeField] float xScale = 1f; // The smaller the scale values, the larger the features will be on the terrain
    [SerializeField] float yScale = 1f;
    [SerializeField] float heightChange = 0f;
    [SerializeField] int octaves = 1;

    [SerializeField] float lacunarity = 2f; // Variables to determine the variation of scale and height change over multiple runs
    [SerializeField] float heightChangeVariation = 0.5f;

    public override void Execute(int mapResolution, float[,] heightMap, Vector3 heightmapScale, int[,] biomeMap = null, int biomeIndex = -1, BiomeGen biome = null)
    {
        float xScaleTemp = xScale;
        float yScaleTemp = yScale;
        float heightChangeTemp = heightChange;

        for (int run = 0; run < octaves; run++) // Run perlin noise function a number of times
        {
            for (int y = 0; y < mapResolution; y++)
            {
                for (int x = 0; x < mapResolution; x++)
                {
                    if (biomeIndex >= 0 && biomeMap[x, y] != biomeIndex) // Skip if this is not the correct biome
                    {
                        continue;
                    }
                    float noise = (Mathf.PerlinNoise(x * xScaleTemp, y * yScaleTemp) * 2f) - 1f; // Adjust noise values so the range is -1 to 1 instead of 0 to 1
                    float newHeight = heightMap[x, y] + ((noise * heightChangeTemp) / heightmapScale.y); // Calculate new height through 2D perlin noise
                    heightMap[x, y] = Mathf.Lerp(heightMap[x, y], newHeight, Strength); // Linearly interpolate based on strength
                }
            }

            // After each run, update variables
            xScaleTemp *= lacunarity;
            yScaleTemp *= lacunarity;
            heightChangeTemp *= heightChangeVariation;
        }
    }
}
