using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EnemyUpgradeCardUI : MonoBehaviour
{
    public static EnemyUpgradeCardUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject cardSelectionPanel;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject cardButtonPrefab;

    [Header("Rarity Frames")]
    [SerializeField] private Sprite commonFrame;
    [SerializeField] private Sprite rareFrame;
    [SerializeField] private Sprite epicFrame;
    [SerializeField] private Sprite legendaryFrame;

    [Header("Manager Reference")]
    [SerializeField] private EnemyUpgradeManager enemyUpgradeManager;

    private List<GameObject> spawnedCards = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (cardSelectionPanel != null)
            cardSelectionPanel.SetActive(false);
    }

    public void ShowCardSelection()
    {
        ClearCards();

        List<EnemyUpgradeData> choices = enemyUpgradeManager.GetUpgradeChoices();

        for (int i = 0; i < choices.Count; i++)
        {
            GameObject card = Instantiate(cardButtonPrefab, cardContainer);
            spawnedCards.Add(card);
            SetupCard(card, choices[i]);
        }

        Time.timeScale = 0f;

        if (cardSelectionPanel != null)
            cardSelectionPanel.SetActive(true);
    }

    private void SetupCard(GameObject card, EnemyUpgradeData upgrade)
    {
        Transform frameTransform = card.transform.Find("Frame");
        Transform iconTransform = card.transform.Find("Icon");
        Transform titleTransform = card.transform.Find("Title");
        Transform descriptionTransform = card.transform.Find("Description");
        Button button = card.GetComponent<Button>();

        if (frameTransform != null)
        {
            Image frameImage = frameTransform.GetComponent<Image>();
            if (frameImage != null)
                frameImage.sprite = GetFrameForRarity(upgrade.rarity);
        }

        if (iconTransform != null && upgrade.icon != null)
        {
            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null)
                iconImage.sprite = upgrade.icon;
        }

        if (titleTransform != null)
        {
            TextMeshProUGUI titleText = titleTransform.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
                titleText.text = upgrade.upgradeName;
        }

        if (descriptionTransform != null)
        {
            TextMeshProUGUI descriptionText = descriptionTransform.GetComponent<TextMeshProUGUI>();
            if (descriptionText != null)
                descriptionText.text = upgrade.description;
        }

        if (button != null)
        {
            button.onClick.AddListener(() => OnCardSelected(upgrade));
        }
    }

    private Sprite GetFrameForRarity(CardRarity rarity)
    {
        if (rarity == CardRarity.Rare) return rareFrame;
        if (rarity == CardRarity.Epic) return epicFrame;
        if (rarity == CardRarity.Legendary) return legendaryFrame;
        return commonFrame;
    }

    private void OnCardSelected(EnemyUpgradeData upgrade)
    {
        enemyUpgradeManager.ApplyUpgrade(upgrade);

        ClearCards();

        if (cardSelectionPanel != null)
            cardSelectionPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void ClearCards()
    {
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            Destroy(spawnedCards[i]);
        }
        spawnedCards.Clear();
    }
}
