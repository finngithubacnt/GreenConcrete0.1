using UnityEngine;

[System.Serializable]
public struct TilePosition
{
    public int x, y;

    public TilePosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static TilePosition FromWorldPosition(Vector3 pos, float tileSize)
    {
        return new TilePosition(
            Mathf.FloorToInt(pos.x / tileSize),
            Mathf.FloorToInt(pos.z / tileSize)
        );
    }

    public Vector3 ToWorldPosition(float tileSize)
    {
        return new Vector3(x * tileSize, 0, y * tileSize);
    }

    public TilePosition GetNeighbor(RoadDirection dir)
    {
        return dir switch
        {
            RoadDirection.North => new TilePosition(x, y + 1),
            RoadDirection.East => new TilePosition(x + 1, y),
            RoadDirection.South => new TilePosition(x, y - 1),
            RoadDirection.West => new TilePosition(x - 1, y),
            _ => this
        };
    }

    public override bool Equals(object obj) => obj is TilePosition other && x == other.x && y == other.y;
    public override int GetHashCode() => (x, y).GetHashCode();
}
