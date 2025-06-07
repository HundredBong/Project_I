using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("이동 및 공격")]
    public float moveSpeed = 5;
    public float attackSpeed = 1;
    public float attackRange = 2;
    public float chaseRange = 10;

    [Header("체력")]
    public float health;
    public float maxHealth = 10;


    [Header("대미지")]
    public float damage = 1;
    public float critical = 0;

    [Header("레벨")]
    public int level = 1;
    public float currentExp = 0;
    public float maxExp = 10;

    [Header("재화")]
    public int statPoint = 0;

    //스탯 레벨
    private Dictionary<StatType, int> statLevels = new Dictionary<StatType, int>();

    //StatPanelUI 업데이트용 액션
    public event Action OnStatChanged;
    public List<StatsSlotUI> slotUIs = new List<StatsSlotUI>();

    private void Awake()
    {
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            statLevels[type] = 0;
        }
    }

    public void GetExp(float exp)
    {
        currentExp += exp;
        //Debug.Log($"{exp} 경험치 획득함, {currentExp} / {maxExp}");
        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        maxExp *= 1.2f;

        Debug.Log($"레벨 상승함, 현재 레벨 : {level}");

        statPoint++;
    }

    public void AddStat(StatType statType, int amount)
    {
        int currentLevel = GetStat(statType);
        int maxLevel = GetMaxStat(statType);
        //강화하려는 수치가 최대 레벨까지 남은 양과 보유한 스탯 포인트중 더 낮은 쪽까지만 사용하게 함.
        int possibleAmount = Mathf.Min(amount, maxLevel - currentLevel, statPoint); //최대까지 남은 수치 계산

        //현재 레벨 280, 최대 300, 선택 수치100, 스탯포인트 200이면 실제 강화량 20

        Debug.Log($"[강화] {statType} + {possibleAmount} (선택값: {amount})");

        //이미 최대치까지 강화된 상태라면
        if (possibleAmount <= 0)
        {
            Debug.Log("강화할 수 없음");
            return;
        }

        ////올리려는 레벨이 현재 스탯포인트보다 크면 리턴
        //if (statPoint < amount) { return; }
        ////현재 스탯이 최대 스탯을 넘은 상태라면 리턴
        //if (GetMaxStat(statType) <= GetStat(statType)) { Debug.Log("최대 레벨에 도달함"); return; }
        ////현재 레벨이 290, 맥스 레벨이 300인 상황에서 amont로 100이 들어왔을 경우 예외처리 필요함.
        //statPoint -= amount;

        //switch (statType)
        //{
        //    case StatType.Attack:
        //        damageLevel += amount;
        //        break;
        //    case StatType.Health:
        //        maxHealthLevel += amount;
        //        break;
        //    case StatType.Critical:
        //        criticalLevel += amount;
        //        break;
        //    case StatType.AttackSpeed:
        //        attackSpeedLevel += amount;
        //        break;
        //    case StatType.MoveSpeed:
        //        moveSpeedLevel += amount;
        //        break;
        //}

        statPoint -= possibleAmount;
        statLevels[statType] += possibleAmount;

        RecalculateStats();
        OnStatChanged?.Invoke();
    }

    public int GetStat(StatType statType)
    {
        //return statType switch
        //{
        //    StatType.Attack => damageLevel,
        //    StatType.Health => maxHealthLevel,
        //    StatType.Critical => criticalLevel,
        //    StatType.AttackSpeed => attackSpeedLevel,
        //    StatType.MoveSpeed => moveSpeedLevel,
        //    _ => 0
        //};

        //return statLevels[statType];
        return statLevels.TryGetValue(statType, out int level) ? level : 0;
    }

    public int GetMaxStat(StatType statType)
    {
        return statType switch
        {
            StatType.Attack => 25000,
            StatType.Health => 25000,
            StatType.Critical => 1000,
            StatType.AttackSpeed => 300,
            StatType.MoveSpeed => 300,
            _ => 0
        };
    }




    public void RecalculateStats()
    {
        //damage = 5 + damageLevel * 3; //기본 공격력 5 + 레벨당 3씩 추가
        //maxHealth = 50 + maxHealthLevel * 10; //기본체력 50 + 레벨당 10씩 추가
        //critical = criticalLevel * 0.01f; //레벨당 크리확률 0.1% 증가
        //attackSpeed = 1 + attackSpeedLevel * 0.01f; //레벨당 0.1% 증가
        //moveSpeed = 5 + moveSpeedLevel * 0.01f; //레벨당 0.1% 증가

        damage = 5 + GetStat(StatType.Attack) * 3;
        maxHealth = 50 + GetStat(StatType.Health) * 10;
        critical = GetStat(StatType.Critical) * 0.01f;
        attackSpeed = 1 + GetStat(StatType.AttackSpeed) * 0.01f;
        moveSpeed = 5 + GetStat(StatType.MoveSpeed) * 0.01f;
    }
    public Dictionary<StatType, int> GetAllLevels()
    {
        return statLevels;
    }

    public void SetAllLevels(Dictionary<StatType, int> newLevels)
    {
        foreach (KeyValuePair<StatType, int> pair in newLevels)
        {
            statLevels[pair.Key] = pair.Value;
        }
        RecalculateStats();
        RefreshAllStatUIs();
    }

    private void RefreshAllStatUIs()
    {
        Debug.Log($"[PlayerStats] RefreshAllStatUIs 실행됨, UI개수 : {slotUIs.Count}");

        foreach (StatsSlotUI ui in slotUIs)
        {
            Debug.Log("foreach");
            ui.Refresh ();
            Debug.Log($"[PlayerStats] 새로고침한 UI : {ui.name}");
        }
    }
}
