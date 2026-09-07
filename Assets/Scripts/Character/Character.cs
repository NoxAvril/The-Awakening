using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterData[] characters;
    private WeaponManager weaponManager;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;

    private float damageMultiplier;
    private float attackSpeedMultiplier;
    private float critChance;
    private float critMultiplier;

    public void Awake()
    {
        weaponManager = GetComponent<WeaponManager>();
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();

        string selectedCharacterName = PlayerPrefs.GetString("SelectedCharacter", "");

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i].characterName == selectedCharacterName)
            {
                SelectCharacter(i);
                break;
            }
        }
    }

    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length)
            return;

        CharacterData selectedCharacter = characters[characterIndex];
        playerHealth.setMaxHealth(selectedCharacter.maxHealth);
        playerMovement.setMoveSpeed(selectedCharacter.movespeed);

        damageMultiplier = selectedCharacter.damageMultiplier;
        attackSpeedMultiplier = selectedCharacter.attackSpeedMultiplier;
        critChance = selectedCharacter.critChance;
        critMultiplier = selectedCharacter.critMultiplier;

        if (selectedCharacter.startingWeapon != null)
        {
            Weapon weapon = Instantiate(selectedCharacter.startingWeapon, transform);
            weaponManager.EquipStartingWeapon(weapon);
        }
    }

    public CharacterData getCharacterData(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length)
            return null;

        return characters[characterIndex];
    }

    public float getDamageMultiplier()
    {
        return damageMultiplier;
    }

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
}
