using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkillFactory
{
    //상태를 가지지않고, 단순 입출력만 처리하는 기능이라 인스턴스를 만들 이유가 없음
    //내부에 필드도 없고 단순히 Create로 입력을 받고 결과만 반환함 -> 스태틱 클래스

    //SkillData를 넣으면 SkillBase를 반환함
    private static readonly Dictionary<SkillId, Func<SkillData, SkillBase>> skillCreators = new Dictionary<SkillId, Func<SkillData, SkillBase>>();

    static SkillFactory()
    {
        //스킬타입에 따라 생성자
        Register(SkillId.Lightning, data => { return new SkillLightning(data); });

        //스킬이 생기면 여기에 추가로 등록
    }

    //함수를 딕셔너리에 저장하는 메서드
    public static void Register(SkillId id, Func<SkillData, SkillBase> constructor)
    {
        if (skillCreators.ContainsKey(id))
        {
            Debug.LogWarning($"[SkillFactory] 이미 등록된 스킬 ID: {id}");
            return;
        }

        skillCreators[id] = constructor;
    }

    //딕셔너리에서 함수를 찾아 실행하는 메서드
    public static SkillBase Create(SkillId id, SkillData data)
    {
        //var constructor = Func<SkillData, SkillBase>로 자동 추론됨
        //딕셔너리에 있는 SkillId에 해당하는 생성자를 찾아서 실행함
        //SkillId.Lightning이 들어왔다고 가정하면 data => new SkillLightning(data) 함수를 정의함
        //constructor.Invoke(data)로 실행해야 SkillLightning의 생성자가 호출됨, 꺼낸다고 해서 자동으로 실행되는게 아님
        if (skillCreators.TryGetValue(id, out Func<SkillData, SkillBase> constructor))
        {
            return constructor.Invoke(data);
        }

        Debug.LogWarning($"[SkillFactory] 등록되지 않은 스킬 ID: {id}");
        return null;
    }
}