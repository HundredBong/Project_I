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
    [SerializeField] private TextMeshProUGUI currentCount; //���� ��ų ���� ����
    [SerializeField] private TextMeshProUGUI requiredCount; //������ �ʿ��� ��ų ���� ����
    [SerializeField] private Button selectButton;
    [SerializeField] private Image skillIcon;

    private SkillData skillData;

    //SkillPage�� Start���� SkillId��� �����͸� �Ѱܹ���
    public void Init(SkillData data)
    {
        skillData = data;
        skillIcon.sprite = DataManager.Instance.spriteDic[data.SkillIcon];
        skillNameText.text = DataManager.Instance.GetLocalizedText(skillData.NameKey);

        selectButton.onClick.AddListener(OnClick);
    }

    public void Refresh()
    {
        skillNameText.text = DataManager.Instance.GetLocalizedText(skillData.NameKey);
    }

    private void OnClick()
    {
        //var popup = UIManager.Instance.PopupOpen<UISkillInfoPopup>();
        //popup.SetSkill(data);sk
    }
}
