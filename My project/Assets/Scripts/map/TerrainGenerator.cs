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

        // Step 1: Build road map
        for (int x = -generationRadius; x <= generationRadius; x++)
        {
            for (int z = -generationRadius; z <= generationRadius; z++)
            {
                Vector2Int pos = center + new Vector2Int(x, z);
                if (!roadMap.ContainsKey(pos)) roadMap[pos] = new RoadTile();

                // Chance to create road right
                Vector2Int right = pos + Vector2Int.right;
                if (Random.value > 0.4f)
                {
                    roadMap[pos].right = true;
                    if (!roadMap.ContainsKey(right)) roadMap[right] = new RoadTile();
                    roadMap[right].left = true;
                }

                // Chance to create road up
                Vector2Int up = pos + Vector2Int.up;
                if (Random.value > 0.4f)
                {
                    roadMap[pos].up = true;
                    if (!roadMap.ContainsKey(up)) roadMap[up] = new RoadTile();
                    roadMap[up].down = true;
                }
            }
        }

        // Step 2: Generate visual content
        foreach (var kvp in roadMap)
        {
            Vector2Int pos = kvp.Key;
            RoadTile tile = kvp.Value;

            if (Vector2Int.Distance(pos, center) > generationRadius)
                continue;

            Vector3 worldPos = new Vector3(pos.x * tileSize, 0, pos.y * tileSize);

            // Generate tile if not already
            if (!generatedTiles.Contains(pos))
            {
                Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                generatedTiles.Add(pos);
            }

            // Generate roads (on tile edges)
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

            // Generate connectors (once per grid point)
            if (!generatedConnectors.Contains(pos))
            {
                int connections = (tile.up ? 1 : 0) + (tile.down ? 1 : 0) + (tile.left ? 1 : 0) + (tile.right ? 1 : 0);

                if (connections == 2 && tile.up && tile.down)
                    Instantiate(roadStraightConnectorPrefab, worldPos, Quaternion.identity, transform);
                else if (connections == 2 && tile.left && tile.right)
                    Instantiate(roadStraightConnectorPrefab, worldPos, Quaternion.Euler(0, 90, 0), transform);
                else if (connections == 4)
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
                    if (tile.up && tile.right)
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
    }
}
