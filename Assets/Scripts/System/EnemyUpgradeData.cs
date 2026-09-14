using UnityEngine;

public enum EnemyUpgradeType
{
    StatBoost,
    NewEnemyType
}

public enum EnemyStatType
{
    MaxHealth,
    MoveSpeed,
    CollisionDamage,
    RangeDamage,
    AttackInterval,
    SpawnRate
}

public enum CardRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "EnemyUpgradeData", menuName = "Game/EnemyUpgradeData")]
public class EnemyUpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;
    public CardRarity rarity = CardRarity.Common;

    public EnemyUpgradeType type;

    [Header("Stat Boost Settings (used when type = StatBoost)")]
    public EnemyStatType targetStat;
    public float statIncreaseAmount;
    public bool isMultiplier = false; // false = add this amount, true = multiply by this amount

    [Header("New Enemy Settings (used when type = NewEnemyType)")]
    public GameObject enemyPrefabToUnlock;
}
