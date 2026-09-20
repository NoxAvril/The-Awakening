using System.Collections;
using UnityEngine;

public class Grenade : Weapon
{
    [Header("Grenade Settings")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private float arcHeight = 2.0f;
    [SerializeField] private float throwDuration = 0.6f;

    [Header("Visuals")]
    [SerializeField] private Sprite grenadeSprite;

    protected override void Awake()
    {
        base.Awake();

        SetAreaDamage(
            true,
            area > 0 ? area : 1.8f
        );
    }

    public override void Attack()
    {
        Vector3 targetPos =
            GetRandomTargetPosition();

        StartCoroutine(
            ThrowGrenade(
                transform.position,
                targetPos
            )
        );
    }

    private Vector3 GetRandomTargetPosition()
    {
        float randomDistance =
            Random.Range(
                1.5f,
                Mathf.Max(
                    1.6f,
                    range
                )
            );

        Vector2 randomDir =
            Random.insideUnitCircle.normalized *
            randomDistance;

        return transform.position +
               (Vector3)randomDir;
    }

    private IEnumerator ThrowGrenade(
        Vector3 startPos,
        Vector3 targetPos
    )
    {
        GameObject grenadeObj =
            new GameObject(
                "GrenadeProjectile"
            );

        grenadeObj.transform.position =
            startPos;

        if (grenadeSprite != null)
        {
            SpriteRenderer sr =
                grenadeObj.AddComponent<
                    SpriteRenderer
                >();

            sr.sprite =
                grenadeSprite;

            sr.sortingLayerName =
                "Default";

            sr.sortingOrder = 5;
        }

        if (indicatorPrefab != null)
        {
            GameObject ind =
                Instantiate(
                    indicatorPrefab,
                    targetPos,
                    Quaternion.identity
                );

            if (
                ind.TryGetComponent<AOEIndicator>(
                    out var aoe
                )
            )
            {
                aoe.Setup(
                    area,
                    new Color(
                        1f,
                        1f,
                        1f,
                        0.4f
                    ),
                    throwDuration
                );
            }
        }

        float timer = 0f;

        while (
            timer <
            throwDuration
        )
        {
            timer +=
                Time.deltaTime;

            float progress =
                timer /
                throwDuration;

            Vector3 currentPos =
                Vector3.Lerp(
                    startPos,
                    targetPos,
                    progress
                );

            currentPos.y +=
                Mathf.Sin(
                    progress *
                    Mathf.PI
                ) *
                arcHeight;

            if (grenadeObj != null)
            {
                grenadeObj.transform.position =
                    currentPos;

                grenadeObj.transform.Rotate(
                    0f,
                    0f,
                    360f *
                    Time.deltaTime
                );
            }

            yield return null;
        }

        Explode(targetPos);

        if (grenadeObj != null)
            Destroy(grenadeObj);
    }

    private void Explode(
        Vector3 position
    )
    {
        float finalDamage =
            GetCalculatedDamage();

        Collider2D[] blastHits =
            Physics2D.OverlapCircleAll(
                position,
                area
            );

        foreach (
            Collider2D hit
            in blastHits
        )
        {
            if (
                hit.CompareTag("Enemy") &&
                hit.TryGetComponent<
                    EnemyHealth
                >(out var enemy)
            )
            {
                Vector2 dir =
                    (
                        hit.transform.position -
                        position
                    ).normalized;

                enemy.TakeDamage(
                    finalDamage,
                    dir * knockback
                );
            }
        }

        // PLAY GRENADE EXPLOSION SOUND HERE
        PlayAttackSound();
    }
}