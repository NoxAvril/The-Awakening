using UnityEngine;
using UnityEngine.SceneManagement;

public class BossManager : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private float bossSpawnTimeSeconds = 60f;
    [SerializeField] private GameObject bossPrefab;

    [Header("Boss Health")]
    [SerializeField] private float normalBossHealthMultiplier = 20f;
    [SerializeField] private float arenaBossHealthMultiplier = 2f;

    [Header("Boss Damage")]
    [SerializeField] private float playerHealthDamagePercent = 0.1f;

    [Header("Boss Movement")]
    [SerializeField] private float speedDifference = 0.25f;

    [Header("Spawn Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float spawnMargin = 2f;

    private float timer = 0f;

    private bool bossSpawned = false;
    private bool bossDefeatedCalled = false;

    private GameObject activeBoss;

    private Transform player;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;

    private Rigidbody2D bossRigidbody;
    private EnemyController bossEnemyController;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        FindPlayer();
    }

    private void Update()
    {
        if (!bossSpawned)
        {
            timer += Time.deltaTime;

            if (timer >= bossSpawnTimeSeconds)
            {
                SpawnBoss();
            }

            return;
        }

        if (activeBoss == null)
        {
            if (!bossDefeatedCalled)
            {
                bossDefeatedCalled = true;
                HandleBossDefeated();
            }

            return;
        }

        FindPlayerIfNeeded();
    }

    private void FixedUpdate()
    {
        if (!bossSpawned)
            return;

        if (activeBoss == null)
            return;

        if (player == null)
            return;

        if (bossRigidbody == null)
            return;

        MoveBoss();
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning(
                "[BossManager] Player could not be found."
            );

            return;
        }

        player = playerObject.transform;

        playerHealth =
            playerObject.GetComponent<PlayerHealth>();

        playerMovement =
            playerObject.GetComponent<PlayerMovement>();
    }

    private void FindPlayerIfNeeded()
    {
        if (player == null)
        {
            FindPlayer();
        }

        if (playerHealth == null && player != null)
        {
            playerHealth =
                player.GetComponent<PlayerHealth>();
        }

        if (playerMovement == null && player != null)
        {
            playerMovement =
                player.GetComponent<PlayerMovement>();
        }
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError(
                "[BossManager] Boss Prefab is not assigned!"
            );

            return;
        }

        if (mainCamera == null)
        {
            Debug.LogError(
                "[BossManager] Main Camera is not assigned!"
            );

            return;
        }

        FindPlayer();

        Vector2 spawnPosition =
            GetRandomOffscreenPosition();

        activeBoss =
            Instantiate(
                bossPrefab,
                spawnPosition,
                Quaternion.identity
            );

        bossSpawned = true;

        SetupBoss();

        Debug.Log(
            "[BossManager] Boss spawned at: " +
            spawnPosition
        );
    }

    private void SetupBoss()
    {
        if (activeBoss == null)
            return;

        bossRigidbody =
            activeBoss.GetComponent<Rigidbody2D>();

        bossEnemyController =
            activeBoss.GetComponent<EnemyController>();

        /*
         * EnemyFollow normally controls enemy movement
         * AND enemy collision damage.
         *
         * We only disable its movement.
         * This keeps its collision damage active.
         */
        EnemyFollow enemyFollow =
            activeBoss.GetComponent<EnemyFollow>();

        if (enemyFollow != null)
        {
            enemyFollow.movementEnabled = false;
        }
        else
        {
            Debug.LogWarning(
                "[BossManager] Boss does not have EnemyFollow. " +
                "Boss collision damage will not use EnemyFollow."
            );
        }

        float highestEnemyHealth =
            GetHighestEnemyHealth();

        float bossHealth =
            highestEnemyHealth *
            normalBossHealthMultiplier;

        /*
         * Arena bosses have 2x the normal boss health.
         */
        if (IsArenaLevel())
        {
            bossHealth *=
                arenaBossHealthMultiplier;
        }

        /*
         * Boss damage is 10% of the player's
         * max health at the moment the boss spawns.
         */
        float bossDamage = 0f;

        if (playerHealth != null)
        {
            bossDamage =
                playerHealth.maxHealth *
                playerHealthDamagePercent;
        }

        if (bossEnemyController != null)
        {
            bossEnemyController.maxHealth =
                bossHealth;

            bossEnemyController.currentHealth =
                bossHealth;

            bossEnemyController.collisionDamage =
                bossDamage;
        }
        else
        {
            Debug.LogWarning(
                "[BossManager] Boss does not have EnemyController!"
            );
        }

        Debug.Log(
            "[BossManager] Boss Stats:\n" +
            "Health: " +
            bossHealth.ToString("0") +
            "\nDamage: " +
            bossDamage.ToString("0.00") +
            "\nHighest Enemy HP: " +
            highestEnemyHealth.ToString("0") +
            "\nArena: " +
            IsArenaLevel()
        );
    }

    private float GetHighestEnemyHealth()
    {
        EnemyController[] enemies =
            FindObjectsByType<EnemyController>(
                FindObjectsSortMode.None
            );

        float highestHealth = 0f;

        foreach (EnemyController enemy in enemies)
        {
            if (enemy == null)
                continue;

            /*
             * Don't use the boss itself when calculating
             * the highest normal enemy HP.
             */
            if (enemy.gameObject == activeBoss)
                continue;

            if (enemy.maxHealth > highestHealth)
            {
                highestHealth =
                    enemy.maxHealth;
            }
        }

        /*
         * Fallback if there are no enemies currently spawned.
         */
        if (highestHealth <= 0f)
        {
            highestHealth = 10f;

            Debug.LogWarning(
                "[BossManager] No active enemies found. " +
                "Using fallback enemy HP of 10."
            );
        }

        return highestHealth;
    }

    private void MoveBoss()
    {
        if (bossRigidbody == null)
            return;

        if (player == null)
            return;

        /*
         * Get the player's CURRENT movement speed.
         */
        float playerSpeed = 0f;

        if (playerMovement != null)
        {
            playerSpeed =
                playerMovement.GetCurrentMoveSpeed();
        }

        /*
         * Boss is always 0.25 slower than the player.
         */
        float bossSpeed =
            Mathf.Max(
                0f,
                playerSpeed - speedDifference
            );

        Vector2 direction =
            (
                player.position -
                activeBoss.transform.position
            ).normalized;

        if (direction == Vector2.zero)
        {
            bossRigidbody.linearVelocity =
                Vector2.zero;

            return;
        }

        bossRigidbody.MovePosition(
            bossRigidbody.position +
            direction *
            bossSpeed *
            Time.fixedDeltaTime
        );
    }

    private bool IsArenaLevel()
    {
        return SceneManager.GetActiveScene().name == "Arena";
    }

    private Vector2 GetRandomOffscreenPosition()
    {
        float height =
            mainCamera.orthographicSize * 2f;

        float width =
            height * mainCamera.aspect;

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
            Random.Range(0, 4);

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

    private void HandleBossDefeated()
    {
        Debug.Log(
            "[BossManager] Boss defeated!"
        );

        Time.timeScale = 0f;

        if (GameEndUI.Instance != null)
        {
            GameEndUI.Instance.ShowVictory();
        }
        else
        {
            Debug.LogWarning(
                "[BossManager] GameEndUI.Instance is missing."
            );
        }
    }
}