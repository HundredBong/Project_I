using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


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
    [SerializeField] private Button purchaseButton;

    [Space(20)]
    [SerializeField] private GameObject soldOutObject;

    private GeneralShopData _data;

    private Dictionary<ShopPriceType, PlayerProgressType> _priceToProgress = new Dictionary<ShopPriceType, PlayerProgressType>()
    {
        { ShopPriceType.Diamond,  PlayerProgressType.Diamond },
        { ShopPriceType.SkillGem, PlayerProgressType.SkillGem },
    };

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

        purchaseButton.onClick.RemoveAllListeners();

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

    public void OnClickPurchaseButton()
    {
        //0. 구매 가능한지 검사
        //1. Data.PriceType에 따라 재화량 감소
        //2. RewardType에 따라 분기 작성
        //4. 현재 구매 제한 횟수 증가

        if (_data.PriceType == ShopPriceType.Ad)
        {
            return;
        }

        int amount = _data.PriceAmount;

        //재화가 충분한지 검사
        if (TrySpendCurrency(_data.PriceType, amount))
        {
            GiveReward(_data.RewardType, amount);
            Refresh();
        }

    }

    private bool TrySpendCurrency(ShopPriceType priceType, int amount)
    {
        if (_priceToProgress.TryGetValue(priceType, out PlayerProgressType progress) == false)
        {
            //광고, 현금 등, 차감할게 없다면
            return true;
        }

        return GameManager.Instance.stats.TrySpendItem(progress, amount);
    }

    private void GiveReward(ShopRewardType rewardType, int amount)
    {
        switch (rewardType)
        {
            case ShopRewardType.AdRemove:             
                break;
            case ShopRewardType.EnhanceDungeonTicket:
                GameManager.Instance.stats.AddCurrency(PlayerProgressType.EnhanceDungeonTicket, amount);
                break;
            case ShopRewardType.SkillDungeonTicket:
                GameManager.Instance.stats.AddCurrency(PlayerProgressType.SkillDungeonTicket, amount);
                break;
            case ShopRewardType.EnhanceStone:
                GameManager.Instance.stats.AddCurrency(PlayerProgressType.EnhanceStone, amount);
                break;
            case ShopRewardType.SkillGem:
                GameManager.Instance.stats.AddCurrency(PlayerProgressType.SkillGem, amount);
                break;
        }
    }
}
