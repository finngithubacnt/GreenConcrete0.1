using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Transform player;
    public float tileSize = 20f;
    public int tileRange = 2; // How many tiles around the player to generate

    public GameObject[] buildingPrefabs;
    public GameObject[] parkPrefabs;

    public GameObject roadStraight;
    public GameObject roadCorner;
    public GameObject roadTJunction;
    public GameObject roadIntersection;

    public GameObject[] foliagePrefabs;

    private Dictionary<Vector2Int, bool> roadMap = new();         // Tracks road presence
    private HashSet<Vector2Int> generatedTiles = new();           // Tracks generated tiles

    void Update()
    {
        GenerateTilesAroundPlayer();
    }

    void GenerateTilesAroundPlayer()
    {
        Vector2Int playerTile = WorldToTileCoords(player.position);

        for (int dx = -tileRange; dx <= tileRange; dx++)
        {
            for (int dz = -tileRange; dz <= tileRange; dz++)
            {
                Vector2Int tileCoord = playerTile + new Vector2Int(dx, dz);

                if (!generatedTiles.Contains(tileCoord))
                {
                    // STEP 1: Decide if this tile wants to be a road (global decision only once)
                    if (!roadMap.ContainsKey(tileCoord))
                        roadMap[tileCoord] = ComputeRoadDesire(tileCoord.x, tileCoord.y);

                    // STEP 2: Generate terrain always
                    GenerateTerrain(tileCoord);

                    // STEP 3: Overlay road only if it was globally flagged
                    if (roadMap[tileCoord])
                        GenerateRoad(tileCoord);

                    generatedTiles.Add(tileCoord);
                }
            }
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
        // Slight grid tendency with randomness
        float noise = Mathf.PerlinNoise(x * 0.2f, z * 0.2f);
        return (x % 3 == 0 || z % 3 == 0) && noise > 0.3f;
    }

    void GenerateTerrain(Vector2Int tileCoord)
    {
        Vector3 position = new Vector3(tileCoord.x * tileSize, 0, tileCoord.y * tileSize);
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
            if (north && south)
            {
                prefab = roadStraight;
                rot = Quaternion.identity;
            }
            else if (east && west)
            {
                prefab = roadStraight;
                rot = Quaternion.Euler(0, 90, 0);
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
            // Optional: Don't spawn a road if it has no neighbors
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

