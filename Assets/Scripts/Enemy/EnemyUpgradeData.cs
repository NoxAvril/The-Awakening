using UnityEngine;

public enum EnemyUpgradeType
{
    NewEnemyType,
    BaseStatBuff,
    EliteSpawner
}

public enum EnemyStatType
{
    Health,
    CollisionDamage,
    RangeDamage,
    MoveSpeed,
    Range,
    AttackInterval
}

[CreateAssetMenu(
    fileName = "NewEnemyUpgrade",
    menuName = "Game/Enemy Upgrade Data"
)]
public class EnemyUpgradeData : ScriptableObject
{
    [Header("General")]
    public string id;
    public string upgradeName;

    [TextArea]
    public string description;

    public Sprite icon;

    public EnemyUpgradeType type;

    [Header("New Enemy Type")]
    public GameObject enemyPrefab;

    [Header("Target Enemy")]
    public GameObject targetEnemyPrefab;

    [Header("Normal Stat Upgrade")]
    public EnemyStatType targetStat;

    public float statIncreaseAmount = 10f;

    [Header("Elite")]
    public bool isRepeatable = true;
    public int eliteCountIncrement = 1;

    [Header("Elite Scaling")]
    public float baseStatMultiplier = 2.0f;
    public float excludedStatMultiplier = 1.5f;

    [Header("Other")]
    public float expRewardModifier = 1.5f;
}