using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterCard : MonoBehaviour
{
    public CharacterData characterData;

    public TMP_Text characterName;
    public TMP_Text startingWeapon;
    public TMP_Text stats;
    
    [Header("UI Image Reference")]
    public Image characterIcon;

    void Start()
    {
        CharacterData data = characterData;

        if (data == null)
            return;

        characterName.text = data.characterName;

        // Automatically use the first frame of idle sprites as the portrait icon
        if (characterIcon != null && data.idleSprites != null && data.idleSprites.Length > 0)
        {
            characterIcon.sprite = data.idleSprites[0];
            characterIcon.gameObject.SetActive(true);
        }
        else if (characterIcon != null)
        {
            characterIcon.gameObject.SetActive(false);
        }

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

        string selectedLevel = MainMenu.GetSelectedLevel();

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayGameplayMusic();
        }

        Debug.Log("Loading level: " + selectedLevel);

        SceneManager.LoadScene(selectedLevel);
    }
}