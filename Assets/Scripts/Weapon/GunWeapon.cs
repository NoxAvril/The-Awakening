using UnityEngine;
using UnityEngine.InputSystem;

public class GunWeapon : Weapon
{
    public float bulletSpeed = 10f;
    public GameObject bulletPrefab;
    public float bulletSpawnDistance = 0.5f;

    public bool aimMode = false;
    public float attackTimer;

    private Transform player;
    private Character character;

    private void Awake()
    {
        player = GetComponentInParent<PlayerMovement>().transform;
        character = GetComponentInParent<Character>();
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

            attackTimer = 1f / finalAttackSpeed;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            aimMode = !aimMode;
        }
    }

    public override void Attack()
    {
        FireBullet(getAimDirection());
    }

    protected void FireBullet(Vector2 direction)
    {
        Vector2 spawnPosition =
            (Vector2)player.position +
            direction * bulletSpawnDistance;

        GameObject bulletObject =
            Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

        Bullet bullet = bulletObject.GetComponent<Bullet>();

        float finalDamage = damage;

        if (character != null)
        {
            finalDamage *= character.getDamageMultiplier();

            if (Random.value < character.getCritChance() / 100f)
            {
                finalDamage *= character.getCritMultiplier();
            }
        }

        bullet.Setup(
            direction,
            bulletSpeed,
            finalDamage,
            range,
            pierce,
            bounce,
            area,
            areaDamage,
            knockback,
            stun
        );
    }

    protected Vector2 getAimDirection()
    {
        if (aimMode)
        {
            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
            Vector2 playerScreenPosition = Camera.main.WorldToScreenPoint(player.position);

            return (mouseScreenPosition - playerScreenPosition).normalized;
        }

        return player.GetComponent<PlayerMovement>().lastMoveDirection;
    }
}