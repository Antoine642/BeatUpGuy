using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject[] breakablePrefabs;
    [SerializeField] private Transform[] spawnPoints;
    
    [SerializeField] private int maxEnemies = 3;
    [SerializeField] private int maxBreakables = 2;
    
    [Header("Timing")]
    [SerializeField] private float initialSpawnDelay = 2f;
    [SerializeField] private float enemySpawnInterval = 5f;
    [SerializeField] private float breakableSpawnInterval = 8f;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<GameObject> activeBreakables = new List<GameObject>();

    void Start()
    {
        // Initial spawn after delay
        StartCoroutine(InitialSpawn());
        // Start regular spawn cycles
        StartCoroutine(SpawnEnemiesRoutine());
        StartCoroutine(SpawnBreakablesRoutine());
    }

    private IEnumerator InitialSpawn()
    {
        yield return new WaitForSeconds(initialSpawnDelay);
        SpawnRandomEnemies();
        SpawnRandomBreakables();
    }

    private IEnumerator SpawnEnemiesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(enemySpawnInterval);
            CleanupDestroyedObjects(activeEnemies);
            SpawnRandomEnemies();
        }
    }

    private IEnumerator SpawnBreakablesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(breakableSpawnInterval);
            CleanupDestroyedObjects(activeBreakables);
            SpawnRandomBreakables();
        }
    }

    private void SpawnRandomEnemies()
    {
        CleanupDestroyedObjects(activeEnemies);
        
        int toSpawn = maxEnemies - activeEnemies.Count;
        if (toSpawn <= 0) return;

        // Only spawn up to the max limit
        for (int i = 0; i < toSpawn; i++)
        {
            if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

            // Select random enemy prefab and spawn point
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Transform spawnPoint = GetRandomAvailableSpawnPoint();
            
            if (spawnPoint != null)
            {
                GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                activeEnemies.Add(enemy);
            }
        }
    }

    private void SpawnRandomBreakables()
    {
        CleanupDestroyedObjects(activeBreakables);
        
        int toSpawn = maxBreakables - activeBreakables.Count;
        if (toSpawn <= 0) return;

        // Only spawn up to the max limit
        for (int i = 0; i < toSpawn; i++)
        {
            if (breakablePrefabs.Length == 0 || spawnPoints.Length == 0) return;

            // Select random breakable prefab and spawn point
            GameObject breakablePrefab = breakablePrefabs[Random.Range(0, breakablePrefabs.Length)];
            Transform spawnPoint = GetRandomAvailableSpawnPoint();
            
            if (spawnPoint != null)
            {
                GameObject breakable = Instantiate(breakablePrefab, spawnPoint.position, Quaternion.identity);
                activeBreakables.Add(breakable);
            }
        }
    }

    private Transform GetRandomAvailableSpawnPoint()
    {
        // Create a shuffled list of spawn points
        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        for (int i = 0; i < availablePoints.Count; i++)
        {
            int randomIndex = Random.Range(i, availablePoints.Count);
            Transform temp = availablePoints[i];
            availablePoints[i] = availablePoints[randomIndex];
            availablePoints[randomIndex] = temp;
        }

        // Check for available points without any objects nearby
        foreach (Transform point in availablePoints)
        {
            // Check if the area is clear (you can adjust the radius as needed)
            Collider[] colliders = Physics.OverlapSphere(point.position, 1.5f);
            if (colliders.Length == 0)
            {
                return point;
            }
        }

        // If no ideal point found, just return a random one
        return spawnPoints.Length > 0 ? spawnPoints[Random.Range(0, spawnPoints.Length)] : null;
    }

    private void CleanupDestroyedObjects(List<GameObject> objectList)
    {
        objectList.RemoveAll(obj => obj == null);
    }
}