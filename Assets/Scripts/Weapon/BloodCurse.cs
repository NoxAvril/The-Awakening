using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodCurse : Weapon
{
    [Header("Curse Settings")]
    [SerializeField] private float dotDuration = 2f;
    [SerializeField] private int tickCount = 4;
    [SerializeField] private float projectileSpeed = 12f;

    [Header("Visual References")]
    [Tooltip("Floating skull sprite over cursed enemies. Uses placeholder shape if left empty.")]
    [SerializeField] private Sprite curseSkullSprite;

    [SerializeField] private Sprite orbSprite;

    [SerializeField] private Color curseColor =
        new Color(0.8f, 0.1f, 0.1f, 0.9f);

    private Sprite generatedCircleSprite;
    private Sprite generatedSkullPlaceholder;


    // ============================================================
    // AWAKE
    // ============================================================

    protected override void Awake()
    {
        base.Awake();

        SetBouncing(true, 1);

        CreatePlaceholderCircleSprite();
        CreatePlaceholderSkullSprite();
    }


    // ============================================================
    // ATTACK
    // ============================================================

    public override void Attack()
    {
        HashSet<GameObject> chainAffectedEnemies =
            new HashSet<GameObject>();


        Transform initialTarget =
            GetNearestEnemy(
                transform.position,
                chainAffectedEnemies
            );


        if (initialTarget != null)
        {
            StartCoroutine(
                FlyToTargetAndCurse(
                    transform.position,
                    initialTarget.gameObject,
                    bounce,
                    chainAffectedEnemies
                )
            );
        }
    }


    // ============================================================
    // FLY TO TARGET
    // ============================================================

    private IEnumerator FlyToTargetAndCurse(
        Vector3 startPos,
        GameObject target,
        int remainingBounces,
        HashSet<GameObject> affectedEnemies)
    {
        if (
            target == null ||
            affectedEnemies.Contains(target)
        )
        {
            yield break;
        }


        // ========================================================
        // SPAWN PROJECTILE
        // ========================================================

        GameObject projObj =
            new GameObject("BloodCurseOrb");

        projObj.transform.position =
            startPos;


        SpriteRenderer sr =
            projObj.AddComponent<SpriteRenderer>();


        sr.sprite =
            orbSprite != null
                ? orbSprite
                : generatedCircleSprite;


        sr.color =
            curseColor;

        sr.sortingLayerName =
            "Default";

        sr.sortingOrder =
            5;


        // Keep track of the projectile's last valid position.
        Vector3 lastProjectilePosition =
            startPos;


        // ========================================================
        // FLY TO TARGET
        // ========================================================

        while (
            target != null &&
            Vector3.Distance(
                projObj.transform.position,
                target.transform.position
            ) > 0.1f
        )
        {
            projObj.transform.position =
                Vector3.MoveTowards(
                    projObj.transform.position,
                    target.transform.position,
                    projectileSpeed *
                    Time.deltaTime
                );


            lastProjectilePosition =
                projObj.transform.position;


            yield return null;
        }


        // Save the final projectile position BEFORE
        // destroying the projectile object.
        if (projObj != null)
        {
            lastProjectilePosition =
                projObj.transform.position;

            Destroy(projObj);
        }


        // ========================================================
        // TARGET DIED WHILE PROJECTILE WAS TRAVELING
        // ========================================================

        if (target == null)
        {
            TryBounceToNext(
                lastProjectilePosition,
                remainingBounces,
                affectedEnemies
            );

            yield break;
        }


        // ========================================================
        // APPLY CURSE
        // ========================================================

        yield return StartCoroutine(
            ApplyCurseRoutine(
                target,
                remainingBounces,
                affectedEnemies
            )
        );
    }


    // ============================================================
    // APPLY CURSE
    // ============================================================

    private IEnumerator ApplyCurseRoutine(
        GameObject target,
        int remainingBounces,
        HashSet<GameObject> affectedEnemies)
    {
        if (
            target == null ||
            affectedEnemies.Contains(target)
        )
        {
            yield break;
        }


        affectedEnemies.Add(
            target
        );


        // ========================================================
        // FLOATING SKULL
        // ========================================================

        GameObject curseIcon =
            new GameObject(
                "CurseSkullIndicator"
            );


        curseIcon.transform.SetParent(
            target.transform
        );


        curseIcon.transform.localPosition =
            new Vector3(
                0f,
                0.8f,
                0f
            );


        curseIcon.transform.localScale =
            Vector3.one * 0.5f;


        SpriteRenderer skullSr =
            curseIcon.AddComponent<SpriteRenderer>();


        skullSr.sprite =
            curseSkullSprite != null
                ? curseSkullSprite
                : generatedSkullPlaceholder;


        skullSr.color =
            curseColor;

        skullSr.sortingLayerName =
            "Default";

        skullSr.sortingOrder =
            10;


        // ========================================================
        // DAMAGE SETTINGS
        // ========================================================

        float interval =
            dotDuration /
            Mathf.Max(1, tickCount);


        float totalCalculatedDamage =
            GetCalculatedDamage();


        float tickDamage =
            totalCalculatedDamage /
            Mathf.Max(1, tickCount);


        // This stores the enemy's LAST VALID position.
        // Even if the enemy dies, we still know where it was.
        Vector3 lastKnownEnemyPosition =
            target.transform.position;


        // ========================================================
        // DAMAGE TICKS
        // ========================================================

        for (
            int i = 0;
            i < tickCount;
            i++
        )
        {
            // ----------------------------------------------------
            // Enemy still exists
            // ----------------------------------------------------

            if (target != null)
            {
                lastKnownEnemyPosition =
                    target.transform.position;
            }
            else
            {
                // Enemy died.
                // Stop the curse and bounce from its
                // last known position.
                break;
            }


            // ----------------------------------------------------
            // Skull animation
            // ----------------------------------------------------

            if (curseIcon != null)
            {
                curseIcon.transform.localScale =
                    Vector3.one * 0.7f;
            }


            // ----------------------------------------------------
            // Deal damage
            // ----------------------------------------------------

            if (
                target != null &&
                target.TryGetComponent<EnemyHealth>(
                    out var enemy
                )
            )
            {
                enemy.TakeDamage(
                    tickDamage,
                    Vector2.zero
                );


                // Save position immediately after damage.
                // This is important because the damage may
                // have killed the enemy.
                if (target != null)
                {
                    lastKnownEnemyPosition =
                        target.transform.position;
                }
            }


            // ----------------------------------------------------
            // Short visual delay
            // ----------------------------------------------------

            yield return new WaitForSeconds(
                Mathf.Min(
                    0.1f,
                    interval
                )
            );


            if (curseIcon != null)
            {
                curseIcon.transform.localScale =
                    Vector3.one * 0.5f;
            }


            // ----------------------------------------------------
            // Remaining tick time
            // ----------------------------------------------------

            float remainingWait =
                interval - 0.1f;


            if (remainingWait > 0f)
            {
                yield return new WaitForSeconds(
                    remainingWait
                );
            }
        }


        // ========================================================
        // CLEAN UP SKULL
        // ========================================================

        if (curseIcon != null)
        {
            Destroy(
                curseIcon
            );
        }


        // ========================================================
        // BOUNCE
        // ========================================================

        // ALWAYS use the last position of the enemy,
        // NOT the player's position.
        TryBounceToNext(
            lastKnownEnemyPosition,
            remainingBounces,
            affectedEnemies
        );
    }


    // ============================================================
    // FIND NEXT BOUNCE TARGET
    // ============================================================

    private void TryBounceToNext(
        Vector3 originPos,
        int remainingBounces,
        HashSet<GameObject> affectedEnemies)
    {
        if (remainingBounces <= 0)
        {
            return;
        }


        Transform nextTarget =
            GetNearestEnemy(
                originPos,
                affectedEnemies
            );


        if (nextTarget != null)
        {
            StartCoroutine(
                FlyToTargetAndCurse(
                    originPos,
                    nextTarget.gameObject,
                    remainingBounces - 1,
                    affectedEnemies
                )
            );
        }
    }


    // ============================================================
    // FIND NEAREST ENEMY
    // ============================================================

    private Transform GetNearestEnemy(
        Vector3 origin,
        HashSet<GameObject> ignoredEnemies)
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        Transform nearest =
            null;


        float minDistance =
            Mathf.Infinity;


        foreach (
            GameObject enemyObject
            in enemies
        )
        {
            if (enemyObject == null)
            {
                continue;
            }


            if (
                ignoredEnemies != null &&
                ignoredEnemies.Contains(
                    enemyObject
                )
            )
            {
                continue;
            }


            float distance =
                Vector2.Distance(
                    origin,
                    enemyObject.transform.position
                );


            if (
                distance < minDistance &&
                distance <= range
            )
            {
                minDistance =
                    distance;

                nearest =
                    enemyObject.transform;
            }
        }


        return nearest;
    }


    // ============================================================
    // PLACEHOLDER ORB SPRITE
    // ============================================================

    private void CreatePlaceholderCircleSprite()
    {
        int size =
            32;


        Texture2D tex =
            new Texture2D(
                size,
                size
            );


        Color[] colors =
            new Color[
                size *
                size
            ];


        Vector2 center =
            new Vector2(
                size / 2f,
                size / 2f
            );


        for (
            int x = 0;
            x < size;
            x++
        )
        {
            for (
                int y = 0;
                y < size;
                y++
            )
            {
                colors[
                    y * size + x
                ] =
                    Vector2.Distance(
                        new Vector2(x, y),
                        center
                    ) <= size / 2f
                        ? Color.white
                        : Color.clear;
            }
        }


        tex.SetPixels(
            colors
        );


        tex.Apply();


        generatedCircleSprite =
            Sprite.Create(
                tex,
                new Rect(
                    0,
                    0,
                    size,
                    size
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                32f
            );
    }


    // ============================================================
    // PLACEHOLDER SKULL SPRITE
    // ============================================================

    private void CreatePlaceholderSkullSprite()
    {
        int size =
            32;


        Texture2D tex =
            new Texture2D(
                size,
                size
            );


        Color[] colors =
            new Color[
                size *
                size
            ];


        for (
            int x = 0;
            x < size;
            x++
        )
        {
            for (
                int y = 0;
                y < size;
                y++
            )
            {
                bool isCranium =
                    Vector2.Distance(
                        new Vector2(x, y),
                        new Vector2(16, 20)
                    ) <= 10;


                bool isJaw =
                    (
                        x >= 10 &&
                        x <= 22
                    ) &&
                    (
                        y >= 4 &&
                        y <= 14
                    );


                bool isLeftEye =
                    Vector2.Distance(
                        new Vector2(x, y),
                        new Vector2(12, 18)
                    ) <= 2.5f;


                bool isRightEye =
                    Vector2.Distance(
                        new Vector2(x, y),
                        new Vector2(20, 18)
                    ) <= 2.5f;


                if (
                    (
                        isCranium ||
                        isJaw
                    ) &&
                    !isLeftEye &&
                    !isRightEye
                )
                {
                    colors[
                        y * size + x
                    ] =
                        Color.white;
                }
                else
                {
                    colors[
                        y * size + x
                    ] =
                        Color.clear;
                }
            }
        }


        tex.SetPixels(
            colors
        );


        tex.Apply();


        generatedSkullPlaceholder =
            Sprite.Create(
                tex,
                new Rect(
                    0,
                    0,
                    size,
                    size
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                32f
            );
    }
}