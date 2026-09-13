using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public EnemyData enemyData;

    private float damageCooldown = 0f;
    private PlayerHealth health;
    private EnemyStatus enemyStatus;

    private Rigidbody2D rb;
    private Vector2 knockbackVelocity;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyStatus = GetComponent<EnemyStatus>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        if (player != null)
        {
            health = player.GetComponent<PlayerHealth>();
        }
    }

    private void FixedUpdate()
    {
        if (player == null || enemyData == null) return;

        // 1. Handle Knockback decay
        if (knockbackVelocity.magnitude > 0.05f)
        {
            rb.MovePosition(rb.position + knockbackVelocity * Time.fixedDeltaTime);
            knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, 15f * Time.fixedDeltaTime);
            return;
        }

        // 2. Dynamic speed calculation (reflects Freeze/Slow status changes live)
        float currentSpeed = enemyData.moveSpeed;
        if (enemyStatus != null)
        {
            currentSpeed *= enemyStatus.GetSpeedMultiplier();
        }

        // If frozen or completely stopped by status, freeze physics movement
        if (currentSpeed <= 0f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 3. Ranged distance check
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (enemyData.hasRangedAttack && distanceToPlayer <= enemyData.range)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 4. Standard chase movement
        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * currentSpeed * Time.fixedDeltaTime);
    }

    private void Update()
    {
        if (damageCooldown > 0f)
        {
            damageCooldown -= Time.deltaTime;
        }
    }

    public void ApplyKnockback(Vector2 force)
    {
        knockbackVelocity = force;
    }

    private void OnCollisionEnter2D(Collision2D collision) => TryDamagePlayer(collision);
    private void OnCollisionStay2D(Collision2D collision) => TryDamagePlayer(collision);

    private void TryDamagePlayer(Collision2D collision)
    {
        GameObject target = collision.gameObject;

        if (!target.CompareTag("Player") || damageCooldown > 0f || enemyData == null)
            return;

        if (health != null)
        {
            Vector2 hitDirection = collision.contactCount > 0 
                ? collision.GetContact(0).normal 
                : (Vector2)(target.transform.position - transform.position).normalized;

            health.TakeDamage(enemyData.collisionDamage, hitDirection);
            damageCooldown = 1f;
        }
    }
}