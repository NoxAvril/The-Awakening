using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    [Header("Player & Camera")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;

    [Header("Grid Setup")]
    private const int columns = 3;
    private const int rows = 5;
    private const int totalTiles = columns * rows; // 15 tiles

    [Header("Spacing")]
    [SerializeField] private float extraSpacing = 0f;

    private Transform[] tiles;
    private float tileWidth;
    private float tileHeight;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        FindTiles();

        if (tiles.Length != totalTiles)
        {
            Debug.LogError($"[BackgroundLoop] You need exactly {totalTiles} child sprites for a 5-row x 3-col grid. Found: {tiles.Length}");
            return;
        }

        CalculateTileSize();
        SetupGrid();
    }

    private void FindTiles()
    {
        tiles = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            tiles[i] = transform.GetChild(i);
        }
    }

    private void CalculateTileSize()
    {
        SpriteRenderer spriteRenderer = tiles[0].GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("[BackgroundLoop] Background sprites need SpriteRenderers.");
            return;
        }

        tileWidth = spriteRenderer.bounds.size.x + extraSpacing;
        tileHeight = spriteRenderer.bounds.size.y + extraSpacing;

        Debug.Log($"[BackgroundLoop] Tile world size = {tileWidth:0.###} x {tileHeight:0.###}");
    }

    private void SetupGrid()
    {
        // Layout: 3 Columns (-1, 0, +1), 5 Rows (+2, +1, 0, -1, -2)
        for (int i = 0; i < totalTiles; i++)
        {
            int column = i % columns; // 0, 1, 2
            int row = i / columns;    // 0, 1, 2, 3, 4

            // Center column is 1 (offsets: -1, 0, +1)
            float x = (column - 1) * tileWidth;

            // Center row is 2 (offsets: +2, +1, 0, -1, -2)
            float y = (2 - row) * tileHeight;

            tiles[i].localPosition = new Vector3(x, y, tiles[i].localPosition.z);
        }
    }

    private void Update()
    {
        if (player == null || tiles == null || tiles.Length != totalTiles || tileWidth <= 0f || tileHeight <= 0f)
            return;

        UpdateBackgroundPosition();
    }

    private void UpdateBackgroundPosition()
    {
        // Snap the parent grid so the player stays within the center tile (column 1, row 2)
        float gridX = Mathf.Floor((player.position.x + tileWidth * 0.5f) / tileWidth) * tileWidth;
        float gridY = Mathf.Floor((player.position.y + tileHeight * 0.5f) / tileHeight) * tileHeight;

        transform.position = new Vector3(gridX, gridY, transform.position.z);
    }
}