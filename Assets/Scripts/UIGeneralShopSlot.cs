using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIGeneralShopSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemLimitText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private TextMeshProUGUI itemSoldOutText;

    [Space(20)]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image priceIconImage;

    private GeneralShopData _data;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += SetLocalizedText;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= SetLocalizedText;
    }

    public void Init(GeneralShopData data)
    {
        _data = data;

        itemIcon.sprite = DataManager.Instance.GetSpriteByKey(data.IconKey);

        switch (data.PriceType)
        {
            case ShopPriceType.Diamond:
                priceIconImage.sprite = DataManager.Instance.GetSpriteByKey(data.IconKey);
                break;
            case ShopPriceType.SkillGem:
                priceIconImage.sprite = DataManager.Instance.GetSpriteByKey(data.IconKey);
                break;
            case ShopPriceType.Cash:
                priceIconImage.sprite = DataManager.Instance.GetSpriteByKey(data.IconKey);
                break;
            case ShopPriceType.Ad:
                priceIconImage.sprite = DataManager.Instance.GetSpriteByKey(data.IconKey);
                break;
            default:
                priceIconImage.sprite = DataManager.Instance.GetSpriteByKey(data.IconKey);
                break;
        }

        SetLocalizedText();
    }

    private void SetLocalizedText()
    {
        itemNameText.text = $"{DataManager.Instance.GetLocalizedText(_data.NameKey)}";
        itemLimitText.text = $"? / {_data.PurchaseLimit}"; //currentLimitCount같은거 필요함
        itemPriceText.text = _data.PurchaseLimit.ToString();
        itemSoldOutText.text = $"{DataManager.Instance.GetLocalizedText($"Shop_SoldOut")}";
    }
}
