using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI levelText;

    private PlayerLevelSystem playerLevelSystem;

    private void Start()
    {
        // Find the player in the scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerLevelSystem = player.GetComponent<PlayerLevelSystem>();
        }

        if (playerLevelSystem == null)
        {
            Debug.LogWarning("ExpBarUI: PlayerLevelSystem not found on Player!");
        }
    }

    private void Update()
    {
        if (playerLevelSystem == null) return;

        // Update Slider values smoothly every frame
        if (expSlider != null)
        {
            expSlider.maxValue = playerLevelSystem.expToNextLevel;
            expSlider.value = playerLevelSystem.currentExp;
        }

        // Update Level Text
        if (levelText != null)
        {
            levelText.text = $"LVL {playerLevelSystem.currentLevel}";
        }
    }
}