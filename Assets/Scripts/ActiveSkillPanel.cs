using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ActiveSkillPanel : MonoBehaviour
{
    [SerializeField] private ActiveSkillSlot[] skillSlots;

    private void OnEnable()
    {
        SkillManager.Instance.onRequestResetAllCooldowns += ResetAllCooldowns;
    }

    private void OnDisable()
    {
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.onRequestResetAllCooldowns -= ResetAllCooldowns;
        }
    }

    private void Start()
    {
        Refresh(SkillManager.Instance.GetEquippedSkills());
    }

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
                //Debug.Log("[ActiveSkillPanel] ºñ¾îÀÖ´Â ½½·Ô ¹«½ÃµÊ");
                continue;
            }
            slot.StartGlobalCooldown(cooldown);
        }
    }

    public ActiveSkillSlot[] GetSlots()
    {
        return skillSlots;
    }

    public void ResetAllCooldowns()
    {
        foreach (ActiveSkillSlot slot in skillSlots)
        {
            if (slot == null || slot.GetEquippedSkill() == null)
            {
                continue;
            }

            slot.ResetAllCooldowns();
        }
    }
}
