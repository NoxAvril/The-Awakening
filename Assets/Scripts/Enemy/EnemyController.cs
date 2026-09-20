using UnityEngine;

public class EnemyController : MonoBehaviour, IEnemyController
{
    [Header("Blueprint Reference")]
    [SerializeField]
    private EnemyData enemyData;


    [Header("Runtime Stats")]
    public string enemyName;

    public float maxHealth;

    [HideInInspector]
    public float currentHealth;

    public float moveSpeed;

    public float collisionDamage;

    public bool hasRangedAttack;

    public float rangeDamage;

    public float attackInterval;

    public float range;


    [Header("EXP")]
    public int baseExpReward;

    public float currentExpReward;


    // ============================================================
    // ORIGINAL BASE STATS
    // ============================================================

    private float originalMaxHealth;

    private float originalMoveSpeed;

    private float originalCollisionDamage;

    private float originalRangeDamage;

    private float originalAttackInterval;

    private float originalRange;

    private int originalBaseExpReward;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        InitializeStats();

        EnemyUpgradeManager.RegisterEnemy(this);
    }


    // ============================================================
    // INITIALIZE STATS
    // ============================================================

    private void InitializeStats()
    {
        if (enemyData != null)
        {
            enemyName =
                enemyData.enemyName;


            // ====================================================
            // LOAD ORIGINAL DATA
            // ====================================================

            originalMaxHealth =
                enemyData.maxHealth;

            originalMoveSpeed =
                enemyData.moveSpeed;

            originalCollisionDamage =
                enemyData.collisionDamage;

            originalRangeDamage =
                enemyData.rangeDamage;

            originalAttackInterval =
                enemyData.attackInterval;

            originalRange =
                enemyData.range;

            originalBaseExpReward =
                enemyData.baseExpReward;


            // ====================================================
            // SET RUNTIME STATS TO ORIGINAL VALUES
            // ====================================================

            maxHealth =
                originalMaxHealth;

            currentHealth =
                maxHealth;

            moveSpeed =
                originalMoveSpeed;

            collisionDamage =
                originalCollisionDamage;

            hasRangedAttack =
                enemyData.hasRangedAttack;

            rangeDamage =
                originalRangeDamage;

            attackInterval =
                originalAttackInterval;

            range =
                originalRange;

            baseExpReward =
                originalBaseExpReward;
        }
        else
        {
            // ====================================================
            // FALLBACK VALUES
            // ====================================================

            originalMaxHealth =
                10f;

            originalMoveSpeed =
                1f;

            originalCollisionDamage =
                1f;

            originalRangeDamage =
                1f;

            originalAttackInterval =
                1f;

            originalRange =
                1f;

            originalBaseExpReward =
                1;


            // ====================================================
            // SET RUNTIME VALUES
            // ====================================================

            maxHealth =
                originalMaxHealth;

            currentHealth =
                maxHealth;

            moveSpeed =
                originalMoveSpeed;

            collisionDamage =
                originalCollisionDamage;

            rangeDamage =
                originalRangeDamage;

            attackInterval =
                originalAttackInterval;

            range =
                originalRange;

            baseExpReward =
                originalBaseExpReward;
        }


        UpdateExpReward();
    }


    // ============================================================
    // RESET TO ORIGINAL DATA
    // ============================================================

    public void ResetToOriginalStats()
    {
        maxHealth =
            originalMaxHealth;


        currentHealth =
            maxHealth;


        moveSpeed =
            originalMoveSpeed;


        collisionDamage =
            originalCollisionDamage;


        rangeDamage =
            originalRangeDamage;


        attackInterval =
            originalAttackInterval;


        range =
            originalRange;


        baseExpReward =
            originalBaseExpReward;


        UpdateExpReward();
    }


    // ============================================================
    // ELITE SCALING
    // ============================================================

    public void ApplyScaling(
        float baseMultiplier,
        float excludedMultiplier)
    {
        // ========================================================
        // BASE STATS
        // ========================================================

        maxHealth *=
            baseMultiplier;


        currentHealth =
            maxHealth;


        collisionDamage *=
            baseMultiplier;


        rangeDamage *=
            baseMultiplier;


        // ========================================================
        // EXCLUDED STATS
        // ========================================================

        moveSpeed *=
            excludedMultiplier;


        range *=
            excludedMultiplier;


        // Lower attack interval = faster attacks.
        attackInterval /=
            excludedMultiplier;


        // EXP does not receive Elite scaling here.
        // Elite EXP is handled by the global multiplier.
        UpdateExpReward();
    }


    // ============================================================
    // NORMAL STAT UPGRADES
    // ============================================================

    public void ApplyStatUpgrade(
        EnemyStatType statType,
        float amount)
    {
        if (amount <= 0f)
            return;


        switch (statType)
        {
            // ====================================================
            // HEALTH
            // ====================================================

            case EnemyStatType.Health:

                maxHealth +=
                    amount;


                currentHealth +=
                    amount;


                EnemyHealth health =
                    GetComponent<EnemyHealth>();


                if (health != null)
                {
                    health.AddHealth(
                        amount
                    );
                }


                break;


            // ====================================================
            // COLLISION DAMAGE
            // ====================================================

            case EnemyStatType.CollisionDamage:

                collisionDamage +=
                    amount;

                break;


            // ====================================================
            // RANGE DAMAGE
            // ====================================================

            case EnemyStatType.RangeDamage:

                rangeDamage +=
                    amount;

                break;


            // ====================================================
            // MOVE SPEED
            // ====================================================

            case EnemyStatType.MoveSpeed:

                moveSpeed +=
                    amount;

                break;


            // ====================================================
            // RANGE
            // ====================================================

            case EnemyStatType.Range:

                range +=
                    amount;

                break;


            // ====================================================
            // ATTACK INTERVAL
            // ====================================================

            case EnemyStatType.AttackInterval:

                // Positive upgrade amount means
                // faster attacks.

                attackInterval =
                    Mathf.Max(
                        0.01f,
                        attackInterval - amount
                    );

                break;
        }
    }


    // ============================================================
    // UPDATE EXP REWARD
    // ============================================================

    public void UpdateExpReward()
    {
        float individualMultiplier =
            enemyData != null
                ? enemyData.expMultiplier
                : 1f;


        currentExpReward =
            baseExpReward *
            individualMultiplier *
            EnemyUpgradeManager
                .GetGlobalExpMultiplier();
    }


    // ============================================================
    // GET EXP REWARD
    // ============================================================

    public float GetExpReward()
    {
        return currentExpReward;
    }
}


// ================================================================
// ENEMY CONTROLLER INTERFACE
// ================================================================

public interface IEnemyController
{
    void ApplyScaling(
        float baseMultiplier,
        float excludedMultiplier
    );
}