using UnityEngine;

public class ParkTile : MonoBehaviour
{
    public GameObject[] trees;
    public int treeCount = 10;

    void Start()
    {
        for (int i = 0; i < treeCount; i++)
        {
            Vector3 pos = transform.position + new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
            Instantiate(trees[Random.Range(0, trees.Length)], pos, Quaternion.identity, transform);
        }
    }
}
