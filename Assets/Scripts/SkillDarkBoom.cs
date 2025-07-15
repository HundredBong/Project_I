using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDarkBoom : SkillBase
{
    public SkillDarkBoom(SkillData data) : base(data) { }

    public override void Execute(GameObject owner)
    {
        //1. �÷��̾� ��ġ�� ����ü ���� -> ��ƼŬ ����� �����
        //���⼭ �� ���� �װ� ��?
        //���� ��
        //� Projectile�� ������ ���� �����ְ� ���� ��, ���� ����� ProjectileDarkBoom���� ����

        ObjectPoolManager.Instance.projectilePool.GetPrefab<ProjectileDarkBoom>(ProjectileId.DarkBoom).Initialize(skillData, owner);
    }

}
