using UnityEngine;
using System.Collections.Generic;

public class EnemyUpgradeManager : MonoBehaviour
{
    [Header("Upgrade Pool")]
    [SerializeField] private List<EnemyUpgradeData> masterUpgradePool = new List<EnemyUpgradeData>();

    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private List<EnemyData> allEnemyTypes = new List<EnemyData>();

    [Header("Card Settings")]
    [SerializeField] private int minCards = 3;
    [SerializeField] private int maxCards = 5;

    public List<EnemyUpgradeData> GetUpgradeChoices()
    {
        List<EnemyUpgradeData> validUpgrades = new List<EnemyUpgradeData>();

        for (int i = 0; i < masterUpgradePool.Count; i++)
        {
            EnemyUpgradeData upgrade = masterUpgradePool[i];
            if (upgrade == null) continue;

            // Skip "unlock new enemy" cards for enemies already unlocked
            if (upgrade.type == EnemyUpgradeType.NewEnemyType &&
                enemySpawner != null &&
                enemySpawner.unlockedEnemies.Contains(upgrade.enemyPrefabToUnlock))
            {
                continue;
            }

            validUpgrades.Add(upgrade);
        }

        if (validUpgrades.Count == 0)
            return validUpgrades;

        int cardCount = Random.Range(minCards, maxCards + 1);
        if (cardCount > validUpgrades.Count)
            cardCount = validUpgrades.Count;

        // Shuffle validUpgrades (Fisher-Yates)
        for (int i = 0; i < validUpgrades.Count; i++)
        {
            int randomIndex = Random.Range(i, validUpgrades.Count);
            EnemyUpgradeData temp = validUpgrades[i];
            validUpgrades[i] = validUpgrades[randomIndex];
            validUpgrades[randomIndex] = temp;
        }

        List<EnemyUpgradeData> selectedCards = new List<EnemyUpgradeData>();
        for (int i = 0; i < cardCount; i++)
        {
            selectedCards.Add(validUpgrades[i]);
        }

        return selectedCards;
    }

    public void ApplyUpgrade(EnemyUpgradeData upgrade)
    {
        if (upgrade == null) return;

        if (upgrade.type == EnemyUpgradeType.StatBoost)
        {
            ApplyStatBoost(upgrade);
        }
        else if (upgrade.type == EnemyUpgradeType.NewEnemyType)
        {
            if (enemySpawner != null && upgrade.enemyPrefabToUnlock != null)
            {
                enemySpawner.UnlockEnemy(upgrade.enemyPrefabToUnlock);
            }
        }
    }

    private void ApplyStatBoost(EnemyUpgradeData upgrade)
    {
        // Spawn rate lives on the spawner itself, not on EnemyData, so handle it separately
        if (upgrade.targetStat == EnemyStatType.SpawnRate)
        {
            if (enemySpawner != null)
            {
                if (upgrade.isMultiplier)
                {
                    enemySpawner.spawnInterval = enemySpawner.spawnInterval * upgrade.statIncreaseAmount;
                }
                else
                {
                    enemySpawner.spawnInterval = enemySpawner.spawnInterval - upgrade.statIncreaseAmount;
                }

                if (enemySpawner.spawnInterval < 0.1f)
                    enemySpawner.spawnInterval = 0.1f;
            }
            return;
        }

        for (int i = 0; i < allEnemyTypes.Count; i++)
        {
            EnemyData enemyData = allEnemyTypes[i];
            if (enemyData == null) continue;

            switch (upgrade.targetStat)
            {
                case EnemyStatType.MaxHealth:
                    enemyData.maxHealth = ApplyAmount(enemyData.maxHealth, upgrade);
                    break;

                case EnemyStatType.MoveSpeed:
                    enemyData.moveSpeed = ApplyAmount(enemyData.moveSpeed, upgrade);
                    break;

                case EnemyStatType.CollisionDamage:
                    enemyData.collisionDamage = ApplyAmount(enemyData.collisionDamage, upgrade);
                    break;

                case EnemyStatType.RangeDamage:
                    enemyData.rangeDamage = ApplyAmount(enemyData.rangeDamage, upgrade);
                    break;

                case EnemyStatType.AttackInterval:
                    enemyData.attackInterval = enemyData.attackInterval - upgrade.statIncreaseAmount;
                    if (enemyData.attackInterval < 0.1f)
                        enemyData.attackInterval = 0.1f;
                    break;
            }
        }
    }

    private float ApplyAmount(float baseValue, EnemyUpgradeData upgrade)
    {
        if (upgrade.isMultiplier)
        {
            return baseValue * upgrade.statIncreaseAmount;
        }
        else
        {
            return baseValue + upgrade.statIncreaseAmount;
        }
    }
}
