using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectileLightning : Projectile
{
    //SkillLightning의 skillData를 사용함
    private SkillData skillData;

    private bool isInitialized = false;

    public void Initialize(SkillData data)
    {
        skillData = data;

        //Enemy를 찾아서 OnHit 메서드를 호출할 수 있도록 설정
    }

    private void Update()
    {
        //아래로 떨어지도록 이동로직 구현
        if (isInitialized == true)
        {

        }
    }

    protected override void OnHit(GameObject other)
    {
        //플레이어 스킬 정보 가져오기
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
                //상태이상 적용 로직, 인자로 StatusEffectType 전달해주기
            }
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
