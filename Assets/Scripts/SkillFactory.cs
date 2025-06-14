using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkillFactory
{
    //���¸� �������ʰ�, �ܼ� ����¸� ó���ϴ� ����̶� �ν��Ͻ��� ���� ������ ����
    //���ο� �ʵ嵵 ���� �ܼ��� Create�� �Է��� �ް� ����� ��ȯ�� -> ����ƽ Ŭ����

    public static SkillBase Create(SkillId id)
    {
        if (DataManager.Instance.skillDataTable.TryGetValue(id, out SkillData skillData) == false)
        {
            Debug.LogWarning($"[SkillFactory] SkillId {id}�� �ش��ϴ� ������ ����");
            return null;
        }

        switch (id)
        {
            case SkillId.Lightning:
                return new SkillLightning(skillData);

            default:
                Debug.LogWarning($"[SkillFactory] SkillId {id}�� �ش��ϴ� Ŭ������ ���ǵ��� ����");
                return null;
        }
    }
}