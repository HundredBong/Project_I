using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectileLightning : Projectile
{
    //SkillLightning�� skillData�� �����
    private SkillData skillData;

    private bool isInitialized = false;

    public void Initialize(SkillData data)
    {
        skillData = data;

        //Enemy�� ã�Ƽ� OnHit �޼��带 ȣ���� �� �ֵ��� ����
    }

    private void Update()
    {
        //�Ʒ��� ���������� �̵����� ����
        if (isInitialized == true)
        {

        }
    }

    protected override void OnHit(GameObject other)
    {
        //�÷��̾� ��ų ���� ��������
        PlayerSkillState state = SkillManager.Instance.GetSkillState(skillData.SkillId);
        float damage = skillData.BaseValue + (skillData.BaseValueIncrease * state.Level);
        float shockChance = skillData.StatucChance;

        List<Enemy> targets = FindClosestEnemies(transform.position, skillData.TargetCount);

        foreach (Enemy enemy in targets)
        {
            for (int i = 0; i < skillData.HitCount; i++)
            {
                enemy.TakeDamage(damage);
            }

            if (Random.value < shockChance / 100f)
            {
                //�����̻� ���� ����, ���ڷ� StatusEffectType �������ֱ�
            }
        }
    }

    private List<Enemy> FindClosestEnemies(Vector3 center, int count)
    {
        //GameManager�� EnemyList���� Ȱ��ȭ�� Enemy�� ã�Ƽ� �Ÿ������� ������ ������ŭ �����ϰ� ��ȯ��
        List<Enemy> allEnemies = GameManager.Instance.enemyList.Where(e => e != null && e.isDead == false)
            .OrderBy(e => Vector3.Distance(center, e.transform.position)).Take(count).ToList();

        return allEnemies;
    }

}
