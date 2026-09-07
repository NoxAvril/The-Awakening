    using UnityEngine;
    using System.Collections.Generic;

    public class EnemySpawner : MonoBehaviour
    {
        public List<GameObject> unlockedEnemies = new List<GameObject>();
        public GameObject startingEnemy;
        public Transform player;

        public float spawnInterval = 1f;
        public float spawnMargin = 2f;
        public Camera mainCamera;

        private float spawnTimer;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (startingEnemy != null)
            {
                UnlockEnemy(startingEnemy);
            }
        }

        // Update is called once per frame
        void Update()
        {
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0)
            {
                SpawnEnemy();
                spawnTimer = spawnInterval;
            }
        }

        void SpawnEnemy()
        {
            if (unlockedEnemies.Count == 0)
                return;
            
            int randomIndex = Random.Range(0, unlockedEnemies.Count);

            GameObject enemyPrefab = unlockedEnemies[randomIndex];

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

            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }

        public void UnlockEnemy(GameObject enemyPrefab)
        {
            if (enemyPrefab == null)
                return;

            if (!unlockedEnemies.Contains(enemyPrefab))
                unlockedEnemies.Add(enemyPrefab); 
        }
    }