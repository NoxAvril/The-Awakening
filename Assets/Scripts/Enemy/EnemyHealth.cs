using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public EnemyData enemyData;
    [SerializeField] private GameObject expGemPrefab;

    private float currentHealth;
    private EnemyFollow enemyFollow;

    public static float GlobalExpBonusPercentage = 0f;

    void Start()
    {
        enemyFollow = GetComponent<EnemyFollow>();

        if (enemyData != null)
        {
            currentHealth = enemyData.maxHealth;
        }
        else
        {
            currentHealth = 10f;
        }
    }

    public void TakeDamage(float damage, Vector2 knockback = default)
    {
        currentHealth -= damage;

        if (knockback != Vector2.zero && enemyFollow != null)
        {
            enemyFollow.ApplyKnockback(knockback);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        SpawnExpGems();
        Destroy(gameObject);
    }

    private void SpawnExpGems()
    {
        if (expGemPrefab == null) return;

        int baseReward = enemyData != null ? enemyData.baseExpReward : 1;
        float individualMultiplier = enemyData != null ? enemyData.expMultiplier : 1f;

        float finalExpFloat = baseReward * individualMultiplier * (1f + GlobalExpBonusPercentage);
        int totalExp = Mathf.Max(1, Mathf.RoundToInt(finalExpFloat));

        ExpGem.DropExp(transform.position, totalExp, expGemPrefab);
    }
}