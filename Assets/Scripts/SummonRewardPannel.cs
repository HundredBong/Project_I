using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonRewardPannel : MonoBehaviour
{
    [SerializeField] private Button claimButton;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TextMeshProUGUI amountText;

    private SummonSubCategory category;
    private RewardType rewardType;
    private int level;
    private SummonRewardData rewardData;

    public void Refresh(SummonSubCategory categoty)
    {
        this.category = categoty;
        level = GameManager.Instance.SummonManager.GetLevel(categoty);

        //보상을 이미 받았다면 
        if (GameManager.Instance.SummonManager.HasClaimed(categoty, level) == true)
        {
            claimButton.interactable = false;
            return;
        }

        rewardData = DataManager.Instance.GetRewardData(categoty, level);

        if (rewardData == null)
        {
            Debug.LogWarning($"[SummonRewardPannel] 보상 데이터가 없음, {category}, {level}");
            claimButton.interactable = false;
            claimButton.gameObject.SetActive(false);
            return;
        }

        switch (rewardData.RewardType)
        {
            case RewardType.Item:
                int itemId = int.Parse(rewardData.Id);
                ItemData itemData = DataManager.Instance.GetItemData()[itemId];
                rewardIcon.sprite = DataManager.Instance.GetSpriteByKey(itemData.IconKey);
                this.rewardType = RewardType.Item;
                break;
            case RewardType.Currency:
                break;
            default:
                break;
        }

        amountText.text = rewardData.Amount.ToString();
        claimButton.onClick.AddListener(OnClickClaim);
        claimButton.interactable = true;
    }

    public void OnClickClaim()
    {
        //중복 보상 방지
        if (GameManager.Instance.SummonManager.HasClaimed(category, level))
        {
            Debug.Log("[SummonRewardPannel] 이미 받은 보상임");
            claimButton.interactable = false;
            return;
        }

        switch (rewardType)
        {
            case RewardType.Item:
                //아이템 지급 및 새로고침
                InventoryManager.Instance.AddItem(DataManager.Instance.GetItemData()[int.Parse(rewardData.Id)], rewardData.Amount);
                if (UIManager.Instance.TryGetPage<UIInventoryPage>(out UIInventoryPage inventoryPage))
                {
                    inventoryPage.RefreshAll();
                }
                break;
            case RewardType.Currency:
                break;
            default:
                break;
        }

        GameManager.Instance.SummonManager.ClaimReward(category, level);
        Refresh(category);
    }

}

