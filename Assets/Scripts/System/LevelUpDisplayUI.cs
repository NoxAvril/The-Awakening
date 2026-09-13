using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpDisplayUI : MonoBehaviour
{
    public static LevelUpDisplayUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private Image upgradeIcon;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void ShowUpgradeNotification(UpgradeData upgrade)
    {
        // 1. Pause the game
        Time.timeScale = 0f;

        // 2. Populate UI with the upgrade chosen automatically
        if (titleText != null) titleText.text = $"Auto-Upgraded: {upgrade.upgradeName}";
        if (descriptionText != null) descriptionText.text = upgrade.description;
        
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

        // 3. Show notification panel
        if (notificationPanel != null)
            notificationPanel.SetActive(true);
    }

    private void OnContinueClicked()
    {
        // Hide panel and resume game
        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        Time.timeScale = 1f; 

        // TODO: Next, we will chain the Enemy Upgrade selection panel here!
    }
}