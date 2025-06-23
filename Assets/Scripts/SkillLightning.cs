using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillLightning : SkillBase
{
    //이럴 경우 SkillBase의 생성자가 먼저 실행되고 자식의 생성자는 부모 생성자가 실행된 뒤에 실행됨
    //지금은 비어있으니까 사실상 skillData = data 하는거임
    public SkillLightning(SkillData data) : base(data) { }

    public override void Execute(GameObject owner)
    {
        Debug.Log($"[SkillLightning] {skillData.SkillId} 실행됨");

        //ProjectileLightning에 대응하는 프리팹 키를 딕셔너리에서 가져옴, 뽑을 준비만 하는거
        GameObject obj = ObjectPoolManager.Instance.projectilePool.GetPrefab(ProjectileId.Lightning);

        //가져온 프리팹 키로 풀 안에 있는 스택에서 ProjectileLightning 오브젝트를 꺼냄, 실제로 생성되는 거
        ProjectileLightning proj = ObjectPoolManager.Instance.projectilePool.Get(obj) as ProjectileLightning;

        if(proj == null)
        {
            Debug.LogError("[SkillLightning] ProjectileLightning이 아님");
            return;
        }

        proj.Initialize(skillData);
    }
}
