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
    private int level; //���� �г��� �ٷ�� ���� ����
    private SummonRewardData rewardData;

    public void Refresh(SummonSubCategory category)
    {
        this.category = category;
        int currentLevel = GameManager.Instance.SummonManager.GetLevel(category); //�÷��̾ ���� ������ ��ȯ ����
        int targetLevel = -1; //UI�� ǥ���� ���� ����

        //�ȹ��� ������ �ִ��� �˻�
        for (int i = 1; i <= currentLevel; i++)
        {
            //csv�� �����Ͱ� ���ٸ� �ѱ�
            if (DataManager.Instance.GetRewardData(category, i) == null)
            {
                continue;
            }

            //������ �����ϳ� ���� �ʾҴٸ�
            if (GameManager.Instance.SummonManager.HasClaimed(category, i) == false)
            {
                targetLevel = i;
                break;
            }
        }

        //������ �޾Ҵٸ� ���� ���� �̸�����
        if (targetLevel == -1)
        {
            targetLevel = currentLevel + 1;
        }

        level = targetLevel;

        //���� ������ �ҷ�����
        rewardData = DataManager.Instance.GetRewardData(category, level);

        //���� Ÿ������ �б� 
        switch (rewardData.RewardType)
        {
            case RewardType.Item:
                int itemId = int.Parse(rewardData.Id);
                ItemData itemData = DataManager.Instance.GetItemData()[itemId];
                rewardIcon.sprite = DataManager.Instance.GetSpriteByKey(itemData.IconKey);
                this.rewardType = RewardType.Item;
                break;
            case RewardType.SkillGem:
                //��������Ʈ ��ųʸ����� ID�� ������ ��������
                //ID �Ľ��ؼ� ��ȭ�� ����� ����
                break;
            default:
                break;
        }

        amountText.text = rewardData.Amount.ToString();

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClickClaim);

        //ǥ�� ���� level�� �÷��̾��� ���� ��ȯ ���� �����̰�, ������ ���� �ʾҴٸ� true
        bool canClaim = level <= currentLevel && GameManager.Instance.SummonManager.HasClaimed(category, level) == false;

        claimButton.interactable = canClaim;
        claimButton.gameObject.SetActive(rewardData != null);
    }

    public void OnClickClaim()
    {
        //�ߺ� ���� ����
        if (GameManager.Instance.SummonManager.HasClaimed(category, level))
        {
            Debug.LogWarning("[SummonRewardPannel] �̹� ���� ������");
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
            case RewardType.SkillGem:
                break;
            default:
                break;
        }

        GameManager.Instance.SummonManager.ClaimReward(category, level);
        Refresh(category);
    }

}

