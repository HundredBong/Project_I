using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillBase
{
    //공통 구조는 여기에 저장하고 구체 동작은 개별 스킬 클래스에서 구현
    //쿨타임, 대미지 등의 공용 필드를 보관함

    protected SkillData skillData;

    //생성자로 데이터 초기화함. 불변성 확보
    public SkillBase(SkillData data)
    {
        skillData = data;
    }

    //스킬 실행 로직, owner는 스킬을 사용하는 주체
    //지금은 플레이어만 스킬을 사용하지만, 추후 확장될 수도 있으므로 인자로 받도록 함
    public abstract void Execute(GameObject owner);
}
