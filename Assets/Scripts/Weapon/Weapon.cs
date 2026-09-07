    using UnityEngine;

    public abstract class Weapon : MonoBehaviour
    {
        public float damage = 1f;
        public float attackRate = 1f;
        public float range = 1f;
        public float knockback = 0f;
        public float stun = 0f;

        public float area = 0f;
        public int pierce = 0;
        public int bounce = 0;

        public bool piercing = false;
        public bool bouncing = false;
        public bool areaDamage = false;
        public abstract void Attack();

        public void SetPiercing(bool enabled, int amount)
        {
            pierce = Mathf.Max(0, amount);
            piercing = enabled && pierce > 0;
            bouncing = false;
            areaDamage = false;
        }

        public void SetBouncing(bool enabled, int amount)
        {
            bounce = Mathf.Max(0, amount);
            bouncing = enabled && bounce > 0;
            piercing = false;
            areaDamage = false;
        }

        public void SetAreaDamage(bool enabled, float AOE)
        {
            areaDamage = enabled;
            area = Mathf.Max(0, AOE);

            if (areaDamage)
            {
                piercing = false;
                bouncing = false;
                pierce = 0;
                bounce = 0;
            }
        }
    }
