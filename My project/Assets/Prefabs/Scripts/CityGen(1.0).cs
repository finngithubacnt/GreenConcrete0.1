using UnityEngine;
using System.Collections.Generic;

public class CityRoadGenerator : MonoBehaviour
{
    public int gridSize = 5;
    public float tileSize = 10f;
    public Transform player;

    [Header("Long Road Prefabs (rectangular)")]
    public GameObject roadLongHorizontal;
    public GameObject roadLongVertical;

    [Header("Connector Prefabs (square)")]
    public GameObject elbow;
    public GameObject tJunction;
    public GameObject intersection;
    public GameObject straightConnector;

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
                GameObject road = Instantiate(roadLongHorizontal, spawnPos, Quaternion.identity, transform);
                road.tag = "LongRoad";
            }

            if (tile.roadTop)
            {
                Vector3 spawnPos = tilePos + new Vector3(0, 0, tileSize / 2f);
                GameObject road = Instantiate(roadLongVertical, spawnPos, Quaternion.identity, transform);
                road.tag = "LongRoad";
            }
        }

        // Step 4: Place connectors only when valid
        foreach (var kvp in tiles)
        {
            Vector2Int pos = kvp.Key;

            // Look at the corner ABOVE and RIGHT of this tile
            Vector2Int topLeft = pos + new Vector2Int(0, 1);     // C
            Vector2Int bottomLeft = pos;                         // A
            Vector2Int bottomRight = pos + new Vector2Int(1, 0); // B

            bool fromLeft = tiles.ContainsKey(bottomLeft) && tiles[bottomLeft].roadRight;
            bool fromDown = tiles.ContainsKey(bottomLeft) && tiles[bottomLeft].roadTop;
            bool fromRight = tiles.ContainsKey(bottomRight) && tiles[bottomRight].roadLeft;
            bool fromUp = tiles.ContainsKey(topLeft) && tiles[topLeft].roadBottom;

            int connections = (fromLeft ? 1 : 0) + (fromRight ? 1 : 0) + (fromUp ? 1 : 0) + (fromDown ? 1 : 0);
            if (connections < 2) continue; // Don't place connector unless 2+ sides connect

            Vector3 spawnPos = new Vector3((pos.x + 1) * tileSize, 0, (pos.y + 1) * tileSize);
            GameObject prefab = null;
            Quaternion rot = Quaternion.identity;

            if (connections == 4)
            {
                prefab = intersection;
            }
            else if (connections == 3)
            {
                prefab = tJunction;
                rot = GetTJunctionRotation(fromUp, fromDown, fromLeft, fromRight);
            }
            else if (connections == 2)
            {
                if ((fromUp && fromDown) || (fromLeft && fromRight))
                {
                    prefab = straightConnector;
                    rot = GetStraightRotation(fromUp, fromDown, fromLeft, fromRight);
                }
                else
                {
                    prefab = elbow;
                    rot = GetElbowRotation(fromUp, fromDown, fromLeft, fromRight);
                }
            }

            if (prefab != null)
            {
                Instantiate(prefab, spawnPos, rot, transform);
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
        return Quaternion.Euler(0, 90, 0); // missing left
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
