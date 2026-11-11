using UnityEngine;

public class GemSpawner : MonoBehaviour
{
    [Header("Gem Settings")]
    public GameObject gemPrefab;
    public int gemAmount = 20;

    [Header("Spawn Area")]
    public float mapSize = 13f;
    public float spawnHeight = 0.7f;

    [Header("Distance From Objects")]
    public float safeDistance = 2f;

    // Layer ของสิ่งที่ Gem ห้ามเกิดใกล้
    public LayerMask obstacleLayer;

    [Header("Spawn Attempts")]
    public int maxAttemptsPerGem = 30;

    void Start()
    {
        SpawnGems();
    }

    void SpawnGems()
    {
        int spawned = 0;

        while (spawned < gemAmount)
        {
            bool foundPosition = false;

            for (int attempt = 0; attempt < maxAttemptsPerGem; attempt++)
            {
                Vector3 spawnPosition = new Vector3(
                    Random.Range(-mapSize, mapSize),
                    spawnHeight,
                    Random.Range(-mapSize, mapSize)
                );

                // ตรวจสอบว่ามีสิ่งกีดขวางในรัศมี safeDistance หรือไม่
                bool hasObstacle = Physics.CheckSphere(
                    spawnPosition,
                    safeDistance,
                    obstacleLayer
                );

                if (!hasObstacle)
                {
                    Instantiate(
                        gemPrefab,
                        spawnPosition,
                        Quaternion.identity
                    );

                    spawned++;
                    foundPosition = true;

                    break;
                }
            }

            // ป้องกัน while loop ค้าง
            // ถ้าหาตำแหน่งไม่ได้
            if (!foundPosition)
            {
                Debug.LogWarning(
                    "GemSpawner: หาตำแหน่ง Spawn ที่ว่างไม่ได้"
                );

                break;
            }
        }
    }
}