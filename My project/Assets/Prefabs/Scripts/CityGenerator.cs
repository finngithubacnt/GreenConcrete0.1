using UnityEngine;
using System.Collections.Generic;

public class CityGenerator : MonoBehaviour
{
    public Transform player;
    public int tileSize = 20;
    public int radius = 3; // how many tiles away to generate
    public GameObject[] tilePrefabs; // cityBlock, park, ruins, etc.

    private Dictionary<Vector2Int, GameObject> spawnedTiles = new Dictionary<Vector2Int, GameObject>();

    void Update()
    {
        Vector2Int playerTile = new Vector2Int(
            Mathf.FloorToInt(player.position.x / tileSize),
            Mathf.FloorToInt(player.position.z / tileSize)
        );

        // Generate tiles around player
        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                Vector2Int tilePos = new Vector2Int(playerTile.x + x, playerTile.y + z);

                if (!spawnedTiles.ContainsKey(tilePos))
                {
                    GenerateTile(tilePos);
                }
            }
        }

        // Optional: Cleanup tiles too far away
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var pos in spawnedTiles.Keys)
        {
            if (Vector2Int.Distance(pos, playerTile) > radius + 1)
            {
                Destroy(spawnedTiles[pos]);
                toRemove.Add(pos);
            }
        }
        foreach (var pos in toRemove)
        {
            spawnedTiles.Remove(pos);
        }
    }

    void GenerateTile(Vector2Int tileCoord)
    {
        Vector3 worldPos = new Vector3(tileCoord.x * tileSize, 0, tileCoord.y * tileSize);

        // Choose tile type randomly (you can later use weighted logic or Perlin noise)
        GameObject tilePrefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
        GameObject tileInstance = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);

        spawnedTiles.Add(tileCoord, tileInstance);
    }
}
