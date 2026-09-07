using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [Header("Knockback")]
    // Increased default force so knockback dominates even during active player input
    [SerializeField] private float knockbackForce = 50f; 
    [SerializeField] private float knockbackDuration = 0.25f;

    private Rigidbody2D rb;

    public Vector2 lastMoveDirection = Vector2.left;

    private Vector2 movementInput;

    private Vector2 knockbackVelocity;
    private Coroutine knockbackCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        ReadMovementInput();
    }

    private void FixedUpdate()
    {
        Vector2 movementVelocity = movementInput * moveSpeed;

        // Vector addition handles tangential movement automatically 
        // when player inputs perpendicular or angled directions
        rb.linearVelocity = movementVelocity + knockbackVelocity;
    }

    private void ReadMovementInput()
    {
        Vector2 movement = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) movement.y += 1f;
        if (Keyboard.current.sKey.isPressed) movement.y -= 1f;
        if (Keyboard.current.aKey.isPressed) movement.x -= 1f;
        if (Keyboard.current.dKey.isPressed) movement.x += 1f;

        movementInput = movement.normalized;

        if (movementInput != Vector2.zero)
        {
            lastMoveDirection = movementInput;
        }
    }

    public void ApplyKnockBack(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(KnockbackCoroutine(-direction.normalized));
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction)
    {
        float elapsed = 0f;
        Vector2 startingVelocity = direction * knockbackForce;

        while (elapsed < knockbackDuration)
        {
            // 1. Calculate force BEFORE yielding so FixedUpdate applies the maximum impulse immediately
            float fade = 1f - (elapsed / knockbackDuration);
            knockbackVelocity = startingVelocity * fade;

            yield return new WaitForFixedUpdate();

            // 2. Advance time after the physics frame runs
            elapsed += Time.fixedDeltaTime;
        }

        knockbackVelocity = Vector2.zero;
        knockbackCoroutine = null;
    }

    public void setMoveSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
    }

    public void moveSpeedMultiplier(float multiplier)
    {
        moveSpeed *= multiplier;
    }
}