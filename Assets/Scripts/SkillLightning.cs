using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillLightning : SkillBase
{
    //이럴 경우 SkillBase의 생성자가 먼저 실행되고 자식의 생성자는 부모 생성자가 실행된 뒤에 실행됨
    //지금은 비어있으니까 사실상 skillData = data 하는거임
    public SkillLightning(SkillData data) : base(data) { }

    public override void Execute(GameObject owner)
    {
        Debug.Log($"[SkillLightning] {skillData.SkillId} 실행됨");

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
        //GameManager의 EnemyList에서 활성화된 Enemy를 찾아서 거리순으로 지정된 개수만큼 정렬하고 반환함
        List<Enemy> allEnemies = GameManager.Instance.enemyList.Where(e => e != null && e.isDead == false)
            .OrderBy(e => Vector3.Distance(center, e.transform.position)).Take(count).ToList();

        return allEnemies;
    }
}
