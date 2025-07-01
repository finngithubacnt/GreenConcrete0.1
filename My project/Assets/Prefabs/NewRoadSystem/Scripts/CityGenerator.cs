using System;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    public Transform player;
    public float tileSize = 20f;
    public int generationRadius = 6;

    [Header("Road Prefabs")]
    public GameObject roadStraight;     // N+S or E+W
    public GameObject roadCorner;       // L shape
    public GameObject roadTJunction;    // T shape
    public GameObject roadIntersection; // 4-way
    public GameObject roadConnector;    // Small straight

    private HashSet<TilePosition> generatedTiles = new();
    private Dictionary<TilePosition, RoadTile> roadMap = new();

    void Update()
    {
        GenerateAroundPlayer();
    }

    void GenerateAroundPlayer()
    {
        TilePosition playerPos = TilePosition.FromWorldPosition(player.position, tileSize);

        for (int dx = -generationRadius; dx <= generationRadius; dx++)
        {
            for (int dy = -generationRadius; dy <= generationRadius; dy++)
            {
                TilePosition current = new TilePosition(playerPos.x + dx, playerPos.y + dy);

                if (!generatedTiles.Contains(current))
                {
                    GenerateTile(current);
                    generatedTiles.Add(current);
                }
            }
        }
    }

    void GenerateTile(TilePosition pos)
    {
        RoadConnections connections = RoadConnections.None;

        foreach (RoadDirection dir in Enum.GetValues(typeof(RoadDirection)))
        {
            TilePosition neighbor = pos.GetNeighbor(dir);

            if (roadMap.TryGetValue(neighbor, out RoadTile neighborTile))
            {
                // Check if neighbor connects to us
                if (neighborTile.connections.HasFlag(OppositeConnection(dir)))
                {
                    connections |= DirectionToConnection(dir);
                }
            }
            else
            {
                // 20% chance to try to connect outward if not yet placed
                if (UnityEngine.Random.value < 0.2f)
                    connections |= DirectionToConnection(dir);
            }
        }

        if (connections == RoadConnections.None)
            return;

        roadMap[pos] = new RoadTile(pos, connections);
        PlaceRoadPrefab(pos, connections);
    }

    void PlaceRoadPrefab(TilePosition pos, RoadConnections conns)
    {
        GameObject prefab = GetPrefabFromConnections(conns, out Quaternion rotation);
        if (prefab == null) return;

        Vector3 worldPos = pos.ToWorldPosition(tileSize);
        Instantiate(prefab, worldPos, rotation);
    }

    GameObject GetPrefabFromConnections(RoadConnections conns, out Quaternion rotation)
    {
        rotation = Quaternion.identity;

        int conn = (int)conns;
        switch (conn)
        {
            //Dont Touch
            case 3: // N+S
                rotation = Quaternion.identity;
                return roadStraight;
            case 10: // E+W
                rotation = Quaternion.Euler(0, 90, 0);
                return roadStraight;
            case 5: // N+E
                rotation = Quaternion.identity;
                return roadCorner;
            case 6: // E+S
                rotation = Quaternion.Euler(0, 90, 0);
                return roadCorner;
            case 12: // S+W
                rotation = Quaternion.Euler(0, 180, 0);
                return roadCorner;
            case 9: // W+N
                rotation = Quaternion.Euler(0, 270, 0);
                return roadCorner;
            case 7: // N+E+S
                rotation = Quaternion.identity;
                return roadTJunction;
            case 14: // E+S+W
                rotation = Quaternion.Euler(0, 90, 0);
                return roadTJunction;
            case 13: // S+W+N
                rotation = Quaternion.Euler(0, 180, 0);
                return roadTJunction;
            case 11: // W+N+E
                rotation = Quaternion.Euler(0, 270, 0);
                return roadTJunction;
            case 15: // All directions
                return roadIntersection;
            case 1: // N only
            case 2: // E only
            case 4: // S only
            case 8: // W only
                rotation = conns switch
                {
                    RoadConnections.North => Quaternion.identity,
                    RoadConnections.East => Quaternion.Euler(0, 90, 0),
                    RoadConnections.South => Quaternion.Euler(0, 180, 0),
                    RoadConnections.West => Quaternion.Euler(0, 270, 0),
                    _ => Quaternion.identity
                };
                return roadConnector;
            default:
                return null;
        }
    }

    RoadConnections DirectionToConnection(RoadDirection dir)
    {
        return (RoadConnections)(1 << (int)dir);
    }

    RoadConnections OppositeConnection(RoadDirection dir)
    {
        return DirectionToConnection(Opposite(dir));
    }

    RoadDirection Opposite(RoadDirection dir)
    {
        return dir switch
        {
            RoadDirection.North => RoadDirection.South,
            RoadDirection.South => RoadDirection.North,
            RoadDirection.East => RoadDirection.West,
            RoadDirection.West => RoadDirection.East,
            _ => dir
        };
    }
}
