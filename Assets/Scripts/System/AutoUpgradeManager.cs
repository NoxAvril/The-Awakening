using UnityEngine;
using System.Collections.Generic;

public class AutoUpgradeManager : MonoBehaviour
{
    [Header("Upgrade Pools")]
    [SerializeField]
    private List<UpgradeData> masterUpgradePool = new List<UpgradeData>();

    [Header("References")]
    [SerializeField]
    private GameObject playerObject;

    private Character playerCharacter;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private WeaponManager weaponManager;
    private bool processingLevelUp;

    private void Awake()
    {
        if (playerObject != null)
        {
            InitializePlayerReferences(playerObject);
        }
        else
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                InitializePlayerReferences(foundPlayer);
            }
        }
    }

    private void Start()
    {
        foreach (UpgradeData upgrade in masterUpgradePool)
        {
            if (upgrade != null)
            {
                upgrade.currentLevel = 0;
            }
        }
    }

    private void InitializePlayerReferences(GameObject targetPlayer)
    {
        playerObject = targetPlayer;
        playerCharacter = targetPlayer.GetComponent<Character>();
        playerHealth = targetPlayer.GetComponent<PlayerHealth>();
        playerMovement = targetPlayer.GetComponent<PlayerMovement>();
        weaponManager = targetPlayer.GetComponent<WeaponManager>();
    }

    private void OnEnable()
    {
        PlayerLevelSystem.OnLevelUp += ProcessAutomaticLevelUp;
    }

    private void OnDisable()
    {
        PlayerLevelSystem.OnLevelUp -= ProcessAutomaticLevelUp;
    }

    private void ProcessAutomaticLevelUp()
    {
        if (processingLevelUp)
        {
            Debug.LogWarning("[AutoUpgrade] Already processing a level up.");
            return;
        }

        processingLevelUp = true;

        bool success = TryCompleteLevelUpUpgrade();

        if (!success)
        {
            Debug.LogError("[AutoUpgrade] No upgrade could be successfully applied. Check masterUpgradePool in Inspector!");
        }

        processingLevelUp = false;
    }

    private bool TryCompleteLevelUpUpgrade()
    {
        List<UpgradeData> attemptedUpgrades = new List<UpgradeData>();

        while (true)
        {
            UpgradeData chosenUpgrade = SelectBestUpgrade(attemptedUpgrades);

            if (chosenUpgrade == null)
            {
                Debug.LogWarning("[AutoUpgrade] No valid upgrade remains.");
                return false;
            }

            Debug.Log($"[AutoUpgrade] Trying upgrade: {chosenUpgrade.upgradeName}");

            bool success = ApplyUpgrade(chosenUpgrade);

            if (success)
            {
                chosenUpgrade.currentLevel++;

                Debug.Log($"[AutoUpgrade] SUCCESS: {chosenUpgrade.upgradeName} Level {chosenUpgrade.currentLevel}");

                // Safety fallback if Instance wasn't set on Awake
                LevelUpDisplayUI ui = LevelUpDisplayUI.Instance != null ? LevelUpDisplayUI.Instance : FindFirstObjectByType<LevelUpDisplayUI>();

                if (ui != null)
                {
                    ui.ShowUpgradeNotification(chosenUpgrade);
                }
                else
                {
                    Debug.LogError("[AutoUpgrade] LevelUpDisplayUI instance not found in scene!");
                }

                return true;
            }

            attemptedUpgrades.Add(chosenUpgrade);
            Debug.LogWarning($"[AutoUpgrade] FAILED: {chosenUpgrade.upgradeName}. Trying another upgrade.");
        }
    }

    private UpgradeData SelectBestUpgrade(List<UpgradeData> attemptedUpgrades)
    {
        RefreshWeaponManagerReference();

        List<UpgradeData> validUpgrades = new List<UpgradeData>();

        foreach (UpgradeData upgrade in masterUpgradePool)
        {
            if (upgrade == null) continue;
            if (attemptedUpgrades.Contains(upgrade)) continue;
            if (upgrade.IsMaxed(playerCharacter, playerHealth)) continue;
            if (!CanUpgradeBeApplied(upgrade)) continue;

            validUpgrades.Add(upgrade);
        }

        if (validUpgrades.Count == 0) return null;

        if (weaponManager != null && weaponManager.hasEmptySlot())
        {
            List<UpgradeData> weaponUpgrades = validUpgrades.FindAll(u => u.type == UpgradeType.NewWeapon);

            if (weaponUpgrades.Count > 0)
            {
                UpgradeData chosen = weaponUpgrades[Random.Range(0, weaponUpgrades.Count)];
                Debug.Log($"[AutoUpgrade] New weapon chosen: {chosen.upgradeName}");
                return chosen;
            }
        }

        return validUpgrades[Random.Range(0, validUpgrades.Count)];
    }

    private bool CanUpgradeBeApplied(UpgradeData upgrade)
    {
        if (upgrade == null) return false;

        switch (upgrade.type)
        {
            case UpgradeType.StatBoost:
                return CanStatUpgradeBeApplied(upgrade);
            case UpgradeType.NewWeapon:
                return CanNewWeaponUpgradeBeApplied(upgrade);
            case UpgradeType.WeaponModifier:
                return CanWeaponModifierBeApplied(upgrade);
        }

        return false;
    }

    private bool CanStatUpgradeBeApplied(UpgradeData upgrade)
    {
        switch (upgrade.targetStat)
        {
            case StatType.Damage:
            case StatType.AOE:
            case StatType.CritDamageMultiplier:
            case StatType.FireRate:
            case StatType.CritChance:
                return playerCharacter != null;
            case StatType.Health:
                return playerHealth != null;
            case StatType.MoveSpeed:
                return playerMovement != null;
        }
        return false;
    }

    private bool CanNewWeaponUpgradeBeApplied(UpgradeData upgrade)
    {
        RefreshWeaponManagerReference();

        if (weaponManager == null || upgrade.weaponPrefab == null) return false;

        Weapon prefabWeapon = upgrade.weaponPrefab.GetComponent<Weapon>();
        if (prefabWeapon == null) return false;

        System.Type requestedType = prefabWeapon.GetType();

        if (weaponManager.HasWeaponType(requestedType)) return true;
        if (!weaponManager.hasEmptySlot()) return false;

        if (prefabWeapon is GunWeapon && weaponManager.HasGunWeapon())
        {
            return false;
        }

        return true;
    }

    private bool CanWeaponModifierBeApplied(UpgradeData upgrade)
    {
        RefreshWeaponManagerReference();

        if (weaponManager == null) return false;

        foreach (Weapon weapon in weaponManager.weaponSlots)
        {
            if (weapon == null) continue;

            switch (upgrade.modifierType)
            {
                case WeaponModifierType.AOE:
                    return true;
                case WeaponModifierType.Pierce:
                    if (!weapon.piercing && !weapon.bouncing && !weapon.areaDamage && weapon.pierce < 7)
                        return true;
                    break;
                case WeaponModifierType.Bounce:
                    if (!weapon.piercing && !weapon.bouncing && !weapon.areaDamage && weapon.bounce < 7)
                        return true;
                    break;
                case WeaponModifierType.Range:
                case WeaponModifierType.Knockback:
                case WeaponModifierType.Stun:
                    if (upgrade.statIncreaseAmount > 0f) return true;
                    break;
            }
        }

        return false;
    }

    private bool ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null) return false;

        switch (upgrade.type)
        {
            case UpgradeType.StatBoost:
                return ApplyStatBoost(upgrade);
            case UpgradeType.NewWeapon:
                return weaponManager != null && weaponManager.TryEquipOrUpgradeWeapon(upgrade.weaponPrefab);
            case UpgradeType.WeaponModifier:
                return ApplyWeaponModifier(upgrade);
        }

        return false;
    }

    private bool ApplyStatBoost(UpgradeData upgrade)
    {
        switch (upgrade.targetStat)
        {
            case StatType.Damage:
                if (playerCharacter == null) return false;
                playerCharacter.AddDamageMultiplier(upgrade.statIncreaseAmount);
                return true;
            case StatType.Health:
                if (playerHealth == null) return false;
                playerHealth.IncreaseMaxHealth(upgrade.statIncreaseAmount);
                return true;
            case StatType.MoveSpeed:
                if (playerMovement == null) return false;
                playerMovement.AddMoveSpeedBonus(upgrade.statIncreaseAmount);
                return true;
            case StatType.AOE:
                if (playerCharacter == null) return false;
                playerCharacter.AddAOEBonus(upgrade.statIncreaseAmount);
                return true;
            case StatType.CritDamageMultiplier:
                if (playerCharacter == null) return false;
                playerCharacter.AddCritDamageMultiplier(upgrade.statIncreaseAmount);
                return true;
            case StatType.FireRate:
                if (playerCharacter == null) return false;
                playerCharacter.AddAttackSpeedMultiplier(upgrade.statIncreaseAmount);
                return true;
            case StatType.CritChance:
                if (playerCharacter == null) return false;
                playerCharacter.addCritChance(upgrade.statIncreaseAmount);
                return true;
        }
        return false;
    }

    private bool ApplyWeaponModifier(UpgradeData upgrade)
    {
        if (weaponManager == null) return false;

        bool success = false;

        foreach (Weapon weapon in weaponManager.weaponSlots)
        {
            if (weapon == null) continue;

            switch (upgrade.modifierType)
            {
                case WeaponModifierType.Pierce:
                    if (!weapon.piercing && !weapon.bouncing && !weapon.areaDamage && weapon.pierce < 7)
                    {
                        weapon.SetPiercing(true, weapon.pierce + 1);
                        success = true;
                    }
                    break;
                case WeaponModifierType.Bounce:
                    if (!weapon.piercing && !weapon.bouncing && !weapon.areaDamage && weapon.bounce < 7)
                    {
                        weapon.SetBouncing(true, weapon.bounce + 1);
                        success = true;
                    }
                    break;
                case WeaponModifierType.AOE:
                    if (upgrade.statIncreaseAmount <= 0f) break;
                    weapon.ApplyAOEModifier(upgrade.statIncreaseAmount);
                    success = true;
                    break;
                case WeaponModifierType.Range:
                    if (weapon.IncreaseRange(upgrade.statIncreaseAmount)) success = true;
                    break;
                case WeaponModifierType.Knockback:
                    if (weapon.IncreaseKnockback(upgrade.statIncreaseAmount)) success = true;
                    break;
                case WeaponModifierType.Stun:
                    if (weapon.IncreaseStun(upgrade.statIncreaseAmount)) success = true;
                    break;
            }
        }

        return success;
    }

    // Fixed redundant return statement bug
    private void RefreshWeaponManagerReference()
    {
        if (weaponManager != null) return;

        if (playerObject != null)
        {
            weaponManager = playerObject.GetComponent<WeaponManager>();
        }

        if (weaponManager == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                InitializePlayerReferences(foundPlayer);
            }
        }
    }
}