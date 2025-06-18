using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEquipPopup : UIPopup
{

    [SerializeField] private Transform slotRoot; //���Ե��� �� �θ� ������Ʈ
    [SerializeField] private GameObject slotPrefab; //���� ������
    [SerializeField] private Transform listRoot; //���� ��ų ��ư�� �θ� ������Ʈ
    [SerializeField] private GameObject skillSelectButtonPrefab; //���� ��ų ��ư ������
    [SerializeField] private ActiveSkillPanel activeSkillPanel; //������Ʈ�� �г�

    private List<SkillEquipSlot> slots = new List<SkillEquipSlot>();

    public override void Open()
    {
        base.Open();

        ClearAllSlotsAndButtons(); //TODO : ���� RefreshUI�� �����丵 �ʿ�
        InitializeSlots();
        InitializeSkillButtons();
        GameManager.Instance.statSaver.LoadSkillEquipData(LoadEquippedSkills); //����� ��ų ���� ������ �ҷ�����
    }

    public override void Close()
    {
        base.Close();
       
        SaveEquippedSkills();
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
        //�ߺ� üũ
        foreach(SkillEquipSlot slot in slots)
        {
            if (slot.GetEquippedSkill() == skill) //�̹� ������ ��ų�̸� �ƹ��͵� ����
            {
                Debug.LogWarning("[SkillEquipPopup] �̹� ������ ��ų�Դϴ�.");
                return;
            }
        }

        //�� ���Կ� ��ų ����
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

    private SkillEquipSaveData BuildSkillEquipSaveData()
    {
        SkillEquipSaveData saveData = new SkillEquipSaveData();

        for (int i = 0; i < slots.Count; i++)
        {
            SkillData skill = slots[i].GetEquippedSkill();
            saveData.equippedSkills[i] = (skill != null) ? skill.SkillId : SkillId.None; //������ ��������� None���� ����
        }

        return saveData;
    }

    public void LoadEquippedSkills(SkillEquipSaveData saveData)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            SkillId id = saveData.equippedSkills[i];

            if (id != SkillId.None && DataManager.Instance.skillDataTable.TryGetValue(id, out SkillData data) == true)
            {
                slots[i].SetSkill(data);
            }
        }
    }

    private void SaveEquippedSkills()
    {
        //������ ���ӸŴ����� ����
        for (int i = 0; i < slots.Count; i++)
        {
            SkillData skill = slots[i].GetEquippedSkill();
            GameManager.Instance.equippedSkills[i] = (skill != null) ? skill.SkillId : SkillId.None;
        }

        //���ӸŴ��� -> ���̾�̽� ����
        SkillEquipSaveData saveData = new SkillEquipSaveData();
        for(int i = 0; i < GameManager.Instance.equippedSkills.Length; i++)
        {
            saveData.equippedSkills[i] = GameManager.Instance.equippedSkills[i];
        }
        GameManager.Instance.statSaver.SaveSkillEquipData(saveData);

        activeSkillPanel.Refresh(GameManager.Instance.equippedSkills);
    }
}
