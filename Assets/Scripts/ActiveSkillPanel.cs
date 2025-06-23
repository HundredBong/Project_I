using System.Collections;
using System.Collections.Generic;
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
            if (slot == null)
            {
                Debug.Log("[ActiveSkillPanel] ºñ¾îÀÖ´Â ½½·Ô ¹«½ÃµÊ");
                continue;
            }
            slot.StartGlobalCooldown(cooldown);
        }
    }
}
