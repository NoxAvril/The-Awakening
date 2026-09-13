using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    private Vector2 moveDirection;
    private float speed;
    private float damage;
    private float maxRange;
    private int remainingPierce;
    private int remainingBounce;
    private float aoeRadius;
    private bool isAreaDamage;
    private float knockbackForce;
    private float stunDuration;

    // Optional Status Effects
    private float slowPercent = 0f;
    private float slowDuration = 0f;
    private float burnDamage = 0f;
    private float burnDuration = 0f;

    private Vector3 startPosition;
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    public void Setup(
        Vector2 direction,
        float bulletSpeed,
        float bulletDamage,
        float range,
        int pierceCount,
        int bounceCount,
        float area,
        bool areaDamage,
        float knockback,
        float stun,
        float slowPct = 0f,
        float slowDur = 0f,
        float burnDmg = 0f,
        float burnDur = 0f)
    {
        moveDirection = direction.normalized;
        speed = bulletSpeed;
        damage = bulletDamage;
        maxRange = range;
        remainingPierce = pierceCount;
        remainingBounce = bounceCount;
        aoeRadius = area;
        isAreaDamage = areaDamage;
        knockbackForce = knockback;
        stunDuration = stun;

        slowPercent = slowPct;
        slowDuration = slowDur;
        burnDamage = burnDmg;
        burnDuration = burnDur;

        startPosition = transform.position;

        // Rotate projectile to align with movement direction
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Update()
    {
        // Move bullet forward
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // Max range check
        if (Vector3.Distance(startPosition, transform.position) >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        GameObject enemyObj = collision.gameObject;

        // Prevent hitting the same enemy multiple times during piercing
        if (hitEnemies.Contains(enemyObj)) return;
        hitEnemies.Add(enemyObj);

        ApplyDamageAndEffects(enemyObj);

        // Area Damage Trigger
        if (isAreaDamage)
        {
            Explode();
            Destroy(gameObject);
            return;
        }

        // Bouncing Logic
        if (remainingBounce > 0)
        {
            remainingBounce--;
            Transform nextTarget = FindNextBounceTarget(enemyObj);

            if (nextTarget != null)
            {
                moveDirection = (nextTarget.position - transform.position).normalized;
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                return;
            }
        }

        // Piercing Logic
        if (remainingPierce > 0)
        {
            remainingPierce--;
            return;
        }

        // No bounce or pierce remaining
        Destroy(gameObject);
    }

    private void ApplyDamageAndEffects(GameObject target)
    {
        Vector2 pushDir = moveDirection * knockbackForce;

        // Apply direct damage & knockback via EnemyHealth
        if (target.TryGetComponent<EnemyHealth>(out var enemy))
        {
            enemy.TakeDamage(damage, pushDir);
        }

        // Apply status effects (Stun, Slow, Burn) via EnemyStatus
        if (target.TryGetComponent<EnemyStatus>(out var status))
        {
            if (stunDuration > 0f)
            {
                status.ApplyStun(stunDuration);
            }

            if (slowPercent > 0f && slowDuration > 0f)
            {
                status.ApplySlow(slowPercent, slowDuration);
            }

            if (burnDamage > 0f && burnDuration > 0f)
            {
                status.ApplyBurn(burnDamage, burnDuration);
            }
        }
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy") || hit.gameObject == null) continue;

            GameObject enemyObj = hit.gameObject;

            // Apply damage and status effects to all enemies inside AoE explosion
            Vector2 pushDir = (enemyObj.transform.position - transform.position).normalized * knockbackForce;

            if (enemyObj.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(damage, pushDir);
            }

            if (enemyObj.TryGetComponent<EnemyStatus>(out var status))
            {
                if (stunDuration > 0f) status.ApplyStun(stunDuration);
                if (slowPercent > 0f && slowDuration > 0f) status.ApplySlow(slowPercent, slowDuration);
                if (burnDamage > 0f && burnDuration > 0f) status.ApplyBurn(burnDamage, burnDuration);
            }
        }
    }

    private Transform FindNextBounceTarget(GameObject currentTarget)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject e in enemies)
        {
            if (e == currentTarget || hitEnemies.Contains(e)) continue;

            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < minDistance && dist <= maxRange)
            {
                minDistance = dist;
                nearest = e.transform;
            }
        }

        return nearest;
    }
}