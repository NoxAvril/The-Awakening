using UnityEngine;

public enum UpgradeType
{
    NewWeapon,
    StatBoost,
    WeaponModifier
}

public enum WeaponModifierType
{
    None,
    AOE,
    Bounce,
    Pierce,

    // New weapon modifier upgrades
    Range,
    Knockback,
    Stun
}

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

[CreateAssetMenu(
    fileName = "UpgradeData",
    menuName = "Game/Upgrade Data"
)]
public class UpgradeData : ScriptableObject
{
    public string id;

    public string upgradeName;

    [TextArea]
    public string description;

    public Sprite icon;

    public UpgradeType type;

    [Header("Leveling Rules")]
    public int currentLevel = 0;

    public bool isUncapped = false;

    public int maxLevel = 5;

    [Header("Weapon Specific Settings")]
    public GameObject weaponPrefab;

    public WeaponModifierType modifierType =
        WeaponModifierType.None;

    [Header("Stat Boost Settings")]
    public StatType targetStat;

    public float statIncreaseAmount = 0.1f;

    public StatType weightBiasStat =
        StatType.Damage;


    // ============================================================
    // MAX LEVEL CHECK
    // ============================================================

    public bool IsMaxed(
        Character character,
        PlayerHealth health)
    {
        // Uncapped upgrades can continue forever.
        if (isUncapped)
            return false;

        // Weapon modifiers use their own currentLevel.
        if (type == UpgradeType.WeaponModifier)
        {
            return currentLevel >= maxLevel;
        }

        // New weapon upgrades use currentLevel as well.
        if (type == UpgradeType.NewWeapon)
        {
            return currentLevel >= maxLevel;
        }

        switch (targetStat)
        {
            case StatType.FireRate:

                if (character == null)
                    return currentLevel >= maxLevel;

                return character.getAttackSpeedMultiplier()
                       >= 3.0f;


            case StatType.MoveSpeed:

                if (health == null)
                    return currentLevel >= maxLevel;

                PlayerMovement movement =
                    health.GetComponent<PlayerMovement>();

                if (movement == null)
                    return currentLevel >= maxLevel;

                return movement.GetMoveSpeedBonus()
                       >= 0.50f;


            case StatType.AOE:

                if (character == null)
                    return currentLevel >= maxLevel;

                return character.getAOEBonus()
                       >= 1.0f;


            case StatType.CritChance:

                if (character == null)
                    return currentLevel >= maxLevel;

                return character.getCritChance()
                       >= 100f;


            default:

                return currentLevel >= maxLevel;
        }
    }
}
