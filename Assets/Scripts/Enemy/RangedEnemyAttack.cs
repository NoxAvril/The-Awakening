using UnityEngine;

public class RangedEnemyAttack : MonoBehaviour
{
    private EnemyFollow enemyFollow;
    private float attackTimer;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 6f;

    [Header("AoE Settings (For Mage / Special Ranged)")]
    [SerializeField] private bool isAoEAttack = false;
    [SerializeField] private float aoeRadius = 2f;

    private void Start()
    {
        enemyFollow = GetComponent<EnemyFollow>();
        
        if (enemyFollow != null && enemyFollow.enemyData != null)
        {
            attackTimer = enemyFollow.enemyData.attackInterval;
        }
    }

    private void Update()
    {
        if (enemyFollow == null || enemyFollow.player == null || enemyFollow.enemyData == null) return;
        if (!enemyFollow.enemyData.hasRangedAttack) return;

        float distanceToPlayer = Vector2.Distance(transform.position, enemyFollow.player.position);

        if (distanceToPlayer <= enemyFollow.enemyData.range)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                ShootAtPlayer();
                attackTimer = enemyFollow.enemyData.attackInterval;
            }
        }
    }

    private void ShootAtPlayer()
    {
        if (projectilePrefab == null) return;

        Vector2 direction = (enemyFollow.player.position - transform.position).normalized;
        if (direction == Vector2.zero) direction = Vector2.right;

        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        if (proj.TryGetComponent<EnemyProjectile>(out var enemyProj))
        {
            enemyProj.Setup(direction, projectileSpeed, enemyFollow.enemyData.rangeDamage, isAoEAttack, aoeRadius);
        }
    }
}