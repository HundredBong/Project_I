using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDragonBreath : SkillBase
{
    public SkillDragonBreath(SkillData data) : base(data) { }

    public override void Execute(GameObject owner)
    {
        //���� ������ ProjectileDragonBreath���� �ؾ� ��.
        //���� : �̰� MonoBehaviour ����
        //�׷� �ؾ� �� �� : �÷��̾� ���濡 ProjectileDragonBreath�� Ǯ���� ��ȯ���ֱ�

        ObjectPoolManager.Instance.projectilePool.GetPrefab<ProjectileDragonBreath>(ProjectileId.DragonBreath).Initialize(skillData);
    }
}
