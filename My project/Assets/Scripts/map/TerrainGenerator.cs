using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Transform player;
    public float tileSize = 20f;
    public int tileRange = 2;

    public GameObject[] buildingPrefabs;
    public GameObject[] parkPrefabs;

    public GameObject roadStraight;
    public GameObject roadCorner;
    public GameObject roadTJunction;
    public GameObject roadIntersection;

    public GameObject[] foliagePrefabs;

    private Dictionary<Vector2Int, bool> roadMap = new();
    private HashSet<Vector2Int> generatedTiles = new();

    public int roadSpacing = 4; 

    void Update()
    {
        GenerateTilesAroundPlayer();
    }

    void GenerateTilesAroundPlayer()
    {
        Vector2Int playerTile = WorldToTileCoords(player.position);
        List<Vector2Int> tilesToGenerate = new();

        // Phase 1: Plan which tiles to generate, and determine road intent
        for (int dx = -tileRange; dx <= tileRange; dx++)
        {
            for (int dz = -tileRange; dz <= tileRange; dz++)
            {
                Vector2Int tileCoord = playerTile + new Vector2Int(dx, dz);
                if (!generatedTiles.Contains(tileCoord))
                {
                    tilesToGenerate.Add(tileCoord);

                    if (!roadMap.ContainsKey(tileCoord))
                        roadMap[tileCoord] = ComputeRoadDesire(tileCoord.x, tileCoord.y);
                }
            }
        }

        // Phase 2: Generate terrain (buildings or parks)
        foreach (var tileCoord in tilesToGenerate)
        {
            GenerateTerrain(tileCoord);
        }

        // Phase 3: Generate roads based on full roadMap knowledge
        foreach (var tileCoord in tilesToGenerate)
        {
            if (roadMap[tileCoord])
                GenerateRoad(tileCoord);

            generatedTiles.Add(tileCoord);
        }
    }

    Vector2Int WorldToTileCoords(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / tileSize);
        int z = Mathf.FloorToInt(pos.z / tileSize);
        return new Vector2Int(x, z);
    }

    bool ComputeRoadDesire(int x, int z)
    {
        return x % roadSpacing == 0 || z % roadSpacing == 10;
    }

    void GenerateTerrain(Vector2Int tileCoord)
    {
        Vector3 position = new Vector3(tileCoord.x * tileSize, 0, tileCoord.y * tileSize);

       
        if (roadMap.ContainsKey(tileCoord) && roadMap[tileCoord])
            return;

        bool isBuilding = Random.value > 0.5f;
        GameObject[] pool = isBuilding ? buildingPrefabs : parkPrefabs;
        if (pool.Length == 0) return;

        GameObject prefab = pool[Random.Range(0, pool.Length)];
        GameObject instance = Instantiate(prefab, position, Quaternion.identity, transform);
        AddFoliage(instance.transform, position);
    }

    void GenerateRoad(Vector2Int tileCoord)
    {
        int x = tileCoord.x;
        int z = tileCoord.y;
        Vector3 position = new Vector3(x * tileSize, 0, z * tileSize);

        bool north = roadMap.ContainsKey(new Vector2Int(x, z + 1)) && roadMap[new Vector2Int(x, z + 1)];
        bool south = roadMap.ContainsKey(new Vector2Int(x, z - 1)) && roadMap[new Vector2Int(x, z - 1)];
        bool east = roadMap.ContainsKey(new Vector2Int(x + 1, z)) && roadMap[new Vector2Int(x + 1, z)];
        bool west = roadMap.ContainsKey(new Vector2Int(x - 1, z)) && roadMap[new Vector2Int(x - 1, z)];

        int count = (north ? 1 : 0) + (south ? 1 : 0) + (east ? 1 : 0) + (west ? 1 : 0);

        GameObject prefab = null;
        Quaternion rot = Quaternion.identity;

        if (count == 4)
        {
            prefab = roadIntersection;
        }
        else if (count == 3)
        {
            prefab = roadTJunction;
            if (!north) rot = Quaternion.Euler(0, 180, 0);
            else if (!east) rot = Quaternion.Euler(0, -90, 0);
            else if (!south) rot = Quaternion.identity;
            else if (!west) rot = Quaternion.Euler(0, 90, 0);
        }
        else if (count == 2)
        {
            if ((north && south) || (east && west))
            {
                prefab = roadStraight;
                rot = (north && south) ? Quaternion.identity : Quaternion.Euler(0, 90, 0);
            }
            else
            {
                prefab = roadCorner;
                if (north && east) rot = Quaternion.Euler(0, 0, 0);
                else if (east && south) rot = Quaternion.Euler(0, 90, 0);
                else if (south && west) rot = Quaternion.Euler(0, 180, 0);
                else if (west && north) rot = Quaternion.Euler(0, 270, 0);
            }
        }
        else if (count == 1)
        {
            prefab = roadStraight;
            if (north) rot = Quaternion.identity;
            else if (east) rot = Quaternion.Euler(0, 90, 0);
            else if (south) rot = Quaternion.Euler(0, 180, 0);
            else if (west) rot = Quaternion.Euler(0, 270, 0);
        }
        else
        {
            // No valid connection — skip
            return;
        }

        Instantiate(prefab, position, rot, transform);
    }

    void AddFoliage(Transform parent, Vector3 basePosition)
    {
        int count = Random.Range(2, 6);
        for (int i = 0; i < count; i++)
        {
            if (foliagePrefabs.Length == 0) return;

            GameObject foliage = foliagePrefabs[Random.Range(0, foliagePrefabs.Length)];
            Vector3 pos = basePosition + new Vector3(Random.Range(1f, tileSize - 1), 0, Random.Range(1f, tileSize - 1));
            Instantiate(foliage, pos, Quaternion.Euler(0, Random.Range(0, 360), 0), parent);
        }
    }
}
