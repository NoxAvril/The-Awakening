using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("References")]
    public EnemyData enemyData;

    [SerializeField]
    private GameObject expGemPrefab;

    private EnemyController enemyController;
    private EnemyFollow enemyFollow;

    private float currentHealth;


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        enemyFollow =
            GetComponent<EnemyFollow>();

        enemyController =
            GetComponent<EnemyController>();


        // Use EnemyController's final runtime HP.
        // This includes Elite scaling if the enemy is Elite.
        if (enemyController != null)
        {
            currentHealth =
                enemyController.currentHealth;
        }
        else if (enemyData != null)
        {
            currentHealth =
                enemyData.maxHealth;
        }
        else
        {
            currentHealth = 10f;
        }
    }


    // ============================================================
    // TAKE DAMAGE
    // ============================================================

    public void TakeDamage(
        float damage,
        Vector2 knockback = default)
    {
        currentHealth -= damage;


        // Keep EnemyController synchronized.
        if (enemyController != null)
        {
            enemyController.currentHealth =
                currentHealth;
        }


        // Apply knockback.
        if (
            knockback != Vector2.zero &&
            enemyFollow != null
        )
        {
            enemyFollow.ApplyKnockback(
                knockback
            );
        }


        // Check for death.
        if (currentHealth <= 0f)
        {
            Die();
        }
    }


    // ============================================================
    // ADD HEALTH FROM NORMAL STAT UPGRADE
    // ============================================================

    public void AddHealth(float amount)
    {
        if (amount <= 0f)
            return;


        currentHealth += amount;


        // Keep EnemyController synchronized.
        if (enemyController != null)
        {
            enemyController.currentHealth =
                currentHealth;
        }
    }


    // ============================================================
    // DIE
    // ============================================================

    private void Die()
    {
        SpawnExpGems();

        Destroy(gameObject);
    }


    // ============================================================
    // SPAWN EXP GEMS
    // ============================================================

    private void SpawnExpGems()
    {
        if (expGemPrefab == null)
            return;


        // Original/base EXP from EnemyData.
        int baseReward =
            enemyData != null
                ? enemyData.baseExpReward
                : 1;


        // Individual EXP multiplier from EnemyData.
        float individualMultiplier =
            enemyData != null
                ? enemyData.expMultiplier
                : 1f;


        // Global multiplier from enemy upgrades.
        float globalMultiplier =
            EnemyUpgradeManager
                .GetGlobalExpMultiplier();


        // Final EXP:
        //
        // Base EXP
        // × Individual EXP Multiplier
        // × Global Upgrade Multiplier
        //
        float finalExpFloat =
            baseReward *
            individualMultiplier *
            globalMultiplier;


        // Round to nearest whole number.
        int totalExp =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    finalExpFloat
                )
            );


        // Drop EXP gems.
        ExpGem.DropExp(
            transform.position,
            totalExp,
            expGemPrefab
        );
    }
}