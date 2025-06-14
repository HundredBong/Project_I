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
    }
}
