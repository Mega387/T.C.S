using System.Collections;
using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    private Transform target;
    private int damage;
    private float speed;
    private float lifetime;
    private bool isInitialized = false;

    public void Initialize(Transform targetTransform, int damageAmount, float arrowSpeed, float arrowLifetime)
    {
        target = targetTransform;
        damage = damageAmount;
        speed = arrowSpeed;
        lifetime = arrowLifetime;
        isInitialized = true;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!isInitialized) return;

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInitialized) return;

        EnemyUnit enemy = other.GetComponent<EnemyUnit>();
        if (enemy != null && target != null && other.transform == target)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}