using UnityEngine;

public class BossManager : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private float bossSpawnTimeSeconds = 60f;
    [SerializeField] private GameObject bossPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float spawnMargin = 2f;

    private float timer = 0f;
    private bool bossSpawned = false;
    private bool bossDefeatedCalled = false;
    private GameObject activeBoss;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
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
        }
        else if (!bossDefeatedCalled && activeBoss == null)
        {
            bossDefeatedCalled = true;
            HandleBossDefeated();
        }
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("[BossManager] Boss Prefab is not assigned!");
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogError("[BossManager] Main Camera is not assigned!");
            return;
        }

        Vector2 spawnPosition = GetRandomOffscreenPosition();

        activeBoss = Instantiate(
            bossPrefab,
            spawnPosition,
            Quaternion.identity
        );

        bossSpawned = true;

        Debug.Log(
            "[BossManager] Boss spawned at: " + spawnPosition
        );
    }

    private Vector2 GetRandomOffscreenPosition()
    {
        // EXACTLY the same camera calculation as EnemySpawner
        float height = mainCamera.orthographicSize * 2f;
        float width = height * mainCamera.aspect;

        float x = Random.Range(-width / 2f, width / 2f);
        float y = Random.Range(-height / 2f, height / 2f);

        int side = Random.Range(0, 4);

        Vector2 spawnPosition;

        switch (side)
        {
            // LEFT
            case 0:
                spawnPosition = new Vector2(
                    mainCamera.transform.position.x - width / 2f - spawnMargin,
                    mainCamera.transform.position.y + y
                );
                break;

            // RIGHT
            case 1:
                spawnPosition = new Vector2(
                    mainCamera.transform.position.x + width / 2f + spawnMargin,
                    mainCamera.transform.position.y + y
                );
                break;

            // BOTTOM
            case 2:
                spawnPosition = new Vector2(
                    mainCamera.transform.position.x + x,
                    mainCamera.transform.position.y - height / 2f - spawnMargin
                );
                break;

            // TOP
            default:
                spawnPosition = new Vector2(
                    mainCamera.transform.position.x + x,
                    mainCamera.transform.position.y + height / 2f + spawnMargin
                );
                break;
        }

        return spawnPosition;
    }

    private void HandleBossDefeated()
    {
        Debug.Log("[BossManager] Boss defeated!");

        Time.timeScale = 0f;
    }
}