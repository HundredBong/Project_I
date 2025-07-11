using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEquipPopup : UIPopup
{

    [SerializeField] private Transform slotRoot; //���Ե��� �� �θ� ������Ʈ
    [SerializeField] private GameObject slotPrefab; //���� ������
    [SerializeField] private Transform listRoot; //���� ��ų ��ư�� �θ� ������Ʈ
    [SerializeField] private SkillSelectButton skillSelectButtonPrefab; //���� ��ų ��ư ������
    [SerializeField] private ActiveSkillPanel activeSkillPanel; //������Ʈ�� �г�
    [SerializeField] private List<SkillEquipSlot> slots = new List<SkillEquipSlot>();


     private List<SkillSelectButton> skillSelectButtons = new List<SkillSelectButton>(64); //���� ��ų ��ư��
    public override void Open()
    {
        base.Open();

        //ClearAllSlotsAndButtons();
        //InitializeSlots(); �̸� �����޵��� ��, ��ų�� 6�� �����̶�� ������ �Ź� ������ �ʿ� ����
        //InitializeSkillButtons(); Start���� ȣ���ϵ��� ������
        //GameManager.Instance.statSaver.LoadSkillEquipData(LoadEquippedSkills); //����� ��ų ���� ������ �ҷ�����

        SkillId[] equippedSkills = SkillManager.Instance.GetEquippedSkills();

        if (equippedSkills != null && equippedSkills.Length != 0)
        {
            //��ų�Ŵ������� equippedSkills�� �����Դٸ�
            LoadEquippedSkills(equippedSkills);
        }
        else
        {
            //equippedSkills�� �������� ���߰ų�, �����Դµ� �� �迭�̶��
            GameManager.Instance.statSaver.LoadSkillEquipData(LoadEquippedSkills);
        }

        RefreshSkillButtons();

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

    //private void InitializeSlots()
    //{
    //    //���� 6�� ����
    //    for (int i = 0; i < 6; i++)
    //    {
    //        GameObject obj = Instantiate(slotPrefab, slotRoot);
    //        SkillEquipSlot slot = obj.GetComponent<SkillEquipSlot>();
    //        slots.Add(slot);
    //    }
    //}

    private void Start()
    {
        InitializeSkillButtons();
    }

    private void InitializeSkillButtons()
    {
        //������ �Ŵ������� ��� ��ų �����͸� ������
        List<SkillData> allSkills = DataManager.Instance.GetAllSkillData();

        //�����ϰ��ִ� ��ų �����ͷ� ��ư ����
        foreach (SkillData skill in allSkills)
        {
            SkillSelectButton button = Instantiate(skillSelectButtonPrefab, listRoot);
            button.SetSkill(skill, OnSkillButtonClicked);
            skillSelectButtons.Add(button);
        }

        //�̰� ������ ��� ��ų �����͸� �����ͼ�, ��ų �����͸�ŭ ��ư�� �����ϰ�, 
        //��ư�� ��ų �����͸� �Ѱ��ְ�, ��ư ���ο��� IsUnlocked�� üũ�Ͽ� ��� ���¸� ǥ���ϵ��� �߾��µ�,
        //�׷��ٸ�? ó�� �ѹ� �����ϰ�, ���Ŀ� �� ������ Refresh�� ȣ���ϴ°� ������ ����
        //�׷�����? �� �޼���� Satrt���� �� �� ȣ���ϰ�, �� �Ŀ��� Open�� ����� ������ Refresh�� ȣ���ϵ��� �ؾ� ��.
    }

    private void RefreshSkillButtons()
    {
        foreach (SkillSelectButton button in skillSelectButtons)
        {
            button.Refresh();
        }
    }

    private void OnSkillButtonClicked(SkillData skill)
    {
        //�ߺ� üũ
        foreach (SkillEquipSlot slot in slots)
        {
            if (slot.GetEquippedSkill() == skill) //�̹� ������ ��ų�̸� �ƹ��͵� ����
            {
                Debug.LogWarning("[SkillEquipPopup] �̹� ������ ��ų��");
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

    public void LoadEquippedSkills(SkillId[] skillId)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (skillId[i] != SkillId.None)
            {
                SkillData skillData = DataManager.Instance.GetSkill(skillId[i]);
                slots[i].SetSkill(skillData);
            }
        }
    }

    public void LoadEquippedSkills(SkillEquipSaveData saveData)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            SkillId id = saveData.equippedSkills[i];

            if (id != SkillId.None)
            {
                SkillData skillData = DataManager.Instance.GetSkill(id);
                slots[i].SetSkill(skillData);
            }
        }
    }

    private void SaveEquippedSkills()
    {
        SkillId[] newEquippedSkills = new SkillId[6];
        //������ ���ӸŴ����� ����
        for (int i = 0; i < slots.Count; i++)
        {
            SkillData skill = slots[i].GetEquippedSkill();
            newEquippedSkills[i] = (skill != null) ? skill.SkillId : SkillId.None; //������ ��������� None���� ����
        }

        SkillManager.Instance.SetEquippedSkills(newEquippedSkills); //��ų �Ŵ����� ������ ��ų ����

        //���ӸŴ��� -> ���̾�̽� ����
        SkillEquipSaveData saveData = new SkillEquipSaveData();
        for (int i = 0; i < SkillManager.Instance.GetEquippedSkills().Length; i++)
        {
            saveData.equippedSkills[i] = newEquippedSkills[i];
        }
        GameManager.Instance.statSaver.SaveSkillEquipData(saveData);

        activeSkillPanel.Refresh(newEquippedSkills);
    }
}
