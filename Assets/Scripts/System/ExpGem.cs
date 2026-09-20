using UnityEngine;

public class ExpGem : MonoBehaviour
{
    public int expValue = 1;

    [Header("Tier Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite tier1Sprite; // Value 1 (Green Book)
    [SerializeField] private Sprite tier2Sprite; // Value 10 (Blue Book)
    [SerializeField] private Sprite tier3Sprite; // Value 100+ (Gold Book)

    [Header("Pickup & Magnet Dynamics")]
    [SerializeField] private float magnetRadius = 3.5f;
    [SerializeField] private float flySpeed = 10f;

    // Procedural fallback book sprites
    private static Sprite generatedBookSprite;
    private static bool placeholdersCreated = false;

    private Transform playerTransform;
    private bool isBeingCollected = false;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        if (!placeholdersCreated)
        {
            generatedBookSprite = CreateBookSprite(32);
            placeholdersCreated = true;
        }

        // Default setup if not called explicitly yet
        UpdateVisualTier();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    public void Setup(int value)
    {
        expValue = value;
        UpdateVisualTier();
    }

    public void AddExpValue(int amount)
    {
        expValue += amount;
        UpdateVisualTier();
    }

    private void UpdateVisualTier()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Assign sprite and color based on tier
        if (expValue >= 100)
        {
            spriteRenderer.sprite = tier3Sprite != null ? tier3Sprite : generatedBookSprite;
        }
        else if (expValue >= 10)
        {
            spriteRenderer.sprite = tier2Sprite != null ? tier2Sprite : generatedBookSprite;
        }
        else
        {
            spriteRenderer.sprite = tier1Sprite != null ? tier1Sprite : generatedBookSprite;
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= magnetRadius)
        {
            isBeingCollected = true;
        }

        if (isBeingCollected)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, flySpeed * Time.deltaTime);

            if (distance <= 0.25f)
            {
                Collect();
            }
        }
    }

    private void Collect()
    {
        if (playerTransform != null && playerTransform.TryGetComponent<PlayerLevelSystem>(out var playerLevel))
        {
            playerLevel.AddExp(expValue);
        }
        Destroy(gameObject);
    }

    public static void DropExp(Vector3 position, int totalExp, GameObject gemPrefab)
    {
        if (gemPrefab == null || totalExp <= 0) return;

        Collider2D[] nearbyHits = Physics2D.OverlapCircleAll(position, 2.0f);
        foreach (var hit in nearbyHits)
        {
            if (hit.TryGetComponent<ExpGem>(out var existingGem) && existingGem.expValue >= 100)
            {
                existingGem.AddExpValue(totalExp);
                return;
            }
        }

        int tier3Count = totalExp / 100;
        totalExp %= 100;

        int tier2Count = totalExp / 10;
        int tier1Count = totalExp % 10;

        SpawnTierGroup(position, 100, tier3Count, gemPrefab);
        SpawnTierGroup(position, 10, tier2Count, gemPrefab);
        SpawnTierGroup(position, 1, tier1Count, gemPrefab);
    }

    private static void SpawnTierGroup(Vector3 position, int value, int count, GameObject gemPrefab)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 scatterPos = position + (Vector3)(Random.insideUnitCircle * 0.4f);
            GameObject gemObj = Instantiate(gemPrefab, scatterPos, Quaternion.identity);

            if (gemObj.TryGetComponent<ExpGem>(out var gem))
            {
                gem.Setup(value);
            }
        }
    }

    // Procedural Open Book / Tome Sprite Generator
    private static Sprite CreateBookSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // Open book shape: Left page and right page split by a center spine gap
                bool isLeftPage = (x >= 4 && x < size / 2 - 1) && (y >= 6 && y <= size - 6);
                bool isRightPage = (x > size / 2 + 1 && x <= size - 5) && (y >= 6 && y <= size - 6);
                bool isSpine = (x >= size / 2 - 1 && x <= size / 2 + 1) && (y >= 7 && y <= size - 7);

                if (isLeftPage || isRightPage)
                {
                    colors[y * size + x] = Color.white;
                }
                else if (isSpine)
                {
                    // Darker center spine shadow
                    colors[y * size + x] = new Color(0.6f, 0.6f, 0.6f, 1f);
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}