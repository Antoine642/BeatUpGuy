using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject[] breakablePrefabs;
    [SerializeField] private Transform[] spawnPoints;
    
    [SerializeField] private int maxEnemies = 8;
    [SerializeField] private int maxBreakables = 4;    
    [Header("Timing")]
    [SerializeField] private float initialSpawnDelay = 15f;
    [SerializeField] private float enemySpawnInterval = 15f;
    [SerializeField] private float breakableSpawnInterval = 20f;
    [SerializeField] private float minSpawnDelay = 5f;
    [SerializeField] private float maxSpawnDelay = 10f;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<GameObject> activeBreakables = new List<GameObject>();

    void Start()
    {
        // Initial spawn after delay
        StartCoroutine(InitialSpawn());
        // Start regular spawn cycles
        StartCoroutine(SpawnEnemiesRoutine());
        StartCoroutine(SpawnBreakablesRoutine());
    }    private IEnumerator InitialSpawn()
    {
        yield return new WaitForSeconds(initialSpawnDelay);
        SpawnRandomEnemies();
        yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
        SpawnRandomBreakables();
    }private IEnumerator SpawnEnemiesRoutine()
    {
        // Initial delay before spawning starts - skip initial spawn since we have InitialSpawn
        yield return new WaitForSeconds(initialSpawnDelay + enemySpawnInterval);
        
        while (true)
        {
            // Main interval between spawn cycles
            yield return new WaitForSeconds(enemySpawnInterval);
            
            CleanupDestroyedObjects(activeEnemies);
            
            // Only spawn if we're under the maximum
            if (activeEnemies.Count < maxEnemies)
            {
                // Add a random delay before actually spawning
                yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
                SpawnRandomEnemies();
            }
        }
    }

    private IEnumerator SpawnBreakablesRoutine()
    {
        // Initial delay before spawning starts
        yield return new WaitForSeconds(initialSpawnDelay + 5f); // Extra delay for breakables
        
        while (true)
        {
            // Main interval between spawn cycles
            yield return new WaitForSeconds(breakableSpawnInterval);
            
            CleanupDestroyedObjects(activeBreakables);
            
            // Only spawn if we're under the maximum
            if (activeBreakables.Count < maxBreakables)
            {
                // Add a random delay before actually spawning
                yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
                SpawnRandomBreakables();
            }
        }
    }    private void SpawnRandomEnemies()
    {
        CleanupDestroyedObjects(activeEnemies);
        
        if (activeEnemies.Count >= maxEnemies) return;

        // Spawn only one enemy at a time
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        // Select random enemy prefab and spawn point
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform spawnPoint = GetRandomAvailableSpawnPoint();
        
        if (spawnPoint != null)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            activeEnemies.Add(enemy);
        }
    }    private void SpawnRandomBreakables()
    {
        CleanupDestroyedObjects(activeBreakables);
        
        if (activeBreakables.Count >= maxBreakables) return;

        // Spawn only one breakable at a time
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