using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public EnemyData enemyData;

    private EnemyController enemyController;
    private EnemyStatus enemyStatus;

    private float damageCooldown = 0f;

    private PlayerHealth health;

    private Rigidbody2D rb;
    private Vector2 knockbackVelocity;


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        rb =
            GetComponent<Rigidbody2D>();

        enemyStatus =
            GetComponent<EnemyStatus>();

        enemyController =
            GetComponent<EnemyController>();


        // Find player.
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player =
                playerObject.transform;
        }


        // Find PlayerHealth.
        if (player != null)
        {
            health =
                player.GetComponent<PlayerHealth>();
        }
    }


    // ============================================================
    // FIXED UPDATE
    // ============================================================

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


        // ========================================================
        // KNOCKBACK
        // ========================================================

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


        // ========================================================
        // MOVEMENT SPEED
        // ========================================================

        // Use EnemyController runtime speed.
        // This includes normal MoveSpeed upgrades
        // and Elite scaling.

        float currentSpeed =
            enemyController.moveSpeed;


        // Apply status effects such as Slow/Freeze.
        if (enemyStatus != null)
        {
            currentSpeed *=
                enemyStatus.GetSpeedMultiplier();
        }


        // Completely stopped.
        if (currentSpeed <= 0f)
        {
            rb.linearVelocity =
                Vector2.zero;

            return;
        }


        // ========================================================
        // RANGED DISTANCE CHECK
        // ========================================================

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


        // ========================================================
        // CHASE PLAYER
        // ========================================================

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


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        if (damageCooldown > 0f)
        {
            damageCooldown -=
                Time.deltaTime;
        }
    }


    // ============================================================
    // KNOCKBACK
    // ============================================================

    public void ApplyKnockback(
        Vector2 force)
    {
        knockbackVelocity =
            force;
    }


    // ============================================================
    // COLLISION ENTER
    // ============================================================

    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        TryDamagePlayer(
            collision
        );
    }


    // ============================================================
    // COLLISION STAY
    // ============================================================

    private void OnCollisionStay2D(
        Collision2D collision)
    {
        TryDamagePlayer(
            collision
        );
    }


    // ============================================================
    // DAMAGE PLAYER
    // ============================================================

    private void TryDamagePlayer(
        Collision2D collision)
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
            Vector2 hitDirection;


            if (collision.contactCount > 0)
            {
                hitDirection =
                    collision
                        .GetContact(0)
                        .normal;
            }
            else
            {
                hitDirection =
                    (
                        target.transform.position -
                        transform.position
                    ).normalized;
            }


            // Use the upgraded runtime
            // collision damage.
            health.TakeDamage(
                enemyController.collisionDamage,
                hitDirection
            );


            damageCooldown =
                1f;
        }
    }
}