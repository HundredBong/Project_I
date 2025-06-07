using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("�̵� �� ����")]
    public float moveSpeed = 5;
    public float attackSpeed = 1;
    public float attackRange = 2;
    public float chaseRange = 10;

    [Header("ü��")]
    public float health;
    public float maxHealth = 10;


    [Header("�����")]
    public float damage = 1;
    public float critical = 0;

    [Header("����")]
    public int level = 1;
    public float currentExp = 0;
    public float maxExp = 10;

    [Header("��ȭ")]
    public int statPoint = 0;

    //���� ����
    private Dictionary<StatType, int> statLevels = new Dictionary<StatType, int>();

    //StatPanelUI ������Ʈ�� �׼�
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
        //Debug.Log($"{exp} ����ġ ȹ����, {currentExp} / {maxExp}");
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

        Debug.Log($"���� �����, ���� ���� : {level}");

        statPoint++;
    }

    public void AddStat(StatType statType, int amount)
    {
        int currentLevel = GetStat(statType);
        int maxLevel = GetMaxStat(statType);
        //��ȭ�Ϸ��� ��ġ�� �ִ� �������� ���� ��� ������ ���� ����Ʈ�� �� ���� �ʱ����� ����ϰ� ��.
        int possibleAmount = Mathf.Min(amount, maxLevel - currentLevel, statPoint); //�ִ���� ���� ��ġ ���

        //���� ���� 280, �ִ� 300, ���� ��ġ100, ��������Ʈ 200�̸� ���� ��ȭ�� 20

        Debug.Log($"[��ȭ] {statType} + {possibleAmount} (���ð�: {amount})");

        //�̹� �ִ�ġ���� ��ȭ�� ���¶��
        if (possibleAmount <= 0)
        {
            Debug.Log("��ȭ�� �� ����");
            return;
        }

        ////�ø����� ������ ���� ��������Ʈ���� ũ�� ����
        //if (statPoint < amount) { return; }
        ////���� ������ �ִ� ������ ���� ���¶�� ����
        //if (GetMaxStat(statType) <= GetStat(statType)) { Debug.Log("�ִ� ������ ������"); return; }
        ////���� ������ 290, �ƽ� ������ 300�� ��Ȳ���� amont�� 100�� ������ ��� ����ó�� �ʿ���.
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
        //damage = 5 + damageLevel * 3; //�⺻ ���ݷ� 5 + ������ 3�� �߰�
        //maxHealth = 50 + maxHealthLevel * 10; //�⺻ü�� 50 + ������ 10�� �߰�
        //critical = criticalLevel * 0.01f; //������ ũ��Ȯ�� 0.1% ����
        //attackSpeed = 1 + attackSpeedLevel * 0.01f; //������ 0.1% ����
        //moveSpeed = 5 + moveSpeedLevel * 0.01f; //������ 0.1% ����

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
        Debug.Log($"[PlayerStats] RefreshAllStatUIs �����, UI���� : {slotUIs.Count}");

        foreach (StatsSlotUI ui in slotUIs)
        {
            Debug.Log("foreach");
            ui.Refresh ();
            Debug.Log($"[PlayerStats] ���ΰ�ħ�� UI : {ui.name}");
        }
    }
}
