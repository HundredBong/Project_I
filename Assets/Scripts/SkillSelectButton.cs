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
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private Button selectButton;

    private SkillData skillData;

    public void SetSkill(SkillData skillData, Action<SkillData> onClick)
    {
        this.skillData = skillData;
        //� ��ų���� ����ϰ�, ������ ��ü�ϰ� ���޹��� �ݹ��� ������.
        //��ư�� Ŭ���� �� ������ �ؾ��ϴµ� �װ� �ܺο��� ������ �� �ֵ��� ��.
        skillNameText.text = DataManager.Instance.GetLocalizedText(skillData.NameKey);
        skillIcon.sprite = DataManager.Instance.GetSpriteByKey(skillData.SkillIcon);

        lockOverlay.SetActive(SkillManager.Instance.IsUnlocked(skillData.SkillId) == false);
        selectButton.interactable = SkillManager.Instance.IsUnlocked(skillData.SkillId);

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() =>
        {
            //��� ������ ��ų�� Ŭ�� �̺�Ʈ�� ������.
            if (SkillManager.Instance.IsUnlocked(skillData.SkillId) == true)
            {
                onClick(skillData);
            }
            else
            {
                //TODO : string�� ���ڷ� �޴� �˾��� UIManager�� �߰��Ͽ� ���
                //       GetLocalizedText�� ���
                Debug.LogWarning($"[SkillSelectButton] ����ִ� ��ų��, {skillData.SkillId}");
            }
        }); 
    }

    public void Refresh()
    {
        //��ų �����ܰ� �̸��� �����ϴ� �޼���
        skillNameText.text = DataManager.Instance.GetLocalizedText(skillData.NameKey);
        skillIcon.sprite = DataManager.Instance.GetSpriteByKey(skillData.SkillIcon);
        lockOverlay.SetActive(SkillManager.Instance.IsUnlocked(skillData.SkillId) == false);
        selectButton.interactable = SkillManager.Instance.IsUnlocked(skillData.SkillId);
    }
}
