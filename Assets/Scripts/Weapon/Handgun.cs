using UnityEngine;

public class Handgun : GunWeapon
{
    public int bulletCount = 1;
    public float bulletInterval = 0.05f;
    public int maxBulletCount = 13;

    private int bulletsRemaining;
    private float bulletTimer;

    private Vector2 fireDirection;

    protected override void Update()
    {
        base.Update();

        if (bulletsRemaining > 0)
        {
            bulletTimer -= Time.deltaTime;

            if (bulletTimer <= 0f)
            {
                FireBullet(fireDirection);

                bulletsRemaining--;

                if (bulletsRemaining > 0)
                {
                    bulletTimer = bulletInterval;
                }
            }
        }
    }

    public override void Attack()
    {
        fireDirection = getAimDirection();

        bulletsRemaining = bulletCount;
        
        FireBullet(fireDirection);

        bulletsRemaining--;

        if (bulletsRemaining > 0)
        {
            bulletTimer = bulletInterval;
        }
    }
}
