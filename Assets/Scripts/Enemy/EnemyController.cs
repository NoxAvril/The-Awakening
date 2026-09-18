using UnityEngine;

public class EnemyController : MonoBehaviour, IEnemyController
{
    [Header("Blueprint Reference")]
    [SerializeField] private EnemyData enemyData;

    [Header("Runtime Stats")]
    public string enemyName;
    public float maxHealth;
    [HideInInspector] public float currentHealth;
    public float moveSpeed;
    public float collisionDamage;
    public bool hasRangedAttack;
    public float rangeDamage;
    public float attackInterval;
    public float range;
    public int baseExpReward;

    private void Awake()
    {
        InitializeStats();
    }

    private void InitializeStats()
    {
        if (enemyData != null)
        {
            enemyName = enemyData.enemyName;
            maxHealth = enemyData.maxHealth;
            currentHealth = maxHealth;
            moveSpeed = enemyData.moveSpeed;
            collisionDamage = enemyData.collisionDamage;
            hasRangedAttack = enemyData.hasRangedAttack;
            rangeDamage = enemyData.rangeDamage;
            attackInterval = enemyData.attackInterval;
            range = enemyData.range;
            baseExpReward = enemyData.baseExpReward;
        }
    }

    // Called by EnemySpawner when spawned or upgraded
    public void ApplyScaling(float baseMultiplier, float excludedMultiplier)
    {
        // Base Stats (Health, Collision Damage, Range Damage)
        maxHealth *= baseMultiplier;
        currentHealth = maxHealth;
        collisionDamage *= baseMultiplier;
        rangeDamage *= baseMultiplier;

        // Excluded Stats (MoveSpeed, Range, Attack Interval)
        moveSpeed *= excludedMultiplier;
        range *= excludedMultiplier;
        attackInterval /= excludedMultiplier; // Faster attack frequency as interval scales down
    }
}

public interface IEnemyController
{
    void ApplyScaling(float baseMultiplier, float excludedMultiplier);
}