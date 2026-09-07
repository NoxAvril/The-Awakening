using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float speed;
    private float damage;
    private float range;
    private Vector2 direction;
    private Vector3 startPosition;

    private float knockback;
    private float stun;

    private int piercing;
    private int bouncing;
    private float area;
    private bool areaDamage;    

    // Update is called once per frame
    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        if (Vector3.Distance(startPosition, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    public void Setup(Vector2 bulletDirection, float bulletSpeed, float bulletDamage, float bulletRange, int bulletPiercing, int bulletBouncing, float bulletArea, bool bulletAreaDamage, float bulletKnockback, float bulletStun)
    {
        direction = bulletDirection.normalized;
        speed = bulletSpeed;
        damage = bulletDamage;
        range = bulletRange;

        piercing = bulletPiercing;
        bouncing = bulletBouncing;
        area = bulletArea;
        areaDamage = bulletAreaDamage;

        knockback = bulletKnockback;
        stun = bulletStun;

        startPosition = transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg -90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy"))
            return;
        
        EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }

        if (piercing > 0)
        {
            piercing--;
        }
        else if (bouncing > 0)
        {
            bouncing--;
            Bounce();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Bounce()
    {
        //
    }
}