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
    [SerializeField] private Image skillIcon;

    private SkillData skilldata;

    //SkillPage�� Start���� SkillId��� �����͸� �Ѱܹ���
    public void Init(SkillData data)
    {
        skilldata = data;
        skillIcon.sprite = DataManager.Instance.spriteDic[data.SkillIcon];
    }

    public void Refresh()
    {

    }
}
