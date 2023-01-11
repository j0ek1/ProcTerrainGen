using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothPainter : BasePainter
{
    [SerializeField] int smoothingSize = 5;

    public override void Execute(ProcGenController controller, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,,] alphaMaps, int alphaMapRes, int[,] biomeMap = null, int biomeIndex = -1, BiomeGen biome = null)
    {
        if (biomeMap != null) // Smoothing feature should not run per biome
        {
            return;
        }

        for (int layer = 0; layer < alphaMaps.GetLength(2); layer++)
        {
            float[,] smoothAlphaMap = new float[alphaMapRes, alphaMapRes]; // New float to store smoothed heights

            for (int y = 0; y < alphaMapRes; y++)
            {
                for (int x = 0; x < alphaMapRes; x++)
                {
                    float alphaSum = 0f;
                    int numValues = 0;

                    // Get sums of neighbouring height values
                    for (int yChange = -smoothingSize; yChange <= smoothingSize; yChange++)
                    {
                        int yTemp = y + yChange;
                        if (yTemp < 0 || yTemp >= alphaMapRes) // If out of bounds, skip
                        {
                            continue;
                        }

                        for (int xChange = -smoothingSize; xChange <= smoothingSize; xChange++)
                        {
                            int xTemp = x + xChange;
                            if (xTemp < 0 || xTemp >= alphaMapRes) // If out of bounds, skip
                            {
                                continue;
                            }
                            alphaSum += alphaMaps[xTemp, yTemp, layer];
                            numValues++;
                        }
                        smoothAlphaMap[x, y] = alphaSum / numValues; // Store smoothed alpha map
                    }
                }
            }

            for (int y = 0; y < alphaMapRes; y++)
            {
                for (int x = 0; x < alphaMapRes; x++)
                {
                    alphaMaps[x, y, layer] = Mathf.Lerp(alphaMaps[x, y, layer], smoothAlphaMap[x, y], Strength); // Linearly interpolate based on strength and apply to terrain
                }
            }
        }
        
    }
}
