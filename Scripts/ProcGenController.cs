using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
#endif

public class ProcGenController : MonoBehaviour
{
    [SerializeField] ProcGen procGen;
    [SerializeField] Terrain targetTerrain;

    Dictionary<string, int> biomeTextureToTerrainLayer = new Dictionary<string, int>();

#if UNITY_EDITOR
    int[,] biomeMap_LowRes;
    float[,] biomeStrength_LowRes;

    int[,] biomeMap;
    float[,] biomeStrength;
#endif

    void Start()
    {

    }

    void Update()
    {
        
    }

#if UNITY_EDITOR // Only run this if we are in the editor
    public void RegenerateTextures()
    {
        LayerSetup();
    }

    public void RegenerateTerrain()
    {
        int mapResolution = targetTerrain.terrainData.heightmapResolution; // Store terrain height map resolution
        int alphaMapRes = targetTerrain.terrainData.alphamapResolution; // Store terrain height map resolution

        GenerateTextureMapping();

        BiomeGeneration_LowRes(procGen.mapRes); // Generate low resolution biome map
        BiomeGeneration_HighRes(procGen.mapRes, mapResolution); // Upscale low res biome map to a high resolution biome map
        TerrainHeightGeneration(mapResolution);

        // Paint the terrain
        TerrainPainting(mapResolution, alphaMapRes);
    }

    void GenerateTextureMapping()
    {
        biomeTextureToTerrainLayer.Clear();

        // Get all biomes
        int layerIndex = 0;
        foreach (var biomeData in procGen.biomes)
        {
            var biome = biomeData.biome;

            // Get all textures
            foreach (var biomeTexture in biome.textures)
            {
                // Add to the layer map
                biomeTextureToTerrainLayer[biomeTexture.uniqueID] = layerIndex;
                ++layerIndex;
            }
        }
    }

    void LayerSetup()
    {
        // Delete any existing layers
        if (targetTerrain.terrainData.terrainLayers != null || targetTerrain.terrainData.terrainLayers.Length > 0) // If there are layers
        {
            // Fill list with asset paths for each layer
            List<string> layersToDelete = new List<string>();
            foreach (var layer in targetTerrain.terrainData.terrainLayers)
            {
                if (layer == null)
                {
                    continue;
                }
                layersToDelete.Add(AssetDatabase.GetAssetPath(layer.GetInstanceID()));
            }
            // Remove all links to layers
            targetTerrain.terrainData.terrainLayers = null;

            foreach (var layerFile in layersToDelete) // Delete each layer
            {
                if (string.IsNullOrEmpty(layerFile))
                {
                    continue;
                }
                AssetDatabase.DeleteAsset(layerFile);
            }
        }

        string scenePath = System.IO.Path.GetDirectoryName(SceneManager.GetActiveScene().path); // Get scene path

        // Get all biomes
        List<TerrainLayer> newLayers = new List<TerrainLayer>();
        foreach (var biomeData in procGen.biomes)
        {
            var biome = biomeData.biome;

            // Get all textures
            foreach (var biomeTexture in biome.textures)
            {
                // Create the layer
                TerrainLayer textureLayer = new TerrainLayer();
                textureLayer.diffuseTexture = biomeTexture.diffuse;
                textureLayer.normalMapTexture = biomeTexture.normalMap;

                // Save layer as an asset
                string layerPath = System.IO.Path.Combine(scenePath, "Layer_" + biome.name + "_" + biomeTexture.uniqueID);
                AssetDatabase.CreateAsset(textureLayer, layerPath);

                // Add to the layer map
                biomeTextureToTerrainLayer[biomeTexture.uniqueID] = newLayers.Count;
                newLayers.Add(textureLayer);
            }
        }
        targetTerrain.terrainData.terrainLayers = newLayers.ToArray(); // Apply the layers
    }

    void BiomeGeneration_LowRes(int mapResolution)
    {
        // Assign biome map and strength
        biomeMap_LowRes = new int[mapResolution, mapResolution];
        biomeStrength_LowRes = new float[mapResolution, mapResolution];

        // List of space for seed points
        int numSeedPoints = Mathf.FloorToInt(mapResolution * mapResolution * procGen.biomeSeedPointDensity);
        List<int> biomesToSpawn = new List<int>(numSeedPoints); // Capacity of list equals number of biome seed points

        // Normalize total weighting and calculate how many seed points will be spawned for each biome depending on weight
        float totalWeighting = procGen.totalWeighting;
        for(int biomeIndex = 0; biomeIndex < procGen.biomeCount; biomeIndex++)
        {
            int numIndices = Mathf.RoundToInt(numSeedPoints * procGen.biomes[biomeIndex].weighting / totalWeighting);
            Debug.Log("will spawn " + numIndices + " seedpoints for " + procGen.biomes[biomeIndex].biome.name); // Print how many seed points generated for each biome
            for (int currentIndex = 0; currentIndex < numIndices; currentIndex++) // Add spawn locations for this biome to list
            {
                biomesToSpawn.Add((int)biomeIndex);
            }
        }

        // Start spawning points for ooze-based biome generation
        while(biomesToSpawn.Count > 0)
        {
            int seedPointIndex = Random.Range(0, biomesToSpawn.Count); // Pick random seed point // Weighting allows us to have control over how large we want different biomes to spawn whilst still being random
            int biomeIndex = biomesToSpawn[seedPointIndex];
            biomesToSpawn.RemoveAt(seedPointIndex); // Remove seed point used
            IndividualBiomeSpawn(biomeIndex, mapResolution);
        }

        Texture2D textureMap = new Texture2D(mapResolution, mapResolution, TextureFormat.RGB24, false); // Initialize texture map
        for (int y = 0; y < mapResolution; y++) // Loop for every pixel of map
        {
            for (int x = 0; x < mapResolution; x++)
            {
                float hue = ((float)biomeMap_LowRes[x, y] / (float)procGen.biomeCount);
                textureMap.SetPixel(x, y, Color.HSVToRGB(hue, 0.75f, 0.75f)); // Change color later //////////////
            }
        }
        textureMap.Apply();
        System.IO.File.WriteAllBytes("BiomeMapLowRes.png", textureMap.EncodeToPNG()); // Export biome map as png
    }

    Vector2Int[] neighbouringPoints = new Vector2Int[] // Array of vectors of neighbouring points on the map (8 options)
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
    };

    void IndividualBiomeSpawn(int biomeIndex, int mapResolution) // Ooze based generation
    {
        BiomeGen biomeGen = procGen.biomes[biomeIndex].biome; // Store biome configuration
        Vector2Int spawnLocation = new Vector2Int(Random.Range(0, mapResolution), Random.Range(0, mapResolution)); // Choose random biome spawn location
        float startingIntensity = Random.Range(biomeGen.minIntensity, biomeGen.maxIntensity); // Choose random starting intensity

        Queue<Vector2Int> tempList = new Queue<Vector2Int>(); // Create queue (FIFO) which will hold spawn locations while the algorithm is working
        tempList.Enqueue(spawnLocation);

        bool[,] visited = new bool[mapResolution, mapResolution]; // Array of visited locations

        float[,] targetIntensity = new float[mapResolution, mapResolution];
        targetIntensity[spawnLocation.x, spawnLocation.y] = startingIntensity; // Assign starting intensity

        while (tempList.Count > 0) // Ooze method algorithm contained in a while loop
        {
            Vector2Int tempLocation = tempList.Dequeue(); // Grab location from queue
            biomeMap_LowRes[tempLocation.x, tempLocation.y] = biomeIndex; // Set biome spawn point
            visited[tempLocation.x, tempLocation.y] = true; // Mark point as visited
            biomeStrength_LowRes[tempLocation.x, tempLocation.y] = targetIntensity[tempLocation.x, tempLocation.y]; // Set biome strength based on intensity values

            for (int neighbourIndex = 0; neighbourIndex < neighbouringPoints.Length; neighbourIndex++)
            {
                Vector2Int neighbourLocation = tempLocation + neighbouringPoints[neighbourIndex];
                if (neighbourLocation.x < 0 || neighbourLocation.y < 0 || neighbourLocation.x >= mapResolution || neighbourLocation.y >= mapResolution)
                {
                    continue; // If neighbour is out of map bounds skip
                }

                if (visited[neighbourLocation.x, neighbourLocation.y])
                {
                    continue; // If neighbour is already visited skip
                }

                visited[neighbourLocation.x, neighbourLocation.y] = true; // Note location as visited

                // To avoid "blocky" biome generation, neighbouring points in the corners are scaled up to decay faster creating more naturally looking biomes (magnitude for diagonal neighbours is higher, thus higher decay rate)
                float decayAmount = Random.Range(biomeGen.minDecayRate, biomeGen.maxDecayRate) * neighbouringPoints[neighbourIndex].magnitude;
                float neighbourStrength = targetIntensity[tempLocation.x, tempLocation.y] - decayAmount; // Decrease intensity of neighbour by a random decay rate
                
                // Intensity of strengths for each location decays overtime as it passes on to the next neighbours
                targetIntensity[neighbourLocation.x, neighbourLocation.y] = neighbourStrength; // Store neighbour strength
                if (neighbourStrength <= 0) // Once the strength has decayed fully, end the generation process
                {
                    continue;
                }

                tempList.Enqueue(neighbourLocation); // Enqueue neighbour location for same process
            }
        }

    }

    void BiomeGeneration_HighRes(int mapSize_LowRes, int mapSize_HighRes) // Upscaling low res map
    {
        // Assign biome map and strength
        biomeMap = new int[mapSize_HighRes, mapSize_HighRes];
        biomeStrength = new float[mapSize_HighRes, mapSize_HighRes];

        float mapScale = (float)mapSize_LowRes / (float)mapSize_HighRes; // Calculate map scale

        //Calculate high res map - point based scaling
        for (int y = 0; y < mapSize_HighRes; y++)
        {
            int yLowRes = Mathf.FloorToInt(y * mapScale); // Floor value for consistency in map size
            for (int x = 0; x < mapSize_HighRes; x++)
            {
                int xLowRes = Mathf.FloorToInt(x * mapScale);

                biomeMap[x, y] = biomeMap_LowRes[xLowRes, yLowRes]; // Assign new high res values to biome map
            }
        }

        Texture2D textureMap = new Texture2D(mapSize_HighRes, mapSize_HighRes, TextureFormat.RGB24, false); // Initialize texture map
        for (int y = 0; y < mapSize_HighRes; y++) // Loop for every pixel of map
        {
            for (int x = 0; x < mapSize_HighRes; x++)
            {
                float hue = ((float)biomeMap[x, y] / (float)procGen.biomeCount);
                textureMap.SetPixel(x, y, Color.HSVToRGB(hue, 0.75f, 0.75f)); // Apply different colour per biome
            }
        }
        textureMap.Apply();
        System.IO.File.WriteAllBytes("BiomeMapHighRes.png", textureMap.EncodeToPNG()); // Export biome map as png
    }

    void TerrainHeightGeneration(int mapResolution)
    {
        float[,] heightMap = targetTerrain.terrainData.GetHeights(0, 0, mapResolution, mapResolution); // 2D array to store height map terrain data
        
        if (procGen.InitialHeightGen != null) // Execute initial height mods
        {
            TerrainHeightGen[] mods = procGen.InitialHeightGen.GetComponents<TerrainHeightGen>();
            foreach (var mod in mods)
            {
                mod.Execute(mapResolution, heightMap, targetTerrain.terrainData.heightmapScale);
            }
        }

        for (int biomeIndex = 0; biomeIndex < procGen.biomeCount; biomeIndex++) // Generate height map for each biome (biome specific modifications)
        {          
            var biome = procGen.biomes[biomeIndex].biome;
            if (biome.heightGen == null)
            {
                continue;
            }
            TerrainHeightGen[] mods = biome.heightGen.GetComponents<TerrainHeightGen>();
            foreach (var mod in mods) // Execute mods
            {
                mod.Execute(mapResolution, heightMap, targetTerrain.terrainData.heightmapScale, biomeMap, biomeIndex, biome);
            }
        }

        if (procGen.HeightPostProcessing != null) // Execute post processing height mods
        {
            TerrainHeightGen[] mods = procGen.HeightPostProcessing.GetComponents<TerrainHeightGen>();
            foreach (var mod in mods)
            {
                mod.Execute(mapResolution, heightMap, targetTerrain.terrainData.heightmapScale);
            }
        }

        targetTerrain.terrainData.SetHeights(0, 0, heightMap);
    }

    public int GetLayerForTexture(string uniqueID) // Return the layer to texture
    {
        return biomeTextureToTerrainLayer[uniqueID];
    }

    void TerrainPainting(int mapResolution, int alphaMapRes)
    {
        float[,] heightMap = targetTerrain.terrainData.GetHeights(0, 0, mapResolution, mapResolution); // 2D array to store height map terrain data
        float[,,] alphaMaps = targetTerrain.terrainData.GetAlphamaps(0, 0, alphaMapRes, alphaMapRes); // 3D array for alpha maps to control alpha of texture layers

        // Zero all layers (reset)
        for (int y = 0; y < alphaMapRes; y++)
        {
            for (int x = 0; x < alphaMapRes; x++)
            {
                for (int layerIndex = 0; layerIndex < targetTerrain.terrainData.alphamapLayers; layerIndex++)
                {
                    alphaMaps[x, y, layerIndex] = 0;
                }   
            }
        }

        // Run terrain painting for each biome
        for (int biomeIndex = 0; biomeIndex < procGen.biomeCount; biomeIndex++)
        {
            var biome = procGen.biomes[biomeIndex].biome;
            if (biome.terrainPainter == null)
            {
                continue;
            }
            BasePainter[] mods = biome.terrainPainter.GetComponents<BasePainter>();
            foreach (var mod in mods) // Execute mods for painting terrain
            {
                mod.Execute(this, mapResolution, heightMap, targetTerrain.terrainData.heightmapScale, alphaMaps, alphaMapRes, biomeMap, biomeIndex, biome);
            }
        }

        // Run texture post processing
        if (procGen.PainterPostProcessing != null)
        {
            BasePainter[] mods = procGen.PainterPostProcessing.GetComponents<BasePainter>();
            foreach (var mod in mods) // Execute mods for painting terrain
            {
                mod.Execute(this, mapResolution, heightMap, targetTerrain.terrainData.heightmapScale, alphaMaps, alphaMapRes);
            }
        }

        targetTerrain.terrainData.SetAlphamaps(0, 0, alphaMaps);
    }

#endif
}
