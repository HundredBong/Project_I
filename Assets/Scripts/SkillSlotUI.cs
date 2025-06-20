using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    //SKillPage�� ������ ������
    [SerializeField] private TextMeshProUGUI skillNameText; //��ų �̸�
    [SerializeField] private TextMeshProUGUI currentLevelText; //���� ����
    [SerializeField] private TextMeshProUGUI requiredCount; //������ �ʿ��� ��ų ���� ����
    [SerializeField] private Button selectButton;
    [SerializeField] private Image skillIcon;
    [SerializeField] private GameObject lockOverlay;

    private SkillData skillData;
    private PlayerSkillState skillState;

    //SkillPage�� Start���� SkillId��� �����͸� �Ѱܹ���
    public void Init(SkillData data)
    {
        skillData = data;
        skillState = SkillManager.Instance.GetSkillState(data.SkillId);

        Refresh();

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnClick);
    }

    public void Refresh()
    {
        skillNameText.text = DataManager.Instance.GetLocalizedText(skillData.NameKey);
        skillIcon.sprite = DataManager.Instance.spriteDic[skillData.SkillIcon];

        if(SkillManager.Instance.GetSkillState(skillData.SkillId) != null)
        {
            currentLevelText.text = $"Lv.{skillState.Level}";
            requiredCount.text = $"{skillState.OwnedCount} / {skillData.AwakenRequiredCount[skillState.AwakenLevel]}";
        }

        //��ų �Ŵ������� ��ų ���̵� �ִ��� Ȯ���ϰ�, ���ٸ� �ش� ��ų�� ��� ���·� ǥ��
        lockOverlay.SetActive(SkillManager.Instance.IsUnlocked(skillData.SkillId) == false);
    }

    private void OnClick()
    {
        //��� ������ ��ų�� ���� �˾��� ���
        if (SkillManager.Instance.IsUnlocked(skillData.SkillId) == true)
        {
            UIManager.Instance.PopupOpen<UISkillInfoPopup>().Init(skillData);
        }
        else
        {
            //TODO : string�� ���ڷ� �޴� �˾��� UIManager�� �߰��Ͽ� ���
            Debug.LogWarning($"[SkillSlotUI] ����ִ� ��ų��, {skillData.SkillId}");
        }
    }
}
