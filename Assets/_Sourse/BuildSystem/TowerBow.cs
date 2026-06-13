using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerBow : MonoBehaviour
{
    [Header("Настройки башни")]
    [SerializeField] private Tilemap buildingsTilemap;
    [SerializeField] private TileBase towerTile;

    [Header("Параметры стрельбы")]
    [SerializeField] private float range = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int damage = 20;

    [Header("Визуал")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private float arrowSpeed = 10f;
    [SerializeField] private float arrowLifetime = 4f;

    [Header("Настройки поиска")]
    [SerializeField] private float searchInterval = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    private Dictionary<Vector3Int, TowerState> towers = new Dictionary<Vector3Int, TowerState>();
    private float lastSearchTime = 0f;
    private float lastBuildingsCheckTime = 0f;
    private float buildingsCheckInterval = 2f;

    private class TowerState
    {
        public Vector3Int cellPosition;
        public Vector3 worldPosition;
        public float lastShootTime;
        public EnemyUnit currentTarget;

        public TowerState(Vector3Int cellPos, Vector3 worldPos)
        {
            cellPosition = cellPos;
            worldPosition = worldPos;
            lastShootTime = -999f;
            currentTarget = null;
        }
    }

    private void Start()
    {
        if (buildingsTilemap == null)
        {
            buildingsTilemap = FindObjectOfType<BuildingFireManager>()?.buildingsTilemap;
        }


        FindAllTowers();
    }

    private void Update()
    {
        if (buildingsTilemap == null) return;

        if (Time.time - lastBuildingsCheckTime >= buildingsCheckInterval)
        {
            lastBuildingsCheckTime = Time.time;
            FindAllTowers();
        }

        if (Time.time - lastSearchTime >= searchInterval)
        {
            lastSearchTime = Time.time;
            UpdateAllTowers();
        }
    }

    private void FindAllTowers()
    {
        if (buildingsTilemap == null || towerTile == null) return;

        BoundsInt bounds = buildingsTilemap.cellBounds;
        bool foundNew = false;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = buildingsTilemap.GetTile(cell);

                if (tile == towerTile && !towers.ContainsKey(cell))
                {
                    Vector3 worldPos = buildingsTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
                    towers[cell] = new TowerState(cell, worldPos);
                    foundNew = true;
                    Debug.Log($"Найдена вышка на позиции {worldPos}");
                }
            }
        }

        if (foundNew)
        {
            Debug.Log($"Всего вышк {towers.Count}");
        }
    }

    private void UpdateAllTowers()
    {
        List<Vector3Int> toRemove = new List<Vector3Int>();

        foreach (var kvp in towers)
        {
            Vector3Int cell = kvp.Key;
            TowerState tower = kvp.Value;

            if (buildingsTilemap.GetTile(cell) != towerTile)
            {
                toRemove.Add(cell);
                continue;
            }

            UpdateTower(tower);
        }

        foreach (Vector3Int cell in toRemove)
        {
            towers.Remove(cell);
        }
    }

    private void UpdateTower(TowerState tower)
    {
        if (Time.time - tower.lastShootTime < fireRate)
        {
            return;
        }

        if (tower.currentTarget == null || !IsEnemyInRange(tower, tower.currentTarget))
        {
            tower.currentTarget = FindNearestEnemy(tower);
        }

        if (tower.currentTarget != null)
        {
            ShootAtEnemy(tower, tower.currentTarget);
            tower.lastShootTime = Time.time;
        }
    }

    private EnemyUnit FindNearestEnemy(TowerState tower)
    {
        EnemyUnit[] allEnemies = FindObjectsOfType<EnemyUnit>();
        EnemyUnit nearest = null;
        float nearestDistance = range;

        foreach (EnemyUnit enemy in allEnemies)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(tower.worldPosition, enemy.transform.position);
            if (distance <= range && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private bool IsEnemyInRange(TowerState tower, EnemyUnit enemy)
    {
        if (enemy == null) return false;
        float distance = Vector2.Distance(tower.worldPosition, enemy.transform.position);
        return distance <= range;
    }

    private void ShootAtEnemy(TowerState tower, EnemyUnit target)
    {
        if (arrowPrefab == null) return;

        Vector3 spawnPos = tower.worldPosition;
        if (arrowSpawnPoint != null)
        {
            spawnPos = arrowSpawnPoint.position;
        }

        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

        ArrowProjectile projectile = arrow.GetComponent<ArrowProjectile>();
        if (projectile == null)
        {
            projectile = arrow.AddComponent<ArrowProjectile>();
        }

        projectile.Initialize(target.transform, damage, arrowSpeed, arrowLifetime);
    }

    public void RefreshTowers()
    {
        FindAllTowers();
    }

    private void OnDrawGizmosSelected()
    {
        if (towers != null)
        {
            Gizmos.color = Color.green;
            foreach (var tower in towers.Values)
            {
                Gizmos.DrawWireSphere(tower.worldPosition, range);
            }
        }
    }
}