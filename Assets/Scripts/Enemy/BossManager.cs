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

    [Header("Boss Animation")]
    [Tooltip(
        "Animator Bool used by bosses that have separate left/right animations."
    )]
    [SerializeField] private string movingLeftParameter = "IsMovingLeft";

    [Tooltip(
        "Used by bosses that do NOT have a left animation. " +
        "The sprite will be flipped instead."
    )]
    [SerializeField] private bool flipSpriteForSingleAnimationBoss = true;

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

    private Animator bossAnimator;
    private SpriteRenderer bossSpriteRenderer;

    private bool bossUsesLeftRightAnimations = false;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        FindPlayer();

        // Make sure gameplay music is playing
        // when the level starts.
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayGameplayMusic();
        }
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

        player =
            playerObject.transform;

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

        // Find the boss components.
        SetupBossAnimation();

        // Switch from gameplay music
        // to boss music.
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayBossMusic();
        }

        SetupBoss();

        Debug.Log(
            "[BossManager] Boss spawned at: " +
            spawnPosition
        );
    }

    private void SetupBossAnimation()
    {
        if (activeBoss == null)
            return;

        // Find Animator on the boss or its children.
        bossAnimator =
            activeBoss.GetComponentInChildren<Animator>();

        // Find SpriteRenderer on the boss or its children.
        bossSpriteRenderer =
            activeBoss.GetComponentInChildren<SpriteRenderer>();

        bossUsesLeftRightAnimations = false;

        // Check whether this Animator actually has
        // the IsMovingLeft parameter.
        if (bossAnimator != null)
        {
            AnimatorControllerParameter[] parameters =
                bossAnimator.parameters;

            foreach (
                AnimatorControllerParameter parameter
                in parameters
            )
            {
                if (
                    parameter.name ==
                    movingLeftParameter &&
                    parameter.type ==
                    AnimatorControllerParameterType.Bool
                )
                {
                    bossUsesLeftRightAnimations = true;
                    break;
                }
            }
        }

        if (bossUsesLeftRightAnimations)
        {
            Debug.Log(
                "[BossManager] Boss uses LEFT/RIGHT animations."
            );

            // Start facing right.
            bossAnimator.SetBool(
                movingLeftParameter,
                false
            );
        }
        else
        {
            Debug.Log(
                "[BossManager] Boss does not have '" +
                movingLeftParameter +
                "'. Using SpriteRenderer flip instead."
            );

            if (bossSpriteRenderer != null)
            {
                bossSpriteRenderer.flipX = false;
            }
        }
    }

    private void SetupBoss()
    {
        if (activeBoss == null)
            return;

        bossRigidbody =
            activeBoss.GetComponent<Rigidbody2D>();

        bossEnemyController =
            activeBoss.GetComponent<EnemyController>();

        EnemyFollow enemyFollow =
            activeBoss.GetComponent<EnemyFollow>();

        if (enemyFollow != null)
        {
            // Stop the normal EnemyFollow movement.
            // BossManager moves the boss manually.
            enemyFollow.movementEnabled = false;
        }
        else
        {
            Debug.LogWarning(
                "[BossManager] Boss does not have EnemyFollow."
            );
        }

        float highestEnemyHealth =
            GetHighestEnemyHealth();

        float bossHealth =
            highestEnemyHealth *
            normalBossHealthMultiplier;

        // Arena bosses have 2x the normal boss health.
        if (IsArenaLevel())
        {
            bossHealth *=
                arenaBossHealthMultiplier;
        }

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

        foreach (
            EnemyController enemy
            in enemies
        )
        {
            if (enemy == null)
                continue;

            // Don't count the boss itself.
            if (enemy.gameObject == activeBoss)
                continue;

            if (enemy.maxHealth > highestHealth)
            {
                highestHealth =
                    enemy.maxHealth;
            }
        }

        // Fallback if there are no enemies
        // alive when the boss spawns.
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

        float playerSpeed = 0f;

        if (playerMovement != null)
        {
            playerSpeed =
                playerMovement.GetCurrentMoveSpeed();
        }

        // Boss is always 0.25 slower than player.
        float bossSpeed =
            Mathf.Max(
                0f,
                playerSpeed -
                speedDifference
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

        // Update the boss visual direction.
        UpdateBossDirection(direction);

        bossRigidbody.MovePosition(
            bossRigidbody.position +
            direction *
            bossSpeed *
            Time.fixedDeltaTime
        );
    }

    private void UpdateBossDirection(
        Vector2 direction
    )
    {
        if (direction.x < -0.01f)
        {
            // Boss is moving LEFT.
            SetBossMovingLeft(true);
        }
        else if (direction.x > 0.01f)
        {
            // Boss is moving RIGHT.
            SetBossMovingLeft(false);
        }
    }

    private void SetBossMovingLeft(
        bool movingLeft
    )
    {
        if (bossUsesLeftRightAnimations)
        {
            // --------------------------------
            // BOSS WITH TWO ANIMATIONS
            // --------------------------------

            if (bossAnimator != null)
            {
                bossAnimator.SetBool(
                    movingLeftParameter,
                    movingLeft
                );
            }

            // Do NOT flip the SpriteRenderer.
            // The Animator handles the direction.
            return;
        }

        // --------------------------------
        // BOSS WITH ONLY ONE ANIMATION
        // --------------------------------

        if (
            flipSpriteForSingleAnimationBoss &&
            bossSpriteRenderer != null
        )
        {
            // This assumes the single animation
            // naturally faces RIGHT.
            bossSpriteRenderer.flipX =
                movingLeft;
        }
    }

    private bool IsArenaLevel()
    {
        return
            SceneManager.GetActiveScene().name ==
            "Arena";
    }

    private Vector2 GetRandomOffscreenPosition()
    {
        float height =
            mainCamera.orthographicSize *
            2f;

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

        // Switch back to the menu music.
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMenuMusic();
        }

        // Stop gameplay.
        Time.timeScale = 0f;

        // Show victory screen.
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