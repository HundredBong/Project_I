using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class SkillBase
{
    //공통 구조는 여기에 저장하고 구체 동작은 개별 스킬 클래스에서 구현
    //쿨타임, 대미지 등의 공용 필드를 보관함

    protected SkillData skillData;

    protected float lastUsedTime = -100;

    public bool IsReady => Time.time >= lastUsedTime + skillData.Cooldown;

    public float Cooldown => skillData.Cooldown;

    //생성자로 데이터 초기화함. 불변성 확보
    public SkillBase(SkillData data)
    {
        skillData = data;
    }

    public bool TryExecute(GameObject owner)
    {
        if (IsReady == true)
        {
            Execute(owner);
            lastUsedTime = Time.time;
            return true;
        }
        else
        {
            Debug.Log($"[SkillBase] {skillData.SkillId} 쿨타임 중, 남은 시간 : {GetRemainingCooldown():F2}초");
            return false;
        }
    }

    public float GetRemainingCooldown()
    {
        //(이전에 쓴 시간 + 스킬 쿨타임) - 현재 시간
        //이전에 쓴 시간이 3초, 스킬 쿨타임이 7초고 현재 시간이 9초면 쿨타임 1초 남게됨 
        return Mathf.Max(0, (lastUsedTime + skillData.Cooldown) - Time.time);
    }

    //스킬 실행 로직, owner는 스킬을 사용하는 주체
    //지금은 플레이어만 스킬을 사용하지만, 추후 확장될 수도 있으므로 인자로 받도록 함
    public abstract void Execute(GameObject owner);
}
