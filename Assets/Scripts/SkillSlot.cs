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
//            Debug.LogWarning($"[SkillSlot] SkillId {id} 장착 실패");
//        }
//        else
//        {
//            Debug.Log($"[SkillSlot] {id} 장착 완료");
//        }
//    }

//    public void Use(GameObject owner)
//    {
//        if (equippedSkill == null)
//        {
//            Debug.LogWarning("[SkillSlot] 장착된 스킬이 없음");
//            return;
//        }

//        equippedSkill.TryExecute(owner);
//    }

//    public SkillBase GetEquippedSkill()
//    {
//        return equippedSkill;
//    }
//}
