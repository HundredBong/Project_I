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
                Debug.Log("[ActiveSkillPanel] ����ִ� ���� ���õ�");
                //Debug.Log(slot == null ? "������ �������" : "������ ����");
                //Debug.Log(slot.GetEquippedSkill() == null ? "�ȿ� ��ų�� ����" : "�ȿ� ��ų�� ����");
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
