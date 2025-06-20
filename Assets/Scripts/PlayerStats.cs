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

    private Dictionary<StatType, int> statLevels = new Dictionary<StatType, int>();
    private Dictionary<PlayerProgressStat, float> playerProgress = new Dictionary<PlayerProgressStat, float>();

    //StatPanelUI 업데이트용 액션
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
        //Debug.Log($"{exp} 경험치 획득함, {currentExp} / {maxExp}");

        //방치했을 때 한 번에 많은 경험치가 들어올 수 있음
        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        maxExp = DataManager.Instance.GetExpData(level);

        Debug.Log($"레벨 상승함, 현재 레벨 : {level}, 다음 경험치 요구량 : {maxExp}");

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

        statPoint -= possibleAmount;
        statLevels[statType] += possibleAmount;

        RecalculateStats(); //스탯 재계산
        GameManager.Instance.statSaver.RequestSave(GetAllLevels()); //스탯 저장 요청
        OnStatChanged?.Invoke(); //UI 새로고침
    }

    public int GetStat(StatType statType)
    {
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
        //이거 제대로 하려면 CSV에서 읽어와야 함, 임시로 하드코딩함
        //만약 한다면 StatType, BaseValue, GrowthPerLevel
        //           Attack, 10, 2.5
        //           Health, 100, 20
        //만들어두면 damage = baseValue + GetStat(StatType.Attack) * growthPerLevel; 이런 식으로 계산 가능함
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
        //어차피 스탯페이지는 열릴때마다 새로고침됨, 여기에 쓸 필요 없음
    }
}
