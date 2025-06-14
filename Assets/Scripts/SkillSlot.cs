using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSlot
{
    private SkillBase equippedSkill;

    public void Equip(SkillId id)
    {
        equippedSkill = SkillFactory.Create(id);

        if (equippedSkill == null)
        {
            Debug.LogWarning($"[SkillSlot] SkillId {id} ���� ����");
        }
        else
        {
            Debug.Log($"[SkillSlot] {id} ���� �Ϸ�");
        }
    }

    public void Use(GameObject owner)
    {
        if (equippedSkill == null)
        {
            Debug.LogWarning("[SkillSlot] ������ ��ų�� ����");
            return;
        }

        equippedSkill.Execute(owner);
    }
}
