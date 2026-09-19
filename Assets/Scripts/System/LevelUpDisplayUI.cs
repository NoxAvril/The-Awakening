using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LevelUpDisplayUI : MonoBehaviour, IPointerClickHandler
{
    public static LevelUpDisplayUI Instance;

    [Header("Player Level Up UI")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private Image upgradeIcon;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Enemy Upgrade UI")]
    [SerializeField] private GameObject enemyUpgradePanel;

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

        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        if (enemyUpgradePanel != null)
            enemyUpgradePanel.SetActive(false);
    }

    // ============================================================
    // PLAYER LEVEL-UP NOTIFICATION
    // ============================================================

    public void ShowUpgradeNotification(UpgradeData upgrade)
    {
        if (upgrade == null)
        {
            Debug.LogWarning(
                "[LevelUpDisplayUI] UpgradeData is null."
            );

            return;
        }

        // Pause the game.
        Time.timeScale = 0f;

        // Set upgrade name.
        if (titleText != null)
        {
            titleText.text = upgrade.upgradeName;
        }

        // Set description.
        if (descriptionText != null)
        {
            descriptionText.text = upgrade.description;
        }

        // Set icon.
        if (upgradeIcon != null)
        {
            if (upgrade.icon != null)
            {
                upgradeIcon.sprite = upgrade.icon;
                upgradeIcon.gameObject.SetActive(true);
            }
            else
            {
                upgradeIcon.gameObject.SetActive(false);
            }
        }

        // Show player upgrade notification.
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "[LevelUpDisplayUI] Notification Panel is not assigned!"
            );
        }

        // Keep enemy panel hidden.
        if (enemyUpgradePanel != null)
        {
            enemyUpgradePanel.SetActive(false);
        }
    }

    // ============================================================
    // CLICK NOTIFICATION PANEL
    // ============================================================

    public void OnPointerClick(PointerEventData eventData)
    {
        // Only react while the player upgrade notification
        // is currently visible.
        if (notificationPanel == null)
            return;

        if (!notificationPanel.activeSelf)
            return;

        HandleNotificationClick();
    }

    private void HandleNotificationClick()
    {
        // --------------------------------------------------------
        // ENEMY UPGRADE PANEL EXISTS
        // --------------------------------------------------------

        if (enemyUpgradePanel != null)
        {
            ShowEnemyUpgradePanel();
            return;
        }

        // --------------------------------------------------------
        // ENEMY UPGRADE PANEL DOES NOT EXIST YET
        // --------------------------------------------------------

        Debug.Log(
            "[LevelUpDisplayUI] Enemy Upgrade Panel is not assigned. " +
            "Resuming game."
        );

        notificationPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    // ============================================================
    // SHOW ENEMY UPGRADE PANEL
    // ============================================================

    public void ShowEnemyUpgradePanel()
    {
        if (notificationPanel != null)
        {  
            notificationPanel.SetActive(false);
        }

        if (enemyUpgradePanel != null)
        {
            enemyUpgradePanel.SetActive(true);

            EnemyUpgradeSelectionUI selectionUI =
                enemyUpgradePanel.GetComponent<EnemyUpgradeSelectionUI>();

            if (selectionUI != null)
            {
                selectionUI.ShowEnemyUpgrades();
            }
            else
            {
                Debug.LogError(
                    "[LevelUpDisplayUI] EnemyUpgradeSelectionUI " +
                    "is missing from EnemyUpgradePanel!"
                );
            }

            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogWarning(
                "[LevelUpDisplayUI] Enemy Upgrade Panel is not assigned."
            );

            Time.timeScale = 1f;
        }
    }

    // ============================================================
    // FINISH ENEMY UPGRADE SELECTION
    // ============================================================

    public void FinishEnemyUpgradeSelection()
    {
        if (enemyUpgradePanel != null)
        {
            enemyUpgradePanel.SetActive(false);
        }

        // Resume gameplay.
        Time.timeScale = 1f;
    }
}
