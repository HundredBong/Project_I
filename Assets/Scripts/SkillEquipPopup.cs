using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEquipPopup : UIPopup
{

    [SerializeField] private Transform slotRoot; //슬롯들이 들어갈 부모 오브젝트
    [SerializeField] private GameObject slotPrefab; //슬롯 프리팹
    [SerializeField] private Transform listRoot; //보유 스킬 버튼들 부모 오브젝트
    [SerializeField] private SkillSelectButton skillSelectButtonPrefab; //보유 스킬 버튼 프리팹
    [SerializeField] private ActiveSkillPanel activeSkillPanel; //업데이트할 패널
    [SerializeField] private List<SkillEquipSlot> slots = new List<SkillEquipSlot>();


     private List<SkillSelectButton> skillSelectButtons = new List<SkillSelectButton>(64); //보유 스킬 버튼들
    public override void Open()
    {
        base.Open();

        //ClearAllSlotsAndButtons();
        //InitializeSlots(); 미리 참조받도록 함, 스킬이 6개 고정이라면 슬롯을 매번 생성할 필요 없음
        //InitializeSkillButtons(); Start에서 호출하도록 변경함
        //GameManager.Instance.statSaver.LoadSkillEquipData(LoadEquippedSkills); //저장된 스킬 장착 데이터 불러오기

        SkillId[] equippedSkills = SkillManager.Instance.GetEquippedSkills();

        if (equippedSkills != null && equippedSkills.Length != 0)
        {
            //스킬매니저에서 equippedSkills를 가져왔다면
            LoadEquippedSkills(equippedSkills);
        }
        else
        {
            //equippedSkills를 가져오지 못했거나, 가져왔는데 빈 배열이라면
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
    //    //슬롯 6개 생성
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
        //데이터 매니저에서 모든 스킬 데이터를 가져옴
        List<SkillData> allSkills = DataManager.Instance.GetAllSkillData();

        //보유하고있는 스킬 데이터로 버튼 생성
        foreach (SkillData skill in allSkills)
        {
            SkillSelectButton button = Instantiate(skillSelectButtonPrefab, listRoot);
            button.SetSkill(skill, OnSkillButtonClicked);
            skillSelectButtons.Add(button);
        }

        //이게 원래는 모든 스킬 데이터를 가져와서, 스킬 데이터만큼 버튼을 생성하고, 
        //버튼에 스킬 데이터를 넘겨주고, 버튼 내부에서 IsUnlocked를 체크하여 잠금 상태를 표시하도록 했었는데,
        //그렇다면? 처음 한번 생성하고, 이후에 열 때마다 Refresh를 호출하는게 나을거 같음
        //그럴려면? 이 메서드는 Satrt에서 한 번 호출하고, 이 후에는 Open이 실행될 때마다 Refresh를 호출하도록 해야 함.
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
        //중복 체크
        foreach (SkillEquipSlot slot in slots)
        {
            if (slot.GetEquippedSkill() == skill) //이미 장착된 스킬이면 아무것도 안함
            {
                Debug.LogWarning("[SkillEquipPopup] 이미 장착된 스킬임");
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
        //슬롯을 게임매니저에 저장
        for (int i = 0; i < slots.Count; i++)
        {
            SkillData skill = slots[i].GetEquippedSkill();
            newEquippedSkills[i] = (skill != null) ? skill.SkillId : SkillId.None; //슬롯이 비어있으면 None으로 설정
        }

        SkillManager.Instance.SetEquippedSkills(newEquippedSkills); //스킬 매니저에 장착된 스킬 설정

        //게임매니저 -> 파이어베이스 저장
        SkillEquipSaveData saveData = new SkillEquipSaveData();
        for (int i = 0; i < SkillManager.Instance.GetEquippedSkills().Length; i++)
        {
            saveData.equippedSkills[i] = newEquippedSkills[i];
        }
        GameManager.Instance.statSaver.SaveSkillEquipData(saveData);

        activeSkillPanel.Refresh(newEquippedSkills);
    }
}
