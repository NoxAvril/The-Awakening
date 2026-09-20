using UnityEngine;
using UnityEngine.Audio;

public class SMG : GunWeapon
{
    public int bulletCount = 10;
    public float bulletInterval = 0.02f;
    public float spread = 20f;
    public int maxBulletCount = 50;

    [Header("SMG Rapid Fire Sound")]
    [SerializeField] private AudioClip rapidFireSound;

    [SerializeField, Range(0f, 1f)]
    private float rapidFireSoundVolume = 1f;

    [Tooltip("Optional. Assign the same mixer group your other SFX use.")]
    [SerializeField] private AudioMixerGroup outputMixerGroup;

    [Tooltip("OFF: the clip plays fully every burst (recommended).\n" +
             "ON: the clip is cut off when the burst ends.")]
    [SerializeField] private bool stopSoundWhenBurstEnds = false;

    [SerializeField] private bool debugLogs = false;

    private int bulletsRemaining;
    private float bulletTimer;
    private Vector2 fireDirection;

    private AudioSource rapidFireAudioSource;

    protected override void Awake()
    {
        base.Awake();

        GameObject audioObject = new GameObject("SMG_RapidFire_Audio");
        audioObject.transform.SetParent(transform);
        audioObject.transform.localPosition = Vector3.zero;

        rapidFireAudioSource = audioObject.AddComponent<AudioSource>();
        rapidFireAudioSource.playOnAwake = false;
        rapidFireAudioSource.loop = false;
        rapidFireAudioSource.spatialBlend = 0f;
        rapidFireAudioSource.volume = rapidFireSoundVolume;

        if (outputMixerGroup != null)
        {
            rapidFireAudioSource.outputAudioMixerGroup = outputMixerGroup;
        }
    }

    protected override void Update()
    {
        if (Time.timeScale == 0f)
        {
            StopRapidFireSound();
            base.Update();
            return;
        }

        base.Update();

        if (bulletsRemaining > 0)
        {
            bulletTimer -= Time.deltaTime;

            if (bulletTimer <= 0f)
            {
                bulletsRemaining--;
                FireBullet();

                if (bulletsRemaining <= 0)
                {
                    if (stopSoundWhenBurstEnds)
                    {
                        StopRapidFireSound();
                    }
                }
                else
                {
                    bulletTimer = bulletInterval;
                }
            }
        }
    }

    public override void Attack()
    {
        if (bulletsRemaining > 0)
        {
            return;
        }

        fireDirection = getAimDirection();

        bulletsRemaining = Mathf.Clamp(bulletCount, 1, maxBulletCount);

        // One sound per burst, started on the first bullet.
        PlayRapidFireSound();

        FireBullet();
        bulletsRemaining--;

        if (bulletsRemaining > 0)
        {
            bulletTimer = bulletInterval;
        }
        else if (stopSoundWhenBurstEnds)
        {
            StopRapidFireSound();
        }
    }

    private void FireBullet()
    {
        if (bulletPrefab == null || playerMovement == null)
        {
            return;
        }

        float angle = Random.Range(-spread / 2f, spread / 2f);

        Vector2 direction =
            Quaternion.Euler(0f, 0f, angle) * fireDirection;

        Vector2 spawnPosition =
            (Vector2)playerMovement.transform.position +
            direction * bulletSpawnDistance;

        GameObject bulletObject =
            Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

        if (bulletObject.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.Setup(
                direction,
                bulletSpeed,
                GetCalculatedDamage(),
                range,
                pierce,
                bounce,
                area,
                areaDamage,
                knockback,
                stun
            );
        }
    }

    private void PlayRapidFireSound()
    {
        if (rapidFireSound == null)
        {
            Debug.LogWarning("[SMG] Rapid Fire Sound is not assigned!");
            return;
        }

        if (rapidFireAudioSource == null)
        {
            return;
        }

        rapidFireAudioSource.clip = rapidFireSound;
        rapidFireAudioSource.loop = false;
        rapidFireAudioSource.volume = rapidFireSoundVolume;
        rapidFireAudioSource.Play();

        if (debugLogs)
        {
            Debug.Log("[SMG] Rapid fire sound played (isPlaying=" +
                      rapidFireAudioSource.isPlaying + ", clip length=" +
                      rapidFireSound.length + "s)");
        }
    }

    private void StopRapidFireSound()
    {
        if (rapidFireAudioSource != null && rapidFireAudioSource.isPlaying)
        {
            rapidFireAudioSource.Stop();
        }
    }

    protected override void OnDisable()
    {
        StopRapidFireSound();
        base.OnDisable();
    }

    protected override void OnDestroy()
    {
        StopRapidFireSound();
        base.OnDestroy();
    }
}