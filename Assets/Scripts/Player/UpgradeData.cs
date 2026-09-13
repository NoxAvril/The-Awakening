using UnityEngine;

public enum UpgradeType { NewWeapon, StatBoost, WeaponModifier }
public enum WeaponModifierType { None, AOE, Bounce, Pierce }
public enum StatType 
{ 
    Health, 
    Damage, 
    CritDamageMultiplier, 
    FireRate, 
    MoveSpeed, 
    AOE, 
    CritChance, 
    Other 
}

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string id;
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;
    public UpgradeType type;

    [Header("Leveling Rules")]
    public int currentLevel = 0;
    public bool isUncapped = false;
    public int maxLevel = 5;

    [Header("Weapon Specific Settings")]
    public GameObject weaponPrefab;
    public WeaponModifierType modifierType = WeaponModifierType.None;

    [Header("Stat Boost Settings")]
    public StatType targetStat;
    public float statIncreaseAmount = 0.1f;
    public StatType weightBiasStat = StatType.Damage;

    public bool IsMaxed(Character character, PlayerHealth health)
    {
        if (isUncapped) return false;

        switch (targetStat)
        {
            case StatType.FireRate:
                return character.getAttackSpeedMultiplier() >= 3.0f; // Cap at +200% attack speed

            case StatType.MoveSpeed:
                return character.getMoveSpeedBonus() >= 0.50f;      // Max +50% move speed

            case StatType.AOE:
                return character.getAOEBonus() >= 1.50f;            // Max +150% AOE

            case StatType.CritChance:
                return character.getCritChance() >= 100f;           // Max 100% chance

            default:
                return currentLevel >= maxLevel;
        }
    }
}