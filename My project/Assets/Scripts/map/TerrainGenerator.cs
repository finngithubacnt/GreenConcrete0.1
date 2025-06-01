using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileType
{
    public string typeName;
    [Range(0, 1)]
    public float weight = 0.33f;
    public GameObject[] tilePrefabs;
}
public class TerrainGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject roadStraightPrefab;
    public GameObject connectorIntersection;
    public GameObject connectorTJunction;
    public GameObject connectorCorner;
    public GameObject connectorStraightConnector;

    [Header("Tile Type Settings")]
    public TileType[] tileTypes; // Editable in inspector

    [Header("Settings")]
    public int tileSize = 10;
    public float roadChance = 0.7f;
    public int generationRadius = 5;
    public Transform player;

    private Dictionary<Vector2Int, TileData> tileGrid = new();
    private HashSet<Vector2Int> generatedTiles = new();
    private HashSet<Vector2Int> generatedCorners = new();

    void Update()
    {
        Vector2Int playerTile = new(
            Mathf.FloorToInt(player.position.x / tileSize),
            Mathf.FloorToInt(player.position.z / tileSize)
        );

        for (int x = -generationRadius; x <= generationRadius; x++)
        {
            for (int y = -generationRadius; y <= generationRadius; y++)
            {
                Vector2Int pos = playerTile + new Vector2Int(x, y);
                if (!tileGrid.ContainsKey(pos))
                {
                    GenerateTile(pos);
                }
            }
        }

        foreach (var kvp in tileGrid)
        {
            Vector2Int tilePos = kvp.Key;
            Vector2Int[] corners = {
                tilePos,
                tilePos + Vector2Int.right,
                tilePos + Vector2Int.up,
                tilePos + Vector2Int.right + Vector2Int.up
            };

            foreach (var corner in corners)
            {
                if (!generatedCorners.Contains(corner))
                {
                    PlaceConnector(corner);
                }
            }
        }
    }

    void GenerateTile(Vector2Int pos)
    {
        Vector3 worldPos = new Vector3(pos.x * tileSize, 0, pos.y * tileSize);
        Instantiate(tilePrefab, worldPos, Quaternion.identity);

        TileData data = new TileData();
        float noise = Mathf.PerlinNoise(pos.x * 0.2f, pos.y * 0.2f);

        if (noise > 0.3f && Random.value < roadChance)
        {
            data.up = true;
            Instantiate(roadStraightPrefab, worldPos + new Vector3(0, 0, tileSize / 2f), Quaternion.identity);
        }
        if (noise < 0.7f && Random.value < roadChance)
        {
            data.right = true;
            Instantiate(roadStraightPrefab, worldPos + new Vector3(tileSize / 2f, 0, 0), Quaternion.Euler(0, 90, 0));
        }

        data.tileType = PickTileType();
        tileGrid[pos] = data;
        generatedTiles.Add(pos);

        TryPlaceTilePrefabs(pos, data.tileType);
    }

    void TryPlaceTilePrefabs(Vector2Int pos, TileType tileType)
    {
        if (tileType == null || tileType.tilePrefabs.Length == 0) return;

        Vector3 center = new Vector3(pos.x * tileSize, 0, pos.y * tileSize);

        int count = Random.Range(1, 3);
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-tileSize / 3f, tileSize / 3f), 0, Random.Range(-tileSize / 3f, tileSize / 3f));
            GameObject prefab = tileType.tilePrefabs[Random.Range(0, tileType.tilePrefabs.Length)];
            Instantiate(prefab, center + offset, Quaternion.Euler(0, 90 * Random.Range(0, 4), 0));
        }
    }

    TileType PickTileType()
    {
        float total = 0f;
        foreach (var t in tileTypes)
            total += t.weight;

        float r = Random.Range(0, total);
        float accum = 0;

        foreach (var t in tileTypes)
        {
            accum += t.weight;
            if (r < accum) return t;
        }

        return tileTypes.Length > 0 ? tileTypes[0] : null;
    }

    void PlaceConnector(Vector2Int corner)
    {
        bool up = tileGrid.ContainsKey(corner) && tileGrid[corner].up;
        bool down = tileGrid.ContainsKey(corner + Vector2Int.down) && tileGrid[corner + Vector2Int.down].up;
        bool right = tileGrid.ContainsKey(corner) && tileGrid[corner].right;
        bool left = tileGrid.ContainsKey(corner + Vector2Int.left) && tileGrid[corner + Vector2Int.left].right;

        int count = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);
        GameObject prefab = null;
        Quaternion rotation = Quaternion.identity;

        if (count == 4) prefab = connectorIntersection;
        else if (count == 3)
        {
            prefab = connectorTJunction;
            if (!up) rotation = Quaternion.Euler(0, 180, 0);
            else if (!right) rotation = Quaternion.Euler(0, 270, 0);
            else if (!down) rotation = Quaternion.identity;
            else rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (count == 2)
        {
            if ((up && down) || (left && right))
            {
                prefab = connectorStraightConnector;
                if (left && right) rotation = Quaternion.Euler(0, 90, 0);
            }
            else
            {
                prefab = connectorCorner;
                if (up && right) rotation = Quaternion.identity;
                else if (right && down) rotation = Quaternion.Euler(0, 90, 0);
                else if (down && left) rotation = Quaternion.Euler(0, 180, 0);
                else if (left && up) rotation = Quaternion.Euler(0, 270, 0);
            }
        }

        if (prefab != null)
        {
            Vector3 worldPos = new Vector3(corner.x * tileSize, 0, corner.y * tileSize);
            Instantiate(prefab, worldPos, rotation);
            generatedCorners.Add(corner);
        }
    }

    class TileData
    {
        public bool up;
        public bool right;
        public TileType tileType;
    }
}
