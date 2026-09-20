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

    [Header("Fire Sound")]
    [Tooltip("Fire sound that plays continuously while the fire is burning.")]
    [SerializeField] private AudioClip fireSound;

    [SerializeField, Range(0f, 1f)]
    private float fireSoundVolume = 1f;

    protected override void Awake()
    {
        base.Awake();

        SetAreaDamage(
            true,
            area > 0 ? area : 2.2f
        );
    }

    public override void Attack()
    {
        Vector3 targetPos =
            GetRandomTargetPosition();

        StartCoroutine(
            ThrowCanister(
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

    private IEnumerator ThrowCanister(
        Vector3 startPos,
        Vector3 targetPos
    )
    {
        GameObject canisterObj =
            new GameObject(
                "CanisterProjectile"
            );

        canisterObj.transform.position =
            startPos;

        if (canisterSprite != null)
        {
            SpriteRenderer sr =
                canisterObj.AddComponent<
                    SpriteRenderer
                >();

            sr.sprite =
                canisterSprite;

            sr.sortingLayerName =
                "Default";

            sr.sortingOrder = 5;
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
                1.5f;

            if (canisterObj != null)
            {
                canisterObj.transform.position =
                    currentPos;
            }

            yield return null;
        }

        if (canisterObj != null)
            Destroy(canisterObj);

        // The canister has hit the ground.
        // Start the fire and fire sound here.
        StartCoroutine(
            BurnArea(targetPos)
        );
    }

    private IEnumerator BurnArea(
        Vector3 position
    )
    {
        // Create the fire AOE indicator.
        if (indicatorPrefab != null)
        {
            GameObject ind =
                Instantiate(
                    indicatorPrefab,
                    position,
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
                        0f,
                        0f,
                        0.4f
                    ),
                    burnDuration
                );
            }
        }

        // Start fire sound when the canister hits the ground.
        StartFireSound();

        float elapsed = 0f;

        while (
            elapsed <
            burnDuration
        )
        {
            // If the game is paused, stop the fire sound immediately.
            if (Time.timeScale == 0f)
            {
                StopFireSound();

                yield return null;

                continue;
            }

            float finalDamage =
                GetCalculatedDamage();

            Collider2D[] hits =
                Physics2D.OverlapCircleAll(
                    position,
                    area
                );

            foreach (
                Collider2D hit
                in hits
            )
            {
                if (
                    hit.CompareTag("Enemy") &&
                    hit.TryGetComponent<
                        EnemyHealth
                    >(out var enemy)
                )
                {
                    enemy.TakeDamage(
                        finalDamage,
                        Vector2.zero
                    );
                }
            }

            yield return new WaitForSeconds(
                damageInterval
            );

            elapsed +=
                damageInterval;
        }

        // Fire finished.
        // Stop the fire sound immediately.
        StopFireSound();
    }

    private void StartFireSound()
    {
        if (fireSound == null)
            return;

        if (audioSource == null)
            return;

        // Stop anything currently playing on this weapon.
        audioSource.Stop();

        audioSource.clip =
            fireSound;

        audioSource.loop = true;

        audioSource.volume =
            fireSoundVolume;

        audioSource.Play();
    }

    private void StopFireSound()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();

        audioSource.loop = false;

        audioSource.clip = null;
    }

    protected override void OnDisable()
    {
        StopFireSound();

        base.OnDisable();
    }

    protected override void OnDestroy()
    {
        StopFireSound();

        base.OnDestroy();
    }
}