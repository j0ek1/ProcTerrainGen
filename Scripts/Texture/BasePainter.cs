using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePainter : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] protected float Strength = 1f;

    public virtual void Execute(ProcGenController controller, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,,] alphaMaps, int alphaMapRes, int[,] biomeMap = null, int biomeIndex = -1, BiomeGen biome = null)
    {
        Debug.LogError("no implementation of Execute func for " + gameObject.name);
    }
}
