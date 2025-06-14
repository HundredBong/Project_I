using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillBase
{
    //���� ������ ���⿡ �����ϰ� ��ü ������ ���� ��ų Ŭ�������� ����
    //��Ÿ��, ����� ���� ���� �ʵ带 ������

    protected SkillData skillData;

    //�����ڷ� ������ �ʱ�ȭ��. �Һ��� Ȯ��
    public SkillBase(SkillData data)
    {
        skillData = data;
    }

    //��ų ���� ����, owner�� ��ų�� ����ϴ� ��ü
    //������ �÷��̾ ��ų�� ���������, ���� Ȯ��� ���� �����Ƿ� ���ڷ� �޵��� ��
    public abstract void Execute(GameObject owner);
}
