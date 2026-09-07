using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public EnemyData enemyData;

    private float damageCooldown = 0f;
    private PlayerHealth health;

    private float speed;
    private float damage;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        if (player == null)
            return;

        health = player.GetComponent<PlayerHealth>();

        if (enemyData == null)
            return;

        speed = enemyData.moveSpeed;
        damage = enemyData.collisionDamage;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        // Move physics-based enemy toward the player using Rigidbody2D
        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
    }

    private void Update()
    {
        if (damageCooldown > 0f)
        {
            damageCooldown -= Time.deltaTime;
        }
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
        GameObject target = collision.gameObject;

        if (!target.CompareTag("Player"))
            return;

        if (damageCooldown > 0f)
            return;

        if (health != null)
        {
            Vector2 hitDirection;

            // Use exact contact point normal if available; fallback to normalized position difference
            if (collision.contactCount > 0)
            {
                hitDirection = collision.GetContact(0).normal;
            }
            else
            {
                hitDirection = (target.transform.position - transform.position).normalized;
            }

            health.TakeDamage(damage, hitDirection);
            damageCooldown = 1f;
        }
    }
}