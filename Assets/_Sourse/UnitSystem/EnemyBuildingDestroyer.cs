using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class EnemyBuildingDestroyer : MonoBehaviour
{
    private Unit unit;
    private Tilemap enemyTilemap;
    private Vector3Int targetCell;
    private float lastAttackTime;
    private bool isAttacking;

    private static Dictionary<Vector3Int, int> globalHits = new Dictionary<Vector3Int, int>();

    void Start()
    {
        unit = GetComponent<Unit>();
        if (unit == null)
        {
            Debug.LogError("EnemyBuildingDestroyer: Unit компонент не найден!");
            return;
        }
        FindTilemap();
    }

    void FindTilemap()
    {
        GameObject obj = GameObject.Find("EnemyTilemap");
        if (obj != null)
        {
            enemyTilemap = obj.GetComponent<Tilemap>();
            Debug.Log("EnemyBuildingDestroyer: EnemyTilemap найден");
        }
        else
        {
            Debug.LogWarning("EnemyBuildingDestroyer: EnemyTilemap НЕ НАЙДЕН!");
        }
    }

    void Update()
    {
        if (unit == null) return;
        if (enemyTilemap == null) return;
        if (isAttacking) return;
        if (unit.IsPlayingAnimation()) return;

        FindNearestBuilding();

        if (targetCell != Vector3Int.zero)
        {
            Vector3 targetWorld = enemyTilemap.CellToWorld(targetCell) + new Vector3(0.5f, 0.5f, 0);
            float dist = Vector2.Distance(transform.position, targetWorld);
            float attackRange = unit.GetBuildingDestroyRange();

            if (dist <= attackRange)
            {
                if (Time.time - lastAttackTime >= unit.GetBuildingDestroyDelay())
                {
                    StartCoroutine(AttackBuilding());
                }
            }
        }
    }

    void FindNearestBuilding()
    {
        if (enemyTilemap == null) return;

        BoundsInt bounds = enemyTilemap.cellBounds;
        float bestDist = unit.GetBuildingDestroyRange();
        Vector3Int bestCell = Vector3Int.zero;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (enemyTilemap.GetTile(cell) != null)
                {
                    Vector3 worldPos = enemyTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
                    float dist = Vector2.Distance(transform.position, worldPos);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestCell = cell;
                    }
                }
            }
        }

        if (bestCell != targetCell)
        {
            targetCell = bestCell;
        }
    }

    IEnumerator AttackBuilding()
    {
        isAttacking = true;
        unit.StopMoving();

        GameObject[] animations = unit.GetAnimationObjects();
        foreach (GameObject anim in animations)
        {
            if (anim != null) anim.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            if (anim != null) anim.SetActive(false);
        }

        int hits = 0;
        if (globalHits.ContainsKey(targetCell))
        {
            hits = globalHits[targetCell];
        }

        hits++;
        globalHits[targetCell] = hits;

        int needed = unit.GetHitsToDestroy();
        Debug.Log($"Удар по {targetCell}: {hits}/{needed}");

        if (hits >= needed)
        {
            enemyTilemap.SetTile(targetCell, null);
            globalHits.Remove(targetCell);
            targetCell = Vector3Int.zero;
        }

        lastAttackTime = Time.time;
        isAttacking = false;
    }
}