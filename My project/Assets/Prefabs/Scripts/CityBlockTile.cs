public class CityBlockTile : MonoBehaviour
{
    public GameObject[] buildingPrefabs;
    public Transform[] buildingSpots;

    void Start()
    {
        foreach (Transform spot in buildingSpots)
        {
            GameObject prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
            Instantiate(prefab, spot.position, spot.rotation, transform);
        }
    }
}
