using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHeightSmooth : TerrainHeightGen
{
    [SerializeField] int smoothingSize = 5;
    
    public override void Execute(int mapResolution, float[,] heightMap, Vector3 heightmapScale, int[,] biomeMap = null, int biomeIndex = -1, BiomeGen biome = null)
    {
        if (biomeMap != null) // Smoothing feature should not run per biome
        {
            return;
        }
        float[,] smoothHeights = new float[mapResolution, mapResolution]; // New float to store smoothed heights

        for (int y = 0; y < mapResolution; y++)
        {
            for (int x = 0; x < mapResolution; x++)
            {               
                float heightSum = 0f;
                int numValues = 0;

                // Get sums of neighbouring height values
                for (int yChange = -smoothingSize; yChange <= smoothingSize; yChange++)
                {
                    int yTemp = y + yChange;
                    if (yTemp < 0 || yTemp >= mapResolution) // If out of bounds, skip
                    {
                        continue;
                    }

                    for (int xChange = -smoothingSize; xChange <= smoothingSize; xChange++)
                    {
                        int xTemp = x + xChange;
                        if (xTemp < 0 || xTemp >= mapResolution) // If out of bounds, skip
                        {
                            continue;
                        }
                        heightSum += heightMap[xTemp, yTemp];
                        numValues++;
                    }
                    smoothHeights[x, y] = heightSum / numValues; // Store smoothed average height
                }
            }
        }

        for (int y = 0; y < mapResolution; y++)
        {
            for (int x = 0; x < mapResolution; x++)
            {
                heightMap[x, y] = Mathf.Lerp(heightMap[x, y], smoothHeights[x, y], Strength); // Linearly interpolate based on strength and apply to terrain
            }
        }
    }
}
