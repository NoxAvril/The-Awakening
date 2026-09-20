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
    // CURRENTLY CURSED ENEMIES
    // ============================================================

    /*
     * This list persists between attacks.
     *
     * An enemy stays inside this HashSet for the entire
     * duration of its Blood Curse.
     *
     * This prevents a new Blood Curse attack from targeting
     * an enemy that is already afflicted.
     */
    private HashSet<GameObject> currentlyCursedEnemies =
        new HashSet<GameObject>();


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
        /*
         * This HashSet is only for THIS chain.
         *
         * currentlyCursedEnemies handles enemies that are
         * already afflicted by previous attacks.
         */
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
            affectedEnemies.Contains(target) ||
            IsCurrentlyCursed(target)
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
            /*
             * The target could become cursed by another
             * Blood Curse while this projectile is traveling.
             *
             * Stop if that happens.
             */
            if (IsCurrentlyCursed(target))
            {
                lastProjectilePosition =
                    projObj.transform.position;

                Destroy(projObj);

                TryBounceToNext(
                    lastProjectilePosition,
                    remainingBounces,
                    affectedEnemies
                );

                yield break;
            }


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


        // ========================================================
        // SAVE FINAL PROJECTILE POSITION
        // ========================================================

        if (projObj != null)
        {
            lastProjectilePosition =
                projObj.transform.position;

            Destroy(projObj);
        }


        // ========================================================
        // TARGET DIED
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
        // TARGET BECAME CURSED
        // ========================================================

        if (IsCurrentlyCursed(target))
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
            affectedEnemies.Contains(target) ||
            IsCurrentlyCursed(target)
        )
        {
            yield break;
        }


        // ========================================================
        // MARK AS AFFECTED
        // ========================================================

        affectedEnemies.Add(
            target
        );

        currentlyCursedEnemies.Add(
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
        // REMOVE FROM CURRENTLY CURSED
        // ========================================================

        /*
         * The curse has finished.
         *
         * The enemy can now be targeted by a future
         * Blood Curse attack.
         */
        currentlyCursedEnemies.Remove(
            target
        );


        // ========================================================
        // BOUNCE
        // ========================================================

        TryBounceToNext(
            lastKnownEnemyPosition,
            remainingBounces,
            affectedEnemies
        );
    }


    // ============================================================
    // CHECK IF CURRENTLY CURSED
    // ============================================================

    private bool IsCurrentlyCursed(
        GameObject enemy)
    {
        if (enemy == null)
        {
            return false;
        }


        /*
         * Remove destroyed enemies from the HashSet.
         */
        currentlyCursedEnemies.RemoveWhere(
            item => item == null
        );


        return currentlyCursedEnemies.Contains(
            enemy
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


            // ====================================================
            // SKIP CHAIN TARGETS
            // ====================================================

            if (
                ignoredEnemies != null &&
                ignoredEnemies.Contains(
                    enemyObject
                )
            )
            {
                continue;
            }


            // ====================================================
            // SKIP ALREADY CURSED ENEMIES
            // ====================================================

            if (
                IsCurrentlyCursed(
                    enemyObject
                )
            )
            {
                continue;
            }


            // ====================================================
            // CHECK DISTANCE
            // ====================================================

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