using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEquipPopup : UIPopup
{

    [SerializeField] private Transform slotRoot; //���Ե��� �� �θ� ������Ʈ
    [SerializeField] private GameObject slotPrefab; //���� ������
    [SerializeField] private Transform listRoot; //���� ��ų ��ư�� �θ� ������Ʈ
    [SerializeField] private GameObject skillSelectButtonPrefab; //���� ��ų ��ư ������

    private List<SkillEquipSlot> slots = new List<SkillEquipSlot>();

    public override void Open()
    {
        base.Open();

        ClearAllSlotsAndButtons(); //TODO : ���� RefreshUI�� �����丵 �ʿ�
        InitializeSlots();
        InitializeSkillButtons();
    }

    private void ClearAllSlotsAndButtons()
    {
        foreach (Transform child in slotRoot)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in listRoot)
        {
            Destroy(child.gameObject);
        }

        slots.Clear();
    }

    private void InitializeSlots()
    {
        //���� 6�� ����
        for (int i = 0; i < 6; i++)
        {
            GameObject obj = Instantiate(slotPrefab, slotRoot);
            SkillEquipSlot slot = obj.GetComponent<SkillEquipSlot>();
            slots.Add(slot);
        }
    }

    private void InitializeSkillButtons()
    {
        //������ �Ŵ������� ��� ��ų �����͸� ������
        List<SkillData> allSkills = DataManager.Instance.GetAllSkillData();

        //�����ϰ��ִ� ��ų �����ͷ� ��ư ����
        foreach (SkillData skill in allSkills)
        {
            GameObject obj = Instantiate(skillSelectButtonPrefab, listRoot);
            SkillSelectButton button = obj.GetComponentInChildren<SkillSelectButton>();
            button.SetSkill(skill, OnSkillButtonClicked);
        }
    }

    private void OnSkillButtonClicked(SkillData skill)
    {
        foreach (SkillEquipSlot slot in slots)
        {
            if (slot.CheckIsEmpty() == true)
            {
                slot.SetSkill(skill);
                return;
            }
        }

        //�Լ��� ����������� ��� ������ ���ִٴ� �ǹ�
        Debug.LogWarning("[SkillEquipPopup] ��� ������ ������");
    }
}
