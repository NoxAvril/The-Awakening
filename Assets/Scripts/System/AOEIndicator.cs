using UnityEngine;

public class AOEIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Setup(float radius, Color color, float duration)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Scale transform to match weapon area radius
        transform.localScale = Vector3.one * (radius * 2f);
        if (spriteRenderer != null) spriteRenderer.color = color;

        if (duration > 0f)
        {
            Destroy(gameObject, duration);
        }
    }
}