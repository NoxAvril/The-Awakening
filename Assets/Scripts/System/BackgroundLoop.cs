using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float tileWidth = 25f;
    [SerializeField] private float tileHeight = 15f;

    private Transform[] tiles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        tiles = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            tiles[i] = transform.GetChild(i);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        foreach (Transform tile in tiles)
        {
            Vector3 difference = player.position - tile.position;
            if (Mathf.Abs(difference.x) > tileWidth * 1.5f)
            {
                tile.position += new Vector3 (Mathf.Sign(difference.x) * tileWidth * 3f, 0f, 0f);
            }

            if (Mathf.Abs(difference.y) > tileHeight * 1.5f)
            {
                tile.position += new Vector3 (0f, Mathf.Sign(difference.y) * tileHeight * 3f, 0f);
            }
        }
    }
}
