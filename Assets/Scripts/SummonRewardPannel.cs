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
    private int level; //현재 패널이 다루는 실제 레벨
    private SummonRewardData rewardData;

    public void Refresh(SummonSubCategory category)
    {
        this.category = category;
        int currentLevel = GameManager.Instance.SummonManager.GetLevel(category); //플레이어가 현재 도달한 소환 레벨
        int targetLevel = -1; //UI에 표시할 보상 레벨

        //안받은 보상이 있는지 검사
        for (int i = 1; i <= currentLevel; i++)
        {
            //csv에 데이터가 없다면 넘김
            if (DataManager.Instance.GetRewardData(category, i) == null)
            {
                continue;
            }

            //보상은 존재하나 받지 않았다면
            if (GameManager.Instance.SummonManager.HasClaimed(category, i) == false)
            {
                targetLevel = i;
                break;
            }
        }

        //보상을 받았다면 다음 레벨 미리보기
        if (targetLevel == -1)
        {
            targetLevel = currentLevel + 1;
        }

        level = targetLevel;

        //보상 데이터 불러오기
        rewardData = DataManager.Instance.GetRewardData(category, level);

        //보상 타입으로 분기 
        switch (rewardData.RewardType)
        {
            case RewardType.Item:
                int itemId = int.Parse(rewardData.Id);
                ItemData itemData = DataManager.Instance.GetItemData()[itemId];
                rewardIcon.sprite = DataManager.Instance.GetSpriteByKey(itemData.IconKey);
                this.rewardType = RewardType.Item;
                break;
            case RewardType.SkillGem:
                //스프라이트 딕셔너리에서 ID로 아이콘 가져오고
                //ID 파싱해서 재화로 만들고 지급
                break;
            default:
                break;
        }

        amountText.text = rewardData.Amount.ToString();

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClickClaim);

        //표시 중인 level이 플레이어의 현재 소환 레벨 이하이고, 보상을 받지 않았다면 true
        bool canClaim = level <= currentLevel && GameManager.Instance.SummonManager.HasClaimed(category, level) == false;

        claimButton.interactable = canClaim;
        claimButton.gameObject.SetActive(rewardData != null);
    }

    public void OnClickClaim()
    {
        //중복 보상 방지
        if (GameManager.Instance.SummonManager.HasClaimed(category, level))
        {
            Debug.LogWarning("[SummonRewardPannel] 이미 받은 보상임");
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
            case RewardType.SkillGem:
                break;
            default:
                break;
        }

        GameManager.Instance.SummonManager.ClaimReward(category, level);
        Refresh(category);
    }

}

