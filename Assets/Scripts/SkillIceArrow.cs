using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillIceArrow : SkillBase
{
    public SkillIceArrow(SkillData data) : base(data) { }

    public override void Execute(GameObject owner)
    {
        //spriteRenderer�� flipX�� ���°� �ƴ϶� localScale.x�� -�� �ٲٴ°Ŷ� transform.right ����
        //-transform.right�� �� ���� �ִµ� ���� scale�� �ٲ� ȸ���� �״�ζ� �� ��쿡���� �ȵ�
        Vector3 baseDir = owner.transform.localScale.x < 0 ? Vector3.right : Vector3.left;
        float spreadAngle = 60f;//120f; //������ ����
        int count = 5; //����ü ����

        float half = spreadAngle * 0.5f; //�߽ɿ��� �� �� �������� ���� (60)
        float angleStep = spreadAngle / (count - 1); //����ü�� ����(30)

        for (int i = 0; i < count; i++)
        {
            float angle = -half + (angleStep * i); // -60 + 30 * 0, -60���� ���� �ؼ� -60, -30, 0, 30, 60 

            Vector3 shootDir = Quaternion.Euler(0, 0, angle) * baseDir;
            Vector3 spawnPos = owner.transform.position + shootDir.normalized * 2f;

            ProjectileIceArrow arrow = ObjectPoolManager.Instance.projectilePool.GetPrefab<ProjectileIceArrow>(ProjectileId.IceArrow);
            arrow.transform.SetPositionAndRotation(spawnPos, Quaternion.FromToRotation(Vector3.right, shootDir));
            arrow.Initialize(shootDir, skillData);
        }

    }
}
