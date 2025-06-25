using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIGoldUpgradeSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI upgradeNameText;
    [SerializeField] private TextMeshProUGUI upgradeValueText;
    [SerializeField] private TextMeshProUGUI upgradeMaxText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button addButton;

    private PlayerStats stats;
    private GoldUpgradeType upgradeType;

    public void Init(PlayerStats stats, GoldUpgradeType type)
    {
        this.stats = stats;
        upgradeType = type;

        Refresh();
        addButton.onClick.AddListener(OnClickAdd);
    }

    public void Refresh()
    {
        int upgradeValue = stats.GetUpgradeLevel(upgradeType);
        float price = DataManager.Instance.GetGoldUpgradeData(upgradeType).Price + (DataManager.Instance.GetGoldUpgradeData(upgradeType).PriceIncrease * upgradeValue);

        upgradeNameText.text = DataManager.Instance.GetLocalizedText(DataManager.Instance.GetGoldUpgradeData(upgradeType).NameKey);
        upgradeValueText.text = $"Lv. {upgradeValue}";
        upgradeMaxText.text = $"Max Lv. {stats.GetMaxUpgradeLevel(upgradeType)}";
        priceText.text = $"{price:F1}";
    }

    private void OnClickAdd()
    {
        //소지 골드 검사나 맥스레벨 검사는 AddStat안에서 실행함
        stats.AddStat(upgradeType, StatUpgradeAmount.statSlotAmount);
    }
}
