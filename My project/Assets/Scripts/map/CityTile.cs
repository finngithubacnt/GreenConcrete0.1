using UnityEngine;

public enum TileType { Building, Street, Park }

public class CityTile
{
    public Vector2Int gridPosition;
    public TileType tileType;
    public GameObject tileObject;
}
