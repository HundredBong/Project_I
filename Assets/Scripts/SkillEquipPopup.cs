using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEquipPopup : UIPopup
{

    [SerializeField] private Transform slotRoot; //슬롯들이 들어갈 부모 오브젝트
    [SerializeField] private GameObject slotPrefab; //슬롯 프리팹
    [SerializeField] private Transform listRoot; //보유 스킬 버튼들 부모 오브젝트
    [SerializeField] private GameObject skillSelectButtonPrefab; //보유 스킬 버튼 프리팹
    [SerializeField] private ActiveSkillPanel activeSkillPanel; //업데이트할 패널

    private List<SkillEquipSlot> slots = new List<SkillEquipSlot>();

    public override void Open()
    {
        base.Open();

        ClearAllSlotsAndButtons(); //TODO : 추후 RefreshUI로 리팩토링 필요
        InitializeSlots();
        InitializeSkillButtons();
        GameManager.Instance.statSaver.LoadSkillEquipData(LoadEquippedSkills); //저장된 스킬 장착 데이터 불러오기
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
        //슬롯 6개 생성
        for (int i = 0; i < 6; i++)
        {
            GameObject obj = Instantiate(slotPrefab, slotRoot);
            SkillEquipSlot slot = obj.GetComponent<SkillEquipSlot>();
            slots.Add(slot);
        }
    }

    private void InitializeSkillButtons()
    {
        //데이터 매니저에서 모든 스킬 데이터를 가져옴
        List<SkillData> allSkills = DataManager.Instance.GetAllSkillData();

        //보유하고있는 스킬 데이터로 버튼 생성
        foreach (SkillData skill in allSkills)
        {
            GameObject obj = Instantiate(skillSelectButtonPrefab, listRoot);
            SkillSelectButton button = obj.GetComponentInChildren<SkillSelectButton>();
            button.SetSkill(skill, OnSkillButtonClicked);
        }
    }

    private void OnSkillButtonClicked(SkillData skill)
    {
        //중복 체크
        foreach(SkillEquipSlot slot in slots)
        {
            if (slot.GetEquippedSkill() == skill) //이미 장착된 스킬이면 아무것도 안함
            {
                Debug.LogWarning("[SkillEquipPopup] 이미 장착된 스킬입니다.");
                return;
            }
        }

        //빈 슬롯에 스킬 장착
        foreach (SkillEquipSlot slot in slots)
        {
            if (slot.CheckIsEmpty() == true)
            {
                slot.SetSkill(skill);
                return;
            }
        }

        //함수가 여기까지오면 모든 슬롯이 차있다는 의미
        Debug.LogWarning("[SkillEquipPopup] 모든 슬롯이 차있음");
    }

    private SkillEquipSaveData BuildSkillEquipSaveData()
    {
        SkillEquipSaveData saveData = new SkillEquipSaveData();

        for (int i = 0; i < slots.Count; i++)
        {
            SkillData skill = slots[i].GetEquippedSkill();
            saveData.equippedSkills[i] = (skill != null) ? skill.SkillId : SkillId.None; //슬롯이 비어있으면 None으로 설정
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
        //슬롯을 게임매니저에 저장
        for (int i = 0; i < slots.Count; i++)
        {
            SkillData skill = slots[i].GetEquippedSkill();
            GameManager.Instance.equippedSkills[i] = (skill != null) ? skill.SkillId : SkillId.None;
        }

        //게임매니저 -> 파이어베이스 저장
        SkillEquipSaveData saveData = new SkillEquipSaveData();
        for(int i = 0; i < GameManager.Instance.equippedSkills.Length; i++)
        {
            saveData.equippedSkills[i] = GameManager.Instance.equippedSkills[i];
        }
        GameManager.Instance.statSaver.SaveSkillEquipData(saveData);

        activeSkillPanel.Refresh(GameManager.Instance.equippedSkills);
    }
}
