using UnityEngine;

public class Shotgun : GunWeapon
{
    public int bulletCount = 5;
    public float spread = 30f;

    public override void Attack()
    {
        Vector2 direction = getAimDirection();

        for (int i = 0; i < bulletCount; i++)
        {
            float angle;

            if (bulletCount == 1)
            {
                angle = 0f;
            }
            else
            {
                angle = Mathf.Lerp(-spread / 2f, spread / 2f, (float)i / (bulletCount - 1f));
            }
            
            Vector2 bulletDirection = Quaternion.Euler(0f, 0f, angle) * direction;

            FireBullet(bulletDirection);
        }
    }
}
