using UnityEngine;
using TMPro;

public class CharacterCard : MonoBehaviour
{
    public CharacterData characterData;

    public TMP_Text characterName;
    public TMP_Text startingWeapon;
    public TMP_Text stats;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CharacterData data = characterData;

        if (data == null)
            return;

        characterName.text = data.characterName;

        if (data.startingWeapon != null)
        {
            startingWeapon.text = "Starting Weapon: " + data.startingWeapon.name;
        }
        else
        {
            startingWeapon.text = "Starting Weapon: None";
        }

        stats.text = "Health: " + data.maxHealth + "\n" +
                     "Damage: " + data.damageMultiplier + "x\n" +
                     "Attack Speed: " + data.attackSpeedMultiplier + "x\n" +
                     "Move Speed: " + data.movespeed + "\n" +
                     "Crit Chance: " + data.critChance + "%\n" +
                     "Crit Damage: " + data.critMultiplier + "x";
    }

    public void SelectCharacter()
    {
        PlayerPrefs.SetString("SelectedCharacter", characterData.characterName);
        PlayerPrefs.Save();

        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
}
