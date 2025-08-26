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

    [SerializeField] private Image tweeningImage;
    [SerializeField] private Image iconImage;

    private PlayerStats stats;
    private GoldUpgradeType upgradeType;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += Refresh;

        if (stats != null)
        {
            Refresh();
        }

        StatUpgradeAmount.Register(this);
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= Refresh;

        StatUpgradeAmount.Unregister(this);
    }

    public void Init(PlayerStats stats, GoldUpgradeType type)
    {
        this.stats = stats;
        upgradeType = type;

        iconImage.sprite = DataManager.Instance.GetSpriteByKey($"Upgrade_{type}");

        Refresh();
        addButton.onClick.RemoveAllListeners();
        addButton.onClick.AddListener(OnClickAdd);
    }

    public void Refresh()
    {
        GoldUpgradeData data = DataManager.Instance.GetGoldUpgradeData(upgradeType);
        int upgradeValue = stats.GetUpgradeLevel(upgradeType);
        int maxUpgradeLevel = stats.GetMaxUpgradeLevel(upgradeType);


        upgradeNameText.text = DataManager.Instance.GetLocalizedText(DataManager.Instance.GetGoldUpgradeData(upgradeType).NameKey);
        upgradeValueText.text = $"Lv. {upgradeValue}";
        upgradeMaxText.text = $"Max Lv. {maxUpgradeLevel}";

        if (upgradeValue < maxUpgradeLevel)
        {
            int amount = StatUpgradeAmount.statSlotAmount;

            float totalPrice = 0f;
            for (int i = 0; i < amount; i++)
            {
                totalPrice += data.Price + data.PriceIncrease * (upgradeValue + i);
            }

            priceText.text = NumberFormatter.FormatNumber(totalPrice);
        }
        else
        {
            priceText.text = $"";
            addButton.interactable = false;
        }
    }

    private void OnClickAdd()
    {
        //소지 골드 검사나 맥스레벨 검사는 AddStat안에서 실행함

        if (stats.AddStat(upgradeType, StatUpgradeAmount.statSlotAmount))
        {
            UITweening.PlayShine(tweeningImage);
            ObjectPoolManager.Instance.audioPool.GetAudio().PlaySFX("Upgrade");
        }
        else
        {
            Debug.LogWarning("골드 부족함");
        }

        Refresh();
    }

}
