using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGoldUpgradePage : UIPage
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Transform slotLoot;
    [SerializeField] private UIGoldUpgradeSlot slotPrefab;
    [SerializeField] private Button openStatPanelButton;

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

    private void OnEnable()
    {
        stats.OnStatChanged += RefreshAll;
        RefreshAll();
    }

    private void OnDisable()
    {
        stats.OnStatChanged -= RefreshAll;
    }

    public void RefreshAll()
    {
        foreach (UIGoldUpgradeSlot slot in slotDict.Values)
        {
            slot.Refresh();
        }
    }
}
