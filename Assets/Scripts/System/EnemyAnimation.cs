using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;

    [Header("Animation")]
    [SerializeField] private string movingParameter = "IsMoving";

    [Header("Sprite Direction")]
    [Tooltip("The original sprite faces LEFT.")]
    [SerializeField] private bool spriteFacesLeftByDefault = true;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (rb == null)
            return;

        UpdateAnimation();
        UpdateDirection();
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        bool isMoving =
            rb.linearVelocity.magnitude > 0.05f;

        animator.SetBool(
            movingParameter,
            isMoving
        );
    }

    private void UpdateDirection()
    {
        if (spriteRenderer == null)
            return;

        Vector2 velocity =
            rb.linearVelocity;

        // Ignore extremely small movement.
        if (Mathf.Abs(velocity.x) < 0.05f)
            return;

        if (velocity.x > 0f)
        {
            // Enemy is moving RIGHT.
            //
            // Original sprite faces LEFT,
            // so flip it horizontally.
            spriteRenderer.flipX =
                spriteFacesLeftByDefault;
        }
        else if (velocity.x < 0f)
        {
            // Enemy is moving LEFT.
            //
            // Original sprite already faces LEFT,
            // so don't flip it.
            spriteRenderer.flipX =
                !spriteFacesLeftByDefault;
        }
    }
}