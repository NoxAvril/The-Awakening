using System.Collections;
using UnityEngine;

public class Canister : Weapon
{
    [Header("Canister Settings")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private float throwDuration = 0.5f;
    [SerializeField] private float burnDuration = 3.5f;
    [SerializeField] private float damageInterval = 0.5f;

    [Header("Visuals")]
    [SerializeField] private Sprite canisterSprite;

    protected override void Awake()
    {
        base.Awake();
        SetAreaDamage(true, area > 0 ? area : 2.2f);
    }

    public override void Attack()
    {
        Vector3 targetPos = GetRandomTargetPosition();
        StartCoroutine(ThrowCanister(transform.position, targetPos));
    }

    private Vector3 GetRandomTargetPosition()
    {
        // Always pick a random spot around the player within range (minimum distance of 1.5 units)
        float randomDistance = Random.Range(1.5f, Mathf.Max(1.6f, range));
        Vector2 randomDir = Random.insideUnitCircle.normalized * randomDistance;
        return transform.position + (Vector3)randomDir;
    }

    private IEnumerator ThrowCanister(Vector3 startPos, Vector3 targetPos)
    {
        GameObject canisterObj = new GameObject("CanisterProjectile");
        canisterObj.transform.position = startPos;

        if (canisterSprite != null)
        {
            SpriteRenderer sr = canisterObj.AddComponent<SpriteRenderer>();
            sr.sprite = canisterSprite;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;
        }

        float timer = 0f;
        while (timer < throwDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / throwDuration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * 1.5f;

            if (canisterObj != null)
            {
                canisterObj.transform.position = currentPos;
            }

            yield return null;
        }

        if (canisterObj != null) Destroy(canisterObj);
        StartCoroutine(BurnArea(targetPos));
    }

    private IEnumerator BurnArea(Vector3 position)
    {
        if (indicatorPrefab != null)
        {
            GameObject ind = Instantiate(indicatorPrefab, position, Quaternion.identity);
            if (ind.TryGetComponent<AOEIndicator>(out var aoe))
            {
                aoe.Setup(area, new Color(1f, 0f, 0f, 0.4f), burnDuration);
            }
        }

        float elapsed = 0f;
        while (elapsed < burnDuration)
        {
            float finalDamage = GetCalculatedDamage();
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, area);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy") && hit.TryGetComponent<EnemyHealth>(out var enemy))
                {
                    enemy.TakeDamage(finalDamage, Vector2.zero);
                }
            }

            yield return new WaitForSeconds(damageInterval);
            elapsed += damageInterval;
        }
    }
}