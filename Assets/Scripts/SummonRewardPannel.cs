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

        //������ �̹� �޾Ҵٸ� 
        if (GameManager.Instance.SummonManager.HasClaimed(categoty, level) == true)
        {
            claimButton.interactable = false;
            return;
        }

        rewardData = DataManager.Instance.GetRewardData(categoty, level);

        if (rewardData == null)
        {
            Debug.LogWarning($"[SummonRewardPannel] ���� �����Ͱ� ����, {category}, {level}");
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
        //�ߺ� ���� ����
        if (GameManager.Instance.SummonManager.HasClaimed(category, level))
        {
            Debug.Log("[SummonRewardPannel] �̹� ���� ������");
            claimButton.interactable = false;
            return;
        }

        switch (rewardType)
        {
            case RewardType.Item:
                //������ ���� �� ���ΰ�ħ
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

