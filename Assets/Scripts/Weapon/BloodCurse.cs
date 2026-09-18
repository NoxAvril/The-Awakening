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
    [SerializeField] private Color curseColor = new Color(0.8f, 0.1f, 0.1f, 0.9f);

    private Sprite generatedCircleSprite;
    private Sprite generatedSkullPlaceholder;

    protected override void Awake()
    {
        base.Awake();
        SetBouncing(true, 1);
        CreatePlaceholderCircleSprite();
        CreatePlaceholderSkullSprite();
    }

    public override void Attack()
    {
        HashSet<GameObject> chainAffectedEnemies = new HashSet<GameObject>();

        Transform initialTarget = GetNearestEnemy(transform.position, chainAffectedEnemies);
        if (initialTarget != null)
        {
            StartCoroutine(FlyToTargetAndCurse(transform.position, initialTarget.gameObject, bounce, chainAffectedEnemies));
        }
    }

    private IEnumerator FlyToTargetAndCurse(Vector3 startPos, GameObject target, int remainingBounces, HashSet<GameObject> affectedEnemies)
    {
        if (target == null || affectedEnemies.Contains(target)) yield break;

        // 1. Spawn Projectile Orb Visual
        GameObject projObj = new GameObject("BloodCurseOrb");
        projObj.transform.position = startPos;

        SpriteRenderer sr = projObj.AddComponent<SpriteRenderer>();
        sr.sprite = orbSprite != null ? orbSprite : generatedCircleSprite;
        sr.color = curseColor;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 5;

        // 2. Fly Projectile to Target Position (Dynamically track moving targets)
        while (target != null && Vector3.Distance(projObj.transform.position, target.transform.position) > 0.1f)
        {
            projObj.transform.position = Vector3.MoveTowards(
                projObj.transform.position, 
                target.transform.position, 
                projectileSpeed * Time.deltaTime
            );
            yield return null;
        }

        Destroy(projObj);

        if (target == null)
        {
            // If target died in transit, bounce immediately from the last projectile position
            TryBounceToNext(projObj.transform.position, remainingBounces, affectedEnemies);
            yield break;
        }

        // 3. Apply DoT Coroutine with Floating Skull
        yield return StartCoroutine(ApplyCurseRoutine(target, remainingBounces, affectedEnemies));
    }

    private IEnumerator ApplyCurseRoutine(GameObject target, int remainingBounces, HashSet<GameObject> affectedEnemies)
    {
        if (target == null || affectedEnemies.Contains(target))
        {
            TryBounceToNext(target != null ? target.transform.position : transform.position, remainingBounces, affectedEnemies);
            yield break;
        }

        affectedEnemies.Add(target);

        // ----------------------------------------------------
        // Floating Skull Overhead Indicator Setup
        // ----------------------------------------------------
        GameObject curseIcon = new GameObject("CurseSkullIndicator");
        curseIcon.transform.SetParent(target.transform);
        
        // Position slightly above the enemy's head
        curseIcon.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        curseIcon.transform.localScale = Vector3.one * 0.5f;

        SpriteRenderer skullSr = curseIcon.AddComponent<SpriteRenderer>();
        skullSr.sprite = curseSkullSprite != null ? curseSkullSprite : generatedSkullPlaceholder;
        skullSr.color = curseColor;
        skullSr.sortingLayerName = "Default";
        skullSr.sortingOrder = 10; // Ensures it renders on top of the enemy sprite

        float interval = dotDuration / tickCount;
        float totalCalculatedDamage = GetCalculatedDamage();
        float tickDamage = totalCalculatedDamage / tickCount;

        for (int i = 0; i < tickCount; i++)
        {
            // If target is destroyed/dead mid-curse, break out early to trigger bounce from dead position
            if (target == null) break;

            // Pulsating visual effect for the skull on every tick
            if (curseIcon != null)
            {
                curseIcon.transform.localScale = Vector3.one * 0.7f;
            }

            if (target.TryGetComponent<EnemyHealth>(out var enemy))
            {
                enemy.TakeDamage(tickDamage, Vector2.zero);
            }

            yield return new WaitForSeconds(0.1f);
            if (curseIcon != null) curseIcon.transform.localScale = Vector3.one * 0.5f;

            yield return new WaitForSeconds(interval - 0.1f);
        }

        // Save position for bouncing before destroying/cleaning up the target reference
        Vector3 lastKnownPosition = target != null ? target.transform.position : transform.position;

        // Clean up overhead icon
        if (curseIcon != null) Destroy(curseIcon);

        // 4. Bounce to Next Target from the enemy's last known position
        TryBounceToNext(lastKnownPosition, remainingBounces, affectedEnemies);
    }

    private void TryBounceToNext(Vector3 originPos, int remainingBounces, HashSet<GameObject> affectedEnemies)
    {
        if (remainingBounces > 0)
        {
            Transform nextTarget = GetNearestEnemy(originPos, affectedEnemies);
            if (nextTarget != null)
            {
                StartCoroutine(FlyToTargetAndCurse(originPos, nextTarget.gameObject, remainingBounces - 1, affectedEnemies));
            }
        }
    }

    private Transform GetNearestEnemy(Vector3 origin, HashSet<GameObject> ignoredEnemies)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject e in enemies)
        {
            if (ignoredEnemies.Contains(e)) continue;

            float dist = Vector2.Distance(origin, e.transform.position);
            if (dist < minDistance && dist <= range)
            {
                minDistance = dist;
                nearest = e.transform;
            }
        }
        return nearest;
    }

    // Helper: Generates simple circle sprite
    private void CreatePlaceholderCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                colors[y * size + x] = Vector2.Distance(new Vector2(x, y), center) <= (size / 2f) ? Color.white : Color.clear;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        generatedCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    // Helper: Generates a distinct skull shape placeholder (T-shape skull outline)
    private void CreatePlaceholderSkullSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool isCranium = Vector2.Distance(new Vector2(x, y), new Vector2(16, 20)) <= 10;
                bool isJaw = (x >= 10 && x <= 22) && (y >= 4 && y <= 14);

                bool isLeftEye = Vector2.Distance(new Vector2(x, y), new Vector2(12, 18)) <= 2.5f;
                bool isRightEye = Vector2.Distance(new Vector2(x, y), new Vector2(20, 18)) <= 2.5f;

                if ((isCranium || isJaw) && !isLeftEye && !isRightEye)
                    colors[y * size + x] = Color.white;
                else
                    colors[y * size + x] = Color.clear;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        generatedSkullPlaceholder = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }
}