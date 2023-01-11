using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPainter : BasePainter
{
    [SerializeField] List<string> textureIDs;
    
    public override void Execute(ProcGenController controller, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,,] alphaMaps, int alphaMapRes, int[,] biomeMap = null, int biomeIndex = -1, BiomeGen biome = null)
    {
        for (int y = 0; y < alphaMapRes; y++)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)mapResolution / (float)alphaMapRes);
            for (int x = 0; x < alphaMapRes; x++)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)mapResolution / (float)alphaMapRes);
                if (biomeIndex >= 0 && biomeMap[heightMapX, heightMapY] != biomeIndex) // Skip if this is not the correct biome
                {
                    continue;
                }

                string randomTexture = textureIDs[Random.Range(0, textureIDs.Count)]; // Get random texture to allow for multiple textures in one biome

                alphaMaps[x, y, controller.GetLayerForTexture(randomTexture)] = Strength; // Apply texture
            }
        }
    }
}
