using UnityEngine;
using System.Collections;

public class EnemyStatus : MonoBehaviour
{
    private EnemyFollow enemyMovement;
    private EnemyHealth enemyHealth;
    private SpriteRenderer spriteRenderer;

    private bool isStunned = false;
    private float currentSpeedMultiplier = 1f;
    private Coroutine burnCoroutine;

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyFollow>();
        enemyHealth = GetComponent<EnemyHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // --- Stun Effect ---
    public void ApplyStun(float duration)
    {
        if (duration <= 0f) return;
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        if (enemyMovement != null) enemyMovement.enabled = false;

        yield return new WaitForSeconds(duration);

        if (enemyMovement != null) enemyMovement.enabled = true;
        isStunned = false;
    }

    // --- Slow / Freeze Effect ---
    public void ApplySlow(float slowPercent, float duration)
    {
        if (duration <= 0f) return;
        StartCoroutine(SlowRoutine(slowPercent, duration));
    }

    private IEnumerator SlowRoutine(float slowPercent, float duration)
    {
        currentSpeedMultiplier = Mathf.Clamp01(1f - slowPercent);
        
        // Optional visual feedback
        if (spriteRenderer != null) spriteRenderer.color = Color.cyan;

        yield return new WaitForSeconds(duration);

        currentSpeedMultiplier = 1f;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    // --- Burn (Damage Over Time) ---
    public void ApplyBurn(float totalDamage, float duration, float tickInterval = 0.5f)
    {
        if (burnCoroutine != null) StopCoroutine(burnCoroutine);
        burnCoroutine = StartCoroutine(BurnRoutine(totalDamage, duration, tickInterval));
    }

    private IEnumerator BurnRoutine(float totalDamage, float duration, float tickInterval)
    {
        int ticks = Mathf.Max(1, Mathf.RoundToInt(duration / tickInterval));
        float damagePerTick = totalDamage / ticks;

        for (int i = 0; i < ticks; i++)
        {
            yield return new WaitForSeconds(tickInterval);
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damagePerTick);
            }
        }
    }

    public bool IsStunned() => isStunned;
    public float GetSpeedMultiplier() => currentSpeedMultiplier;
}