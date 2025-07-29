using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGoldUpgradePage : UIPage
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Transform slotLoot;
    [SerializeField] private UIGoldUpgradeSlot slotPrefab;
    [SerializeField] private Button openStatPanelButton;

    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [SerializeField] private TextMeshProUGUI statButtonText;

    private Dictionary<GoldUpgradeType, UIGoldUpgradeSlot> slotDict = new();

    private void Start()
    {
        foreach (GoldUpgradeType type in Enum.GetValues(typeof(GoldUpgradeType)))
        {
            UIGoldUpgradeSlot slot = Instantiate(slotPrefab, slotLoot);
            slot.Init(stats, type);
            slotDict[type] = slot;
        }

        openStatPanelButton.onClick.AddListener(() => UIManager.Instance.PageOpen<UIStatPage>());
    }

    protected override void Awake()
    {
        base.Awake();

        if (stats == null)
        {
            stats = GameManager.Instance.stats;
        }
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += SetLocalizedText;

        stats.OnStatChanged += RefreshAll;
        RefreshAll();

        SetLocalizedText();
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= SetLocalizedText;

        stats.OnStatChanged -= RefreshAll;
    }

    public void RefreshAll()
    {
        foreach (UIGoldUpgradeSlot slot in slotDict.Values)
        {
            slot.Refresh();
        }
    }

    private void SetLocalizedText()
    {
        upgradeButtonText.text = $"{DataManager.Instance.GetLocalizedText("UI_Upgrade")}";
        statButtonText.text = $"{DataManager.Instance.GetLocalizedText("UI_Stat")}";
    }
}
