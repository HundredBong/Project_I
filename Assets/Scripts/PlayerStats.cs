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
    public event Action OnStatChanged;

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
        Debug.Log($"{exp} 경험치 획득함, {currentExp} / {maxExp}");
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
        //올리려는 레벨이 현재 스탯포인트보다 크면 리턴
        if (statPoint < amount) { return; }
        //현재 스탯이 최대 스탯을 넘은 상태라면 리턴
        if (GetStatMax(statType) <= GetStat(statType)) { Debug.Log("최대 레벨에 도달함"); return; }
        //현재 레벨이 290, 맥스 레벨이 300인 상황에서 amont로 100이 들어왔을 경우 예외처리 필요함.
        statPoint -= amount;

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

        statLevels[statType] += amount;

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

    public int GetStatMax(StatType statType)
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
}
