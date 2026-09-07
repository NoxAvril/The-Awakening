using UnityEngine;

public class SMG : GunWeapon
{
    public int bulletCount = 10;
    public float bulletInterval = 0.02f;
    public float spread = 20f;
    public int maxBulletCount = 50;

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
                FireNextBullet();

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
        if (bulletsRemaining > 0)
        {
            return;
        }

        fireDirection = getAimDirection();

        bulletsRemaining = bulletCount;

        FireNextBullet();
        bulletsRemaining--;

        if (bulletsRemaining > 0)
        {
            bulletTimer = bulletInterval;
        }
    }
    
    private void FireNextBullet()
    {
        float angleOffset = Random.Range(-spread / 2f, spread / 2f);

        Vector2 bulletDirection = Quaternion.Euler(0f, 0f, angleOffset) * fireDirection;

        FireBullet(bulletDirection);
    }
}
