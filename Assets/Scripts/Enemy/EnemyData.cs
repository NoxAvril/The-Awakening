using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;

    public float maxHealth = 1f;
    public float moveSpeed = 2f;

    public float collisionDamage = 1f;

    public bool hasRangedAttack = false;
    public float rangeDamage = 1f;
    public float attackInterval = 5f;
    public float range = 3f;
}
