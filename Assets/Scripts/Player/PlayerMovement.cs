using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float baseMoveSpeed = 5f;
    private float moveSpeedBonus = 0f; // Tracks additive speed upgrades (e.g., +0.05 per level)

    [Header("Knockback")]
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
        // Calculate final speed using base speed and upgrade bonuses
        float currentSpeed = baseMoveSpeed * (1f + moveSpeedBonus);
        Vector2 movementVelocity = movementInput * currentSpeed;

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

    public void AddMoveSpeedBonus(float amount)
    {
        moveSpeedBonus += amount;
    }

    public float GetMoveSpeedBonus()
    {
        return moveSpeedBonus;
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
            float fade = 1f - (elapsed / knockbackDuration);
            knockbackVelocity = startingVelocity * fade;

            yield return new WaitForFixedUpdate();

            elapsed += Time.fixedDeltaTime;
        }

        knockbackVelocity = Vector2.zero;
        knockbackCoroutine = null;
    }
}