using UnityEngine;

[CreateAssetMenu(
    fileName = "CharacterData",
    menuName = "Game/CharacterData"
)]
public class CharacterData : ScriptableObject
{
    [Header("Character")]
    public string characterName;

    [Header("Weapon")]
    public Weapon startingWeapon;

    [Header("Sprites")]
    public Sprite[] idleSprites;
    public Sprite[] movingSprites;

    [Header("Animation")]
    public RuntimeAnimatorController animatorController;

    [Header("Stats")]
    public float movespeed = 5f;
    public float critChance = 0f;
    public float critMultiplier = 2f;
    public float maxHealth = 10f;
    public float attackSpeedMultiplier = 1f;
    public float damageMultiplier = 1f;
}