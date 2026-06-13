using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Unit : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    [SerializeField] private string unitTag = "UnitPlayer";

    [Header("Атака по врагам")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 6f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Анимация атаки")]
    [SerializeField] private GameObject[] attackAnimationObjects;
    [SerializeField] private float animationFrameDelay = 0.1f;

    [Header("Регенерация")]
    [SerializeField] private float regenDelay = 20f;
    [SerializeField] private float regenAmount = 10f;
    [SerializeField] private float regenInterval = 10f;

    [Header("Движение")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float escapeForce = 3f;
    [SerializeField] private float escapeCheckInterval = 0.3f;
    [SerializeField] private float escapeDuration = 0.2f;

    [Header("Взуал")]
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Slider healthBar;

    [Header("Разрушение")]
    [SerializeField] private float buildingDestroyRange = 1.2f;
    [SerializeField] private float buildingDestroyDelay = 2f;
    [SerializeField] private int hitsToDestroy = 5;

    [HideInInspector] public bool isSelected = false;

    public Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 targetPosition;
    private bool hasTarget = false;
    private TilemapRestriction restrictionSystem;
    private EnemyUnit currentEnemyTarget;

    private float nextAttackTime = 0f;
    private bool isPlayingAnimation = false;

    private float lastDamageTime = -999f;
    private Coroutine regenCoroutine;

    private EnemyBuildingDestroyer buildingDestroyer;

    private Coroutine currentAttackCoroutine;

    private float lastEscapeCheck = 0f;
    private bool isEscaping = false;
    private Vector2 escapeDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.mass = 1f;
        rb.drag = 5f;
        rb.angularDrag = 5f;
        rb.gravityScale = 0;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = normalColor;
        restrictionSystem = FindObjectOfType<TilemapRestriction>();

        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        DisableAllAnimationObjects();

        buildingDestroyer = GetComponent<EnemyBuildingDestroyer>();
        if (buildingDestroyer == null)
            buildingDestroyer = gameObject.AddComponent<EnemyBuildingDestroyer>();
    }

    private void Update()
    {
        spriteRenderer.color = isSelected ? selectedColor : normalColor;

        CheckAndEscapeFromRestrictedTile();

        FindNearestEnemy();

        if (currentEnemyTarget != null && !isPlayingAnimation && !isEscaping && Time.time >= nextAttackTime)
        {
            float distance = Vector2.Distance(transform.position, currentEnemyTarget.transform.position);
            if (distance <= attackRange)
            {
                StartAttackEnemy();
                rb.velocity = Vector2.zero;
            }
            else
            {
                MoveTo(currentEnemyTarget.transform.position);
            }
        }
    }

    private void FixedUpdate()
    {
        if (isEscaping)
        {
            return;
        }

        if (hasTarget && !isPlayingAnimation)
        {
            Vector2 currentPos = rb.position;
            Vector2 direction = (targetPosition - currentPos).normalized;
            Vector2 nextPosition = currentPos + direction * moveSpeed * Time.fixedDeltaTime;

            if (restrictionSystem != null && !restrictionSystem.IsPositionWalkable(nextPosition))
            {
                rb.velocity = Vector2.zero;
                return;
            }

            rb.velocity = direction * moveSpeed;

            if (Vector2.Distance(currentPos, targetPosition) < 0.15f)
            {
                rb.velocity = Vector2.zero;
                hasTarget = false;
            }
        }
    }

    private void CheckAndEscapeFromRestrictedTile()
    {
        if (Time.time - lastEscapeCheck < escapeCheckInterval) return;
        lastEscapeCheck = Time.time;

        if (restrictionSystem == null) return;

        Vector2 currentPos = transform.position;

        if (!restrictionSystem.IsPositionWalkable(currentPos))
        {
            if (!isEscaping)
            {
                Vector2 nearestWalkable = restrictionSystem.FindNearestWalkablePosition(currentPos);
                escapeDirection = (nearestWalkable - currentPos).normalized;
                isEscaping = true;

                StopAllAttacks();
                StopMoving();

                StartCoroutine(StopEscaping());
            }
        }
        else
        {
            isEscaping = false;
        }
    }

    private IEnumerator StopEscaping()
    {
        float elapsed = 0f;
        Vector2 initialVelocity = escapeDirection * escapeForce;

        while (elapsed < escapeDuration)
        {
            float t = 1f - (elapsed / escapeDuration);
            rb.velocity = initialVelocity * t;
            elapsed += Time.deltaTime;
            yield return null;
        }

        isEscaping = false;
        rb.velocity = Vector2.zero;
    }

    private void FindNearestEnemy()
    {
        EnemyUnit[] enemies = FindObjectsOfType<EnemyUnit>();
        float closestDistance = attackRange;
        EnemyUnit closestEnemy = null;

        foreach (EnemyUnit enemy in enemies)
        {
            if (enemy == null) continue;
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestEnemy = enemy;
            }
        }

        currentEnemyTarget = closestEnemy;
    }

    public void MoveTo(Vector2 position)
    {
        if (isPlayingAnimation || isEscaping)
        {
            StopAllAttacks();
        }

        if (restrictionSystem != null && !restrictionSystem.IsPositionWalkable(position))
        {
            position = restrictionSystem.FindNearestWalkablePosition(position);
        }

        targetPosition = position;
        hasTarget = true;
    }

    public void StopAllAttacks()
    {
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
            currentAttackCoroutine = null;
        }

        isPlayingAnimation = false;

        DisableAllAnimationObjects();
        rb.velocity = Vector2.zero;
    }

    public void StopMoving()
    {
        if (rb != null) rb.velocity = Vector2.zero;
        hasTarget = false;
    }

    public GameObject[] GetAnimationObjects()
    {
        return attackAnimationObjects;
    }

    public void StartAttackEnemy()
    {
        if (currentAttackCoroutine != null) return;
        currentAttackCoroutine = StartCoroutine(AttackEnemy());
    }

    private IEnumerator AttackEnemy()
    {
        isPlayingAnimation = true;
        rb.velocity = Vector2.zero;
        StopMoving();

        DisableAllAnimationObjects();

        for (int i = 0; i < attackAnimationObjects.Length; i++)
        {
            if (attackAnimationObjects[i] != null)
            {
                attackAnimationObjects[i].SetActive(true);
                yield return new WaitForSeconds(animationFrameDelay);
                attackAnimationObjects[i].SetActive(false);
            }
        }

        if (currentEnemyTarget != null)
        {
            currentEnemyTarget.TakeDamage(damage);
        }

        isPlayingAnimation = false;
        currentAttackCoroutine = null;

        nextAttackTime = Time.time + attackCooldown;
    }

    private void DisableAllAnimationObjects()
    {
        foreach (GameObject obj in attackAnimationObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        lastDamageTime = Time.time;

        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }

        if (healthBar != null)
            healthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (regenCoroutine == null)
            {
                regenCoroutine = StartCoroutine(RegenerationRoutine());
            }
        }
    }

    private IEnumerator RegenerationRoutine()
    {
        float timeSinceLastDamage = Time.time - lastDamageTime;
        if (timeSinceLastDamage < regenDelay)
        {
            yield return new WaitForSeconds(regenDelay - timeSinceLastDamage);
        }

        while (currentHealth < maxHealth)
        {
            if (Time.time - lastDamageTime < regenInterval)
            {
                yield break;
            }

            currentHealth += regenAmount;
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            if (healthBar != null)
                healthBar.value = currentHealth;

            if (currentHealth >= maxHealth)
                break;

            yield return new WaitForSeconds(regenInterval);
        }

        regenCoroutine = null;
    }

    private void Die()
    {
        if (regenCoroutine != null)
            StopCoroutine(regenCoroutine);

        DisableAllAnimationObjects();
        Destroy(gameObject);
    }

    public bool IsPlayingAnimation()
    {
        return isPlayingAnimation;
    }

    public float GetBuildingDestroyDelay()
    {
        return buildingDestroyDelay;
    }

    public int GetHitsToDestroy()
    {
        return hitsToDestroy;
    }

    public float GetBuildingDestroyRange()
    {
        return buildingDestroyRange;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, buildingDestroyRange);
    }
}