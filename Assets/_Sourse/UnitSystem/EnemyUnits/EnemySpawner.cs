using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxEnemies = 20;
    [SerializeField] private int enemiesPerSpawn = 1;

    [Header("Волны")]
    [SerializeField] private bool useWaves = true;
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float waveCooldown = 30f;
    [SerializeField] private int currentWave = 0;
    [SerializeField] private bool infiniteWaves = true;

    private List<EnemyUnit> activeEnemies = new List<EnemyUnit>();
    private bool isSpawning = true;
    private int enemiesSpawnedInWave = 0;

    private void Start()
    {
        if (useWaves)
        {
            StartCoroutine(WaveRoutine());
        }
        else
        {
            StartCoroutine(SpawnRoutine());
        }
    }

    private IEnumerator WaveRoutine()
    {
        while (isSpawning)
        {
            currentWave++;
            enemiesSpawnedInWave = 0;

            for (int i = 0; i < enemiesPerWave; i++)
            {
                if (activeEnemies.Count >= maxEnemies)
                {
                    yield return new WaitUntil(() => activeEnemies.Count < maxEnemies);
                }

                SpawnEnemy();
                enemiesSpawnedInWave++;
                yield return new WaitForSeconds(spawnInterval);
            }

            yield return new WaitForSeconds(waveCooldown);

            if (!infiniteWaves)
            {
                isSpawning = false;
            }
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            if (activeEnemies.Count < maxEnemies)
            {
                for (int i = 0; i < enemiesPerSpawn; i++)
                {
                    SpawnEnemy();
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;
        if (spawnPoints.Length == 0) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        EnemyUnit enemy = enemyObj.GetComponent<EnemyUnit>();

        if (enemy != null)
        {
            activeEnemies.Add(enemy);
            StartCoroutine(RemoveFromListWhenDead(enemy));
        }
    }

    private IEnumerator RemoveFromListWhenDead(EnemyUnit enemy)
    {
        yield return new WaitUntil(() => enemy == null || enemy.gameObject == null);
        activeEnemies.Remove(enemy);
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void StartSpawning()
    {
        isSpawning = true;

        if (useWaves)
            StartCoroutine(WaveRoutine());
        else
            StartCoroutine(SpawnRoutine());
    }

    public int GetActiveEnemyCount()
    {
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies.Count;
    }

    public void KillAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        activeEnemies.Clear();
    }
}