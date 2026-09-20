using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Weapon
{
    [Header("Mine Settings")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private float detectionRadius = 0.8f;

    [Header("Visuals")]
    [SerializeField] private Sprite mineSprite;

    [Header("Explosion Indicator")]
    [SerializeField] private float explosionIndicatorDuration = 0.25f;

    [Header("Capacity Upgrade Stats")]
    [Range(2, 5)]
    [SerializeField] private int maxActiveMines = 2;

    private List<GameObject> activeMines =
        new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();

        SetAreaDamage(true, 1.8f);
    }

    public override void Attack()
    {
        // Remove destroyed or inactive mines.
        activeMines.RemoveAll(
            mine =>
                mine == null ||
                !mine.activeInHierarchy
        );

        // Do not place another mine if
        // the maximum number is already active.
        if (activeMines.Count >= maxActiveMines)
            return;

        Vector3 dropPos =
            transform.position;

        GameObject mineObj =
            new GameObject(
                "ProximityMine"
            );

        mineObj.transform.position =
            dropPos;

        activeMines.Add(
            mineObj
        );

        // ------------------------------------------------
        // MINE VISUAL
        // ------------------------------------------------

        if (mineSprite != null)
        {
            GameObject visualObj =
                new GameObject(
                    "MineVisual"
                );

            visualObj.transform.SetParent(
                mineObj.transform
            );

            visualObj.transform.localPosition =
                Vector3.zero;

            SpriteRenderer sr =
                visualObj.AddComponent<
                    SpriteRenderer
                >();

            sr.sprite =
                mineSprite;

            sr.sortingLayerName =
                "Default";

            sr.sortingOrder = 2;
        }

        // Start waiting for an enemy.
        StartCoroutine(
            MineLogic(mineObj)
        );
    }

    private IEnumerator MineLogic(
        GameObject mineObj
    )
    {
        bool triggered = false;

        // ------------------------------------------------
        // WAIT FOR ENEMY
        // ------------------------------------------------

        while (
            !triggered &&
            mineObj != null
        )
        {
            Collider2D[] hits =
                Physics2D.OverlapCircleAll(
                    mineObj.transform.position,
                    detectionRadius
                );

            foreach (Collider2D hit in hits)
            {
                if (
                    hit.CompareTag("Enemy")
                )
                {
                    triggered = true;
                    break;
                }
            }

            yield return new WaitForSeconds(
                0.1f
            );
        }

        if (mineObj == null)
            yield break;

        // ------------------------------------------------
        // EXPLOSION INDICATOR
        // ------------------------------------------------

        if (indicatorPrefab != null)
        {
            GameObject indicator =
                Instantiate(
                    indicatorPrefab,
                    mineObj.transform.position,
                    Quaternion.identity
                );

            if (
                indicator.TryGetComponent<
                    AOEIndicator
                >(out var aoe)
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
                    explosionIndicatorDuration
                );
            }
        }

        // ------------------------------------------------
        // DAMAGE
        // ------------------------------------------------

        float finalDamage =
            GetCalculatedDamage();

        Collider2D[] blastHits =
            Physics2D.OverlapCircleAll(
                mineObj.transform.position,
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
                        mineObj.transform.position
                    ).normalized;

                enemy.TakeDamage(
                    finalDamage,
                    dir * knockback
                );
            }
        }

        // ------------------------------------------------
        // EXPLOSION SOUND
        // ------------------------------------------------

        PlayAttackSound();

        // ------------------------------------------------
        // REMOVE MINE
        // ------------------------------------------------

        mineObj.SetActive(false);

        activeMines.Remove(
            mineObj
        );

        Destroy(
            mineObj
        );
    }

    public void IncreaseMaxMines(
        int amount = 1
    )
    {
        maxActiveMines =
            Mathf.Clamp(
                maxActiveMines + amount,
                2,
                5
            );
    }
}