using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Base Weapon Stats")]
    public float damage = 1f;
    public float attackRate = 1f;

    // Maximum attack/projectile distance.
    public float range = 1f;

    // Knockback force applied to enemies.
    public float knockback = 0f;

    // Stun duration in seconds.
    public float stun = 0f;


    [Header("Behavior Properties")]
    public float area = 0f;

    public int pierce = 0;

    public int bounce = 0;

    public bool piercing = false;

    public bool bouncing = false;

    public bool areaDamage = false;


    [Header("Timer & References")]
    protected float attackTimer;

    protected Character character;

    protected PlayerMovement playerMovement;


    // ============================================================
    // INITIALIZATION
    // ============================================================

    protected virtual void Awake()
    {
        character =
            GetComponentInParent<Character>();

        playerMovement =
            GetComponentInParent<PlayerMovement>();
    }


    // ============================================================
    // WEAPON UPDATE
    // ============================================================

    protected virtual void Update()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            Attack();

            float finalAttackSpeed =
                attackRate;

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


    // ============================================================
    // ATTACK
    // ============================================================

    public abstract void Attack();


    // ============================================================
    // DAMAGE
    // ============================================================

    public virtual float GetCalculatedDamage()
    {
        float finalDamage = damage;

        if (character != null)
        {
            finalDamage *=
                character.getDamageMultiplier();

            if (Random.value <
                character.getCritChance() / 100f)
            {
                finalDamage *=
                    character.getCritMultiplier();
            }
        }

        return finalDamage;
    }


    // ============================================================
    // PIERCE
    // ============================================================

    public void SetPiercing(
        bool enabled,
        int amount)
    {
        // Cannot combine with bounce or AOE.
        if (bouncing || areaDamage)
            return;

        pierce =
            Mathf.Max(0, amount);

        piercing =
            enabled &&
            pierce > 0;
    }


    // ============================================================
    // BOUNCE
    // ============================================================

    public void SetBouncing(
        bool enabled,
        int amount)
    {
        // Cannot combine with pierce or AOE.
        if (piercing || areaDamage)
            return;

        bounce =
            Mathf.Max(0, amount);

        bouncing =
            enabled &&
            bounce > 0;
    }


    // ============================================================
    // AOE
    // ============================================================

    public void SetAreaDamage(
        bool enabled,
        float AOE)
    {
        // Cannot combine with pierce or bounce.
        if (piercing || bouncing)
            return;

        areaDamage =
            enabled;

        area =
            Mathf.Max(0, AOE);
    }


    public void ApplyAOEModifier(
        float amount)
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


    // ============================================================
    // NORMAL WEAPON LEVEL UP
    // ============================================================
    //
    // IMPORTANT:
    // Weapon Level Up ONLY increases damage.
    //
    // Range, knockback and stun are separate upgrades.
    //
    // ============================================================

    public virtual bool LevelUp()
    {
        damage *= 1.2f;

        Debug.Log(
            $"[Weapon] {name} damage increased to {damage}"
        );

        return true;
    }


    // ============================================================
    // RANGE UPGRADE
    // ============================================================

    public virtual bool IncreaseRange(
        float amount)
    {
        if (amount <= 0f)
            return false;

        range += amount;

        Debug.Log(
            $"[Weapon] {name} range increased to {range}"
        );

        return true;
    }


    // ============================================================
    // KNOCKBACK UPGRADE
    // ============================================================

    public virtual bool IncreaseKnockback(
        float amount)
    {
        if (amount <= 0f)
            return false;

        knockback += amount;

        Debug.Log(
            $"[Weapon] {name} knockback increased to {knockback}"
        );

        return true;
    }


    // ============================================================
    // STUN UPGRADE
    // ============================================================

    public virtual bool IncreaseStun(
        float amount)
    {
        if (amount <= 0f)
            return false;

        stun += amount;

        Debug.Log(
            $"[Weapon] {name} stun increased to {stun}"
        );

        return true;
    }
}
