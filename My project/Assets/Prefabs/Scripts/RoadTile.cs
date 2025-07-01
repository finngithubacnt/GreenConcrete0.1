using UnityEngine;

public class RoadTile : MonoBehaviour
{
    public GameObject[] roadPrefabs; // e.g. straight, T-junction, corner
    public GameObject[] buildingPrefabs;
    public Transform roadSpawnPoint;
    public Transform[] buildingSpots;

    void Start()
    {
        // Spawn a random road prefab
        GameObject road = Instantiate(roadPrefabs[Random.Range(0, roadPrefabs.Length)],
            roadSpawnPoint.position, roadSpawnPoint.rotation, transform);

        // For each building spot, randomly decide to spawn a building
        foreach (Transform spot in buildingSpots)
        {
            if (Random.value < 0.6f) // 60% chance to spawn building
            {
                GameObject building = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
                Instantiate(building, spot.position, spot.rotation, transform);
            }
        }
    }
}
