using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameEndUI : MonoBehaviour
{
    public static GameEndUI Instance;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Victory Stats")]
    [SerializeField] private TMP_Text playerStatsText;

    [Header("Main Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Time.timeScale = 1f;

        HideAllPanels();
    }

    private void HideAllPanels()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        Time.timeScale = 0f;

        HideAllPanels();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log("[GameEndUI] Player died. Game Over.");
    }

    public void ShowVictory()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        Time.timeScale = 0f;

        HideAllPanels();

        UpdatePlayerStats();

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        Debug.Log("[GameEndUI] Boss defeated. Victory!");
    }

    private void UpdatePlayerStats()
    {
        if (playerStatsText == null)
        {
            Debug.LogWarning(
                "[GameEndUI] Player Stats Text is not assigned."
            );

            return;
        }

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            playerStatsText.text =
                "PLAYER STATS\n\n" +
                "Player not found.";

            Debug.LogWarning(
                "[GameEndUI] Could not find Player."
            );

            return;
        }

        Character character =
            player.GetComponent<Character>();

        PlayerHealth playerHealth =
            player.GetComponent<PlayerHealth>();

        PlayerMovement playerMovement =
            player.GetComponent<PlayerMovement>();

        if (character == null)
        {
            playerStatsText.text =
                "PLAYER STATS\n\n" +
                "Character component not found.";

            return;
        }

        float maxHealth = 0f;

        if (playerHealth != null)
        {
            maxHealth = playerHealth.maxHealth;
        }

        float moveSpeed = 0f;

        if (playerMovement != null)
        {
            moveSpeed = playerMovement.GetCurrentMoveSpeed();
        }

        playerStatsText.text =
            "PLAYER STATS\n\n" +

            "Health: " +
            maxHealth.ToString("0") +
            "\n" +

            "Damage: " +
            character.getDamageMultiplier().ToString("0.00") +
            "x\n" +

            "Attack Speed: " +
            character.getAttackSpeedMultiplier().ToString("0.00") +
            "x\n" +

            "Move Speed: " +
            moveSpeed.ToString("0.00") +
            "\n" +

            "Crit Chance: " +
            character.getCritChance().ToString("0.00") +
            "%\n" +

            "Crit Damage: " +
            character.getCritMultiplier().ToString("0.00") +
            "x\n" +

            "AOE Bonus: " +
            character.getAOEBonus().ToString("0.00") +
            "\n" +

            "Move Speed Bonus: " +
            character.getMoveSpeedBonus().ToString("0.00");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        gameEnded = false;

        EnemyUpgradeManager.ResetRunUpgrades();

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;

        gameEnded = false;

        EnemyUpgradeManager.ResetRunUpgrades();

        SceneManager.LoadScene(mainMenuSceneName);
    }
}