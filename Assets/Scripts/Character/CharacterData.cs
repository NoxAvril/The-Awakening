using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Weapon startingWeapon;

    public float movespeed = 5f;
    public float critChance = 0f;
    public float critMultiplier = 2f;
    public float maxHealth = 10f;
    public float attackSpeedMultiplier = 1f;
    public float damageMultiplier = 1f;
}
