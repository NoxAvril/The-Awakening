using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public EnemyData enemyData;

    [Header("Movement")]
    public bool movementEnabled = true;

    private EnemyController enemyController;
    private EnemyStatus enemyStatus;

    private float damageCooldown = 0f;

    private PlayerHealth health;

    private Rigidbody2D rb;
    private Vector2 knockbackVelocity;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyStatus = GetComponent<EnemyStatus>();
        enemyController = GetComponent<EnemyController>();

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

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
        if (
            player == null ||
            enemyController == null ||
            rb == null
        )
        {
            return;
        }

        // BossManager can disable normal enemy movement
        // while keeping this script active for collision damage.
        if (!movementEnabled)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (knockbackVelocity.magnitude > 0.05f)
        {
            rb.MovePosition(
                rb.position +
                knockbackVelocity *
                Time.fixedDeltaTime
            );

            knockbackVelocity =
                Vector2.Lerp(
                    knockbackVelocity,
                    Vector2.zero,
                    15f *
                    Time.fixedDeltaTime
                );

            return;
        }

        float currentSpeed =
            enemyController.moveSpeed;

        if (enemyStatus != null)
        {
            currentSpeed *=
                enemyStatus.GetSpeedMultiplier();
        }

        if (currentSpeed <= 0f)
        {
            rb.linearVelocity =
                Vector2.zero;

            return;
        }

        float distanceToPlayer =
            Vector2.Distance(
                transform.position,
                player.position
            );

        if (
            enemyController.hasRangedAttack &&
            distanceToPlayer <=
            enemyController.range
        )
        {
            rb.linearVelocity =
                Vector2.zero;

            return;
        }

        Vector2 direction =
            (
                player.position -
                transform.position
            ).normalized;

        rb.MovePosition(
            rb.position +
            direction *
            currentSpeed *
            Time.fixedDeltaTime
        );
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision);
    }

    private void TryDamagePlayer(Collision2D collision)
    {
        GameObject target =
            collision.gameObject;

        if (
            !target.CompareTag("Player") ||
            damageCooldown > 0f ||
            enemyController == null
        )
        {
            return;
        }

        if (health == null)
        {
            health =
                target.GetComponent<PlayerHealth>();
        }

        if (health != null)
        {
            Vector2 hitDirection =
                (
                    target.transform.position -
                    transform.position
                ).normalized;

            health.TakeDamage(
                enemyController.collisionDamage,
                hitDirection
            );

            damageCooldown = 1f;
        }
    }
}