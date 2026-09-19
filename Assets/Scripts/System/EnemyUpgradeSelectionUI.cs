using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EnemyUpgradeSelectionUI : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeChoice
    {
        public Button button;
        public Image icon;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
    }


    // ============================================================
    // ENEMY UPGRADE POOL
    // ============================================================

    [Header("Enemy Upgrade Pool")]
    [SerializeField]
    private List<EnemyUpgradeData> upgradePool =
        new List<EnemyUpgradeData>();


    // ============================================================
    // THREE UPGRADE CHOICES
    // ============================================================

    [Header("Three Upgrade Choices")]
    [SerializeField]
    private UpgradeChoice[] choices =
        new UpgradeChoice[3];


    // ============================================================
    // REFERENCES
    // ============================================================

    [Header("References")]
    [SerializeField]
    private EnemySpawner enemySpawner;

    [SerializeField]
    private LevelUpDisplayUI levelUpDisplayUI;


    // ============================================================
    // RUNTIME DATA
    // ============================================================

    private EnemyUpgradeData[] currentChoices =
        new EnemyUpgradeData[3];

    private HashSet<EnemyUpgradeData>
        usedNonRepeatableUpgrades =
        new HashSet<EnemyUpgradeData>();

    private List<EnemyUpgradeData>
        cachedAvailableUpgrades =
        new List<EnemyUpgradeData>();


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (enemySpawner == null)
        {
            enemySpawner =
                FindFirstObjectByType<EnemySpawner>();
        }

        if (levelUpDisplayUI == null)
        {
            levelUpDisplayUI =
                LevelUpDisplayUI.Instance;
        }

        SetupButtons();

        HideAllChoices();
    }


    // ============================================================
    // SETUP BUTTONS
    // ============================================================

    private void SetupButtons()
    {
        if (choices == null)
        {
            Debug.LogError(
                "[EnemyUpgradeUI] Choices array is missing!"
            );

            return;
        }

        for (int i = 0; i < choices.Length; i++)
        {
            if (choices[i] == null)
            {
                Debug.LogWarning(
                    "[EnemyUpgradeUI] Choice " +
                    i +
                    " is missing."
                );

                continue;
            }

            if (choices[i].button == null)
            {
                Debug.LogWarning(
                    "[EnemyUpgradeUI] Button is missing on choice " +
                    i +
                    "."
                );

                continue;
            }

            int index = i;

            choices[i].button.onClick.AddListener(
                () => SelectUpgrade(index)
            );
        }
    }


    // ============================================================
    // SHOW ENEMY UPGRADES
    // ============================================================

    public void ShowEnemyUpgrades()
    {
        if (enemySpawner == null)
        {
            enemySpawner =
                FindFirstObjectByType<EnemySpawner>();
        }

        if (enemySpawner == null)
        {
            Debug.LogError(
                "[EnemyUpgradeUI] EnemySpawner could not be found!"
            );

            return;
        }


        // Build the list of upgrades that are
        // currently allowed to appear.
        BuildAvailableUpgradeList();


        if (
            cachedAvailableUpgrades == null ||
            cachedAvailableUpgrades.Count == 0
        )
        {
            Debug.LogWarning(
                "[EnemyUpgradeUI] No valid enemy upgrades " +
                "are currently available."
            );

            return;
        }


        List<EnemyUpgradeData> selectedUpgrades =
            GetRandomUpgrades(3);


        // Clear previous choices.
        for (
            int i = 0;
            i < currentChoices.Length;
            i++
        )
        {
            currentChoices[i] = null;
        }


        // Store new choices.
        for (
            int i = 0;
            i < selectedUpgrades.Count &&
            i < currentChoices.Length;
            i++
        )
        {
            currentChoices[i] =
                selectedUpgrades[i];
        }


        // Configure the three UI choices.
        for (
            int i = 0;
            i < choices.Length;
            i++
        )
        {
            if (
                choices[i] == null ||
                choices[i].button == null
            )
            {
                Debug.LogWarning(
                    "[EnemyUpgradeUI] Choice " +
                    i +
                    " does not have a Button assigned."
                );

                continue;
            }


            if (
                i < selectedUpgrades.Count &&
                selectedUpgrades[i] != null
            )
            {
                SetupChoice(
                    choices[i],
                    selectedUpgrades[i]
                );

                choices[i].button.gameObject.SetActive(
                    true
                );
            }
            else
            {
                HideChoice(
                    choices[i]
                );
            }
        }


        gameObject.SetActive(true);

        Time.timeScale = 0f;
    }


    // ============================================================
    // BUILD AVAILABLE UPGRADE LIST
    // ============================================================

    private void BuildAvailableUpgradeList()
    {
        cachedAvailableUpgrades =
            new List<EnemyUpgradeData>();


        if (enemySpawner == null)
        {
            Debug.LogError(
                "[EnemyUpgradeUI] EnemySpawner is not assigned!"
            );

            return;
        }


        if (upgradePool == null)
        {
            Debug.LogError(
                "[EnemyUpgradeUI] Upgrade Pool is missing!"
            );

            return;
        }


        foreach (
            EnemyUpgradeData upgrade
            in upgradePool
        )
        {
            if (upgrade == null)
            {
                continue;
            }


            if (!IsUpgradeValid(upgrade))
            {
                continue;
            }


            cachedAvailableUpgrades.Add(
                upgrade
            );
        }
    }


    // ============================================================
    // CHECK IF UPGRADE IS VALID
    // ============================================================

    private bool IsUpgradeValid(
        EnemyUpgradeData upgrade)
    {
        if (upgrade == null)
        {
            return false;
        }


        // Non-repeatable upgrade already used.
        if (
            !upgrade.isRepeatable &&
            usedNonRepeatableUpgrades.Contains(
                upgrade
            )
        )
        {
            return false;
        }


        switch (upgrade.type)
        {
            // ====================================================
            // NEW ENEMY TYPE
            // ====================================================

            case EnemyUpgradeType.NewEnemyType:

                if (upgrade.enemyPrefab == null)
                {
                    return false;
                }


                if (
                    enemySpawner.unlockedEnemies.Contains(
                        upgrade.enemyPrefab
                    )
                )
                {
                    return false;
                }


                return true;


            // ====================================================
            // NORMAL STAT UPGRADE
            // ====================================================

            case EnemyUpgradeType.BaseStatBuff:

                if (
                    upgrade.targetEnemyPrefab == null
                )
                {
                    return false;
                }


                return enemySpawner.unlockedEnemies.Contains(
                    upgrade.targetEnemyPrefab
                );


            // ====================================================
            // ELITE UPGRADE
            // ====================================================

            case EnemyUpgradeType.EliteSpawner:

                if (
                    upgrade.targetEnemyPrefab == null
                )
                {
                    return false;
                }


                return enemySpawner.unlockedEnemies.Contains(
                    upgrade.targetEnemyPrefab
                );
        }


        return false;
    }


    // ============================================================
    // GET RANDOM UPGRADES
    // ============================================================

    private List<EnemyUpgradeData> GetRandomUpgrades(
        int amount)
    {
        List<EnemyUpgradeData> pool =
            new List<EnemyUpgradeData>(
                cachedAvailableUpgrades
            );


        List<EnemyUpgradeData> result =
            new List<EnemyUpgradeData>();


        while (
            pool.Count > 0 &&
            result.Count < amount
        )
        {
            int randomIndex =
                Random.Range(
                    0,
                    pool.Count
                );


            EnemyUpgradeData selected =
                pool[randomIndex];


            result.Add(
                selected
            );


            pool.RemoveAt(
                randomIndex
            );
        }


        return result;
    }


    // ============================================================
    // SETUP ONE CHOICE
    // ============================================================

    private void SetupChoice(
        UpgradeChoice choice,
        EnemyUpgradeData upgrade)
    {
        if (
            choice == null ||
            upgrade == null
        )
        {
            return;
        }


        if (choice.titleText != null)
        {
            choice.titleText.text =
                upgrade.upgradeName;
        }


        if (choice.descriptionText != null)
        {
            choice.descriptionText.text =
                upgrade.description;
        }


        if (choice.icon != null)
        {
            if (upgrade.icon != null)
            {
                choice.icon.sprite =
                    upgrade.icon;

                choice.icon.gameObject.SetActive(
                    true
                );
            }
            else
            {
                choice.icon.gameObject.SetActive(
                    false
                );
            }
        }
    }


    // ============================================================
    // SELECT UPGRADE
    // ============================================================

    private void SelectUpgrade(
        int index)
    {
        if (
            index < 0 ||
            index >= currentChoices.Length
        )
        {
            return;
        }


        EnemyUpgradeData selectedUpgrade =
            currentChoices[index];


        if (selectedUpgrade == null)
        {
            Debug.LogWarning(
                "[EnemyUpgradeUI] " +
                "No upgrade assigned to this choice."
            );

            return;
        }


        if (enemySpawner == null)
        {
            Debug.LogError(
                "[EnemyUpgradeUI] EnemySpawner is missing!"
            );

            return;
        }


        // ========================================================
        // NORMAL STAT UPGRADE
        // ========================================================

        if (
            selectedUpgrade.type ==
            EnemyUpgradeType.BaseStatBuff
        )
        {
            EnemyUpgradeManager.ApplyEnemyStatUpgrade(
                selectedUpgrade
            );
        }


        // ========================================================
        // ELITE UPGRADE
        // ========================================================

        else if (
            selectedUpgrade.type ==
            EnemyUpgradeType.EliteSpawner
        )
        {
            enemySpawner.ApplyEnemyUpgrade(
                selectedUpgrade
            );

            EnemyUpgradeManager.AddEliteUpgradeExp();
        }


        // ========================================================
        // NEW ENEMY TYPE
        // ========================================================

        else if (
            selectedUpgrade.type ==
            EnemyUpgradeType.NewEnemyType
        )
        {
            enemySpawner.ApplyEnemyUpgrade(
                selectedUpgrade
            );
        }


        // ========================================================
        // NON-REPEATABLE
        // ========================================================

        if (!selectedUpgrade.isRepeatable)
        {
            usedNonRepeatableUpgrades.Add(
                selectedUpgrade
            );
        }


        Debug.Log(
            "[EnemyUpgradeUI] Selected: " +
            selectedUpgrade.upgradeName
        );


        Debug.Log(
            "[EnemyUpgradeUI] Global Enemy EXP Multiplier: " +
            EnemyUpgradeManager
                .GetGlobalExpMultiplier()
                .ToString("0.00") +
            "x"
        );


        // ========================================================
        // CLOSE LEVEL-UP UI
        // ========================================================

        if (levelUpDisplayUI != null)
        {
            levelUpDisplayUI.FinishEnemyUpgradeSelection();
        }
        else
        {
            Time.timeScale = 1f;
        }


        gameObject.SetActive(false);
    }


    // ============================================================
    // HIDE ALL CHOICES
    // ============================================================

    private void HideAllChoices()
    {
        if (choices == null)
        {
            return;
        }


        for (
            int i = 0;
            i < choices.Length;
            i++
        )
        {
            HideChoice(
                choices[i]
            );
        }
    }


    // ============================================================
    // HIDE ONE CHOICE
    // ============================================================

    private void HideChoice(
        UpgradeChoice choice)
    {
        if (choice == null)
        {
            return;
        }


        if (choice.button != null)
        {
            choice.button.gameObject.SetActive(
                false
            );
        }
    }
}