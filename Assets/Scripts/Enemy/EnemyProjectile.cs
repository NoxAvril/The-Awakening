using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector2 lockedDirection;
    private float speed;
    private float damage;
    private bool isAoE;
    private float aoeRadius;
    private bool isInitialized = false;

    public void Setup(Vector2 dir, float moveSpeed, float dmg, bool aoe = false, float radius = 0f)
    {
        lockedDirection = dir.normalized;
        speed = moveSpeed;
        damage = dmg;
        isAoE = aoe;
        aoeRadius = radius;

        // Visual rotation towards trajectory
        if (lockedDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(lockedDirection.y, lockedDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        isInitialized = true;
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Direct position movement guarantees movement regardless of local transform space
        transform.position += (Vector3)(lockedDirection * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (isAoE)
        {
            ExplodeAoE();
        }
        else if (collision.TryGetComponent<PlayerHealth>(out var health))
        {
            health.TakeDamage(damage, lockedDirection);
        }

        Destroy(gameObject);
    }

    private void ExplodeAoE()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") && hit.TryGetComponent<PlayerHealth>(out var health))
            {
                Vector2 pushDir = (hit.transform.position - transform.position).normalized;
                health.TakeDamage(damage, pushDir);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (isAoE)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }
    }
}