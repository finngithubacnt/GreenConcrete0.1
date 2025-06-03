using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileType
{
    public string typeName;
    [Range(0, 1)]
    public float weight = 0.33f;
    public GameObject baseTilePrefab;
    public GameObject[] tilePropsPrefabs;
}

public class OvergrownCityGenerator : MonoBehaviour
{
    public int tileSize = 10;
    public float roadChance = 0.5f;
    public float generateRadius = 50f;
    public Transform player;

    public GameObject roadStraightPrefab;
    public GameObject roadConnectorT;
    public GameObject roadConnectorX;
    public GameObject roadConnectorElbow;
    public GameObject roadConnectorStraight;

    public List<TileType> tileTypes = new List<TileType>();

    private Dictionary<Vector2Int, TileData> tileGrid = new Dictionary<Vector2Int, TileData>();
    private HashSet<Vector2Int> generatedTiles = new HashSet<Vector2Int>();

    void Update()
    {
        GenerateTilesAroundPlayer();
    }

    void GenerateTilesAroundPlayer()
    {
        Vector2Int playerTile = new Vector2Int(
            Mathf.FloorToInt(player.position.x / tileSize),
            Mathf.FloorToInt(player.position.z / tileSize)
        );

        int radiusInTiles = Mathf.CeilToInt(generateRadius / tileSize);

        for (int x = -radiusInTiles; x <= radiusInTiles; x++)
        {
            for (int y = -radiusInTiles; y <= radiusInTiles; y++)
            {
                Vector2Int tilePos = new Vector2Int(playerTile.x + x, playerTile.y + y);
                if (!generatedTiles.Contains(tilePos))
                {
                    GenerateTile(tilePos);
                }
            }
        }
    }

    void GenerateTile(Vector2Int pos)
    {
        Vector3 worldPos = new Vector3(pos.x * tileSize, 0, pos.y * tileSize);

        TileData data = new TileData();
        data.tileType = PickTileType();

        // Instantiate tile base prefab
        if (data.tileType != null && data.tileType.baseTilePrefab != null)
        {
            Instantiate(data.tileType.baseTilePrefab, worldPos, Quaternion.identity);
        }

        // Determine roads (straight)
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

        tileGrid[pos] = data;
        generatedTiles.Add(pos);

        // Place tile props (like buildings, trees)
        TryPlaceTileProps(pos, data.tileType);

        // Try placing road connectors at 4 corners of this tile
        TryPlaceConnectorsAtCorners(pos);
    }

    void TryPlaceTileProps(Vector2Int pos, TileType tileType)
    {
        if (tileType == null || tileType.tilePropsPrefabs == null || tileType.tilePropsPrefabs.Length == 0)
            return;

        Vector3 basePos = new Vector3(pos.x * tileSize, 0, pos.y * tileSize);
        GameObject prefabToPlace = tileType.tilePropsPrefabs[Random.Range(0, tileType.tilePropsPrefabs.Length)];

        Vector3 offset = new Vector3(tileSize / 4f, 0, tileSize / 4f); // Example offset for placing buildings inside tile
        Instantiate(prefabToPlace, basePos + offset, Quaternion.identity);
    }

    void TryPlaceConnectorsAtCorners(Vector2Int tilePos)
    {
        // Evaluate 4 corners (bottom-left of current tile)
        Vector2Int[] cornerOffsets = new Vector2Int[]
        {
            Vector2Int.zero,
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1)
        };

        foreach (Vector2Int offset in cornerOffsets)
        {
            Vector2Int cornerPos = tilePos + offset;
            PlaceConnectorAtCorner(cornerPos);
        }
    }

    void PlaceConnectorAtCorner(Vector2Int cornerPos)
    {
        bool up = tileGrid.ContainsKey(cornerPos) && tileGrid[cornerPos].up;
        bool down = tileGrid.ContainsKey(cornerPos + Vector2Int.down) && tileGrid[cornerPos + Vector2Int.down].up;
        bool left = tileGrid.ContainsKey(cornerPos + Vector2Int.left) && tileGrid[cornerPos + Vector2Int.left].right;
        bool right = tileGrid.ContainsKey(cornerPos) && tileGrid[cornerPos].right;

        int connections = 0;
        if (up) connections++;
        if (down) connections++;
        if (left) connections++;
        if (right) connections++;

        Vector3 pos = new Vector3(cornerPos.x * tileSize, 0, cornerPos.y * tileSize);

        if (connections == 4)
        {
            Instantiate(roadConnectorX, pos, Quaternion.identity);
        }
        else if (connections == 3)
        {
            if (!up) Instantiate(roadConnectorT, pos, Quaternion.Euler(0, 180, 0));
            else if (!down) Instantiate(roadConnectorT, pos, Quaternion.identity);
            else if (!left) Instantiate(roadConnectorT, pos, Quaternion.Euler(0, 90, 0));
            else if (!right) Instantiate(roadConnectorT, pos, Quaternion.Euler(0, -90, 0));
        }
        else if (connections == 2)
        {
            if ((up && down) || (left && right))
            {
                Instantiate(roadConnectorStraight, pos, (up && down) ? Quaternion.identity : Quaternion.Euler(0, 90, 0));
            }
            else if (up && right)
                Instantiate(roadConnectorElbow, pos, Quaternion.identity);
            else if (right && down)
                Instantiate(roadConnectorElbow, pos, Quaternion.Euler(0, 90, 0));
            else if (down && left)
                Instantiate(roadConnectorElbow, pos, Quaternion.Euler(0, 180, 0));
            else if (left && up)
                Instantiate(roadConnectorElbow, pos, Quaternion.Euler(0, 270, 0));
        }
        else if (connections == 1)
        {
            // Optional: could place dead-end connector
        }
    }

    TileType PickTileType()
    {
        float totalWeight = 0f;
        foreach (var type in tileTypes)
        {
            totalWeight += type.weight;
        }

        float rand = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var type in tileTypes)
        {
            cumulative += type.weight;
            if (rand <= cumulative)
                return type;
        }

        return tileTypes.Count > 0 ? tileTypes[0] : null;
    }
}

public class TileData
{
    public bool up, right;
    public TileType tileType;
}
