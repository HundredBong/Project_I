using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillLightning : SkillBase
{
    //�̷� ��� SkillBase�� �����ڰ� ���� ����ǰ� �ڽ��� �����ڴ� �θ� �����ڰ� ����� �ڿ� �����
    //������ ��������ϱ� ��ǻ� skillData = data �ϴ°���
    public SkillLightning(SkillData data) : base(data) { }

    public override void Execute(GameObject owner)
    {
        Debug.Log($"[SkillLightning] {skillData.SkillId} �����");
    }
}
