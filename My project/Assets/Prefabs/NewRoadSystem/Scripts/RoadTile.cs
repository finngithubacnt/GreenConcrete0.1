public enum RoadDirection { North = 0, East = 1, South = 2, West = 3 }

[System.Flags]
public enum RoadConnections
{
    None = 0,
    North = 1 << 0,
    East = 1 << 1,
    South = 1 << 2,
    West = 1 << 3
}

public class RoadTile
{
    public TilePosition position;
    public RoadConnections connections;

    public RoadTile(TilePosition pos, RoadConnections conns)
    {
        position = pos;
        connections = conns;
    }
}
