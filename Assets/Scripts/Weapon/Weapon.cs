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

    [Header("Timer & References")]
    protected float attackTimer;
    protected Character character;
    protected PlayerMovement playerMovement;

    protected virtual void Awake()
    {
        character = GetComponentInParent<Character>();
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    protected virtual void Update()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            Attack();

            float finalAttackSpeed = attackRate;
            if (character != null)
            {
                finalAttackSpeed *= character.getAttackSpeedMultiplier();
            }

            attackTimer = 1f / Mathf.Max(0.0001f, finalAttackSpeed);
        }
    }

    public abstract void Attack();

    public virtual float GetCalculatedDamage()
    {
        float finalDamage = damage;

        if (character != null)
        {
            finalDamage *= character.getDamageMultiplier();

            if (Random.value < (character.getCritChance() / 100f))
            {
                finalDamage *= character.getCritMultiplier();
            }
        }

        return finalDamage;
    }

    public void SetPiercing(bool enabled, int amount)
    {
        if (bouncing || areaDamage) return; 
        pierce = Mathf.Max(0, amount);
        piercing = enabled && pierce > 0;
    }

    public void SetBouncing(bool enabled, int amount)
    {
        if (piercing || areaDamage) return; 
        bounce = Mathf.Max(0, amount);
        bouncing = enabled && bounce > 0;
    }

    public void SetAreaDamage(bool enabled, float AOE)
    {
        if (piercing || bouncing) return; 
        areaDamage = enabled;
        area = Mathf.Max(0, AOE);
    }
}