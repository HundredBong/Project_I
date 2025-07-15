using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ActiveSkillPanel : MonoBehaviour
{
    [SerializeField] private ActiveSkillSlot[] skillSlots;

    public void Refresh(SkillId[] equippedSkills)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            skillSlots[i].Init(equippedSkills[i]);
            skillSlots[i].OnSkillExecuted = HandleSkillExecuted;
        }
    }

    private void HandleSkillExecuted(ActiveSkillSlot slot)
    {
        StartGlobalCooldown(0.5f);
    }

    public void StartGlobalCooldown(float cooldown)
    {
        foreach (ActiveSkillSlot slot in skillSlots)
        {
            if (slot == null || slot.GetEquippedSkill() == null)
            {
                Debug.Log("[ActiveSkillPanel] 비어있는 슬롯 무시됨");
                //Debug.Log(slot == null ? "슬롯이 비어있음" : "슬롯은 있음");
                //Debug.Log(slot.GetEquippedSkill() == null ? "안에 스킬이 없음" : "안에 스킬은 있음");
                continue;
            }
            slot.StartGlobalCooldown(cooldown);
        }
    }

    public ActiveSkillSlot[] GetSlots()
    {
        return skillSlots;
    }
}
