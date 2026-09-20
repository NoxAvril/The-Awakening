using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Pools & References")]
    public List<GameObject> unlockedEnemies =
        new List<GameObject>();

    public GameObject startingEnemy;

    public Transform player;

    public Camera mainCamera;


    [Header("Spawn Settings")]
    public float spawnInterval = 1f;

    public float spawnMargin = 2f;


    private float spawnTimer;


    // ============================================================
    // RUNTIME ENEMY DATA
    // ============================================================

    private Dictionary<string, int>
        eliteCountsPerEnemy =
        new Dictionary<string, int>();


    private Dictionary<string, EnemyUpgradeData>
        activeEnemyUpgrades =
        new Dictionary<string, EnemyUpgradeData>();


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        if (startingEnemy != null)
        {
            UnlockEnemy(startingEnemy);
        }
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnWave();

            spawnTimer =
                spawnInterval;
        }
    }


    // ============================================================
    // SPAWN WAVE
    // ============================================================

    private void SpawnWave()
    {
        if (unlockedEnemies.Count == 0)
            return;


        // ========================================================
        // PICK RANDOM ENEMY
        // ========================================================

        int randomIndex =
            Random.Range(
                0,
                unlockedEnemies.Count
            );


        GameObject enemyPrefab =
            unlockedEnemies[randomIndex];


        if (enemyPrefab == null)
            return;


        string enemyKey =
            enemyPrefab.name;


        // ========================================================
        // GET NORMAL STAT UPGRADE
        // ========================================================

        EnemyUpgradeData currentConfig = null;


        if (
            activeEnemyUpgrades.ContainsKey(
                enemyKey
            )
        )
        {
            currentConfig =
                activeEnemyUpgrades[enemyKey];
        }


        // ========================================================
        // SPAWN NORMAL ENEMY
        // ========================================================

        Vector2 baseSpawnPos =
            GetRandomOffscreenPosition();


        GameObject baseEnemy =
            Instantiate(
                enemyPrefab,
                baseSpawnPos,
                Quaternion.identity
            );


        ConfigureEnemy(
            baseEnemy,
            currentConfig,
            false
        );


        // ========================================================
        // SPAWN ELITES
        // ========================================================

        int elitesToSpawn = 0;


        if (
            eliteCountsPerEnemy.ContainsKey(
                enemyKey
            )
        )
        {
            elitesToSpawn =
                eliteCountsPerEnemy[enemyKey];
        }


        for (
            int i = 0;
            i < elitesToSpawn;
            i++
        )
        {
            Vector2 eliteOffset =
                new Vector2(
                    Random.Range(
                        -1.5f,
                        1.5f
                    ),
                    Random.Range(
                        -1.5f,
                        1.5f
                    )
                );


            GameObject eliteEnemy =
                Instantiate(
                    enemyPrefab,
                    baseSpawnPos +
                    eliteOffset,
                    Quaternion.identity
                );


            ConfigureEnemy(
                eliteEnemy,
                currentConfig,
                true
            );
        }
    }


    // ============================================================
    // CONFIGURE ENEMY
    // ============================================================

    private void ConfigureEnemy(
        GameObject enemyObj,
        EnemyUpgradeData data,
        bool isElite)
    {
        if (enemyObj == null)
            return;


        // ========================================================
        // NORMAL ENEMY
        // ========================================================

        if (!isElite)
        {
            // ----------------------------------------------------
            // IMPORTANT:
            //
            // A normal enemy starts at its original EnemyData
            // values.
            //
            // Only apply a configuration if it is a REAL
            // BaseStatBuff.
            // ----------------------------------------------------

            if (
                data != null &&
                data.type ==
                EnemyUpgradeType.BaseStatBuff
            )
            {
                // The actual stat upgrade is handled by
                // EnemyUpgradeManager.
                //
                // Do NOT call ApplyScaling here.
            }


            return;
        }


        // ========================================================
        // ELITE ENEMY
        // ========================================================

        enemyObj.transform.localScale *=
            1.5f;


        float baseMult =
            2.0f;


        float excludedMult =
            1.5f;


        // ========================================================
        // USE ELITE SCALING DATA
        // ========================================================

        if (
            data != null &&
            data.type ==
            EnemyUpgradeType.EliteSpawner
        )
        {
            baseMult =
                data.baseStatMultiplier;


            excludedMult =
                data.excludedStatMultiplier;
        }


        // ========================================================
        // APPLY ELITE SCALING
        // ========================================================

        if (
            enemyObj.TryGetComponent<IEnemyController>(
                out var controller
            )
        )
        {
            controller.ApplyScaling(
                baseMult,
                excludedMult
            );
        }
    }


    // ============================================================
    // RANDOM OFFSCREEN POSITION
    // ============================================================

    private Vector2 GetRandomOffscreenPosition()
    {
        float height =
            mainCamera.orthographicSize * 2f;


        float width =
            height *
            mainCamera.aspect;


        float x =
            Random.Range(
                -width / 2f,
                width / 2f
            );


        float y =
            Random.Range(
                -height / 2f,
                height / 2f
            );


        int side =
            Random.Range(
                0,
                4
            );


        Vector2 spawnPosition;


        switch (side)
        {
            case 0:

                spawnPosition =
                    new Vector2(
                        mainCamera.transform.position.x -
                        width / 2f -
                        spawnMargin,

                        mainCamera.transform.position.y +
                        y
                    );

                break;


            case 1:

                spawnPosition =
                    new Vector2(
                        mainCamera.transform.position.x +
                        width / 2f +
                        spawnMargin,

                        mainCamera.transform.position.y +
                        y
                    );

                break;


            case 2:

                spawnPosition =
                    new Vector2(
                        mainCamera.transform.position.x +
                        x,

                        mainCamera.transform.position.y -
                        height / 2f -
                        spawnMargin
                    );

                break;


            default:

                spawnPosition =
                    new Vector2(
                        mainCamera.transform.position.x +
                        x,

                        mainCamera.transform.position.y +
                        height / 2f +
                        spawnMargin
                    );

                break;
        }


        return spawnPosition;
    }


    // ============================================================
    // APPLY ENEMY UPGRADE
    // ============================================================

    public void ApplyEnemyUpgrade(
        EnemyUpgradeData upgrade)
    {
        if (upgrade == null)
            return;


        switch (upgrade.type)
        {
            // ====================================================
            // NEW ENEMY TYPE
            // ====================================================

            case EnemyUpgradeType.NewEnemyType:

                if (
                    upgrade.enemyPrefab != null
                )
                {
                    UnlockEnemy(
                        upgrade.enemyPrefab
                    );


                    // IMPORTANT:
                    //
                    // Do NOT put the NewEnemyType upgrade
                    // into activeEnemyUpgrades.
                    //
                    // Otherwise ConfigureEnemy() thinks
                    // the new enemy has an active scaling
                    // upgrade.
                }

                break;


            // ====================================================
            // NORMAL STAT BUFF
            // ====================================================

            case EnemyUpgradeType.BaseStatBuff:

                if (
                    upgrade.targetEnemyPrefab != null
                )
                {
                    string key =
                        upgrade
                            .targetEnemyPrefab
                            .name;


                    activeEnemyUpgrades[key] =
                        upgrade;
                }

                break;


            // ====================================================
            // ELITE SPAWNER
            // ====================================================

            case EnemyUpgradeType.EliteSpawner:

                if (
                    upgrade.targetEnemyPrefab != null
                )
                {
                    string key =
                        upgrade
                            .targetEnemyPrefab
                            .name;


                    if (
                        !eliteCountsPerEnemy
                            .ContainsKey(key)
                    )
                    {
                        eliteCountsPerEnemy[key] =
                            0;
                    }


                    eliteCountsPerEnemy[key] +=
                        upgrade.eliteCountIncrement;
                }

                break;
        }
    }


    // ============================================================
    // UNLOCK ENEMY
    // ============================================================

    public void UnlockEnemy(
        GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
            return;


        if (
            !unlockedEnemies.Contains(
                enemyPrefab
            )
        )
        {
            unlockedEnemies.Add(
                enemyPrefab
            );


            string key =
                enemyPrefab.name;


            // Create an entry for the enemy,
            // but do NOT give it an upgrade.

            if (
                !activeEnemyUpgrades.ContainsKey(
                    key
                )
            )
            {
                activeEnemyUpgrades[key] =
                    null;
            }
        }
    }
}