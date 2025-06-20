using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkillFactory
{
    //���¸� �������ʰ�, �ܼ� ����¸� ó���ϴ� ����̶� �ν��Ͻ��� ���� ������ ����
    //���ο� �ʵ嵵 ���� �ܼ��� Create�� �Է��� �ް� ����� ��ȯ�� -> ����ƽ Ŭ����


    //���� ��ųʸ��� �����ϱ�
    //private Dictionary<SkillType, Func<ISkill>> skillMap;

    //    public SkillFactory()
    //{
    //    skillMap = new Dictionary<SkillType, Func<ISkill>>
    //    {
    //        { SkillType.Fire, () => new FireSkill() },
    //        { SkillType.Ice, () => new IceSkill() }
    //    };
    //}

    //public ISkill CreateSkill(SkillType type)
    //{
    //    if (skillMap.TryGetValue(type, out var constructor))
    //    {
    //        return constructor();
    //    }

    //    throw new ArgumentException($"��ų Ÿ�� {type}��(��) ��ϵ��� �ʾ���");
    //}

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