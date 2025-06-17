using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectButton : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;

    private SkillData skillData; //�� ��ư�� ����ϴ� ��ų

    public void SetSkill(SkillData skillData, Action<SkillData> onClick)
    {
        Button button = GetComponentInChildren<Button>();

        //� ��ų���� ����ϰ�, ������ ��ü�ϰ� ���޹��� �ݹ��� ������.
        //��ư�� Ŭ���� �� ������ �ؾ��ϴµ� �װ� �ܺο��� ������ �� �ֵ��� ��.
        this.skillData = skillData;
        skillNameText.text = DataManager.Instance.GetLocalizedText(skillData.NameKey);
        skillIcon.sprite = DataManager.Instance.spriteDic[skillData.SkillIcon];

        button.onClick.AddListener(() => onClick(skillData));
    }
}
