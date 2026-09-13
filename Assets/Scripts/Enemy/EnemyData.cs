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

    [Header("EXP Drop Settings")]
    public int baseExpReward = 1;
    public float expMultiplier = 1f; // Individual base multiplier (e.g. Slime = 1.0, Elite = 2.5)
}