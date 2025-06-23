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

        //ProjectileLightning�� �����ϴ� ������ Ű�� ��ųʸ����� ������, ���� �غ� �ϴ°�
        GameObject obj = ObjectPoolManager.Instance.projectilePool.GetPrefab(ProjectileId.Lightning);

        //������ ������ Ű�� Ǯ �ȿ� �ִ� ���ÿ��� ProjectileLightning ������Ʈ�� ����, ������ �����Ǵ� ��
        ProjectileLightning proj = ObjectPoolManager.Instance.projectilePool.Get(obj) as ProjectileLightning;

        if(proj == null)
        {
            Debug.LogError("[SkillLightning] ProjectileLightning�� �ƴ�");
            return;
        }

        proj.Initialize(skillData);
    }
}
