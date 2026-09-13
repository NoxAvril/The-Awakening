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

    [Header("Capacity Upgrade Stats")]
    [Range(2, 5)]
    [SerializeField] private int maxActiveMines = 2;

    private List<GameObject> activeMines = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        SetAreaDamage(true, 1.8f);
    }

    public override void Attack()
    {
        // Purge destroyed or deactivated mines from tracking list
        activeMines.RemoveAll(mine => mine == null || !mine.activeInHierarchy);

        if (activeMines.Count >= maxActiveMines) return;

        Vector3 dropPos = transform.position;

        GameObject mineObj = new GameObject("ProximityMine");
        mineObj.transform.position = dropPos;
        activeMines.Add(mineObj);

        // 1. Spawn Ground AOE Indicator First (Renders on Indicators/Background layer)
        if (indicatorPrefab != null)
        {
            GameObject ind = Instantiate(indicatorPrefab, dropPos, Quaternion.identity, mineObj.transform);
            if (ind.TryGetComponent<AOEIndicator>(out var aoe))
            {
                aoe.Setup(area, new Color(0f, 1f, 0f, 0.4f), -1f);
            }
        }

        // 2. Spawn Mine Sprite Object (Explicitly render on top of indicator)
        if (mineSprite != null)
        {
            GameObject visualObj = new GameObject("MineVisual");
            visualObj.transform.SetParent(mineObj.transform);
            visualObj.transform.localPosition = Vector3.zero;

            SpriteRenderer sr = visualObj.AddComponent<SpriteRenderer>();
            sr.sprite = mineSprite;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 2; // Higher than indicator (0) so it renders clearly in center
        }

        StartCoroutine(MineLogic(mineObj));
    }

    private IEnumerator MineLogic(GameObject mineObj)
    {
        bool triggered = false;

        while (!triggered && mineObj != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(mineObj.transform.position, detectionRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    triggered = true;
                    break;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        if (mineObj != null)
        {
            float finalDamage = GetCalculatedDamage();
            Collider2D[] blastHits = Physics2D.OverlapCircleAll(mineObj.transform.position, area);

            foreach (var hit in blastHits)
            {
                if (hit.CompareTag("Enemy") && hit.TryGetComponent<EnemyHealth>(out var enemy))
                {
                    Vector2 dir = (hit.transform.position - mineObj.transform.position).normalized;
                    enemy.TakeDamage(finalDamage, dir * knockback);
                }
            }

            mineObj.SetActive(false);
            activeMines.Remove(mineObj);
            Destroy(mineObj);
        }
    }

    public void IncreaseMaxMines(int amount = 1)
    {
        maxActiveMines = Mathf.Clamp(maxActiveMines + amount, 2, 5);
    }
}