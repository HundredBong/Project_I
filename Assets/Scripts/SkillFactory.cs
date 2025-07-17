using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkillFactory
{
    //���¸� �������ʰ�, �ܼ� ����¸� ó���ϴ� ����̶� �ν��Ͻ��� ���� ������ ����
    //���ο� �ʵ嵵 ���� �ܼ��� Create�� �Է��� �ް� ����� ��ȯ�� -> ����ƽ Ŭ����

    //SkillData�� ������ SkillBase�� ��ȯ��
    //T, TResultŸ��
    private static readonly Dictionary<SkillId, Func<SkillData, SkillBase>> skillCreators = new Dictionary<SkillId, Func<SkillData, SkillBase>>();

    static SkillFactory()
    {
        //��ųŸ�Կ� ���� ������
        Register(SkillId.Lightning, data => { return new SkillLightning(data); });
        Register(SkillId.DarkBoom, data => { return new SkillDarkBoom(data); });
        Register(SkillId.HolyBurst, data => { return new SkillHolyBurst(data); });
        Register(SkillId.DragonBreath, data => { return new SkillDragonBreath(data); });
        Register(SkillId.IceArrow, data => { return new SkillIceArrow(data); });
        Register(SkillId.Explosion, data => { return new SkillExplosion(data); });
        Register(SkillId.Fireball, data => { return new SkillFireball(data); });
        Register(SkillId.Charge, data => { return new SkillFireball(data); });

        //��ų�� ����� ���⿡ �߰��� ���
    }

    //�Լ��� ��ųʸ��� �����ϴ� �޼���
    public static void Register(SkillId id, Func<SkillData, SkillBase> constructor)
    {
        if (skillCreators.ContainsKey(id))
        {
            Debug.LogWarning($"[SkillFactory] �̹� ��ϵ� ��ų ID: {id}");
            return;
        }

        skillCreators[id] = constructor;
    }

    //��ųʸ����� �Լ��� ã�� �����ϴ� �޼���
    public static SkillBase Create(SkillId id, SkillData data)
    {
        //var constructor = Func<SkillData, SkillBase>�� �ڵ� �߷е�
        //��ųʸ��� �ִ� SkillId�� �ش��ϴ� �����ڸ� ã�Ƽ� ������
        //SkillId.Lightning�� ���Դٰ� �����ϸ� data => new SkillLightning(data) �Լ��� ������
        //constructor.Invoke(data)�� �����ؾ� SkillLightning�� �����ڰ� ȣ���, �����ٰ� �ؼ� �ڵ����� ����Ǵ°� �ƴ�
        if (skillCreators.TryGetValue(id, out Func<SkillData, SkillBase> constructor))
        {
            return constructor.Invoke(data);
        }

        Debug.LogWarning($"[SkillFactory] ��ϵ��� ���� ��ų ID: {id}");
        return null;
    }
}