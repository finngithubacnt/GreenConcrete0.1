using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float tileSize = 20f;

    public GameObject[] buildingPrefabs;
    public GameObject[] parkPrefabs;
    public GameObject roadStraight;
    public GameObject roadCorner;
    public GameObject roadIntersection;
    public GameObject roadTJunction;

    public GameObject[] foliagePrefabs;

    private TileType[,] tileMap;

    void Start()
    {
        tileMap = new TileType[width, height];
        GenerateRoadNetwork();
        GenerateTiles();
    }

    //Roads Gen
    void GenerateRoadNetwork()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (x % 3 == 0 || z % 3 == 0)
                    tileMap[x, z] = TileType.Street;
                else
                    tileMap[x, z] = GetRandomTileType(); // building or park
            }
        }
    }
    //Tiles Gen
    void GenerateTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(x * tileSize, 0, z * tileSize);
                TileType type = tileMap[x, z];

                if (type == TileType.Street)
                    SpawnStreetTile(x, z, position);
                else
                    SpawnTile(type, position);
            }
        }
    }

    void SpawnTile(TileType type, Vector3 position)
    {
        GameObject[] prefabPool = type == TileType.Building ? buildingPrefabs : parkPrefabs;
        GameObject prefab = prefabPool[Random.Range(0, prefabPool.Length)];
        GameObject instance = Instantiate(prefab, position, Quaternion.identity, transform);
        AddFoliage(instance.transform, position);
    }

    void SpawnStreetTile(int x, int z, Vector3 position)
    {
        //has north
        bool hasN = z < height - 1 && tileMap[x, z + 1] == TileType.Street;
        //has South
        bool hasS = z > 0 && tileMap[x, z - 1] == TileType.Street;
        //has East
        bool hasE = x < width - 1 && tileMap[x + 1, z] == TileType.Street;
        //Has West
        bool hasW = x > 0 && tileMap[x - 1, z] == TileType.Street;

        GameObject selected = roadStraight;
        Quaternion rot = Quaternion.identity;

        if ((hasN && hasS && hasE && hasW))
        {
            selected = roadIntersection;
        }
        else if ((hasN && hasS && (hasE || hasW)) || (hasE && hasW && (hasN || hasS)))
        {
            selected = roadTJunction;
        }
        else if ((hasN && hasE) || (hasE && hasS) || (hasS && hasW) || (hasW && hasN))
        {
            //Align Streets
            selected = roadCorner;
            if (hasN && hasE) rot = Quaternion.Euler(0, 0, 0);
            else if (hasE && hasS) rot = Quaternion.Euler(0, 90, 0);
            else if (hasS && hasW) rot = Quaternion.Euler(0, 180, 0);
            else if (hasW && hasN) rot = Quaternion.Euler(0, 270, 0);
        }
        else if ((hasN && hasS) || (hasE && hasW))
        {
            selected = roadStraight;
            if (hasE && hasW) rot = Quaternion.Euler(0, 90, 0);
        }

        Instantiate(selected, position, rot, transform);
    }

    TileType GetRandomTileType()
    {
        return Random.value > 0.5f ? TileType.Building : TileType.Park;
    }

    void AddFoliage(Transform parent, Vector3 basePosition)
    {
        int count = Random.Range(2, 6);
        for (int i = 0; i < count; i++)
        {
            GameObject foliage = foliagePrefabs[Random.Range(0, foliagePrefabs.Length)];
            Vector3 pos = basePosition + new Vector3(Random.Range(1f, tileSize - 1), 0, Random.Range(1f, tileSize - 1));
            Instantiate(foliage, pos, Quaternion.Euler(0, Random.Range(0, 360), 0), parent);
        }
    }
}
