//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class SkillSlot
//{
//    private SkillBase equippedSkill;

//    public void Equip(SkillId id)
//    {
//        SkillData data = DataManager.Instance.GetSkill(id);
//        SkillBase skill = SkillFactory.Create(id, data);

//        if (equippedSkill == null)
//        {
//            Debug.LogWarning($"[SkillSlot] SkillId {id} ���� ����");
//        }
//        else
//        {
//            Debug.Log($"[SkillSlot] {id} ���� �Ϸ�");
//        }
//    }

//    public void Use(GameObject owner)
//    {
//        if (equippedSkill == null)
//        {
//            Debug.LogWarning("[SkillSlot] ������ ��ų�� ����");
//            return;
//        }

//        equippedSkill.TryExecute(owner);
//    }

//    public SkillBase GetEquippedSkill()
//    {
//        return equippedSkill;
//    }
//}
