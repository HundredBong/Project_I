using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkillFactory
{
    //상태를 가지지않고, 단순 입출력만 처리하는 기능이라 인스턴스를 만들 이유가 없음
    //내부에 필드도 없고 단순히 Create로 입력을 받고 결과만 반환함 -> 스태틱 클래스

    public static SkillBase Create(SkillId id)
    {
        if (DataManager.Instance.skillDataTable.TryGetValue(id, out SkillData skillData) == false)
        {
            Debug.LogWarning($"[SkillFactory] SkillId {id}에 해당하는 데이터 없음");
            return null;
        }

        switch (id)
        {
            case SkillId.Lightning:
                return new SkillLightning(skillData);

            default:
                Debug.LogWarning($"[SkillFactory] SkillId {id}에 해당하는 클래스가 정의되지 않음");
                return null;
        }
    }
}