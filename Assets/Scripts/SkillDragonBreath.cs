using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDragonBreath : SkillBase
{
    public SkillDragonBreath(SkillData data) : base(data) { }

    public override void Execute(GameObject owner)
    {
        //세부 로직은 ProjectileDragonBreath에서 해야 함.
        //사유 : 이거 MonoBehaviour 없음
        //그럼 해야 할 일 : 플레이어 전방에 ProjectileDragonBreath를 풀에서 소환해주기

        ObjectPoolManager.Instance.projectilePool.GetPrefab<ProjectileDragonBreath>(ProjectileId.DragonBreath).Initialize(skillData);
    }
}
