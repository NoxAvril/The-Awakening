using UnityEngine;

public enum EnemyUpgradeType
{
    NewEnemyType,
    BaseStatBuff,
    EliteSpawner
}

[CreateAssetMenu(fileName = "NewEnemyUpgrade", menuName = "Game/Enemy Upgrade Data")]
public class EnemyUpgradeData : ScriptableObject
{
    public string id;
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    public EnemyUpgradeType type;

    [Header("For New Enemy Type")]
    public GameObject enemyPrefab;

    [Header("For Base Stat / Elite Buffs")]
    public GameObject targetEnemyPrefab;

    [Header("Stat Multipliers (Base = 2x, Excluded = 1.5x)")]
    public float baseStatMultiplier = 2.0f;       
    public float excludedStatMultiplier = 1.5f;   

    [Header("Elite & Progression Rules")]
    public bool isRepeatable = true;
    public int eliteCountIncrement = 1; 
    public float expRewardModifier = 1.5f; 
}