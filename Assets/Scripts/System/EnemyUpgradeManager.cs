using UnityEngine;
using System.Collections.Generic;

public class EnemyUpgradeManager : MonoBehaviour
{
    public static EnemyUpgradeManager Instance;

    // ============================================================
    // GLOBAL EXP MULTIPLIER
    // ============================================================

    private static float globalExpMultiplier = 1.0f;

    private const float STAT_UPGRADE_EXP_INCREASE = 0.10f;
    private const float ELITE_UPGRADE_EXP_INCREASE = 0.15f;


    // ============================================================
    // ENEMY STAT BONUSES
    // ============================================================

    private class EnemyBonusData
    {
        public Dictionary<EnemyStatType, float> bonuses =
            new Dictionary<EnemyStatType, float>();
    }

    private static Dictionary<string, EnemyBonusData>
        enemyBonuses =
        new Dictionary<string, EnemyBonusData>();


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // ============================================================
    // RESET ALL RUN UPGRADES
    // ============================================================

    public static void ResetRunUpgrades()
    {
        // Reset all enemy stat bonuses.
        enemyBonuses.Clear();

        // Reset global EXP multiplier.
        globalExpMultiplier = 1.0f;

        Debug.Log(
            "[EnemyUpgradeManager] " +
            "All run enemy upgrades have been reset."
        );
    }


    // ============================================================
    // GET GLOBAL EXP MULTIPLIER
    // ============================================================

    public static float GetGlobalExpMultiplier()
    {
        return globalExpMultiplier;
    }


    // ============================================================
    // NORMAL STAT UPGRADE = +10% EXP
    // ============================================================

    public static void AddStatUpgradeExp()
    {
        globalExpMultiplier +=
            STAT_UPGRADE_EXP_INCREASE;

        Debug.Log(
            "[EnemyUpgradeManager] " +
            "Stat upgrade EXP multiplier is now " +
            globalExpMultiplier.ToString("0.00") +
            "x"
        );

        UpdateAllEnemyExp();
    }


    // ============================================================
    // ELITE UPGRADE = +15% EXP
    // ============================================================

    public static void AddEliteUpgradeExp()
    {
        globalExpMultiplier +=
            ELITE_UPGRADE_EXP_INCREASE;

        Debug.Log(
            "[EnemyUpgradeManager] " +
            "Elite upgrade EXP multiplier is now " +
            globalExpMultiplier.ToString("0.00") +
            "x"
        );

        UpdateAllEnemyExp();
    }


    // ============================================================
    // UPDATE CURRENT ENEMY EXP
    // ============================================================

    private static void UpdateAllEnemyExp()
    {
        EnemyController[] enemies =
            Object.FindObjectsByType<EnemyController>(
                FindObjectsSortMode.None
            );

        foreach (EnemyController enemy in enemies)
        {
            if (enemy == null)
                continue;

            enemy.UpdateExpReward();
        }
    }


    // ============================================================
    // REGISTER NEW ENEMY
    // ============================================================

    public static void RegisterEnemy(
        EnemyController enemy)
    {
        if (enemy == null)
            return;

        ApplySavedBonuses(enemy);

        enemy.UpdateExpReward();
    }


    // ============================================================
    // APPLY NORMAL STAT UPGRADE
    // ============================================================

    public static void ApplyEnemyStatUpgrade(
        EnemyUpgradeData upgrade)
    {
        if (upgrade == null)
            return;

        if (upgrade.targetEnemyPrefab == null)
        {
            Debug.LogWarning(
                "[EnemyUpgradeManager] " +
                "Target Enemy Prefab is missing."
            );

            return;
        }

        string enemyKey =
            upgrade.targetEnemyPrefab.name;


        // Create data for this enemy.
        if (!enemyBonuses.ContainsKey(enemyKey))
        {
            enemyBonuses[enemyKey] =
                new EnemyBonusData();
        }


        EnemyBonusData data =
            enemyBonuses[enemyKey];


        // Create this stat entry.
        if (!data.bonuses.ContainsKey(
            upgrade.targetStat))
        {
            data.bonuses[
                upgrade.targetStat
            ] = 0f;
        }


        // Add the stat bonus for this run.
        data.bonuses[
            upgrade.targetStat
        ] += upgrade.statIncreaseAmount;


        // ========================================================
        // +10% EXP
        // ========================================================

        AddStatUpgradeExp();


        // ========================================================
        // APPLY TO EXISTING ENEMIES
        // ========================================================

        EnemyController[] enemies =
            Object.FindObjectsByType<EnemyController>(
                FindObjectsSortMode.None
            );

        foreach (EnemyController enemy in enemies)
        {
            if (enemy == null)
                continue;

            if (GetEnemyKey(enemy) != enemyKey)
                continue;

            enemy.ApplyStatUpgrade(
                upgrade.targetStat,
                upgrade.statIncreaseAmount
            );
        }
    }


    // ============================================================
    // APPLY RUN BONUSES TO NEW ENEMIES
    // ============================================================

    private static void ApplySavedBonuses(
        EnemyController enemy)
    {
        if (enemy == null)
            return;

        string enemyKey =
            GetEnemyKey(enemy);


        if (!enemyBonuses.ContainsKey(enemyKey))
            return;


        EnemyBonusData data =
            enemyBonuses[enemyKey];


        foreach (
            KeyValuePair<
                EnemyStatType,
                float
            > bonus in data.bonuses)
        {
            enemy.ApplyStatUpgrade(
                bonus.Key,
                bonus.Value
            );
        }
    }


    // ============================================================
    // GET ENEMY KEY
    // ============================================================

    private static string GetEnemyKey(
        EnemyController enemy)
    {
        string objectName =
            enemy.gameObject.name;

        objectName =
            objectName.Replace(
                "(Clone)",
                ""
            ).Trim();

        return objectName;
    }
}