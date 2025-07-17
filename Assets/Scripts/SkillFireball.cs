using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillFireball : SkillBase
{
    public SkillFireball(SkillData data) : base(data) { }
    private Collider2D[] _buffer = new Collider2D[32];

    public override void Execute(GameObject owner)
    {
        //시전자 기준으로 Range안에서 Enemy검출..?
        //PVP생각하면 언젠가 ICombat 같은거 만들고 거기서 검출해야 함

        int count = Physics2D.OverlapCircleNonAlloc(owner.transform.position, skillData.Range, _buffer, SkillManager.Instance.targetMask);

        List<Enemy> enemyList = new List<Enemy>(_buffer.Length);

        for (int i = 0; i < count; i++)
        {
            if (i > skillData.TargetCount)
            {
                break;
            }

            Collider2D col = _buffer[i];

            if (col.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemyList.Add(enemy);
            }
        }

        foreach (Enemy enemy in enemyList)
        {
            ProjectileFireball fireball = ObjectPoolManager.Instance.projectilePool.GetPrefab<ProjectileFireball>(ProjectileId.Fireball);
            fireball.SetTarget(skillData, enemy, owner);
        }
    }
}
