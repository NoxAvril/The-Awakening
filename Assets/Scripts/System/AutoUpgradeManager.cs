using UnityEngine;
using System.Collections.Generic;

public class AutoUpgradeManager : MonoBehaviour
{
    [Header("Upgrade Pools")]
    [SerializeField] private List<UpgradeData> masterUpgradePool = new List<UpgradeData>();

    [Header("References")]
    [SerializeField] private GameObject playerObject; 

    private Character playerCharacter;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        if (playerObject != null)
        {
            playerCharacter = playerObject.GetComponent<Character>();
            playerHealth = playerObject.GetComponent<PlayerHealth>();
            playerMovement = playerObject.GetComponent<PlayerMovement>();
        }
        else
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                playerObject = foundPlayer;
                playerCharacter = foundPlayer.GetComponent<Character>();
                playerHealth = foundPlayer.GetComponent<PlayerHealth>();
                playerMovement = foundPlayer.GetComponent<PlayerMovement>();
            }
        }
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
        UpgradeData chosenUpgrade = SelectBestUpgrade();

        if (chosenUpgrade != null)
        {
            ApplyUpgrade(chosenUpgrade);
            
            if (LevelUpDisplayUI.Instance != null)
            {
                LevelUpDisplayUI.Instance.ShowUpgradeNotification(chosenUpgrade);
            }
        }
        else
        {
            Debug.LogWarning("AutoUpgradeManager: No valid upgrades available!");
        }
    }

    private UpgradeData SelectBestUpgrade()
    {
        List<UpgradeData> validUpgrades = new List<UpgradeData>();

        foreach (var upgrade in masterUpgradePool)
        {
            if (upgrade != null && (playerCharacter == null || !upgrade.IsMaxed(playerCharacter, playerHealth)))
            {
                validUpgrades.Add(upgrade);
            }
        }

        if (validUpgrades.Count == 0) return null;

        // Prioritize New Weapons if slots are empty (Bias Logic)
        bool hasEmptySlot = CheckForEmptyWeaponSlots();
        if (hasEmptySlot)
        {
            List<UpgradeData> weaponUpgrades = validUpgrades.FindAll(u => u.type == UpgradeType.NewWeapon);
            if (weaponUpgrades.Count > 0)
            {
                return weaponUpgrades[Random.Range(0, weaponUpgrades.Count)];
            }
        }

        // Fallback to random pick from valid pool
        return validUpgrades[Random.Range(0, validUpgrades.Count)];
    }

    private bool CheckForEmptyWeaponSlots()
    {
        // Stub: Replace with your actual weapon slot check logic if available
        return false; 
    }

    private void ApplyUpgrade(UpgradeData upgrade)
    {
        upgrade.currentLevel++;
        Debug.Log($"[Auto-Upgrade] Character automatically chose: {upgrade.upgradeName} (Level {upgrade.currentLevel})");

        switch (upgrade.type)
        {
            case UpgradeType.StatBoost:
                ApplyStatBoost(upgrade);
                break;

            case UpgradeType.NewWeapon:
                if (upgrade.weaponPrefab != null && playerCharacter != null)
                {
                    // playerCharacter.EquipWeapon(upgrade.weaponPrefab);
                }
                break;

            case UpgradeType.WeaponModifier:
                ApplyWeaponModifier(upgrade);
                break;
        }
    }

    private void ApplyStatBoost(UpgradeData upgrade)
    {
        switch (upgrade.targetStat)
        {
            case StatType.Damage:
                if (playerCharacter != null) playerCharacter.AddDamageMultiplier(upgrade.statIncreaseAmount);
                break;
            case StatType.Health:
                if (playerHealth != null) playerHealth.IncreaseMaxHealth(upgrade.statIncreaseAmount);
                break;
            case StatType.MoveSpeed:
                if (playerMovement != null) playerMovement.AddMoveSpeedBonus(upgrade.statIncreaseAmount);
                break;
            case StatType.AOE:
                if (playerCharacter != null) playerCharacter.AddAOEBonus(upgrade.statIncreaseAmount);
                break;
            case StatType.CritDamageMultiplier:
                if (playerCharacter != null) playerCharacter.AddCritDamageMultiplier(upgrade.statIncreaseAmount);
                break;
        }
    }

    private void ApplyWeaponModifier(UpgradeData upgrade)
    {
        // Apply piercing/bounce capped at 7, or area modifiers
        // (Assumes your weapon instances have these properties accessible)
        /*
        foreach (var weapon in playerCharacter.GetActiveWeapons())
        {
            if (upgrade.modifierType == WeaponModifierType.Pierce && weapon.pierceCount < 7)
            {
                weapon.pierceCount++;
            }
            else if (upgrade.modifierType == WeaponModifierType.Bounce && weapon.bounceCount < 7)
            {
                weapon.bounceCount++;
            }
        }
        */
    }
}