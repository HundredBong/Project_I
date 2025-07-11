using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillEquipSlot : MonoBehaviour
{
    //스킬 아이콘을 표시하고, 해제버튼 누르면 장착 해제하고, 지금 슬롯이 비어있는지 확인하는 기능을 가짐

    [SerializeField] private Image skillIcon; //스킬 아이콘 표시용
    [SerializeField] private Button removeButton; //해제버튼용

    private SkillData equippedSkill; //현재 장착된 스킬

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
        //기존 Start에서 Awake로 변경, SetSkill을 호출하기 전에 ClearSkill이 호출될 수 있음
        ClearSkill();
    }

    public void SetSkill(SkillData skillData)
    {
        //내부에 SkillData를 저장하고, 아이콘을 표시하고, 해제버튼 활성화
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

