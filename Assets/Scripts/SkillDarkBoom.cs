using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDarkBoom : SkillBase
{
    public SkillDarkBoom(SkillData data) : base(data) { }

    public override void Execute(GameObject owner)
    {
        //1. 플레이어 위치에 투사체 생성 -> 파티클 재생후 사라짐
        //여기서 할 일은 그게 끝?
        //ㅇㅇ 끝
        //어떤 Projectile을 생성할 지만 정해주고 할일 끝, 세부 기능은 ProjectileDarkBoom에서 진행

        ObjectPoolManager.Instance.projectilePool.GetPrefab<ProjectileDarkBoom>(ProjectileId.DarkBoom).Initialize(skillData, owner);
    }

}
