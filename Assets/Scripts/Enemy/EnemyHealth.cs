using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public EnemyData enemyData;

    private float currentHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (enemyData == null)
            return;
            
        currentHealth = enemyData.maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
