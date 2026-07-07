using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyUnit : MonoBehaviour
{
    [Header("Основные характеристики")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Атака по юнитам")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRangeUnit = 1.5f;

    [Header("Атака по постройкам")]
    [SerializeField] private float attackRangeBuilding = 1.2f;
    [SerializeField] private float buildingAttackDelay = 5f;
    [SerializeField] private float stoneWallAttackDelay = 20f;

    [Header("Движение")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.3f;
    [SerializeField] private float pathUpdateInterval = 0.5f;
    [SerializeField] private bool ignoreRestrictions = false;

    [Header("Визуал")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Color enemyColor = Color.red;

    [Header("Анимация атаки")]
    [SerializeField] private GameObject[] attackAnimationObjects;
    [SerializeField] private float animationFrameDelay = 0.15f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private TilemapRestrictionEnemy restrictionSystem;

    private Unit playerUnitTarget;
    private Vector3 currentBuildingTargetPos;
    private Vector3Int currentBuildingTargetCell;
    private bool isAttackingBuilding = false;
    private bool isAttacking = false;
    private bool isMovingToUnit = false;

    private BuildingFireManager fireManager;
    private DemolitionSystem demolitionSystem;
    private Tilemap buildingsTilemap;
    private TileBase kingTile;

    private List<Vector2> currentPath = new List<Vector2>();
    private int currentPathIndex = 0;
    private float lastPathUpdate = 0f;

    private Vector2 lastPosition;
    private float stuckTimer = 0f;
    private float lastBuildingAttackTime = -999f;
    private float nextUnitAttackTime = 0f;

    private Vector3Int kingPosition;
    private bool kingPositionFound = false;

    private float minDistanceToBuilding = float.MaxValue;
    private Vector3Int nearestBuildingCell;

    private float stuckToUnitTimer = 0f;
    private Vector3 lastUnitPosition;

    private float stuckToBuildingTimer = 0f;
    private Vector3 lastBuildingTargetPos;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = enemyColor;
        rb.gravityScale = 0;

        restrictionSystem = FindObjectOfType<TilemapRestrictionEnemy>();
        fireManager = FindObjectOfType<BuildingFireManager>();
        demolitionSystem = FindObjectOfType<DemolitionSystem>();

        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (fireManager != null)
        {
            buildingsTilemap = fireManager.buildingsTilemap;
            kingTile = fireManager.kingTile;
            FindKingPositionOnce();
        }

        DisableAllAnimationObjects();
        StartCoroutine(FindTargetRoutine());

        lastPosition = transform.position;

        FindNearestBuildingTile();
        if (nearestBuildingCell != Vector3Int.zero)
        {
            SetBuildingTarget(nearestBuildingCell);
        }
    }

    private void FindKingPositionOnce()
    {
        if (buildingsTilemap == null || kingTile == null) return;

        BoundsInt bounds = buildingsTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = buildingsTilemap.GetTile(cell);
                if (tile == kingTile)
                {
                    kingPosition = cell;
                    kingPositionFound = true;
                    return;
                }
            }
        }
    }

    private void Update()
    {
        CheckIfStuck();
        CheckIfStuckToUnit();
        CheckIfStuckToBuilding();

        if (isAttacking) return;

        FindNearestBuildingTile();
        FindNearestPlayerUnit();

        float distanceToUnit = playerUnitTarget != null ? Vector2.Distance(transform.position, playerUnitTarget.transform.position) : float.MaxValue;
        float distanceToBuilding = minDistanceToBuilding;

        bool isStuckToUnit = (playerUnitTarget != null && stuckToUnitTimer > 1.5f);
        bool isStuckToBuildingTarget = (isAttackingBuilding && stuckToBuildingTimer > 2f);

        if (isStuckToBuildingTarget)
        {
            currentPath.Clear();
            stuckToBuildingTimer = 0f;
            FindBestTarget();
            return;
        }

        if (playerUnitTarget != null && distanceToUnit <= attackRangeUnit + 1f && !isStuckToUnit)
        {
            isMovingToUnit = true;
            isAttackingBuilding = false;

            if (distanceToUnit <= attackRangeUnit)
            {
                rb.velocity = Vector2.zero;
                if (Time.time >= nextUnitAttackTime)
                {
                    StartCoroutine(AttackUnit());
                }
            }
            else
            {
                MoveToUnit();
            }
            return;
        }

        if (isStuckToUnit)
        {
            if (nearestBuildingCell != Vector3Int.zero)
            {
                isMovingToUnit = false;
                if (!isAttackingBuilding || currentBuildingTargetCell != nearestBuildingCell)
                {
                    SetBuildingTarget(nearestBuildingCell);
                    stuckToUnitTimer = 0f;
                }
            }
        }

        if (nearestBuildingCell != Vector3Int.zero && !isMovingToUnit)
        {
            if (!isAttackingBuilding || currentBuildingTargetCell != nearestBuildingCell)
            {
                SetBuildingTarget(nearestBuildingCell);
            }

            if (isAttackingBuilding && currentBuildingTargetCell != null && buildingsTilemap != null)
            {
                if (buildingsTilemap.GetTile(currentBuildingTargetCell) == null)
                {
                    isAttackingBuilding = false;
                    currentPath.Clear();
                    FindBestTarget();
                    return;
                }

                float distance = Vector2.Distance(transform.position, currentBuildingTargetPos);

                if (distance <= attackRangeBuilding)
                {
                    rb.velocity = Vector2.zero;
                    if (Time.time - lastBuildingAttackTime >= buildingAttackDelay)
                    {
                        StartCoroutine(AttackBuilding());
                    }
                }
                else
                {
                    MoveAlongPath();
                }
            }
            return;
        }

        if (playerUnitTarget != null)
        {
            isMovingToUnit = true;
            MoveToUnit();
            return;
        }

        if (isAttackingBuilding && currentBuildingTargetCell != null)
        {
            MoveAlongPath();
        }
    }

    private void CheckIfStuckToBuilding()
    {
        if (!isAttackingBuilding || currentBuildingTargetCell == null)
        {
            stuckToBuildingTimer = 0f;
            return;
        }

        Vector2 currentPos = transform.position;
        Vector2 targetPos = currentBuildingTargetPos;

        float distanceToTarget = Vector2.Distance(currentPos, targetPos);

        if (distanceToTarget <= attackRangeBuilding)
        {
            stuckToBuildingTimer = 0f;
            return;
        }

        float movedDistance = Vector2.Distance(lastBuildingTargetPos, currentPos);

        if (movedDistance < 0.05f)
        {
            stuckToBuildingTimer += Time.deltaTime;
        }
        else
        {
            stuckToBuildingTimer = 0f;
        }

        lastBuildingTargetPos = currentPos;
    }

    private void FixedUpdate()
    {
        if (isAttacking) return;
    }

    private void FindNearestBuildingTile()
    {
        if (buildingsTilemap == null)
        {
            minDistanceToBuilding = float.MaxValue;
            nearestBuildingCell = Vector3Int.zero;
            return;
        }

        BoundsInt bounds = buildingsTilemap.cellBounds;
        float closestDistance = float.MaxValue;
        Vector3Int closestCell = Vector3Int.zero;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = buildingsTilemap.GetTile(cell);

                if (tile != null)
                {
                    Vector3 worldPos = buildingsTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
                    float dist = Vector2.Distance(transform.position, worldPos);

                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestCell = cell;
                    }
                }
            }
        }

        minDistanceToBuilding = closestDistance;
        nearestBuildingCell = closestCell;
    }

    private void MoveToUnit()
    {
        if (playerUnitTarget == null) return;

        Vector2 direction = (playerUnitTarget.transform.position - transform.position).normalized;

        if (!ignoreRestrictions && restrictionSystem != null)
        {
            Vector2 nextPos = rb.position + direction * 0.3f;
            if (!restrictionSystem.IsPositionWalkable(nextPos))
            {
                direction = GetAvoidanceDirection(direction);
            }
        }

        rb.velocity = direction * moveSpeed;
    }

    private void CheckIfStuck()
    {
        if (ignoreRestrictions) return;

        Vector2 currentPos = transform.position;
        float movedDistance = Vector2.Distance(lastPosition, currentPos);

        if (movedDistance < 0.03f && currentPath.Count > 0 && !isMovingToUnit)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 2f)
            {
                UpdatePathToTarget();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = currentPos;
    }

    private void CheckIfStuckToUnit()
    {
        if (playerUnitTarget == null)
        {
            stuckToUnitTimer = 0f;
            return;
        }

        if (!isMovingToUnit)
        {
            stuckToUnitTimer = 0f;
            return;
        }

        float distanceToUnit = Vector2.Distance(transform.position, playerUnitTarget.transform.position);

        if (distanceToUnit <= attackRangeUnit)
        {
            stuckToUnitTimer = 0f;
            return;
        }

        float enemyMovement = Vector2.Distance(lastPosition, transform.position);

        if (enemyMovement < 0.05f && distanceToUnit > attackRangeUnit)
        {
            stuckToUnitTimer += Time.deltaTime;
        }
        else if (enemyMovement > 0.05f)
        {
            stuckToUnitTimer -= Time.deltaTime;
            stuckToUnitTimer = Mathf.Max(0, stuckToUnitTimer);
        }
    }

    private void MoveAlongPath()
    {
        if (currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            if (Time.time - lastPathUpdate > pathUpdateInterval)
            {
                UpdatePathToTarget();
            }
            return;
        }

        Vector2 targetPoint = currentPath[currentPathIndex];
        Vector2 direction = (targetPoint - rb.position).normalized;

        if (Vector2.Distance(rb.position, targetPoint) < 0.2f)
        {
            currentPathIndex++;
            if (currentPathIndex >= currentPath.Count)
            {
                rb.velocity = Vector2.zero;
                return;
            }
            targetPoint = currentPath[currentPathIndex];
            direction = (targetPoint - rb.position).normalized;
        }

        if (!ignoreRestrictions && restrictionSystem != null)
        {
            Vector2 nextPos = rb.position + direction * 0.3f;
            if (!restrictionSystem.IsPositionWalkable(nextPos))
            {
                UpdatePathToTarget();
                return;
            }
        }

        rb.velocity = direction * moveSpeed;

        if (Time.time - lastPathUpdate > pathUpdateInterval)
        {
            UpdatePathToTarget();
        }
    }

    private Vector2 GetAvoidanceDirection(Vector2 desiredDirection)
    {
        float bestScore = -1f;
        Vector2 bestDirection = desiredDirection;

        for (int i = 0; i < 12; i++)
        {
            float angle = (360f / 12f) * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 testDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector2 testPos = rb.position + testDirection * 0.4f;

            if (restrictionSystem != null && restrictionSystem.IsPositionWalkable(testPos))
            {
                float score = Vector2.Dot(testDirection, desiredDirection);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = testDirection;
                }
            }
        }

        return bestDirection;
    }

    private void UpdatePathToTarget()
    {
        if (currentBuildingTargetCell == null) return;

        lastPathUpdate = Time.time;

        Vector2 start = rb.position;
        Vector2 target = currentBuildingTargetPos;

        if (Vector2.Distance(start, target) < 0.5f)
        {
            currentPath.Clear();
            return;
        }

        currentPath = FindPath(start, target);
        currentPathIndex = 0;
    }

    private List<Vector2> FindPath(Vector2 start, Vector2 target)
    {
        List<Vector2> path = new List<Vector2>();

        if (Vector2.Distance(start, target) < 1.5f)
        {
            path.Add(target);
            return path;
        }

        Vector2 direction = (target - start).normalized;
        Vector2 currentPoint = start;
        int maxSteps = 30;
        int steps = 0;

        while (Vector2.Distance(currentPoint, target) > 0.5f && steps < maxSteps)
        {
            Vector2 nextPoint = currentPoint + direction * 0.5f;

            if (!ignoreRestrictions && restrictionSystem != null && !restrictionSystem.IsPositionWalkable(nextPoint))
            {
                Vector2 newDirection = GetAvoidanceDirection(direction);
                if (newDirection == Vector2.zero)
                {
                    break;
                }
                direction = newDirection;
                nextPoint = currentPoint + direction * 0.5f;
            }

            path.Add(nextPoint);
            currentPoint = nextPoint;
            steps++;
        }

        if (path.Count > 0 && Vector2.Distance(path[path.Count - 1], target) > 0.5f)
        {
            path.Add(target);
        }

        return path;
    }

    private IEnumerator FindTargetRoutine()
    {
        while (true)
        {
            FindBestTarget();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void FindBestTarget()
    {
        FindNearestPlayerUnit();
        FindNearestBuildingTile();

        float distanceToUnit = playerUnitTarget != null ? Vector2.Distance(transform.position, playerUnitTarget.transform.position) : float.MaxValue;
        float distanceToBuilding = minDistanceToBuilding;

        if (playerUnitTarget != null && distanceToUnit <= attackRangeUnit + 1f)
        {
            isAttackingBuilding = false;
            return;
        }

        if (nearestBuildingCell != Vector3Int.zero)
        {
            SetBuildingTarget(nearestBuildingCell);
            return;
        }

        if (playerUnitTarget != null)
        {
            isAttackingBuilding = false;
            return;
        }

        if (buildingsTilemap != null)
        {
            if (kingPositionFound)
            {
                Vector3Int buildingInPath = FindBuildingBlockingPathToKing();
                if (buildingInPath != Vector3Int.zero)
                {
                    SetBuildingTarget(buildingInPath);
                    return;
                }
            }

            Vector3Int nearestNonKingBuilding = FindNearestNonKingBuilding();
            if (nearestNonKingBuilding.x != 0 || nearestNonKingBuilding.y != 0)
            {
                SetBuildingTarget(nearestNonKingBuilding);
                return;
            }

            if (kingPositionFound)
            {
                SetBuildingTarget(kingPosition);
                return;
            }
        }
    }

    private Vector3Int FindBuildingBlockingPathToKing()
    {
        if (buildingsTilemap == null || !kingPositionFound) return Vector3Int.zero;

        Vector2 currentPos = transform.position;
        Vector2 kingWorldPos = buildingsTilemap.CellToWorld(kingPosition) + new Vector3(0.5f, 0.5f, 0);

        Vector2 direction = (kingWorldPos - currentPos).normalized;
        Vector2 checkPos = currentPos;
        float checkDistance = 0.5f;
        float maxCheckDistance = Vector2.Distance(currentPos, kingWorldPos);

        List<Vector3Int> foundBuildings = new List<Vector3Int>();

        while (checkDistance < maxCheckDistance)
        {
            checkPos = currentPos + direction * checkDistance;
            Vector3Int cell = buildingsTilemap.WorldToCell(checkPos);
            TileBase tile = buildingsTilemap.GetTile(cell);

            if (tile != null)
            {
                if (!foundBuildings.Contains(cell))
                {
                    foundBuildings.Add(cell);
                }
            }

            checkDistance += 0.5f;
        }

        if (foundBuildings.Count > 0)
        {
            float closestDist = float.MaxValue;
            Vector3Int closestBuilding = Vector3Int.zero;

            foreach (Vector3Int building in foundBuildings)
            {
                Vector3 buildingPos = buildingsTilemap.CellToWorld(building) + new Vector3(0.5f, 0.5f, 0);
                float dist = Vector2.Distance(currentPos, buildingPos);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestBuilding = building;
                }
            }

            return closestBuilding;
        }

        return Vector3Int.zero;
    }

    private Vector3Int FindKingPosition()
    {
        if (buildingsTilemap == null || kingTile == null) return Vector3Int.zero;

        BoundsInt bounds = buildingsTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = buildingsTilemap.GetTile(cell);
                if (tile == kingTile)
                {
                    return cell;
                }
            }
        }

        return Vector3Int.zero;
    }

    private Vector3Int FindNearestNonKingBuilding()
    {
        if (buildingsTilemap == null) return Vector3Int.zero;

        BoundsInt bounds = buildingsTilemap.cellBounds;
        float minDistance = float.MaxValue;
        Vector3Int closestCell = Vector3Int.zero;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = buildingsTilemap.GetTile(cell);
                if (tile != null && tile != kingTile)
                {
                    Vector3 worldPos = buildingsTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
                    float dist = Vector2.Distance(transform.position, worldPos);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestCell = cell;
                    }
                }
            }
        }

        return closestCell;
    }

    private void FindNearestPlayerUnit()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        float searchRange = 20f;
        Unit closestUnit = null;
        float closestDistance = searchRange;

        foreach (Unit unit in allUnits)
        {
            if (unit == null) continue;
            float dist = Vector2.Distance(transform.position, unit.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestUnit = unit;
            }
        }

        playerUnitTarget = closestUnit;
    }

    private void SetBuildingTarget(Vector3Int cell)
    {
        if (currentBuildingTargetCell == cell && isAttackingBuilding) return;

        currentBuildingTargetCell = cell;
        currentBuildingTargetPos = buildingsTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
        isAttackingBuilding = true;
        isMovingToUnit = false;
        currentPath.Clear();
        stuckToUnitTimer = 0f;
        stuckToBuildingTimer = 0f;

        UpdatePathToTarget();
    }

    private IEnumerator AttackBuilding()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        for (int i = 0; i < attackAnimationObjects.Length; i++)
        {
            if (attackAnimationObjects[i] != null)
                attackAnimationObjects[i].SetActive(true);

            yield return new WaitForSeconds(animationFrameDelay);

            if (attackAnimationObjects[i] != null)
                attackAnimationObjects[i].SetActive(false);
        }

        if (buildingsTilemap != null && currentBuildingTargetCell != null)
        {
            TileBase buildingTile = buildingsTilemap.GetTile(currentBuildingTargetCell);

            if (buildingTile != null)
            {
                float attackDelay = buildingAttackDelay;

                if (fireManager != null)
                {
                    if (fireManager.IsStoneWall(buildingTile))
                    {
                        attackDelay = stoneWallAttackDelay;
                    }

                    int currentStage = fireManager.AddFireStage(currentBuildingTargetCell, buildingTile);

                    if (currentStage == 6)
                    {
                        if (demolitionSystem != null)
                        {
                            demolitionSystem.DestroySingleBuilding(currentBuildingTargetCell);
                        }
                        else
                        {
                            buildingsTilemap.SetTile(currentBuildingTargetCell, null);
                        }

                        fireManager.ClearFire(currentBuildingTargetCell);
                        isAttackingBuilding = false;
                        currentBuildingTargetPos = Vector3.zero;
                        FindBestTarget();
                    }
                }

                lastBuildingAttackTime = Time.time;
                yield return new WaitForSeconds(attackDelay);
            }
            else
            {
                isAttackingBuilding = false;
                FindBestTarget();
            }
        }

        isAttacking = false;
    }

    private IEnumerator AttackUnit()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        for (int i = 0; i < attackAnimationObjects.Length; i++)
        {
            if (attackAnimationObjects[i] != null)
                attackAnimationObjects[i].SetActive(true);

            yield return new WaitForSeconds(animationFrameDelay);

            if (attackAnimationObjects[i] != null)
                attackAnimationObjects[i].SetActive(false);
        }

        if (playerUnitTarget != null && playerUnitTarget.gameObject.activeSelf)
        {
            playerUnitTarget.TakeDamage(attackDamage);
        }

        nextUnitAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (healthBar != null)
            healthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        DisableAllAnimationObjects();
        Destroy(gameObject);
    }

    private void DisableAllAnimationObjects()
    {
        foreach (GameObject obj in attackAnimationObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRangeUnit);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRangeBuilding);

        if (currentPath != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
        }

        if (kingPositionFound && buildingsTilemap != null)
        {
            Gizmos.color = Color.magenta;
            Vector3 kingWorldPos = buildingsTilemap.CellToWorld(kingPosition) + new Vector3(0.5f, 0.5f, 0);
            Gizmos.DrawWireCube(kingWorldPos, Vector3.one);
        }

        if (nearestBuildingCell != Vector3Int.zero && buildingsTilemap != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 buildingWorldPos = buildingsTilemap.CellToWorld(nearestBuildingCell) + new Vector3(0.5f, 0.5f, 0);
            Gizmos.DrawWireCube(buildingWorldPos, Vector3.one);
        }
    }
}