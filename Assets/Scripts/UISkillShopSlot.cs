using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillShopSlot : MonoBehaviour
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


    private SkillShopData _data;
    private bool _isClicking;

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

    public void Init(SkillShopData data)
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

        purchaseButton?.onClick.RemoveAllListeners();
        purchaseButton?.onClick.AddListener(OnClickPurchaseButton);

        Refresh();
    }

    private void Refresh()
    {
        bool isSoldOut = GameManager.Instance.ShopManager.IsLimitExceeded(_data.ShopId, _data.LimitType, _data.PurchaseLimit);

        soldOutObject.SetActive(isSoldOut);
        purchaseButton.interactable = !isSoldOut;

        int used = 0;
        if (_data.LimitType != ShopLimitType.None)
        {
            int remaining = GameManager.Instance.ShopManager.GetRemainingForLimit(_data.ShopId, _data.LimitType, _data.PurchaseLimit);
            used = Mathf.Clamp(_data.PurchaseLimit - remaining, 0, _data.PurchaseLimit);
        }

        itemNameText.text = $"{DataManager.Instance.GetLocalizedText(_data.NameKey)} {_data.RewardCount}{DataManager.Instance.GetLocalizedText("UI_EA")}";
        itemLimitText.text = $"{used} / {_data.PurchaseLimit}";
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

        itemPurchasedText.text = $"{limitType} {used} / {_data.PurchaseLimit}";
    }

    public void RefreshWithUtc(DateTime uiUtcDate)
    {
        bool soldOut = GameManager.Instance.ShopManager.IsLimitExceeded(_data.ShopId, _data.LimitType, _data.PurchaseLimit, uiUtcDate);

        soldOutObject.SetActive(soldOut);
        purchaseButton.interactable = !soldOut;

        int used = 0;
        if (_data.LimitType != ShopLimitType.None)
        {
            int remaining = GameManager.Instance.ShopManager.GetRemainingForLimit(_data.ShopId, _data.LimitType, _data.PurchaseLimit, uiUtcDate);
            used = Mathf.Clamp(_data.PurchaseLimit - remaining, 0, _data.PurchaseLimit);
        }

        itemNameText.text = $"{DataManager.Instance.GetLocalizedText(_data.NameKey)} {_data.RewardCount}{DataManager.Instance.GetLocalizedText("UI_EA")}";
        itemLimitText.text = $"{used} / {_data.PurchaseLimit}";
        itemPriceText.text = _data.PriceAmount.ToString();
        itemSoldOutText.text = $"{DataManager.Instance.GetLocalizedText("Shop_SoldOut")}";

        string limitType = "";
        switch (_data.LimitType)
        {
            case ShopLimitType.Account:
                limitType = DataManager.Instance.GetLocalizedText("UI_Account");
                break;
            case
            ShopLimitType.Daily:
                limitType = DataManager.Instance.GetLocalizedText("UI_Daily");
                break;
            case
            ShopLimitType.Weekly:
                limitType = DataManager.Instance.GetLocalizedText("UI_Weekly");
                break;
            case
            ShopLimitType.Monthly:
                limitType = DataManager.Instance.GetLocalizedText("UI_Monthly");
                break;
        }
        itemPurchasedText.text = (_data.LimitType == ShopLimitType.None) ? "" : $"{limitType} {used} / {_data.PurchaseLimit}";
    }

    public void OnClickPurchaseButton()
    {
        if (_isClicking)
        {
            return;
        }

        _isClicking = true;

        //제한 초과 검사
        if (GameManager.Instance.ShopManager.IsLimitExceeded(_data.ShopId, _data.LimitType, _data.PurchaseLimit))
        {
            ObjectPoolManager.Instance.uiPool.GetMessage().Init("Warning_SoldOut");
            _isClicking = false;
            return;
        }

        if (_priceToProgress.TryGetValue(_data.PriceType, out PlayerProgressType progress) == false)
        {
            Debug.LogWarning($"[ShopSlot] PriceType 매핑 실패: {_data.PriceType}");
            _isClicking = false;
            return;
        }

        //최대 구매 가능 개수 계산
        int max = GetMaxAmount(progress, _data.PriceAmount);

        if (max <= 0)
        {
            ObjectPoolManager.Instance.uiPool.GetMessage().Init("Warning_NotPurchasable");
            _isClicking = false;
            return;
        }

        UIManager.Instance.PopupOpen<UISliderPopup>().Init(_data.IconKey, _data.NameKey, max, async (int selectedCount) =>
        {
            //한 번 더 계산
            int freshMax = GetMaxAmount(progress, _data.PriceAmount);

            //선택한 개수가 최대 구매 가능 개수를 초과하지 않도록 클램핑
            if (selectedCount > freshMax)
            {
                selectedCount = freshMax;
            }

            if (selectedCount <= 0)
            {
                _isClicking = false;
                return;
            }

            if (GameManager.Instance.ShopManager.IsLimitExceeded(_data.ShopId, _data.LimitType, _data.PurchaseLimit))
            {
                ObjectPoolManager.Instance.uiPool.GetMessage().Init("Warning_SoldOut");
                _isClicking = false;
                Refresh();
                return;
            }

            int totalPrice = selectedCount * _data.PriceAmount;

            if (TrySpendCurrency(_data.PriceType, totalPrice))
            {
                GiveReward(_data.skillId, selectedCount * _data.RewardCount);
                if (UIManager.Instance.TryGetPage<UIShopPage>(out UIShopPage page))
                {
                    page.RefreshCurrency();
                }
                await GameManager.Instance.ShopManager.UpdatePurchaseAsync(_data.ShopId, _data.LimitType, selectedCount);
                ObjectPoolManager.Instance.uiPool.GetReward().Init(_data.IconKey, selectedCount);
            }
            else
            {
                ObjectPoolManager.Instance.uiPool.GetMessage().Init("Warning_NotEnoughCurrency");
            }

            _isClicking = false;
            Refresh();
        },
        () =>
        {
            //팝업 닫을 때
            _isClicking = false;
        });
    }

    private int GetMaxAmount(PlayerProgressType progress, int priceAmount)
    {
        if (priceAmount <= 0)
        {
            return 0;
        }

        int progressAmount = (int)GameManager.Instance.stats.GetProgress(progress);
        int maxCurrency = progressAmount / priceAmount;
        int remainingLimit = (_data.LimitType == ShopLimitType.None) ? int.MaxValue : GameManager.Instance.ShopManager.GetRemainingForLimit(_data.ShopId, _data.LimitType, _data.PurchaseLimit);
        int result = Mathf.Max(0, Mathf.Min(maxCurrency, remainingLimit));

        return result;
    }

    private bool TrySpendCurrency(ShopPriceType priceType, int amount)
    {
        if (_priceToProgress.TryGetValue(priceType, out PlayerProgressType progress) == false)
        {
            return true;
        }

        return GameManager.Instance.stats.TrySpendItem(progress, amount);
    }

    private void GiveReward(SkillId skillId, int amount)
    {
        SkillManager.Instance.AddSkill(skillId, amount);
    }
}