using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Base Weapon Stats")]
    public float damage = 1f;
    public float attackRate = 1f;
    public float range = 1f;
    public float knockback = 0f;
    public float stun = 0f;

    [Header("Behavior Properties")]
    public float area = 0f;
    public int pierce = 0;
    public int bounce = 0;
    public bool piercing = false;
    public bool bouncing = false;
    public bool areaDamage = false;

    [Header("Attack Sound")]
    [Tooltip("Leave empty if this weapon should have no attack sound.")]
    [SerializeField] protected AudioClip attackSound;

    [SerializeField, Range(0f, 1f)]
    protected float attackSoundVolume = 1f;

    [Tooltip("Optional. If empty, one will be created automatically.")]
    [SerializeField] protected AudioSource audioSource;

    [Header("Timer & References")]
    protected float attackTimer;
    protected Character character;
    protected PlayerMovement playerMovement;

    protected virtual void Awake()
    {
        character = GetComponentInParent<Character>();
        playerMovement = GetComponentInParent<PlayerMovement>();

        SetupAudioSource();
    }

    private void SetupAudioSource()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = attackSoundVolume;
    }

    protected void PlayAttackSound()
    {
        if (attackSound == null)
            return;

        if (audioSource == null)
            return;

        // Stop the previous sound immediately.
        // This prevents sounds from stacking.
        audioSource.Stop();

        // Play only one copy of the sound.
        audioSource.PlayOneShot(
            attackSound,
            attackSoundVolume
        );
    }

    protected void StopAttackSound()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();
    }

    protected virtual void Update()
    {
        // If the game is paused, stop weapon sound immediately.
        if (Time.timeScale == 0f)
        {
            StopAttackSound();
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            Attack();

            float finalAttackSpeed = attackRate;

            if (character != null)
            {
                finalAttackSpeed *=
                    character.getAttackSpeedMultiplier();
            }

            attackTimer =
                1f /
                Mathf.Max(
                    0.0001f,
                    finalAttackSpeed
                );
        }
    }

    public abstract void Attack();

    public virtual float GetCalculatedDamage()
    {
        float finalDamage = damage;

        if (character != null)
        {
            finalDamage *=
                character.getDamageMultiplier();

            if (
                Random.value <
                character.getCritChance() / 100f
            )
            {
                finalDamage *=
                    character.getCritMultiplier();
            }
        }

        return finalDamage;
    }

    public void SetPiercing(
        bool enabled,
        int amount
    )
    {
        if (bouncing || areaDamage)
            return;

        pierce = Mathf.Max(0, amount);

        piercing =
            enabled &&
            pierce > 0;
    }

    public void SetBouncing(
        bool enabled,
        int amount
    )
    {
        if (piercing || areaDamage)
            return;

        bounce = Mathf.Max(0, amount);

        bouncing =
            enabled &&
            bounce > 0;
    }

    public void SetAreaDamage(
        bool enabled,
        float AOE
    )
    {
        if (piercing || bouncing)
            return;

        areaDamage = enabled;

        area =
            Mathf.Max(
                0,
                AOE
            );
    }

    public void ApplyAOEModifier(
        float amount
    )
    {
        if (amount <= 0f)
            return;

        area += amount;
        areaDamage = true;

        transform.localScale +=
            new Vector3(
                amount,
                amount,
                0f
            );
    }

    public virtual bool LevelUp()
    {
        damage *= 1.2f;

        Debug.Log(
            $"[Weapon] {name} damage increased to {damage}"
        );

        return true;
    }

    public virtual bool IncreaseRange(
        float amount
    )
    {
        if (amount <= 0f)
            return false;

        range += amount;

        Debug.Log(
            $"[Weapon] {name} range increased to {range}"
        );

        return true;
    }

    public virtual bool IncreaseKnockback(
        float amount
    )
    {
        if (amount <= 0f)
            return false;

        knockback += amount;

        Debug.Log(
            $"[Weapon] {name} knockback increased to {knockback}"
        );

        return true;
    }

    public virtual bool IncreaseStun(
        float amount
    )
    {
        if (amount <= 0f)
            return false;

        stun += amount;

        Debug.Log(
            $"[Weapon] {name} stun increased to {stun}"
        );

        return true;
    }

    protected virtual void OnDisable()
    {
        StopAttackSound();
    }

    protected virtual void OnDestroy()
    {
        StopAttackSound();
    }
}