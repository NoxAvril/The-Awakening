using UnityEngine;
using UnityEngine.InputSystem;

public class GunWeapon : Weapon
{
    public float bulletSpeed = 10f;
    public GameObject bulletPrefab;
    public float bulletSpawnDistance = 0.5f;

    public bool aimMode = false;

    protected override void Update()
    {
        base.Update();

        if (
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame
        )
        {
            aimMode = !aimMode;
        }
    }

    public override void Attack()
    {
        FireBullet(
            getAimDirection(),
            true
        );
    }

    protected void FireBullet(
        Vector2 direction
    )
    {
        FireBullet(
            direction,
            true
        );
    }

    protected void FireBullet(
        Vector2 direction,
        bool playSound
    )
    {
        if (
            bulletPrefab == null ||
            playerMovement == null
        )
        {
            return;
        }

        Vector2 spawnPosition =
            (Vector2)playerMovement.transform.position +
            direction *
            bulletSpawnDistance;

        GameObject bulletObject =
            Instantiate(
                bulletPrefab,
                spawnPosition,
                Quaternion.identity
            );

        if (
            bulletObject.TryGetComponent<Bullet>(
                out var bullet
            ))
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

            if (playSound)
            {
                bullet.SetFirstMovementCallback(
                    PlayAttackSound
                );
            }
        }
    }

    protected Vector2 getAimDirection()
    {
        if (
            aimMode &&
            Mouse.current != null
        )
        {
            Vector2 mouseScreenPosition =
                Mouse.current.position.ReadValue();

            Vector2 playerScreenPosition =
                Camera.main.WorldToScreenPoint(
                    playerMovement.transform.position
                );

            return (
                mouseScreenPosition -
                playerScreenPosition
            ).normalized;
        }

        if (
            playerMovement != null
        )
        {
            return playerMovement.lastMoveDirection;
        }

        return Vector2.right;
    }
}