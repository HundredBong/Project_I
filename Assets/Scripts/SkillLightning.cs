using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillLightning : SkillBase
{
    //�̷� ��� SkillBase�� �����ڰ� ���� ����ǰ� �ڽ��� �����ڴ� �θ� �����ڰ� ����� �ڿ� �����
    //������ ��������ϱ� ��ǻ� skillData = data �ϴ°���
    public SkillLightning(SkillData data) : base(data) { }

    public override void Execute(GameObject owner)
    {
        Debug.Log($"[SkillLightning] {skillData.SkillId} �����");

        List<Enemy> targets = FindClosestEnemies(owner.transform.position, skillData.TargetCount);

        foreach (var enemy in targets)
        {
            Vector3 spawnPos = enemy.transform.position + Vector3.up * 5f;
            Vector3 direction = Vector3.down;

            ProjectileLightning lightning = ObjectPoolManager.Instance.projectilePool.GetPrefab<ProjectileLightning>(ProjectileId.Lightning);
            lightning.Initialize(skillData, enemy.gameObject);
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
