using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class tilemapGenerator : MonoBehaviour
{
    // Start is called before the first frame update

    public Tilemap tilemap;
    public TileBase[] tileAssets;

    public int mapWidth = 200;
    public int mapHeight = 200;
    void Start()
    {
        GenerateTileMap();
    }

    private void GenerateTileMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                
                int offsetX = y % 2; 
                
                TileBase randomTile = tileAssets[Random.Range(0, tileAssets.Length)];
                
                tilemap.SetTile(new Vector3Int(x - offsetX, y, 0), randomTile);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
