using System.Collections.Generic;
using UnityEngine;
public class OverGrownCityGenerator : MonoBehaviour
{
    public Transform player;
    public int renderRadius = 10;
    public float tileSize = 10f;

    public GameObject roadStraight;
    public GameObject roadElbow;
    public GameObject roadTJunction;
    public GameObject roadIntersection;

    public List<TileType> tileTypes;

    private Dictionary<Vector2Int, TileData> generatedTiles = new Dictionary<Vector2Int, TileData>();
    private HashSet<Vector2Int> roadPositions = new HashSet<Vector2Int>();

    void Update()
    {
        Vector2Int playerTile = WorldToTilePos(player.position);
        for (int x = -renderRadius; x <= renderRadius; x++)
        {
            for (int y = -renderRadius; y <= renderRadius; y++)
            {
                Vector2Int tilePos = new Vector2Int(playerTile.x + x, playerTile.y + y);
                if (!generatedTiles.ContainsKey(tilePos))
                {
                    GenerateTile(tilePos);
                }
            }
        }
    }

    Vector2Int WorldToTilePos(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / tileSize),
            Mathf.FloorToInt(worldPos.z / tileSize)
        );
    }

    void GenerateTile(Vector2Int pos)
    {
        TileType selectedType = ChooseTileType();
        Vector3 worldPos = new Vector3(pos.x * tileSize, 0, pos.y * tileSize);

        GameObject tile = Instantiate(selectedType.baseTilePrefab, worldPos, Quaternion.identity, transform);

        TileData data = new TileData
        {
            tileType = selectedType,
            tileObject = tile
        };

        generatedTiles[pos] = data;

        // Road decisions
        data.up = Random.value > 0.3f;
        data.down = Random.value > 0.3f;
        data.left = Random.value > 0.3f;
        data.right = Random.value > 0.3f;

        if (data.up) roadPositions.Add(pos + Vector2Int.up);
        if (data.down) roadPositions.Add(pos + Vector2Int.down);
        if (data.left) roadPositions.Add(pos + Vector2Int.left);
        if (data.right) roadPositions.Add(pos + Vector2Int.right);

        PlaceSideRoads(pos, data);
        PlaceCornerConnectors(pos);
        PlaceTileBehaviorObjects(pos, selectedType, data);
    }

    TileType ChooseTileType()
    {
        float totalWeight = 0f;
        foreach (var type in tileTypes) totalWeight += type.weight;

        float rand = Random.Range(0, totalWeight);
        float current = 0f;

        foreach (var type in tileTypes)
        {
            current += type.weight;
            if (rand <= current)
                return type;
        }

        return tileTypes[0];
    }

    void PlaceSideRoads(Vector2Int pos, TileData data)
    {
        Vector3 basePos = new Vector3(pos.x * tileSize, 0, pos.y * tileSize);

        if (data.up)
            Instantiate(roadStraight, basePos + new Vector3(0, 0, tileSize / 2), Quaternion.identity, transform);
        if (data.down)
            Instantiate(roadStraight, basePos + new Vector3(0, 0, -tileSize / 2), Quaternion.Euler(0, 180, 0), transform);
        if (data.left)
            Instantiate(roadStraight, basePos + new Vector3(-tileSize / 2, 0, 0), Quaternion.Euler(0, 90, 0), transform);
        if (data.right)
            Instantiate(roadStraight, basePos + new Vector3(tileSize / 2, 0, 0), Quaternion.Euler(0, -90, 0), transform);
    }

    void PlaceCornerConnectors(Vector2Int pos)
    {
        Vector2Int[] corners = {
            pos,
            pos + Vector2Int.up,
            pos + Vector2Int.right,
            pos + Vector2Int.up + Vector2Int.right
        };

        foreach (var corner in corners)
        {
            if (generatedTiles.ContainsKey(corner)) continue;

            bool up = roadPositions.Contains(corner + Vector2Int.up);
            bool down = roadPositions.Contains(corner + Vector2Int.down);
            bool left = roadPositions.Contains(corner + Vector2Int.left);
            bool right = roadPositions.Contains(corner + Vector2Int.right);

            int connections = 0;
            if (up) connections++;
            if (down) connections++;
            if (left) connections++;
            if (right) connections++;

            Vector3 connectorPos = new Vector3(corner.x * tileSize, 0, corner.y * tileSize);

            if (connections == 4)
                Instantiate(roadIntersection, connectorPos, Quaternion.identity, transform);
            else if (connections == 3)
            {
                if (!up)
                    Instantiate(roadTJunction, connectorPos, Quaternion.Euler(0, 180, 0), transform);
                else if (!down)
                    Instantiate(roadTJunction, connectorPos, Quaternion.identity, transform);
                else if (!left)
                    Instantiate(roadTJunction, connectorPos, Quaternion.Euler(0, 90, 0), transform);
                else
                    Instantiate(roadTJunction, connectorPos, Quaternion.Euler(0, -90, 0), transform);
            }
            else if (connections == 2)
            {
                if ((up && down) || (left && right))
                    return;

                if (up && right)
                    Instantiate(roadElbow, connectorPos, Quaternion.identity, transform);
                else if (right && down)
                    Instantiate(roadElbow, connectorPos, Quaternion.Euler(0, 90, 0), transform);
                else if (down && left)
                    Instantiate(roadElbow, connectorPos, Quaternion.Euler(0, 180, 0), transform);
                else if (left && up)
                    Instantiate(roadElbow, connectorPos, Quaternion.Euler(0, -90, 0), transform);
            }

            generatedTiles[corner] = new TileData(); // Placeholder to avoid re-spawning
        }
    }

    void PlaceTileBehaviorObjects(Vector2Int pos, TileType tileType, TileData tileData)
    {
        if (tileType == null) return;

        Vector3 basePos = new Vector3(pos.x * tileSize, 0, pos.y * tileSize);

        switch (tileType.behavior)
        {
            case TileBehavior.CityBlock:
                PlaceCityBlockBuildings(basePos, tileType, tileData);
                break;

            default:
                TryPlaceTileProps(pos, tileType);
                break;
        }
    }

    void PlaceCityBlockBuildings(Vector3 basePos, TileType tileType, TileData tileData)
    {
        if (tileType.cityBlockBuildings == null || tileType.cityBlockBuildings.Length == 0) return;

        float spacing = 4f;
        float height = 0f;

        List<Vector3> positions = new List<Vector3>();

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if ((z == 1 && tileData.up) || (x == 1 && tileData.right))
                    continue;

                Vector3 offset = new Vector3(x * spacing, height, z * spacing);
                Vector3 spawnPos = basePos + offset;
                positions.Add(spawnPos);
            }
        }

        foreach (var pos in positions)
        {
            GameObject prefab = tileType.cityBlockBuildings[Random.Range(0, tileType.cityBlockBuildings.Length)];
            Instantiate(prefab, pos, Quaternion.identity, transform);
        }
    }

    void TryPlaceTileProps(Vector2Int pos, TileType tileType)
    {
        if (tileType.tilePropsPrefabs == null || tileType.tilePropsPrefabs.Length == 0) return;

        Vector3 basePos = new Vector3(pos.x * tileSize, 0, pos.y * tileSize);

        int propCount = Random.Range(1, 4);
        for (int i = 0; i < propCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
            GameObject propPrefab = tileType.tilePropsPrefabs[Random.Range(0, tileType.tilePropsPrefabs.Length)];
            Instantiate(propPrefab, basePos + offset, Quaternion.identity, transform);
        }
    }
}


public enum TileBehavior
{
    None,
    CityBlock,
    Park,
    ParkingLot,
}

[System.Serializable]
public class TileType
{
    public string typeName;
    [Range(0, 1)]
    public float weight = 0.33f;
    public GameObject baseTilePrefab;
    public GameObject[] tilePropsPrefabs;
    public TileBehavior behavior = TileBehavior.None;
    public GameObject[] cityBlockBuildings;
}

public class TileData
{
    public TileType tileType;
    public GameObject tileObject;
    public bool up, down, left, right;
}

