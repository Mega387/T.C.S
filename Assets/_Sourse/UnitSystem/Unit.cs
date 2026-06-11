using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Unit : MonoBehaviour
{
    [Header("Основные характеристики")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    [SerializeField] private string unitTag = "UnitPlayer";

    [Header("Атака")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private GameObject[] attackAnimationObjects;
    [SerializeField] private float animationFrameDelay = 0.15f;

    [Header("Регенерация")]
    [SerializeField] private float regenDelay = 20f;
    [SerializeField] private float regenAmount = 10f;
    [SerializeField] private float regenInterval = 10f;

    [Header("Движение")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Визуал")]
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Slider healthBar;

    [HideInInspector] public bool isSelected = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 targetPosition;
    private bool hasTarget = false;
    private TilemapRestriction restrictionSystem;
    private Unit currentTarget;
    private bool canAttack = true;

    private float lastDamageTime = -999f;
    private Coroutine regenCoroutine;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0;
        spriteRenderer.color = normalColor;
        restrictionSystem = FindObjectOfType<TilemapRestriction>();

        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        DisableAllAnimationObjects();
    }

    private void Update()
    {
        spriteRenderer.color = isSelected ? selectedColor : normalColor;

        if (currentTarget == null && canAttack)
        {
            FindTarget();
        }

        if (currentTarget != null && canAttack)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.transform.position);
            if (distance <= attackRange)
            {
                StartCoroutine(FullAttackCycle());
            }
        }
    }

    private void FixedUpdate()
    {
        if (hasTarget && canAttack)
        {
            Vector2 currentPos = rb.position;
            Vector2 direction = (targetPosition - currentPos).normalized;
            Vector2 nextPosition = currentPos + direction * moveSpeed * Time.fixedDeltaTime;

            if (restrictionSystem != null && !restrictionSystem.IsPositionWalkable(nextPosition))
            {
                rb.velocity = Vector2.zero;
                return;
            }

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(nextPosition, 0.3f);
            bool willPushOtherUnit = false;

            foreach (Collider2D hit in hitColliders)
            {
                Unit otherUnit = hit.GetComponent<Unit>();
                if (otherUnit != null && otherUnit != this)
                {
                    Vector2 otherNextPosition = otherUnit.transform.position + (Vector3)direction * moveSpeed * Time.fixedDeltaTime;
                    if (restrictionSystem != null && !restrictionSystem.IsPositionWalkable(otherNextPosition))
                    {
                        willPushOtherUnit = true;
                        break;
                    }
                }
            }

            if (willPushOtherUnit)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            rb.velocity = direction * moveSpeed;

            if (Vector2.Distance(currentPos, targetPosition) < 0.1f)
            {
                rb.velocity = Vector2.zero;
                hasTarget = false;
            }
        }
    }

    public void MoveTo(Vector2 position)
    {
        if (restrictionSystem != null && !restrictionSystem.IsPositionWalkable(position))
        {
            position = restrictionSystem.FindNearestWalkablePosition(position);
        }

        targetPosition = position;
        hasTarget = true;
    }

    private void FindTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        float closestDistance = attackRange;
        Unit closestUnit = null;

        foreach (Collider2D collider in colliders)
        {
            Unit unit = collider.GetComponent<Unit>();
            if (unit != null && unit.unitTag != this.unitTag && unit.currentHealth > 0)
            {
                float distance = Vector2.Distance(transform.position, unit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestUnit = unit;
                }
            }
        }

        currentTarget = closestUnit;
    }

    private IEnumerator FullAttackCycle()
    {
        canAttack = false;
        rb.velocity = Vector2.zero;

        for (int i = 0; i < attackAnimationObjects.Length; i++)
        {
            if (attackAnimationObjects[i] != null)
            {
                attackAnimationObjects[i].SetActive(true);
            }

            yield return new WaitForSeconds(animationFrameDelay);

            if (attackAnimationObjects[i] != null)
            {
                attackAnimationObjects[i].SetActive(false);
            }
        }

        if (currentTarget != null && currentTarget.currentHealth > 0)
        {
            currentTarget.TakeDamage(damage);
        }

        float cooldown = 1f / attackRate;
        yield return new WaitForSeconds(cooldown);

        canAttack = true;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}