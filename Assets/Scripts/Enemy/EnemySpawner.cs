using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Pools & References")]
    public List<GameObject> unlockedEnemies = new List<GameObject>();
    public GameObject startingEnemy;
    public Transform player;
    public Camera mainCamera;

    [Header("Spawn Settings")]
    public float spawnInterval = 1f;
    public float spawnMargin = 2f;

    private float spawnTimer;

    // Track upgrade data structures for scaling and elite management
    private Dictionary<string, int> eliteCountsPerEnemy = new Dictionary<string, int>();
    private Dictionary<string, EnemyUpgradeData> activeEnemyUpgrades = new Dictionary<string, EnemyUpgradeData>();

    void Start()
    {
        if (startingEnemy != null)
        {
            UnlockEnemy(startingEnemy);
        }
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0)
        {
            SpawnWave();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnWave()
    {
        if (unlockedEnemies.Count == 0) return;

        // 1. Pick a random unlocked base enemy prefab
        int randomIndex = Random.Range(0, unlockedEnemies.Count);
        GameObject enemyPrefab = unlockedEnemies[randomIndex];
        string enemyKey = enemyPrefab.name;

        // Get the active configuration/upgrades for this specific enemy type
        EnemyUpgradeData currentConfig = activeEnemyUpgrades.ContainsKey(enemyKey) ? activeEnemyUpgrades[enemyKey] : null;

        // 2. Spawn Base Enemy
        Vector2 baseSpawnPos = GetRandomOffscreenPosition();
        GameObject baseEnemy = Instantiate(enemyPrefab, baseSpawnPos, Quaternion.identity);
        ConfigureEnemy(baseEnemy, currentConfig, isElite: false);

        // 3. Spawn Tied Elites for this enemy type (if any were unlocked/stacked)
        int elitesToSpawn = eliteCountsPerEnemy.ContainsKey(enemyKey) ? eliteCountsPerEnemy[enemyKey] : 0;

        for (int i = 0; i < elitesToSpawn; i++)
        {
            Vector2 eliteOffset = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f));
            GameObject eliteEnemy = Instantiate(enemyPrefab, baseSpawnPos + eliteOffset, Quaternion.identity);
            ConfigureEnemy(eliteEnemy, currentConfig, isElite: true);
        }
    }

    private void ConfigureEnemy(GameObject enemyObj, EnemyUpgradeData data, bool isElite)
    {
        // Elite 1.5x Physical Size Scaling
        if (isElite)
        {
            enemyObj.transform.localScale *= 1.5f;
        }

        // Calculate Multipliers (Elites stack 2x base and 1.5x excluded rules)
        float baseMult = isElite ? 2.0f : 1.0f;
        float excludedMult = isElite ? 1.5f : 1.0f;

        if (data != null)
        {
            baseMult = isElite ? data.baseStatMultiplier * 2f : data.baseStatMultiplier;
            excludedMult = isElite ? data.excludedStatMultiplier * 1.5f : data.excludedStatMultiplier;
        }

        // Apply stats via your EnemyController component
        if (enemyObj.TryGetComponent<IEnemyController>(out var controller))
        {
            controller.ApplyScaling(baseMult, excludedMult);
        }
    }

    private Vector2 GetRandomOffscreenPosition()
    {
        float height = mainCamera.orthographicSize * 2f;
        float width = height * mainCamera.aspect;

        float x = Random.Range(-width / 2f, width / 2f);
        float y = Random.Range(-height / 2f, height / 2f);

        int side = Random.Range(0, 4);
        Vector2 spawnPosition;

        switch (side)
        {
            case 0:
                spawnPosition = new Vector2(mainCamera.transform.position.x - width / 2f - spawnMargin, mainCamera.transform.position.y + y);
                break;
            case 1:
                spawnPosition = new Vector2(mainCamera.transform.position.x + width / 2f + spawnMargin, mainCamera.transform.position.y + y);
                break;
            case 2:
                spawnPosition = new Vector2(mainCamera.transform.position.x + x, mainCamera.transform.position.y - height / 2f - spawnMargin);
                break;
            default:
                spawnPosition = new Vector2(mainCamera.transform.position.x + x, mainCamera.transform.position.y + height / 2f + spawnMargin);
                break;
        }

        return spawnPosition;
    }

    // Called by your AutoUpgradeManager when an upgrade card is chosen
    public void ApplyEnemyUpgrade(EnemyUpgradeData upgrade)
    {
        switch (upgrade.type)
        {
            case EnemyUpgradeType.NewEnemyType:
                if (upgrade.enemyPrefab != null)
                {
                    UnlockEnemy(upgrade.enemyPrefab);
                    activeEnemyUpgrades[upgrade.enemyPrefab.name] = upgrade;
                }
                break;

            case EnemyUpgradeType.BaseStatBuff:
                if (upgrade.targetEnemyPrefab != null)
                {
                    activeEnemyUpgrades[upgrade.targetEnemyPrefab.name] = upgrade;
                }
                break;

            case EnemyUpgradeType.EliteSpawner:
                if (upgrade.targetEnemyPrefab != null)
                {
                    string key = upgrade.targetEnemyPrefab.name;
                    if (!eliteCountsPerEnemy.ContainsKey(key))
                    {
                        eliteCountsPerEnemy[key] = 0;
                    }
                    eliteCountsPerEnemy[key] += upgrade.eliteCountIncrement;
                }
                break;
        }
    }

    public void UnlockEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
            return;

        if (!unlockedEnemies.Contains(enemyPrefab))
        {
            unlockedEnemies.Add(enemyPrefab);
            
            string key = enemyPrefab.name;
            if (!activeEnemyUpgrades.ContainsKey(key))
            {
                activeEnemyUpgrades[key] = null;
            }
        }
    }
}