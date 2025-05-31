using UnityEngine;
using System.Collections.Generic;

public class CityGenerator : MonoBehaviour
{
    [Header("Settings")]
    public float tileSize = 10f;
    public int generationRadius = 5;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject roadStraightPrefab;
    public GameObject roadCornerPrefab;
    public GameObject roadTJunctionPrefab;
    public GameObject roadIntersectionPrefab;
    public GameObject roadStraightConnectorPrefab;

    [Header("Buildings")]
    public GameObject buildingCornerPrefab;

    [Header("Player Reference")]
    public Transform player;

    private Dictionary<Vector2Int, RoadTile> roadMap = new Dictionary<Vector2Int, RoadTile>();
    private HashSet<Vector2Int> generatedTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> generatedConnectors = new HashSet<Vector2Int>();
    private Vector2Int currentPlayerTile = Vector2Int.zero;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player not assigned.");
            enabled = false;
            return;
        }

        UpdateGeneration();
    }

    void Update()
    {
        Vector2Int playerTile = GetTileCoord(player.position);

        if (playerTile != currentPlayerTile)
        {
            currentPlayerTile = playerTile;
            UpdateGeneration();
        }
    }

    Vector2Int GetTileCoord(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / tileSize);
        int z = Mathf.FloorToInt(position.z / tileSize);
        return new Vector2Int(x, z);
    }

    void UpdateGeneration()
    {
        Vector2Int center = GetTileCoord(player.position);

        // Step 1: Build road map with consistent decisions
        for (int x = -generationRadius; x <= generationRadius; x++)
        {
            for (int z = -generationRadius; z <= generationRadius; z++)
            {
                Vector2Int pos = center + new Vector2Int(x, z);
                if (!roadMap.ContainsKey(pos))
                    roadMap[pos] = new RoadTile();

                RoadTile current = roadMap[pos];

                // Check or decide right connection
                Vector2Int right = pos + Vector2Int.right;
                if (!roadMap.ContainsKey(right))
                    roadMap[right] = new RoadTile();

                RoadTile neighborRight = roadMap[right];
                if (!current.hasRightDecided)
                {
                    bool connect = Random.value > 0.4f;
                    current.right = connect;
                    current.hasRightDecided = true;

                    neighborRight.left = connect;
                    neighborRight.hasLeftDecided = true;
                }

                // Check or decide up connection
                Vector2Int up = pos + Vector2Int.up;
                if (!roadMap.ContainsKey(up))
                    roadMap[up] = new RoadTile();

                RoadTile neighborUp = roadMap[up];
                if (!current.hasUpDecided)
                {
                    bool connect = Random.value > 0.4f;
                    current.up = connect;
                    current.hasUpDecided = true;

                    neighborUp.down = connect;
                    neighborUp.hasDownDecided = true;
                }
            }
        }

        // Step 2: Generate visuals
        foreach (var kvp in roadMap)
        {
            Vector2Int pos = kvp.Key;
            RoadTile tile = kvp.Value;

            if (Vector2Int.Distance(pos, center) > generationRadius)
                continue;

            Vector3 worldPos = new Vector3(pos.x * tileSize, 0, pos.y * tileSize);

            // Place tile
            if (!generatedTiles.Contains(pos))
            {
                Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                generatedTiles.Add(pos);

                // Building on inside corner if it forms one
                if ((tile.up && tile.right) ||
                    (tile.right && tile.down) ||
                    (tile.down && tile.left) ||
                    (tile.left && tile.up))
                {
                    Quaternion rotation = Quaternion.identity;
                    Vector3 offset = Vector3.zero;

                    if (tile.up && tile.right)
                    {
                        rotation = Quaternion.Euler(0, 0, 0);
                        offset = new Vector3(tileSize / 4f, 0, tileSize / 4f);
                    }
                    else if (tile.right && tile.down)
                    {
                        rotation = Quaternion.Euler(0, 90, 0);
                        offset = new Vector3(tileSize / 4f, 0, -tileSize / 4f);
                    }
                    else if (tile.down && tile.left)
                    {
                        rotation = Quaternion.Euler(0, 180, 0);
                        offset = new Vector3(-tileSize / 4f, 0, -tileSize / 4f);
                    }
                    else if (tile.left && tile.up)
                    {
                        rotation = Quaternion.Euler(0, -90, 0);
                        offset = new Vector3(-tileSize / 4f, 0, tileSize / 4f);
                    }

                    if (Random.value > 0.5f)
                    {
                        Vector3 buildingPos = worldPos + offset;
                        Instantiate(buildingCornerPrefab, buildingPos, rotation, transform);
                    }
                }
            }

            // Straight roads
            if (tile.up)
            {
                Vector3 upPos = worldPos + new Vector3(0, 0, tileSize / 2f);
                Instantiate(roadStraightPrefab, upPos, Quaternion.identity, transform);
            }

            if (tile.right)
            {
                Vector3 rightPos = worldPos + new Vector3(tileSize / 2f, 0, 0);
                Instantiate(roadStraightPrefab, rightPos, Quaternion.Euler(0, 90, 0), transform);
            }

            // Connectors
            if (!generatedConnectors.Contains(pos))
            {
                int connections = (tile.up ? 1 : 0) + (tile.down ? 1 : 0) + (tile.left ? 1 : 0) + (tile.right ? 1 : 0);

                if (connections == 4)
                    Instantiate(roadIntersectionPrefab, worldPos, Quaternion.identity, transform);
                else if (connections == 3)
                {
                    if (!tile.up)
                        Instantiate(roadTJunctionPrefab, worldPos, Quaternion.Euler(0, 180, 0), transform);
                    else if (!tile.down)
                        Instantiate(roadTJunctionPrefab, worldPos, Quaternion.identity, transform);
                    else if (!tile.left)
                        Instantiate(roadTJunctionPrefab, worldPos, Quaternion.Euler(0, 90, 0), transform);
                    else
                        Instantiate(roadTJunctionPrefab, worldPos, Quaternion.Euler(0, -90, 0), transform);
                }
                else if (connections == 2)
                {
                    if (tile.up && tile.down)
                        Instantiate(roadStraightConnectorPrefab, worldPos, Quaternion.identity, transform);
                    else if (tile.left && tile.right)
                        Instantiate(roadStraightConnectorPrefab, worldPos, Quaternion.Euler(0, 90, 0), transform);
                    else if (tile.up && tile.right)
                        Instantiate(roadCornerPrefab, worldPos, Quaternion.identity, transform);
                    else if (tile.right && tile.down)
                        Instantiate(roadCornerPrefab, worldPos, Quaternion.Euler(0, 90, 0), transform);
                    else if (tile.down && tile.left)
                        Instantiate(roadCornerPrefab, worldPos, Quaternion.Euler(0, 180, 0), transform);
                    else if (tile.left && tile.up)
                        Instantiate(roadCornerPrefab, worldPos, Quaternion.Euler(0, -90, 0), transform);
                }

                generatedConnectors.Add(pos);
            }
        }
    }

    public class RoadTile
    {
        public bool up, down, left, right;
        public bool hasUpDecided, hasDownDecided, hasLeftDecided, hasRightDecided;
    }
}
