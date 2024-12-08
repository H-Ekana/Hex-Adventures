using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class test : MonoBehaviour
{
    // Tilemap and Tile assets
    public Tilemap tilemap;
    
    public TileBase dirtTile;
    public TileBase dirtRandTree1;
    public TileBase dirtRandTree2;
    public TileBase dirtTileStone;
    public TileBase dirtForestTile1;
    public TileBase dirtForestTile2;
    public TileBase dirtForestStone;
    
    public TileBase stoneTile;
    public TileBase stoneTileTree1;
    public TileBase stoneTileTree2;
    
    public TileBase sandTile;  
    public TileBase desertTile;             
    public TileBase desertTileCactus1;     
    public TileBase desertTileCactus2;     
    public TileBase desertTileDirt;
    
    public TileBase grassTile; 
    public TileBase grassRandTree1;
    public TileBase grassRandTree2;
    
    public TileBase waterTileWave;
    public TileBase waterTile;
    public TileBase riverTile;           
    public TileBase riverBankTile; 
    
    public TileBase forestTile;          
    public TileBase forestTileStone1; 
    public TileBase forestTileStone2;  
    public TileBase forestTiledirt;
    public TileBase forestCampSite;
    
    public TileBase[] grassHouseTiles; 
    public TileBase[] dirtHouseTiles;  
    public TileBase[] stoneHouseTile;
    
    
    
    // Map size variables
    public int mapWidth = 100;
    public int mapHeight = 100;
    
    // Variables for Perlin Noise and FBM
    private float noiseScale;
    private int octaves;
    private float persistence;
    private float lacunarity;

    public int seed = 0;
    // Variables to store previous parameter values
    private float prevNoiseScale;
    private int prevOctaves;
    private float prevPersistence;
    private float prevLacunarity;
    
    // UI Sliders
    public Slider noiseScaleSlider;
    public Slider octavesSlider;
    public Slider persistenceSlider;
    public Slider lacunaritySlider;

    
    // River generation variables
    public int numberOfRivers = 100;     
    public int maxRiverLength = 1000;    
    public float minRiverHeight = 0.0f;  
    public int riverWidth = 2; 
    
    // Forest generation parameters
    public float forestNoiseScale = 0.5f;
    public float forestThreshold = 0.6f;
    
    // Desert generation parameters 
    public float desertNoiseScale = 1f;
    public float desertThreshold = 0.4f;
    private List<Vector2Int> townCenters = new List<Vector2Int>();
    // Town generation parameters
    public int numberOfTowns = 100;     
    public int townRadius = 10;
    
    // Arrays to store tile data and height values
    private TileBase[,] GeneratedTiles;
    private float[,] heightMap;          
    private float[,] forestMap;          
    private float[,] desertMap; 
    void Start()
    {    // Initialize previous values
        prevNoiseScale = noiseScaleSlider.value;
        prevOctaves = (int)octavesSlider.value;
        prevPersistence = persistenceSlider.value;
        prevLacunarity = lacunaritySlider.value;

        // Set initial values
        noiseScale = prevNoiseScale;
        octaves = prevOctaves;
        persistence = prevPersistence;
        lacunarity = prevLacunarity;
        
        // Setting random seed
        seed = Random.Range(0, 100000);

        // Initialize arrays
        GeneratedTiles = new TileBase[mapWidth, mapHeight]; 
        heightMap = new float[mapWidth, mapHeight];
        forestMap = new float[mapWidth, mapHeight];   
        desertMap = new float[mapWidth, mapHeight];
        
        // Generate map and rivers
        GenerateTileMap();
        GenerateRivers();
        SandNextToWater();
        GenerateTowns();

        // Refresh tilemap
        tilemap.RefreshAllTiles();
    }

    // Function to generate tile map
    private void GenerateTileMap()
    {   
        int xOffset = mapWidth / 2;
        int yOffset = mapHeight / 2;
        
        //Generating specific tilemap
        GenerateForestMap();
        GenerateDesertMap();
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                int offsetY = x % 2;

                Vector3Int tilePosition = new Vector3Int(x - xOffset, y - yOffset - offsetY, 0);

                // Get FBM value and store it in heightMap
                float FBMValue = FractalBrownianMotion(x + seed, y + seed, octaves, persistence, lacunarity);
                heightMap[x, y] = FBMValue;

                // Select and place tile
                TileBase selectedTile = TileSelector(FBMValue);
                tilemap.SetTile(tilePosition, selectedTile);
                GeneratedTiles[x, y] = selectedTile;

                if (selectedTile == waterTile || selectedTile == waterTileWave)
                {
                    float desertValue = desertMap[x, y];

                    if (desertValue > desertThreshold)
                    {       
                        float randomValue = Random.Range(0f, 1f);

                        if (randomValue < 0.85f)
                        {
                            tilemap.SetTile(tilePosition, desertTile);
                            GeneratedTiles[x, y] = desertTile;
                        }
                        else if (randomValue < 0.90f)
                        {
                            tilemap.SetTile(tilePosition, desertTileCactus1);
                            GeneratedTiles[x, y] = desertTileCactus1;
                        }
                        else if (randomValue < 0.95f)
                        {
                            tilemap.SetTile(tilePosition, desertTileCactus2);
                            GeneratedTiles[x, y] = desertTileCactus2;
                        }
                        else
                        {
                            tilemap.SetTile(tilePosition, desertTileDirt);
                            GeneratedTiles[x, y] = desertTileDirt;
                        }
                    }
                }
                
                // Place forest tile if conditions are met
                if (selectedTile == grassTile)
                {   
                    float randomValue1 = Random.Range(0f, 1f);
                    if (randomValue1 < 0.1f)
                    {
                        tilemap.SetTile(tilePosition, grassRandTree1);
                        GeneratedTiles[x, y] = grassRandTree1;
                    }
                    else if (randomValue1 < 0.2f)
                    {
                        tilemap.SetTile(tilePosition, grassRandTree2);
                        GeneratedTiles[x, y] = grassRandTree2;
                    }
                    
                    float forestValue = forestMap[x, y];

                    if (forestValue > forestThreshold)
                    {
                        float randomValue2 = Random.Range(0f, 1f);

                        if (randomValue2 < 0.85f)
                        {
                            tilemap.SetTile(tilePosition, forestTile);
                            GeneratedTiles[x, y] = forestTile;
                        }
                        else if (randomValue2 < 0.90f)
                        {
                            tilemap.SetTile(tilePosition, forestTileStone1);
                            GeneratedTiles[x, y] = forestTileStone1;
                        }
                        else if (randomValue2 < 0.94f)
                        {
                            tilemap.SetTile(tilePosition, forestTileStone2);
                            GeneratedTiles[x, y] = forestTileStone2;
                        }else if (randomValue2 < 0.95f)
                        {
                            tilemap.SetTile(tilePosition, forestCampSite);
                            GeneratedTiles[x, y] = forestCampSite;
                        }
                        else
                        {
                            tilemap.SetTile(tilePosition, forestTiledirt);
                            GeneratedTiles[x, y] = forestTiledirt;
                        }
                    }
                    
                    float desertValue = desertMap[x, y];

                    if (desertValue > desertThreshold)
                    {
                        float randomValue = Random.Range(0f, 1f);

                        if (randomValue < 0.85f)
                        {
                            tilemap.SetTile(tilePosition, desertTile);
                            GeneratedTiles[x, y] = desertTile;
                        }
                        else if (randomValue < 0.90f)
                        {
                            tilemap.SetTile(tilePosition, desertTileCactus1);
                            GeneratedTiles[x, y] = desertTileCactus1;
                        }
                        else if (randomValue < 0.95f)
                        {
                            tilemap.SetTile(tilePosition, desertTileCactus2);
                            GeneratedTiles[x, y] = desertTileCactus2;
                        }
                        else
                        {
                            tilemap.SetTile(tilePosition, desertTileDirt);
                            GeneratedTiles[x, y] = desertTileDirt;
                        }
                    }
                    
                }else if (selectedTile == dirtTile)
                {   
                    float randomValue1 = Random.Range(0f, 1f);
                    if (randomValue1 < 0.15f)
                    {
                        tilemap.SetTile(tilePosition, dirtRandTree1);
                        GeneratedTiles[x, y] = dirtRandTree1;
                    }
                    else if (randomValue1 < 0.3f)
                    {
                        tilemap.SetTile(tilePosition, dirtRandTree2);
                        GeneratedTiles[x, y] = dirtRandTree2;
                    }
                    
                    float forestValue = forestMap[x, y];

                    if (forestValue > forestThreshold)
                    {
                        float randomValue2 = Random.Range(0f, 1f);

                        if (randomValue2 < 0.85f)
                        {
                            tilemap.SetTile(tilePosition, dirtForestTile1);
                            GeneratedTiles[x, y] = dirtForestTile1;
                        }
                        else if (randomValue2 < 0.90f)
                        {
                            tilemap.SetTile(tilePosition, dirtForestTile2);
                            GeneratedTiles[x, y] = dirtForestTile2;
                        }
                        else if (randomValue2 < 0.95f)
                        {
                            tilemap.SetTile(tilePosition, dirtForestStone);
                            GeneratedTiles[x, y] = dirtForestStone;
                        }
                        else
                        {
                            tilemap.SetTile(tilePosition, dirtTileStone);
                            GeneratedTiles[x, y] = dirtTileStone;
                        }
                    }
                    
                    float desertValue = desertMap[x, y];

                    if (desertValue > desertThreshold)
                    {
                        float randomValue = Random.Range(0f, 1f);

                        if (randomValue < 0.85f)
                        {
                            tilemap.SetTile(tilePosition, desertTile);
                            GeneratedTiles[x, y] = desertTile;
                        }
                        else if (randomValue < 0.90f)
                        {
                            tilemap.SetTile(tilePosition, desertTileCactus1);
                            GeneratedTiles[x, y] = desertTileCactus1;
                        }
                        else if (randomValue < 0.95f)
                        {
                            tilemap.SetTile(tilePosition, desertTileCactus2);
                            GeneratedTiles[x, y] = desertTileCactus2;
                        }
                        else
                        {
                            tilemap.SetTile(tilePosition, desertTileDirt);
                            GeneratedTiles[x, y] = desertTileDirt;
                        }
                    }
                }
            }
        }
        
        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float height = heightMap[x, y];
                if (height > maxHeight) maxHeight = height;
                if (height < minHeight) minHeight = height;
            }
        }

    }

    // Function to generate FBM value
    float FractalBrownianMotion(float x, float y, int octaves, float persistence, float lacunarity)
    {
        float total = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float maxValue = 0f;  

        for (int i = 0; i < octaves; i++)
        {   
            float sampleX = (x + seed) * frequency * noiseScale;
            float sampleY = (y + seed) * frequency * noiseScale;

            total += Mathf.PerlinNoise(sampleX, sampleY) * amplitude;

            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxValue;  // Normalizing the value between 0 and 1
    }


    // Function to generate rivers
    void GenerateRivers()
    {
        int riversCreated = 0;
        int attempts = 0;
        int maxAttempts = numberOfRivers * 1000; 

        // Create a list to hold high elevation points
        List<(int x, int y, float elevation)> highElevationPoints = new List<(int, int, float)>();

        // Collect all high elevation points
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (heightMap[x, y] > 0.75f && desertMap[x , y] < desertThreshold)
                {
                    highElevationPoints.Add((x, y, heightMap[x, y]));
                }
            }
        }

        // Sort the list by elevation in descending order
        highElevationPoints.Sort((a, b) => b.elevation.CompareTo(a.elevation));

        // Iterate over the sorted high elevation points
        foreach (var point in highElevationPoints)
        {
            if (riversCreated >= numberOfRivers || attempts >= maxAttempts)
            {
                break;
            }

            attempts++;

            if (CreateRiver(point.x, point.y))
            {
                riversCreated++;
            }
        }

        Debug.Log("Rivers Created: " + riversCreated);
    }

    // Function to create a single river
    bool CreateRiver(int startX, int startY)
    {
        int x = startX;
        int y = startY;
        int riverLength = 0;

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        while (riverLength < maxRiverLength)
        {
            riverLength++;

            // Set river tiles with width and add to visited
            SetRiverTile(x, y, visited);

            // Check if river has reached sea level
            if (heightMap[x, y] < minRiverHeight)
            {
                return true; // River successfully created
            }

            // Find the neighbor with the lowest average height within river width
            Vector2Int nextPos = GetLowestNeighbor(x, y, visited);

            if (nextPos == new Vector2Int(x, y))
            {
                return false; // No valid path found
            }

            x = nextPos.x;
            y = nextPos.y;
        }

        return false; // Reached max river length without success
    }


    // Function to find the lowest neighboring tile
    Vector2Int GetLowestNeighbor(int x, int y, HashSet<Vector2Int> visited)
    {
        Vector2Int lowestPos = new Vector2Int(x, y);
        float lowestAvgHeight = AverageHeight(x, y);

        // Directions to check
        Vector2Int[] directions = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(1, 1),
            new Vector2Int(-1, -1)
        };

        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            if (IsInBounds(nx, ny))
            {
                // Check if the area is unvisited
                bool areaVisited = false;
                int halfWidth = riverWidth / 2;

                for (int dx = -halfWidth; dx <= halfWidth; dx++)
                {
                    for (int dy = -halfWidth; dy <= halfWidth; dy++)
                    {
                        int nnx = nx + dx;
                        int nny = ny + dy;
                        if (IsInBounds(nnx, nny))
                        {
                            if (visited.Contains(new Vector2Int(nnx, nny)))
                            {
                                areaVisited = true;
                                break;
                            }
                        }
                    }
                    if (areaVisited)
                        break;
                }

                if (!areaVisited)
                {
                    float neighborAvgHeight = AverageHeight(nx, ny);

                    if (neighborAvgHeight < lowestAvgHeight)
                    {
                        lowestAvgHeight = neighborAvgHeight;
                        lowestPos = new Vector2Int(nx, ny);
                    }
                }
            }
        }

        return lowestPos;
    }
    
    float AverageHeight(int x, int y)
    {
        int halfWidth = riverWidth / 2;
        float totalHeight = 0f;
        int count = 0;

        for (int dx = -halfWidth; dx <= halfWidth; dx++)
        {
            for (int dy = -halfWidth; dy <= halfWidth; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (IsInBounds(nx, ny))
                {
                    totalHeight += heightMap[nx, ny];
                    count++;
                }
            }
        }

        return totalHeight / count;
    }



    // Function to check if coordinates are within map bounds
    bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
    }

    // Function to set a river tile and riverbanks
    void SetRiverTile(int x, int y, HashSet<Vector2Int> visited)
    {
        int halfWidth = riverWidth / 2;

        for (int dx = -halfWidth; dx <= halfWidth; dx++)
        {
            for (int dy = -halfWidth; dy <= halfWidth; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (IsInBounds(nx, ny))
                {
                    Vector2Int pos = new Vector2Int(nx, ny);

                    if (!visited.Contains(pos))
                    {
                        visited.Add(pos);

                        GeneratedTiles[nx, ny] = riverTile;
                        Vector3Int tilePosition = new Vector3Int(nx - (mapWidth / 2), ny - (mapHeight / 2) - (nx % 2), 0);
                        tilemap.SetTile(tilePosition, riverTile);

                        // Optionally set river banks
                        SetRiverBanks(nx, ny);
                    }
                }
            }
        }
    }

    // Function to set riverbank tiles around the river
    void SetRiverBanks(int x, int y)
    {
        int halfWidth = riverWidth / 2 + 1;

        for (int dx = -halfWidth; dx <= halfWidth; dx++)
        {
            for (int dy = -halfWidth; dy <= halfWidth; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (IsInBounds(nx, ny))
                {
                    if (GeneratedTiles[nx, ny] != riverTile && GeneratedTiles[nx, ny] != waterTile && GeneratedTiles[nx, ny] != waterTileWave)
                    {
                        GeneratedTiles[nx, ny] = riverBankTile;
                        Vector3Int tilePosition = new Vector3Int(nx - (mapWidth / 2), ny - (mapHeight / 2) - (nx % 2), 0);
                        tilemap.SetTile(tilePosition, riverBankTile);
                    }
                }
            }
        }
    }


    // Special function to place sand tiles next to water
    private void SandNextToWater()
    {
        for (int x = 1; x < mapWidth - 1; x++) 
        {
            for (int y = 1; y < mapHeight - 1; y++)
            {
                if (GeneratedTiles[x, y] == waterTile || GeneratedTiles[x, y] == riverTile)
                {
                    // Checking neighbouring tiles
                    SetSandIfWaterAdjacent(x + 1, y);
                    SetSandIfWaterAdjacent(x - 1, y);
                    SetSandIfWaterAdjacent(x, y + 1);
                    SetSandIfWaterAdjacent(x, y - 1);
                    SetSandIfWaterAdjacent(x + 1, y - 1);
                    SetSandIfWaterAdjacent(x - 1, y + 1);
                }
            }
        }
    }

    // Function to replace shore tiles with sand
    void SetSandIfWaterAdjacent(int x, int y)
    {
        if (IsInBounds(x, y) && GeneratedTiles[x, y] != waterTileWave && GeneratedTiles[x, y] != waterTile && GeneratedTiles[x, y] != sandTile && GeneratedTiles[x, y] != riverTile)
        {   
            GeneratedTiles[x, y] = sandTile;
            Vector3Int tilePosition = new Vector3Int(x - (mapWidth / 2), y - (mapHeight / 2) - (x % 2), 0);
            tilemap.SetTile(tilePosition, sandTile);  
        }
    }

    // Tile selection logic based on the height map generated by FBM
    TileBase TileSelector(float FBMValue)
    {
        if (FBMValue < 0.3f)  
        {
            if (Random.Range(0f,1f) < 0.30f)
            {
                return waterTileWave;
            }
            else
            {
                return waterTile;
            }
        }
        else if (FBMValue < 0.4f)
        {
            return dirtTile;   
        }
        else if (FBMValue < 0.65f)
        {
            return grassTile;  
        }
        else
        {   float chance = Random.Range(0f, 1f);
            if (chance < 0.2f)
            {
                return stoneTileTree1;
            }
            else if (chance < 0.22f)
            {
                return stoneTileTree2;
            }
            else
            {
                return stoneTile;
            }
        }
    }
    bool ParametersChanged()
    {
        if (noiseScale != prevNoiseScale || octaves != prevOctaves ||
            persistence != prevPersistence || lacunarity != prevLacunarity)
        {
            // Update previous values
            prevNoiseScale = noiseScale;
            prevOctaves = octaves;
            prevPersistence = persistence;
            prevLacunarity = lacunarity;

            return true;
        }
        return false;
    }
    void RegenerateMap()
    {
        // Clear existing tiles
        tilemap.ClearAllTiles();

        // Re-initialize arrays
        GeneratedTiles = new TileBase[mapWidth, mapHeight];
        heightMap = new float[mapWidth, mapHeight];
        forestMap = new float[mapWidth, mapHeight];
        desertMap = new float[mapWidth, mapHeight];
        
        townCenters.Clear();
        
        // Regenerate the map
        GenerateTileMap();
        GenerateRivers();
        SandNextToWater();
        GenerateTowns();

        // Refresh tilemap
        tilemap.RefreshAllTiles();
        Debug.Log("Generated tiles have been changed");
    }
    
    void GenerateForestMap()
    {
        int forestSeed = Random.Range(0, 100000); 

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float nx = (float)x / mapWidth;
                float ny = (float)y / mapHeight;

                float forestValue = Mathf.PerlinNoise(
                    nx * forestNoiseScale + forestSeed,
                    ny * forestNoiseScale + forestSeed
                );

                forestMap[x, y] = forestValue;
            }
        }
    }
    
    void GenerateDesertMap()
    {
        int desertSeed = Random.Range(0, 100000); 

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float nx = (float)x / mapWidth;
                float ny = (float)y / mapHeight;

                float desertValue = Mathf.PerlinNoise(
                    nx * desertNoiseScale + desertSeed,
                    ny * desertNoiseScale + desertSeed
                );

                desertMap[x, y] = desertValue;
            }
        }
    }
    
    void GenerateTowns()
    {
        for (int i = 0; i < numberOfTowns; i++)
        {
            bool townPlaced = false;
            int attempts = 0;
            int maxAttempts = 1000;

            while (!townPlaced && attempts < maxAttempts)
            {
                attempts++;
                int x = Random.Range(0, mapWidth);
                int y = Random.Range(0, mapHeight);

                TileBase baseTile = GeneratedTiles[x, y];

                // Check if the tile is suitable
                if (baseTile == grassTile || baseTile == dirtTile || baseTile == stoneTile)
                {
                    // Check if there is enough space to place a town
                    if (IsAreaClearForTown(x, y))
                    {
                        // Place the town
                        PlaceTown(x, y);
                        townPlaced = true;
                    }
                }
            }
            if (!townPlaced)
            {
                Debug.LogWarning("Could not place town number " + (i + 1));
            }
        }
    }
    
    bool IsAreaClearForTown(int centerX, int centerY)
    {
        int radius = townRadius;

        // Check if the area overlaps with other towns
        foreach (Vector2Int townCenter in townCenters)
        {
            int dx = centerX - townCenter.x;
            int dy = centerY - townCenter.y;
            int minDistance = radius + townRadius + 10; 
            if (dx * dx + dy * dy < minDistance * minDistance)
            {
                return false;
            }
        }

        // Check for terrain
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (IsInBounds(x, y))
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        TileBase tile = GeneratedTiles[x, y];
                        if (tile == riverTile || tile == waterTile || tile == waterTileWave || tile == riverBankTile)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        return true;
    }

    
    void PlaceTown(int centerX, int centerY)
    {
        int radius = townRadius;
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (IsInBounds(x, y))
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        TileBase baseTile = GeneratedTiles[x, y];
                        TileBase houseTile = null;

                        if (baseTile == grassTile)
                        {
                            houseTile = grassHouseTiles[Random.Range(0, grassHouseTiles.Length)];
                        }
                        else if (baseTile == dirtTile)
                        {
                            houseTile = dirtHouseTiles[Random.Range(0, dirtHouseTiles.Length)];
                        }
                        else if (baseTile == stoneTile)
                        {
                            houseTile = stoneHouseTile[Random.Range(0, stoneHouseTile.Length)];
                        }

                        if (houseTile != null)
                        {
                            GeneratedTiles[x, y] = houseTile;
                            Vector3Int tilePosition =
                                new Vector3Int(x - (mapWidth / 2), y - (mapHeight / 2) - (x % 2), 0);
                            tilemap.SetTile(tilePosition, houseTile);
                        }
                    }
                }
            }
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        // Update variables based on slider values
        noiseScale = noiseScaleSlider.value;
        octaves = (int)octavesSlider.value;
        persistence = persistenceSlider.value;
        lacunarity = lacunaritySlider.value;

        // Regenerate the map when any parameter changes
        if (ParametersChanged())
        {
            RegenerateMap();
        }
    }
}