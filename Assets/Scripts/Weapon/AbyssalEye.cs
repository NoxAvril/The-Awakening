using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbyssalEye : Weapon
{
    [Header("Eldritch Beam Settings")]
    [SerializeField] private LineRenderer beamLine;
    [SerializeField] private float beamWidth = 0.3f;
    [SerializeField] private float displayDuration = 0.15f;
    [SerializeField] private Color beamColor = new Color(0.5f, 0f, 0.8f, 0.9f); // Dark Purple Eldritch Beam

    [Header("Visual References")]
    [Tooltip("Assign a beam texture or sprite here later to replace the color beam.")]
    [SerializeField] private Texture2D customBeamTexture;

    protected override void Awake()
    {
        base.Awake();
        SetPiercing(true, 1);
        EnsureLineRenderer();
    }

    public override void Attack()
    {
        Transform nearestEnemy = GetNearestEnemy();
        if (nearestEnemy == null) return;

        Vector2 dir = (nearestEnemy.position - transform.position).normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, range);

        // CRITICAL FIX: Sort raycast hits by distance so closest enemies are processed first
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        int enemiesHit = 0;
        int maxHits = 1 + pierce; // Base hit (1) + pierce count
        float finalDamage = GetCalculatedDamage();

        HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();
        
        // Beam visual extends all the way to max range to pierce visually through everything
        Vector3 beamEndPos = transform.position + (Vector3)(dir * range);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                if (hit.collider.TryGetComponent<EnemyHealth>(out var enemy))
                {
                    if (hitEnemies.Contains(enemy)) continue;
                    hitEnemies.Add(enemy);

                    // Deal damage only to enemies within the pierce limit
                    if (enemiesHit < maxHits)
                    {
                        enemy.TakeDamage(finalDamage, dir * knockback);
                    }

                    enemiesHit++;
                }
            }
        }

        StartCoroutine(DrawBeam(transform.position, beamEndPos));
    }

    private IEnumerator DrawBeam(Vector3 start, Vector3 end)
    {
        if (beamLine == null) yield break;

        beamLine.enabled = true;
        beamLine.SetPosition(0, start);
        beamLine.SetPosition(1, end);

        float elapsed = 0f;
        Color initialColor = beamColor;

        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(initialColor.a, 0f, elapsed / displayDuration);

            // Fade out beam width & color transparency
            beamLine.startColor = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            beamLine.endColor = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            beamLine.startWidth = Mathf.Lerp(beamWidth, 0f, elapsed / displayDuration);

            yield return null;
        }

        beamLine.enabled = false;
    }

    private void EnsureLineRenderer()
    {
        if (beamLine == null)
        {
            beamLine = GetComponent<LineRenderer>();
            if (beamLine == null)
            {
                beamLine = gameObject.AddComponent<LineRenderer>();
            }
        }

        beamLine.enabled = false;
        beamLine.positionCount = 2;
        beamLine.useWorldSpace = true;
        beamLine.startWidth = beamWidth;
        beamLine.endWidth = beamWidth;
        beamLine.sortingLayerName = "Default";
        beamLine.sortingOrder = 5;

        Material beamMat = new Material(Shader.Find("Sprites/Default"));
        if (customBeamTexture != null)
        {
            beamMat.mainTexture = customBeamTexture;
        }
        beamLine.material = beamMat;
    }

    private Transform GetNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < minDistance && dist <= range)
            {
                minDistance = dist;
                nearest = e.transform;
            }
        }
        return nearest;
    }
}