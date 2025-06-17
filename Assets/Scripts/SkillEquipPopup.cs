using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEquipPopup : UIPopup
{

    [SerializeField] private Transform slotRoot; //슬롯들이 들어갈 부모 오브젝트
    [SerializeField] private GameObject slotPrefab; //슬롯 프리팹
    [SerializeField] private Transform listRoot; //보유 스킬 버튼들 부모 오브젝트
    [SerializeField] private GameObject skillSelectButtonPrefab; //보유 스킬 버튼 프리팹

    private List<SkillEquipSlot> slots = new List<SkillEquipSlot>();

    public override void Open()
    {
        base.Open();

        ClearAllSlotsAndButtons(); //TODO : 추후 RefreshUI로 리팩토링 필요
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
}
