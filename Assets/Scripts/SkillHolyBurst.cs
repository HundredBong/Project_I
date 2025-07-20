using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillHolyBurst : SkillBase
{
    private readonly Collider2D[] _buffer = new Collider2D[64];

    public SkillHolyBurst(SkillData skillData) : base(skillData) { }

    public override void Execute(GameObject owner)
    {
        //���⼭ �ؾ� �� ��. ����ü�� �ʿ����, ��� Enemy�� ã�Ƽ�, �� Enemy���� skillData��� TakeDamage�޼��� ���� �� ��ƼŬ ���

        //The Colliders in the returned array are sorted in order of increasing Z coordinate. ������ ������ �ƴ϶� ������� ��
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
            //Ʃ�� ����
            (enemyList[i], enemyList[rand]) = (enemyList[rand], enemyList[i]);
        }

        //���� ����
        int targetCount = Mathf.Min(skillData.TargetCount, enemyList.Count);

        for (int i = 0; i < targetCount; i++)
        {
            Enemy enemy = enemyList[i];

            for (int k = 0; k < skillData.HitCount; k++)
            {
                enemy.TakeDamage(damage);
            }
            //Debug.Log($"{enemy.name}���� {damage}�� ������� {skillData.HitCount}�� ����");
            ObjectPoolManager.Instance.particlePool.GetPrefab(ParticleId.HolyBurst).Play(enemy.transform.position);
        }


    }
}
