using System;
using UnityEngine;
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

    private float slowPercent = 0f;
    private float slowDuration = 0f;
    private float burnDamage = 0f;
    private float burnDuration = 0f;

    private Vector3 startPosition;

    private HashSet<GameObject> hitEnemies =
        new HashSet<GameObject>();

    private Action firstMovementCallback;
    private bool firstMovementComplete = false;

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

        UpdateBulletRotation();
    }

    private void UpdateBulletRotation()
    {
        if (moveDirection == Vector2.zero)
            return;

        float angle =
            Mathf.Atan2(
                moveDirection.y,
                moveDirection.x
            ) * Mathf.Rad2Deg;

        // The bullet sprite points UP by default.
        // Subtract 90 degrees so the sprite's
        // forward direction matches movement.
        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle - 90f
            );
    }

    public void SetFirstMovementCallback(
        Action callback
    )
    {
        firstMovementCallback = callback;
    }

    private void Update()
    {
        transform.Translate(
            moveDirection *
            speed *
            Time.deltaTime,
            Space.World
        );

        if (!firstMovementComplete)
        {
            firstMovementComplete = true;

            if (firstMovementCallback != null)
            {
                Action callback =
                    firstMovementCallback;

                firstMovementCallback = null;

                callback.Invoke();
            }
        }

        if (
            Vector3.Distance(
                startPosition,
                transform.position
            ) >= maxRange
        )
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(
        Collider2D collision
    )
    {
        if (!collision.CompareTag("Enemy"))
            return;

        GameObject enemyObj =
            collision.gameObject;

        if (hitEnemies.Contains(enemyObj))
            return;

        hitEnemies.Add(enemyObj);

        ApplyDamageAndEffects(enemyObj);

        if (isAreaDamage)
        {
            Explode();
            Destroy(gameObject);
            return;
        }

        if (remainingBounce > 0)
        {
            remainingBounce--;

            Transform nextTarget =
                FindNextBounceTarget(enemyObj);

            if (nextTarget != null)
            {
                moveDirection =
                    (
                        nextTarget.position -
                        transform.position
                    ).normalized;

                UpdateBulletRotation();

                return;
            }
        }

        if (remainingPierce > 0)
        {
            remainingPierce--;
            return;
        }

        Destroy(gameObject);
    }

    private void ApplyDamageAndEffects(
        GameObject target
    )
    {
        Vector2 pushDir =
            moveDirection *
            knockbackForce;

        if (
            target.TryGetComponent<
                EnemyHealth
            >(out var enemy)
        )
        {
            enemy.TakeDamage(
                damage,
                pushDir
            );
        }

        if (
            target.TryGetComponent<
                EnemyStatus
            >(out var status)
        )
        {
            if (stunDuration > 0f)
            {
                status.ApplyStun(
                    stunDuration
                );
            }

            if (
                slowPercent > 0f &&
                slowDuration > 0f
            )
            {
                status.ApplySlow(
                    slowPercent,
                    slowDuration
                );
            }

            if (
                burnDamage > 0f &&
                burnDuration > 0f
            )
            {
                status.ApplyBurn(
                    burnDamage,
                    burnDuration
                );
            }
        }
    }

    private void Explode()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                aoeRadius
            );

        foreach (
            Collider2D hit
            in hits
        )
        {
            if (
                !hit.CompareTag("Enemy") ||
                hit.gameObject == null
            )
            {
                continue;
            }

            GameObject enemyObj =
                hit.gameObject;

            Vector2 pushDir =
                (
                    enemyObj.transform.position -
                    transform.position
                ).normalized *
                knockbackForce;

            if (
                enemyObj.TryGetComponent<
                    EnemyHealth
                >(out var enemyHealth)
            )
            {
                enemyHealth.TakeDamage(
                    damage,
                    pushDir
                );
            }

            if (
                enemyObj.TryGetComponent<
                    EnemyStatus
                >(out var status)
            )
            {
                if (stunDuration > 0f)
                {
                    status.ApplyStun(
                        stunDuration
                    );
                }

                if (
                    slowPercent > 0f &&
                    slowDuration > 0f
                )
                {
                    status.ApplySlow(
                        slowPercent,
                        slowDuration
                    );
                }

                if (
                    burnDamage > 0f &&
                    burnDuration > 0f
                )
                {
                    status.ApplyBurn(
                        burnDamage,
                        burnDuration
                    );
                }
            }
        }
    }

    private Transform FindNextBounceTarget(
        GameObject currentTarget
    )
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );

        Transform nearest = null;

        float minDistance =
            Mathf.Infinity;

        foreach (
            GameObject e
            in enemies
        )
        {
            if (
                e == currentTarget ||
                hitEnemies.Contains(e)
            )
            {
                continue;
            }

            float dist =
                Vector2.Distance(
                    transform.position,
                    e.transform.position
                );

            if (
                dist < minDistance &&
                dist <= maxRange
            )
            {
                minDistance = dist;
                nearest = e.transform;
            }
        }

        return nearest;
    }
}