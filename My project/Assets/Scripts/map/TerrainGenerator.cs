using System.Collections.Generic;
using UnityEngine;

public class SmartCityGenerator : MonoBehaviour
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

    [Header("Grid Control")]
    public float breakProbability = 0.2f;
    public float perlinScale = 0.1f;

    private HashSet<Vector2Int> generatedTiles = new();
    private Dictionary<Vector2Int, RoadConnection> connectionMap = new();

    void Update()
    {
        GenerateTilesAroundPlayer();
    }

    void GenerateTilesAroundPlayer()
    {
        Vector2Int center = WorldToTileCoords(player.position);

        for (int dx = -tileRange; dx <= tileRange; dx++)
        {
            for (int dz = -tileRange; dz <= tileRange; dz++)
            {
                Vector2Int coord = center + new Vector2Int(dx, dz);
                if (generatedTiles.Contains(coord)) continue;

                EvaluateConnections(coord);
                GenerateTerrain(coord);
                GenerateRoad(coord);

                generatedTiles.Add(coord);
            }
        }
    }

    Vector2Int WorldToTileCoords(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / tileSize);
        int z = Mathf.FloorToInt(pos.z / tileSize);
        return new Vector2Int(x, z);
    }

    void EvaluateConnections(Vector2Int coord)
    {
        RoadConnection conn = new();

        foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
        {
            Vector2Int neighbor = coord + DirectionOffset(dir);
            if (!connectionMap.ContainsKey(neighbor))
            {
                // Evaluate neighbor to maintain bidirectional logic
                RoadConnection temp = new();
                temp.Connect(Opposite(dir), ShouldConnect(coord, neighbor, dir));
                connectionMap[neighbor] = temp;
            }

            if (connectionMap[neighbor].HasConnection(Opposite(dir)))
                conn.Connect(dir);
        }

        connectionMap[coord] = conn;
    }

    bool ShouldConnect(Vector2Int from, Vector2Int to, Direction dir)
    {
        float noise = Mathf.PerlinNoise(to.x * perlinScale, to.y * perlinScale);
        return noise > breakProbability;
    }

    void GenerateTerrain(Vector2Int coord)
    {
        Vector3 pos = new Vector3(coord.x * tileSize, 0, coord.y * tileSize);

        bool isPark = Random.value < 0.3f;
        GameObject[] pool = isPark ? parkPrefabs : buildingPrefabs;
        if (pool.Length == 0) return;

        GameObject prefab = pool[Random.Range(0, pool.Length)];
        GameObject tile = Instantiate(prefab, pos, Quaternion.identity, transform);
        AddFoliage(tile.transform, pos);
    }

    void GenerateRoad(Vector2Int coord)
    {
        if (!connectionMap.ContainsKey(coord)) return;

        Vector3 pos = new Vector3(coord.x * tileSize, 0, coord.y * tileSize);
        RoadConnection conn = connectionMap[coord];
        GameObject roadPrefab = null;
        Quaternion rotation = Quaternion.identity;

        int count = conn.Count;

        if (count == 4)
        {
            roadPrefab = roadIntersection;
        }
        else if (count == 3)
        {
            roadPrefab = roadTJunction;
            if (!conn.north) rotation = Quaternion.Euler(0, 180, 0);
            else if (!conn.east) rotation = Quaternion.Euler(0, 270, 0);
            else if (!conn.south) rotation = Quaternion.identity;
            else if (!conn.west) rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (count == 2)
        {
            if ((conn.north && conn.south) || (conn.east && conn.west))
            {
                roadPrefab = roadStraight;
                rotation = (conn.east && conn.west) ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
            }
            else
            {
                roadPrefab = roadCorner;
                if (conn.north && conn.east) rotation = Quaternion.Euler(0, 0, 0);
                else if (conn.east && conn.south) rotation = Quaternion.Euler(0, 90, 0);
                else if (conn.south && conn.west) rotation = Quaternion.Euler(0, 180, 0);
                else if (conn.west && conn.north) rotation = Quaternion.Euler(0, 270, 0);
            }
        }
        else if (count == 1)
        {
            // If it connects to just one other road, use a straight road
            roadPrefab = roadStraight;
            if (conn.north) rotation = Quaternion.identity;
            else if (conn.east) rotation = Quaternion.Euler(0, 90, 0);
            else if (conn.south) rotation = Quaternion.Euler(0, 180, 0);
            else if (conn.west) rotation = Quaternion.Euler(0, 270, 0);
        }
        else
        {
            // Optional: skip or place a dirt path
            return;
        }

        Instantiate(roadPrefab, pos, rotation, transform);
    }

    void AddFoliage(Transform parent, Vector3 basePosition)
    {
        if (foliagePrefabs.Length == 0) return;

        int count = Random.Range(2, 5);
        for (int i = 0; i < count; i++)
        {
            GameObject f = foliagePrefabs[Random.Range(0, foliagePrefabs.Length)];
            Vector3 pos = basePosition + new Vector3(Random.Range(1f, tileSize - 1), 0, Random.Range(1f, tileSize - 1));
            Instantiate(f, pos, Quaternion.Euler(0, Random.Range(0, 360), 0), parent);
        }
    }

    enum Direction { North, East, South, West }

    Direction Opposite(Direction d) => d switch
    {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East => Direction.West,
        Direction.West => Direction.East,
        _ => d
    };

    Vector2Int DirectionOffset(Direction d) => d switch
    {
        Direction.North => new Vector2Int(0, 1),
        Direction.South => new Vector2Int(0, -1),
        Direction.East => new Vector2Int(1, 0),
        Direction.West => new Vector2Int(-1, 0),
        _ => Vector2Int.zero
    };

    class RoadConnection
    {
        public bool north, east, south, west;
        public int Count => (north ? 1 : 0) + (east ? 1 : 0) + (south ? 1 : 0) + (west ? 1 : 0);

        public void Connect(Direction d, bool value = true)
        {
            switch (d)
            {
                case Direction.North: north = value; break;
                case Direction.East: east = value; break;
                case Direction.South: south = value; break;
                case Direction.West: west = value; break;
            }
        }

        public bool HasConnection(Direction d) => d switch
        {
            Direction.North => north,
            Direction.East => east,
            Direction.South => south,
            Direction.West => west,
            _ => false
        };
    }
}
