using UnityEngine;

public class PlayerCharacterLoader : MonoBehaviour
{
    [Header("Characters")]
    public CharacterData[] availableCharacters;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator =
                GetComponent<Animator>();
        }
    }

    private void Start()
    {
        LoadSelectedCharacter();
    }

    private void LoadSelectedCharacter()
    {
        string selectedName =
            PlayerPrefs.GetString(
                "SelectedCharacter",
                "Police"
            );

        CharacterData activeData = null;

        if (availableCharacters != null)
        {
            foreach (
                CharacterData character
                in availableCharacters
            )
            {
                if (character == null)
                    continue;

                if (
                    character.characterName ==
                    selectedName
                )
                {
                    activeData = character;
                    break;
                }
            }
        }

        if (activeData == null)
        {
            Debug.LogWarning(
                "[PlayerCharacterLoader] " +
                "Selected character data not found: " +
                selectedName
            );

            return;
        }

        ApplyCharacterData(activeData);
    }

    private void ApplyCharacterData(
        CharacterData data
    )
    {
        if (data == null)
            return;

        // Load this character's Animator Controller.
        if (
            animator != null &&
            data.animatorController != null
        )
        {
            animator.runtimeAnimatorController =
                data.animatorController;
        }

        // Set the first idle sprite immediately.
        if (
            spriteRenderer != null &&
            data.idleSprites != null &&
            data.idleSprites.Length > 0
        )
        {
            spriteRenderer.sprite =
                data.idleSprites[0];
        }

        // Start in Idle.
        if (animator != null)
        {
            animator.SetBool(
                "IsMoving",
                false
            );

            animator.Play(
                "Idle",
                0,
                0f
            );
        }

        Debug.Log(
            "[PlayerCharacterLoader] Loaded character: " +
            data.characterName
        );
    }
}