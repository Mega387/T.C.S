using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LogovoSpawner : MonoBehaviour
{
    [Header("Настройки логова")]
    [SerializeField] private Tilemap enemyTilemap;
    [SerializeField] private TileBase logovoTile;

    [Header("Префабы для спавна")]
    [SerializeField] private List<GameObject> enemyPrefabs;

    [Header("Настройки спавна")]
    [SerializeField] private float spawnRadius = 2f;
    [SerializeField] private float spawnCooldown = 30f;

    [Header("Визуализация")]
    [SerializeField] private bool showDebugRadius = true;
    [SerializeField] private Color debugRadiusColor = Color.red;

    public class LogovoState
    {
        public Vector3Int cellPosition;
        public Vector3 worldPosition;
        public float lastSpawnTime;
        public int spawnedEnemiesCount;
        public List<GameObject> activeEnemies;
        public bool isSpawningNow;

        public LogovoState(Vector3Int cellPos, Vector3 worldPos)
        {
            cellPosition = cellPos;
            worldPosition = worldPos;
            lastSpawnTime = -999f;
            spawnedEnemiesCount = 0;
            activeEnemies = new List<GameObject>();
            isSpawningNow = false;
        }
    }

    private Dictionary<Vector3Int, LogovoState> logovos = new Dictionary<Vector3Int, LogovoState>();
    private Transform playerTransform;
    private float checkInterval = 0.3f;
    private float lastCheckTime = 0f;
    private float refreshLogovosTimer = 0f;
    private float refreshInterval = 2f;
    private int findPlayerAttempts = 0;

    private void Start()
    {
        StartCoroutine(FindPlayerRepeatedly());
        StartCoroutine(WaitAndFindLogovos());
    }

    private IEnumerator FindPlayerRepeatedly()
    {
        while (playerTransform == null && findPlayerAttempts < 30)
        {
            FindPlayer();
            findPlayerAttempts++;
            yield return new WaitForSeconds(0.5f);
        }

        if (playerTransform != null)
        {
            Debug.Log($"LogovoSpawner: Игрок найден - {playerTransform.name}");
        }
        else
        {
            Debug.LogError("LogovoSpawner: Игрок не найден после 15 секунд!");
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("UnitPlayer");

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player == null)
        {
            Unit unit = FindObjectOfType<Unit>();
            if (unit != null)
            {
                player = unit.gameObject;
            }
        }

        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private IEnumerator WaitAndFindLogovos()
    {
        yield return new WaitForSeconds(1.5f);
        FindAllLogovos();

        yield return new WaitForSeconds(2f);
        FindAllLogovos();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        refreshLogovosTimer += Time.deltaTime;
        if (refreshLogovosTimer >= refreshInterval)
        {
            refreshLogovosTimer = 0f;
            FindAllLogovos();
        }

        if (Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time;
            CheckAllLogovos();
        }
    }

    private void FindAllLogovos()
    {
        if (enemyTilemap == null)
        {
            return;
        }

        if (logovoTile == null)
        {
            Debug.LogError("LogovoSpawner: logovoTile не назначен!");
            return;
        }

        BoundsInt bounds = enemyTilemap.cellBounds;

        if (bounds.size.x == 0 || bounds.size.y == 0)
        {
            return;
        }

        int foundCount = 0;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = enemyTilemap.GetTile(cell);

                if (tile == logovoTile && !logovos.ContainsKey(cell))
                {
                    Vector3 worldPos = enemyTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
                    logovos[cell] = new LogovoState(cell, worldPos);
                    foundCount++;
                    Debug.Log($"LogovoSpawner: Найдено логово #{foundCount} на позиции: {worldPos}");
                }
            }
        }

        if (foundCount > 0)
        {
            Debug.Log($"LogovoSpawner: Всего найдено логовов: {logovos.Count}");
        }
    }

    private void CheckAllLogovos()
    {
        List<Vector3Int> toRemove = new List<Vector3Int>();

        foreach (var kvp in logovos)
        {
            Vector3Int cell = kvp.Key;
            LogovoState logovo = kvp.Value;

            if (enemyTilemap.GetTile(cell) != logovoTile)
            {
                toRemove.Add(cell);
                continue;
            }

            logovo.activeEnemies.RemoveAll(e => e == null);

            if (logovo.isSpawningNow)
            {
                continue;
            }

            if (Time.time - logovo.lastSpawnTime < spawnCooldown)
            {
                continue;
            }

            float distance = Vector3.Distance(playerTransform.position, logovo.worldPosition);

            if (distance <= spawnRadius)
            {
                StartCoroutine(SpawnAllEnemiesCoroutine(logovo));
            }
        }

        foreach (Vector3Int cell in toRemove)
        {
            logovos.Remove(cell);
        }
    }

    private IEnumerator SpawnAllEnemiesCoroutine(LogovoState logovo)
    {
        logovo.isSpawningNow = true;

        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("LogovoSpawner: Нет префабов для спавна!");
            logovo.isSpawningNow = false;
            yield break;
        }

        int enemiesToSpawn = enemyPrefabs.Count;
        float distanceToPlayer = Vector3.Distance(playerTransform.position, logovo.worldPosition);

        Debug.Log($"LogovoSpawner: 🦇 СПАВН из логова {logovo.worldPosition}! Игрок на расстоянии {distanceToPlayer:F2}. Врагов: {enemiesToSpawn}");

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject enemyPrefab = enemyPrefabs[i % enemyPrefabs.Count];
            Vector3 spawnPosition = GetSpawnPositionAroundLogovo(logovo.worldPosition, i);

            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            logovo.activeEnemies.Add(newEnemy);
            logovo.spawnedEnemiesCount++;

            LogovoEnemyLink enemyLink = newEnemy.GetComponent<LogovoEnemyLink>();
            if (enemyLink == null)
            {
                enemyLink = newEnemy.AddComponent<LogovoEnemyLink>();
            }
            enemyLink.Initialize(logovo, this);

            yield return new WaitForSeconds(0.1f);
        }

        logovo.lastSpawnTime = Time.time;
        logovo.isSpawningNow = false;

        Debug.Log($"LogovoSpawner: ✅ Логово {logovo.worldPosition} заспавнило {enemiesToSpawn} врагов. Следующий спавн через {spawnCooldown} сек");
    }

    private Vector3 GetSpawnPositionAroundLogovo(Vector3 logovoWorldPos, int index)
    {
        float angle = (360f / (enemyPrefabs.Count + 1)) * index * Mathf.Deg2Rad;
        float radius = 1.2f;

        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;

        return logovoWorldPos + new Vector3(x, y, 0);
    }

    public void RemoveEnemyFromLogovo(LogovoState logovo, GameObject enemy)
    {
        if (logovo != null && logovo.activeEnemies.Contains(enemy))
        {
            logovo.activeEnemies.Remove(enemy);
        }
    }

    public void RefreshLogovos()
    {
        FindAllLogovos();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugRadius) return;

        if (logovos != null)
        {
            Gizmos.color = debugRadiusColor;
            foreach (var logovo in logovos.Values)
            {
                Gizmos.DrawWireSphere(logovo.worldPosition, spawnRadius);
            }
        }
    }
}