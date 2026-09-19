using UnityEngine;

public class RangedEnemyAttack : MonoBehaviour
{
    private EnemyFollow enemyFollow;
    private EnemyController enemyController;

    private float attackTimer;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 6f;

    [Header("AoE Settings (For Mage / Special Ranged)")]
    [SerializeField] private bool isAoEAttack = false;
    [SerializeField] private float aoeRadius = 2f;


    private void Start()
    {
        enemyFollow =
            GetComponent<EnemyFollow>();

        enemyController =
            GetComponent<EnemyController>();

        if (enemyController != null)
        {
            attackTimer =
                enemyController.attackInterval;
        }
        else
        {
            attackTimer = 1f;
        }
    }


    private void Update()
    {
        if (
            enemyFollow == null ||
            enemyController == null ||
            enemyFollow.player == null
        )
        {
            return;
        }

        if (!enemyController.hasRangedAttack)
        {
            return;
        }

        float distanceToPlayer =
            Vector2.Distance(
                transform.position,
                enemyFollow.player.position
            );

        if (
            distanceToPlayer <=
            enemyController.range
        )
        {
            attackTimer -=
                Time.deltaTime;

            if (attackTimer <= 0f)
            {
                ShootAtPlayer();

                attackTimer =
                    Mathf.Max(
                        0.01f,
                        enemyController.attackInterval
                    );
            }
        }
    }


    private void ShootAtPlayer()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        if (enemyFollow.player == null)
        {
            return;
        }

        Vector2 direction =
            (
                enemyFollow.player.position -
                transform.position
            ).normalized;

        if (direction == Vector2.zero)
        {
            direction =
                Vector2.right;
        }

        GameObject projectile =
            Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.identity
            );

        if (
            projectile.TryGetComponent<EnemyProjectile>(
                out var enemyProjectile
            )
        )
        {
            enemyProjectile.Setup(
                direction,
                projectileSpeed,
                enemyController.rangeDamage,
                isAoEAttack,
                aoeRadius
            );
        }
    }
}