using UnityEngine;
using System.Collections.Generic;

public class CityRoadGenerator : MonoBehaviour
{
    public int gridSize = 5;
    public float tileSize = 10f;
    public Transform player;

    [Header("Long Road Prefabs (rectangular)")]
    public GameObject roadLongHorizontal; // stretched along X
    public GameObject roadLongVertical;   // stretched along Z

    [Header("Connector Prefabs (square)")]
    public GameObject elbow;
    public GameObject tJunction;
    public GameObject intersection;
    public GameObject straightConnector; // square 180° connector

    private Dictionary<Vector2Int, Tile> tiles = new();

    void Start()
    {
        GenerateCity();
    }

    void GenerateCity()
    {
        Vector2Int center = Vector2Int.RoundToInt(new Vector2(player.position.x, player.position.z) / tileSize);

        // Step 1: Create all tiles
        for (int x = -gridSize; x <= gridSize; x++)
        {
            for (int y = -gridSize; y <= gridSize; y++)
            {
                Vector2Int pos = center + new Vector2Int(x, y);
                tiles[pos] = new Tile(pos);
            }
        }

        // Step 2: Randomly create mutual connections between adjacent tiles
        foreach (var kvp in tiles)
        {
            Vector2Int pos = kvp.Key;
            Tile tile = kvp.Value;

            Vector2Int right = pos + Vector2Int.right;
            Vector2Int top = pos + Vector2Int.up;

            if (tiles.ContainsKey(right) && Random.value > 0.5f)
            {
                tile.roadRight = true;
                tiles[right].roadLeft = true;
            }

            if (tiles.ContainsKey(top) && Random.value > 0.5f)
            {
                tile.roadTop = true;
                tiles[top].roadBottom = true;
            }
        }

        // Step 3: Place long roads (rectangular)
        foreach (var tile in tiles.Values)
        {
            Vector3 tilePos = new Vector3(tile.position.x * tileSize, 0, tile.position.y * tileSize);

            if (tile.roadRight)
            {
                Vector3 spawnPos = tilePos + new Vector3(tileSize / 2f, 0, 0);
                Instantiate(roadLongHorizontal, spawnPos, Quaternion.identity, transform);
            }

            if (tile.roadTop)
            {
                Vector3 spawnPos = tilePos + new Vector3(0, 0, tileSize / 2f);
                Instantiate(roadLongVertical, spawnPos, Quaternion.identity, transform);
            }
        }

        // Step 4: Place connectors (square prefabs) at corners
        foreach (var kvp in tiles)
        {
            Vector2Int pos = kvp.Key;

            // Evaluate corner at (pos + (1,1)) — the top-right corner of the current tile
            bool left = tiles.ContainsKey(pos) && tiles[pos].roadTop;
            bool down = tiles.ContainsKey(pos) && tiles[pos].roadRight;

            bool right = tiles.ContainsKey(pos + Vector2Int.right) && tiles[pos + Vector2Int.right].roadTop;
            bool up = tiles.ContainsKey(pos + Vector2Int.up) && tiles[pos + Vector2Int.up].roadRight;

            int connections = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);
            if (connections == 0) continue;

            Vector3 spawnPos = new Vector3((pos.x + 1) * tileSize, 0, (pos.y + 1) * tileSize);

            if (connections == 4)
            {
                Instantiate(intersection, spawnPos, Quaternion.identity, transform);
            }
            else if (connections == 3)
            {
                Instantiate(tJunction, spawnPos, GetTJunctionRotation(up, down, left, right), transform);
            }
            else if (connections == 2)
            {
                if ((up && down) || (left && right))
                    Instantiate(straightConnector, spawnPos, GetStraightRotation(up, down, left, right), transform);
                else
                    Instantiate(elbow, spawnPos, GetElbowRotation(up, down, left, right), transform);
            }
        }
    }

    Quaternion GetElbowRotation(bool up, bool down, bool left, bool right)
    {
        if (down && right) return Quaternion.Euler(0, 0, 0);
        if (left && down) return Quaternion.Euler(0, 90, 0);
        if (left && up) return Quaternion.Euler(0, 180, 0);
        return Quaternion.Euler(0, 270, 0); // up + right
    }

    Quaternion GetTJunctionRotation(bool up, bool down, bool left, bool right)
    {
        if (!up) return Quaternion.Euler(0, 180, 0);
        if (!right) return Quaternion.Euler(0, 270, 0);
        if (!down) return Quaternion.Euler(0, 0, 0);
        return Quaternion.Euler(0, 90, 0); // no left
    }

    Quaternion GetStraightRotation(bool up, bool down, bool left, bool right)
    {
        return (up && down) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 90, 0);
    }

    class Tile
    {
        public Vector2Int position;
        public bool roadTop, roadBottom, roadLeft, roadRight;

        public Tile(Vector2Int pos)
        {
            position = pos;
        }
    }
}
