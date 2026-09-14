using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterData[] characters;
    private WeaponManager weaponManager;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;

    // Active Combat Stats
    private float damageMultiplier = 1f;
    private float attackSpeedMultiplier = 1f;
    private float critChance = 0f;       // Base stored as percentage (0 to 100) or normalized (0 to 1)
    private float critMultiplier = 2f;
    private float aoeBonus = 0f;
    private float moveSpeedBonus = 0f;

    public void Awake()
    {
        weaponManager = GetComponent<WeaponManager>();
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        string selectedCharacterName = PlayerPrefs.GetString("SelectedCharacter", "");
        bool characterFound = false;

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null && characters[i].characterName == selectedCharacterName)
            {
                SelectCharacter(i);
                characterFound = true;
                break;
            }
        }

        // Fallback to default index 0 if PlayerPrefs selection wasn't found
        if (!characterFound && characters.Length > 0)
        {
            SelectCharacter(0);
        }
    }

    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length)
            return;

        CharacterData selectedCharacter = characters[characterIndex];
        
        if (playerHealth != null) playerHealth.setMaxHealth(selectedCharacter.maxHealth);
        if (playerMovement != null) playerMovement.setMoveSpeed(selectedCharacter.movespeed);

        if (selectedCharacter.characterSprite != null)
        {
            spriteRenderer.sprite = selectedCharacter.characterSprite;
        }

        damageMultiplier = selectedCharacter.damageMultiplier;
        attackSpeedMultiplier = selectedCharacter.attackSpeedMultiplier;
        critChance = selectedCharacter.critChance;
        critMultiplier = selectedCharacter.critMultiplier;

        if (selectedCharacter.startingWeapon != null && weaponManager != null)
        {
            Weapon weapon = Instantiate(selectedCharacter.startingWeapon, transform);
            weaponManager.EquipStartingWeapon(weapon);
        }
    }

    // --- Stat Getters ---
    public float getDamageMultiplier() => damageMultiplier;
    public float getAttackSpeedMultiplier() => attackSpeedMultiplier;
    public float getCritChance() => critChance;
    public float getCritMultiplier() => critMultiplier;
    public float getAOEBonus() => aoeBonus;
    public float getMoveSpeedBonus() => moveSpeedBonus;

    // --- Stat Modifiers (Used by Upgrades) ---
    public void AddDamageMultiplier(float amount) => damageMultiplier += amount;
    public void AddAttackSpeedMultiplier(float amount) => attackSpeedMultiplier += amount;
    public void addCritChance(float chance) => critChance += chance;
    public void addCritMultiplier(float multiplier) => critMultiplier += multiplier;
    public void AddAOEBonus(float amount) => aoeBonus += amount;

    public void AddMoveSpeedBonus(float amount)
    {
        moveSpeedBonus += amount;
        if (playerMovement != null)
        {
            playerMovement.moveSpeedMultiplier(1f + amount);
        }
    }
<<<<<<< Updated upstream
=======

    public float getAttackSpeedMultiplier()
    {
        return attackSpeedMultiplier;
    }

    public float getCritChance()
    {
        return critChance;
    }

    public float getCritMultiplier()
    {
        return critMultiplier;
    }

    public void MultiplyDamage(float multiplier)
    {
        damageMultiplier *= multiplier;
    }

    public void MultiplyAttackSpeed(float multiplier)
    {
        attackSpeedMultiplier *= multiplier;
    }

    public void addCritChance(float chance)
    {
        critChance += chance;
    }

    public void addCritMultiplier(float multiplier)
    {
        critMultiplier += multiplier;
    }
>>>>>>> Stashed changes
}