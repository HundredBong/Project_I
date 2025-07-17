using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillExplosion : SkillBase
{
    public SkillExplosion(SkillData data) : base(data) { }

    public override void Execute(GameObject owner)
    {
        ObjectPoolManager.Instance.projectilePool.GetPrefab<ProjectileExplosion>(ProjectileId.Explosion).Initaialize(skillData, owner);
    }
}
