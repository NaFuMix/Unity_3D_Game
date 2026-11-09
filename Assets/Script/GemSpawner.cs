
using UnityEngine;

public class GemSpawner : MonoBehaviour
{
    public GameObject gemPrefab;

    public int gemAmount = 20;

    public float mapSize = 13f;

    public float spawnHeight = 0.7f;

    void Start()
    {
        SpawnGems();
    }

    void SpawnGems()
    {
        for (int i = 0; i < gemAmount; i++)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(-mapSize, mapSize),
                spawnHeight,
                Random.Range(-mapSize, mapSize)
            );

            Instantiate(
                gemPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }
    }
}
