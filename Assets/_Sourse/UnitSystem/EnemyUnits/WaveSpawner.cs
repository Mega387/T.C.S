using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class WaveEnemy
{
    public GameObject enemyPrefab;
    public int totalCount;
}

[System.Serializable]
public class Wave
{
    public string waveName;
    public List<WaveEnemy> enemies;
}

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Tilemap enemyTilemap;
    [Header("Логова")]
    [SerializeField] private Tilemap lairTilemap;
    [SerializeField] private TileBase lairTile;
    [SerializeField] private Tilemap cemeteryTilemap;
    [SerializeField] private TileBase cemeteryTile;

    [Header("Настройки волн")]
    [SerializeField] private List<Wave> waves;
    [SerializeField] private float firstWaveDelay = 400f;
    [SerializeField] private float waveInterval = 200f;

    [Header("Бесконечные волны")]
    [SerializeField] private bool infiniteWavesAfterLast = true;
    [SerializeField] private float infiniteWaveInterval = 100f;

    [Header("Защита логова")]
    [SerializeField] private bool lairInvincible = true;

    [Header("Настройки спавна")]
    [SerializeField] private float spawnOffsetY = 0.5f;
    [SerializeField] private Transform fallbackSpawnPoint;
    [SerializeField] private float spawnDelayBetweenEnemies = 0.3f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI waveTimerText;
    [SerializeField] private Slider waveTimerSlider;
    [SerializeField] private TextMeshProUGUI waveNumberText;

    private int currentWaveIndex = 0;
    private int infiniteWaveNumber = 1;
    private List<EnemyUnit> activeEnemies = new List<EnemyUnit>();
    private bool isGameRunning = true;
    private List<Vector3> lairPositions = new List<Vector3>();

    private float currentTimerValue = 0f;
    private float currentMaxTimerValue = 0f;
    private bool isWaitingForWave = false;
    private bool isRespawning = false;
    private bool isSpawning = false;
    private Coroutine spawnWaveCoroutine;
    private bool isInfiniteMode = false;

    private void Start()
    {
        StartCoroutine(WaveScheduler());
    }

    private void Update()
    {
        if (isWaitingForWave && waveTimerText != null)
        {
            int seconds = Mathf.Max(0, Mathf.CeilToInt(currentTimerValue));
            waveTimerText.text = seconds.ToString();
        }

        if (isWaitingForWave && waveTimerSlider != null && currentMaxTimerValue > 0)
        {
            waveTimerSlider.value = currentTimerValue / currentMaxTimerValue;
        }
    }

    private void RefreshLairPositions()
    {
        lairPositions.Clear();

        if (lairTilemap == null || lairTile == null)
        {
            if (fallbackSpawnPoint != null)
                lairPositions.Add(fallbackSpawnPoint.position);
            else
                lairPositions.Add(transform.position);
            return;
        }

        BoundsInt bounds = lairTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (lairTilemap.GetTile(cell) == lairTile)
                {
                    Vector3 worldPos = lairTilemap.CellToWorld(cell);
                    worldPos += new Vector3(0.5f, 0.5f + spawnOffsetY, 0);
                    lairPositions.Add(worldPos);
                }
            }
        }

        if (lairPositions.Count == 0)
        {
            if (fallbackSpawnPoint != null)
                lairPositions.Add(fallbackSpawnPoint.position);
            else
                lairPositions.Add(transform.position);
        }
    }

    private IEnumerator WaveScheduler()
    {
        RefreshLairPositions();

        isWaitingForWave = true;
        currentTimerValue = firstWaveDelay;
        currentMaxTimerValue = firstWaveDelay;

        while (currentTimerValue > 0 && isGameRunning)
        {
            currentTimerValue -= Time.deltaTime;
            yield return null;
        }

        isWaitingForWave = false;

        if (waveTimerText != null)
            waveTimerText.text = "";

        if (waveTimerSlider != null)
            waveTimerSlider.value = 0;

        while (isGameRunning)
        {
            RefreshLairPositions();

            if (currentWaveIndex < waves.Count)
            {
                Wave currentWave = waves[currentWaveIndex];
                currentWaveIndex++;

                if (waveNumberText != null)
                    waveNumberText.text = $"Волна {currentWaveIndex}/{waves.Count}";

                yield return StartCoroutine(SpawnWave(currentWave));
            }
            else if (infiniteWavesAfterLast)
            {
                if (!isInfiniteMode)
                {
                    isInfiniteMode = true;
                    Debug.Log("Все волны пройдены! Начинаются бесконечные волны!");
                }

                if (waveNumberText != null)
                    waveNumberText.text = $"Бесконечная волна {infiniteWaveNumber}";

                Wave infiniteWave = CreateInfiniteWave();
                yield return StartCoroutine(SpawnWave(infiniteWave));
                infiniteWaveNumber++;
            }
            else
            {
                Debug.Log("Все волны пройдены!");
                yield break;
            }

            if (isGameRunning)
            {
                float interval = isInfiniteMode ? infiniteWaveInterval : waveInterval;

                isWaitingForWave = true;
                currentTimerValue = interval;
                currentMaxTimerValue = interval;

                while (currentTimerValue > 0 && isGameRunning)
                {
                    currentTimerValue -= Time.deltaTime;
                    yield return null;
                }

                isWaitingForWave = false;

                if (waveTimerText != null)
                    waveTimerText.text = "";

                if (waveTimerSlider != null)
                    waveTimerSlider.value = 0;
            }
        }
    }

    private Wave CreateInfiniteWave()
    {
        Wave infiniteWave = new Wave();
        infiniteWave.waveName = "Infinite";
        infiniteWave.enemies = new List<WaveEnemy>();

        if (waves.Count > 0 && waves[waves.Count - 1].enemies != null)
        {
            foreach (WaveEnemy enemy in waves[waves.Count - 1].enemies)
            {
                WaveEnemy newEnemy = new WaveEnemy();
                newEnemy.enemyPrefab = enemy.enemyPrefab;
                newEnemy.totalCount = Mathf.Max(1, enemy.totalCount + infiniteWaveNumber / 2);
                infiniteWave.enemies.Add(newEnemy);
            }
        }

        return infiniteWave;
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        isSpawning = true;

        if (lairPositions.Count == 0)
            RefreshLairPositions();

        int lairCount = lairPositions.Count;

        if (lairCount == 0)
        {
            isSpawning = false;
            yield break;
        }

        float maxPercentPerLair = 100f / lairCount;

        foreach (WaveEnemy waveEnemy in wave.enemies)
        {
            int maxPerLair = Mathf.FloorToInt(waveEnemy.totalCount * (maxPercentPerLair / 100f));

            if (maxPerLair < 1) maxPerLair = 1;

            int totalSpawned = 0;
            int[] spawnedPerLair = new int[lairCount];



            for (int i = 0; i < lairCount; i++)
            {
                int toSpawn = Mathf.Min(maxPerLair, waveEnemy.totalCount - totalSpawned);

                if (toSpawn <= 0) break;

                Vector3 spawnPoint = lairPositions[i];

                for (int j = 0; j < toSpawn; j++)
                {
                    GameObject enemyObj = Instantiate(waveEnemy.enemyPrefab, spawnPoint, Quaternion.identity);
                    EnemyUnit enemy = enemyObj.GetComponent<EnemyUnit>();

                    if (enemy != null)
                    {
                        activeEnemies.Add(enemy);
                        StartCoroutine(RemoveFromListWhenDead(enemy));
                    }

                    spawnedPerLair[i]++;
                    totalSpawned++;

                    yield return new WaitForSeconds(spawnDelayBetweenEnemies);
                }
            }

            if (totalSpawned < waveEnemy.totalCount)
            {
                Debug.Log($"Осталось не заспавнено {waveEnemy.totalCount - totalSpawned} {waveEnemy.enemyPrefab.name} (достигнут лимит на логово)");
            }
        }

        isSpawning = false;
    }

    private IEnumerator RemoveFromListWhenDead(EnemyUnit enemy)
    {
        yield return new WaitUntil(() => enemy == null || enemy.gameObject == null);
        activeEnemies.Remove(enemy);
    }

    public void RefreshLairPositionsExternal()
    {
        RefreshLairPositions();
    }

    public bool IsLairTile(Vector3Int cell)
    {
        if (lairTilemap == null || lairTile == null) return false;
        return lairTilemap.GetTile(cell) == lairTile;
    }

    public bool IsCemeteryTile(Vector3Int cell)
    {
        if (cemeteryTilemap == null || cemeteryTile == null) return false;
        return cemeteryTilemap.GetTile(cell) == cemeteryTile;
    }

    public bool CanDestroyLair()
    {
        return !lairInvincible;
    }

    public void GameEnded()
    {
        isGameRunning = false;

        if (waveTimerText != null)
            waveTimerText.text = "";

        if (waveTimerSlider != null)
            waveTimerSlider.gameObject.SetActive(false);
    }

    public int GetActiveEnemyCount()
    {
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies.Count;
    }

    public void KillAllEnemies()
    {
        foreach (EnemyUnit enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        activeEnemies.Clear();
    }

    public float GetTimeUntilNextWave()
    {
        if (!isWaitingForWave) return 0f;
        return currentTimerValue;
    }

    public bool IsWaitingForWave()
    {
        return isWaitingForWave;
    }

    public bool IsSpawning()
    {
        return isSpawning;
    }

    public Tilemap GetEnemyTilemap()
    {
        return enemyTilemap;
    }
}