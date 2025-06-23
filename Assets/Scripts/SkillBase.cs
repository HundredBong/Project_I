using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class SkillBase
{
    //���� ������ ���⿡ �����ϰ� ��ü ������ ���� ��ų Ŭ�������� ����
    //��Ÿ��, ����� ���� ���� �ʵ带 ������

    protected SkillData skillData;

    protected float lastUsedTime = -100;

    public bool IsReady => Time.time >= lastUsedTime + skillData.Cooldown;

    public float Cooldown => skillData.Cooldown;

    //�����ڷ� ������ �ʱ�ȭ��. �Һ��� Ȯ��
    public SkillBase(SkillData data)
    {
        skillData = data;
    }

    public bool TryExecute(GameObject owner)
    {
        if (IsReady == true)
        {
            Execute(owner);
            lastUsedTime = Time.time;
            return true;
        }
        else
        {
            Debug.Log($"[SkillBase] {skillData.SkillId} ��Ÿ�� ��, ���� �ð� : {GetRemainingCooldown():F2}��");
            return false;
        }
    }

    public float GetRemainingCooldown()
    {
        //(������ �� �ð� + ��ų ��Ÿ��) - ���� �ð�
        //������ �� �ð��� 3��, ��ų ��Ÿ���� 7�ʰ� ���� �ð��� 9�ʸ� ��Ÿ�� 1�� ���Ե� 
        return Mathf.Max(0, (lastUsedTime + skillData.Cooldown) - Time.time);
    }

    //��ų ���� ����, owner�� ��ų�� ����ϴ� ��ü
    //������ �÷��̾ ��ų�� ���������, ���� Ȯ��� ���� �����Ƿ� ���ڷ� �޵��� ��
    public abstract void Execute(GameObject owner);
}
