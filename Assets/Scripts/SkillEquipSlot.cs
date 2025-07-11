using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillEquipSlot : MonoBehaviour
{
    //��ų �������� ǥ���ϰ�, ������ư ������ ���� �����ϰ�, ���� ������ ����ִ��� Ȯ���ϴ� ����� ����

    [SerializeField] private Image skillIcon; //��ų ������ ǥ�ÿ�
    [SerializeField] private Button removeButton; //������ư��

    private SkillData equippedSkill; //���� ������ ��ų

    private void OnEnable()
    {
        removeButton.onClick.AddListener(ClearSkill);
    }

    private void OnDisable()
    {
        removeButton.onClick.RemoveListener(ClearSkill);
    }

    private void Awake()
    {
        //���� Start���� Awake�� ����, SetSkill�� ȣ���ϱ� ���� ClearSkill�� ȣ��� �� ����
        ClearSkill();
    }

    public void SetSkill(SkillData skillData)
    {
        //���ο� SkillData�� �����ϰ�, �������� ǥ���ϰ�, ������ư Ȱ��ȭ
        equippedSkill = skillData;
        skillIcon.sprite = DataManager.Instance.GetSpriteByKey(skillData.SkillIcon);
        skillIcon.enabled = true;
        removeButton.gameObject.SetActive(true);
    }

    public void ClearSkill()
    {
        equippedSkill = null;
        skillIcon.sprite = null;
        skillIcon.enabled = false;
        removeButton.gameObject.SetActive(false);
    
    }
    
    public bool CheckIsEmpty()
    {
        return equippedSkill == null;
    }

    public SkillData GetEquippedSkill()
    {
        return equippedSkill;
    }
}

