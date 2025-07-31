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
    [SerializeField] private TextMeshProUGUI itemPurchasedText;

    [Space(20)]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image priceIconImage;

    [Space(20)]
    [SerializeField] private GameObject soldOutObject;

    private GeneralShopData _data;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += Refresh;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= Refresh;
    }

    public void Init(GeneralShopData data)
    {
        _data = data;

        itemIcon.sprite = DataManager.Instance.GetSpriteByKey(data.IconKey);

        switch (data.PriceType)
        {
            case ShopPriceType.Diamond:
                priceIconImage.sprite = DataManager.Instance.GetSpriteByKey("UI_Diamond");
                break;
            case ShopPriceType.SkillGem:
                priceIconImage.sprite = DataManager.Instance.GetSpriteByKey("UI_SkillGem");
                break;
            case ShopPriceType.Cash:
                priceIconImage.sprite = DataManager.Instance.GetSpriteByKey("UI_Cash");
                break;
            case ShopPriceType.Ad:
                priceIconImage.sprite = DataManager.Instance.GetSpriteByKey("UI_Ad");
                break;
            default:
                priceIconImage.sprite = null;
                break;
        }


        Refresh();
    }

    private void Refresh()
    {
        soldOutObject.SetActive(false); //바꿔야 함

        itemNameText.text = $"{DataManager.Instance.GetLocalizedText(_data.NameKey)} {_data.RewardCount}{DataManager.Instance.GetLocalizedText("UI_EA")}";
        itemLimitText.text = $"? / {_data.PurchaseLimit}"; //currentLimitCount같은거 필요함
        itemPriceText.text = _data.PriceAmount.ToString();
        itemSoldOutText.text = $"{DataManager.Instance.GetLocalizedText($"Shop_SoldOut")}";

        string limitType = "";

        if (_data.LimitType != ShopLimitType.None)
        {
            switch (_data.LimitType)
            {
                case ShopLimitType.Account:
                    limitType = DataManager.Instance.GetLocalizedText("UI_Account");
                    break;
                case ShopLimitType.Daily:
                    limitType = DataManager.Instance.GetLocalizedText("UI_Daily");
                    break;
                case ShopLimitType.Weekly:
                    limitType = DataManager.Instance.GetLocalizedText("UI_Weekly");
                    break;
                case ShopLimitType.Monthly:
                    limitType = DataManager.Instance.GetLocalizedText("UI_Monthly");
                    break;
                default:
                    limitType = "";
                    break;
            }
        }

        itemPurchasedText.text = $"{limitType} ? / {_data.PurchaseLimit}";
    }
}
