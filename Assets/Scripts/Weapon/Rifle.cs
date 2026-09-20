using UnityEngine;

public class Rifle : GunWeapon
{
    public int bulletCount = 1;
    public float bulletInterval = 0.20f;
    public int maxBulletCount = 7;

    private int bulletsRemaining;
    private float bulletTimer;
    private Vector2 fireDirection;

    protected override void Update()
    {
        base.Update();

        if (
            bulletsRemaining > 0
        )
        {
            bulletTimer -=
                Time.deltaTime;

            if (
                bulletTimer <= 0f
            )
            {
                FireBullet(
                    fireDirection,
                    true
                );

                bulletsRemaining--;

                if (
                    bulletsRemaining > 0
                )
                {
                    bulletTimer =
                        bulletInterval;
                }
            }
        }
    }

    public override void Attack()
    {
        fireDirection =
            getAimDirection();

        bulletsRemaining =
            Mathf.Clamp(
                bulletCount,
                1,
                maxBulletCount
            );

        FireBullet(
            fireDirection,
            true
        );

        bulletsRemaining--;

        if (
            bulletsRemaining > 0
        )
        {
            bulletTimer =
                bulletInterval;
        }
    }
}