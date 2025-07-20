using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillHolyBurst : SkillBase
{
    private readonly Collider2D[] _buffer = new Collider2D[64];

    public SkillHolyBurst(SkillData skillData) : base(skillData) { }

    public override void Execute(GameObject owner)
    {
        //여기서 해야 할 일. 투사체는 필요없고, 즉시 Enemy를 찾아서, 그 Enemy에게 skillData대로 TakeDamage메서드 실행 및 파티클 재생

        //The Colliders in the returned array are sorted in order of increasing Z coordinate. 순서가 랜덤이 아니라서 섞어줘야 함
        //Collider2D[] colliders = Physics2D.OverlapCircleAll(owner.transform.position, skillData.Range, SkillManager.Instance.targetMask);

        int count = Physics2D.OverlapCircleNonAlloc(owner.transform.position, skillData.Range, _buffer, SkillManager.Instance.targetMask);

        List<Enemy> enemyList = new List<Enemy>(skillData.TargetCount);


        for (int i = 0; i < count; i++)
        {
            Collider2D col = _buffer[i];

            if (col.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemyList.Add(enemy);
            }
        }

        float damage = SkillManager.Instance.CalculateSkillDamage(skillData);

        for (int i = enemyList.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            //튜플 스왑
            (enemyList[i], enemyList[rand]) = (enemyList[rand], enemyList[i]);
        }

        //예외 방지
        int targetCount = Mathf.Min(skillData.TargetCount, enemyList.Count);

        for (int i = 0; i < targetCount; i++)
        {
            Enemy enemy = enemyList[i];

            for (int k = 0; k < skillData.HitCount; k++)
            {
                enemy.TakeDamage(damage);
            }
            //Debug.Log($"{enemy.name}에게 {damage}의 대미지를 {skillData.HitCount}번 입힘");
            ObjectPoolManager.Instance.particlePool.GetPrefab(ParticleId.HolyBurst).Play(enemy.transform.position);
        }


    }
}
