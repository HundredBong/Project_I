using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillIceArrow : SkillBase
{
    public SkillIceArrow(SkillData data) : base(data) { }

    public override void Execute(GameObject owner)
    {
        //spriteRenderer의 flipX를 쓰는게 아니라 localScale.x를 -로 바꾸는거라서 transform.right 못씀
        //-transform.right로 쓸 수는 있는데 지금 scale이 바뀌어도 회전은 그대로라 이 경우에서는 안됨
        Vector3 baseDir = owner.transform.localScale.x < 0 ? Vector3.right : Vector3.left;
        float spreadAngle = 60f;//120f; //퍼지는 각도
        int count = 5; //투사체 개수

        float half = spreadAngle * 0.5f; //중심에서 양 쪽 끝까지의 각도 (60)
        float angleStep = spreadAngle / (count - 1); //투사체간 간격(30)

        for (int i = 0; i < count; i++)
        {
            float angle = -half + (angleStep * i); // -60 + 30 * 0, -60부터 시작 해서 -60, -30, 0, 30, 60 

            Vector3 shootDir = Quaternion.Euler(0, 0, angle) * baseDir;
            Vector3 spawnPos = owner.transform.position + shootDir.normalized * 2f;

            ProjectileIceArrow arrow = ObjectPoolManager.Instance.projectilePool.GetPrefab<ProjectileIceArrow>(ProjectileId.IceArrow);
            arrow.transform.SetPositionAndRotation(spawnPos, Quaternion.FromToRotation(Vector3.right, shootDir));
            arrow.Initialize(shootDir, skillData);
        }

    }
}
