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
    public int gold = 0;
    public int diamond = 0;

    private Dictionary<StatUpgradeType, int> statLevels = new Dictionary<StatUpgradeType, int>();
    private Dictionary<PlayerProgressType, float> playerProgress = new Dictionary<PlayerProgressType, float>();

    //StatPanelUI ������Ʈ�� �׼�
    public event Action OnStatChanged;

    private void Awake()
    {
        foreach (StatUpgradeType type in Enum.GetValues(typeof(StatUpgradeType)))
        {
            statLevels[type] = 0;
        }

        foreach (PlayerProgressType type in Enum.GetValues(typeof(PlayerProgressType)))
        {
            playerProgress[type] = 0;
        }
    }

    public void GetExp(float exp)
    {
        currentExp += exp;
        //Debug.Log($"{exp} ����ġ ȹ����, {currentExp} / {maxExp}");
        playerProgress[PlayerProgressType.CurrentExp] = currentExp;

        //��ġ���� �� �� ���� ���� ����ġ�� ���� �� ����
        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            playerProgress[PlayerProgressType.CurrentExp] = currentExp;

            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        maxExp = DataManager.Instance.GetExpData(level);
        playerProgress[PlayerProgressType.Level] = level;

        Debug.Log($"���� �����, ���� ���� : {level}, ���� ����ġ �䱸�� : {maxExp}");

        statPoint++;
        playerProgress[PlayerProgressType.StatPoint] = statPoint;

        GameManager.Instance.statSaver.SavePlayerProgressData(GameManager.Instance.stats.GetProgressSaveData());
    }

    public void AddStat(StatUpgradeType statType, int amount)
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

        statPoint -= possibleAmount;
        statLevels[statType] += possibleAmount;

        RecalculateStats(); //���� ����
        GameManager.Instance.statSaver.RequestSave(GetProgressSaveData()); //���� ���� ��û
        OnStatChanged?.Invoke(); //UI ���ΰ�ħ
    }

    public int GetStat(StatUpgradeType statType)
    {
        return statLevels.TryGetValue(statType, out int level) ? level : 0;
    }

    public float GetProgress(PlayerProgressType progressType)
    {
        return playerProgress.TryGetValue(progressType, out float value) ? value : 0;
    }

    public int GetMaxStat(StatUpgradeType statType)
    {
        return statType switch
        {
            StatUpgradeType.Attack => 25000,
            StatUpgradeType.Health => 25000,
            StatUpgradeType.Critical => 1000,
            StatUpgradeType.AttackSpeed => 300,
            StatUpgradeType.MoveSpeed => 300,
            _ => 0
        };
    }

    public void RecalculateStats()
    {
        level = Mathf.Max((int)GetProgress(PlayerProgressType.Level), 1);
        currentExp = GetProgress(PlayerProgressType.CurrentExp);
        maxExp = maxExp = DataManager.Instance.GetExpData(level);
        statPoint = (int)GetProgress(PlayerProgressType.StatPoint);
        gold = (int)GetProgress(PlayerProgressType.Gold);
        diamond = (int)GetProgress(PlayerProgressType.Diamond);

        damage = 5 + GetStat(StatUpgradeType.Attack) * 3;
        maxHealth = 50 + GetStat(StatUpgradeType.Health) * 10;
        critical = GetStat(StatUpgradeType.Critical) * 0.01f;
        attackSpeed = 1 + GetStat(StatUpgradeType.AttackSpeed) * 0.01f;
        moveSpeed = 5 + GetStat(StatUpgradeType.MoveSpeed) * 0.01f;

        //�̰� ����� �Ϸ��� CSV���� �о�;� ��, �ӽ÷� �ϵ��ڵ���
        //���� �Ѵٸ� StatType, BaseValue, GrowthPerLevel
        //           Attack, 10, 2.5
        //           Health, 100, 20
        //�����θ� damage = baseValue + GetStat(StatType.Attack) * growthPerLevel; �̷� ������ ��� ������
    }
    public Dictionary<StatUpgradeType, int> GetAllLevels()
    {
        return statLevels;
    }

    public void SetAllLevels(Dictionary<StatUpgradeType, int> newLevels)
    {
        foreach (KeyValuePair<StatUpgradeType, int> pair in newLevels)
        {
            statLevels[pair.Key] = pair.Value;
        }
        RecalculateStats();
        RefreshAllStatUIs();
    }

    private void RefreshAllStatUIs()
    {
        //������ ������������ ���������� ���ΰ�ħ��, ���⿡ �� �ʿ� ����
    }

    public PlayerProgressSaveData GetProgressSaveData()
    {
        //Ŭ���� ����
        PlayerProgressSaveData data = new PlayerProgressSaveData();

        //foreach�� ��ųʸ� ��ȯ
        foreach (KeyValuePair<PlayerProgressType, float> progress in playerProgress)
        {
            //Add�� ProgressEntry ����Ʈ�� �߰�
            //Add���ο��� new ProgressEnety�� ��ü ���� �� �ʱ�ȭ
            Debug.Log($"[GetProgressSaveData] {progress.Key} = {progress.Value}");
            data.progressValues.Add(new ProgressEntry
            {
                PlayerProgressType = progress.Key,
                Value = progress.Value
            });
        }

        foreach (KeyValuePair<StatUpgradeType, int> stat in statLevels)
        {
            data.statLevels.Add(new StatLevelEntry
            {
                StatUpgradeType = stat.Key,
                Level = stat.Value
            });
        }

        return data;
    }

    public void LoadProgressSaveData(PlayerProgressSaveData data)
    {
        //�Ѿ�� �������� ProgressEntry����Ʈ�� StatLevelEntry����Ʈ�� ��ȸ�ϸ� ��ųʸ� �ʱ�ȭ

        //���� ���� �ʱ�ȭ
        foreach (ProgressEntry entry in data.progressValues)
        {
            playerProgress[entry.PlayerProgressType] = entry.Value;
            Debug.Log($"[PlayerStats] {entry.PlayerProgressType} : {entry.Value}");
        }

        //���� ��ȭ ���� �ʱ�ȸ
        foreach (StatLevelEntry entry in data.statLevels)
        {
            statLevels[entry.StatUpgradeType] = entry.Level;
            Debug.Log($"[PlayerStats] {entry.StatUpgradeType} : {entry.Level}");
        }

        //��� �ݿ�
        RecalculateStats();
        OnStatChanged?.Invoke(); //(UIStatPage) playerStats.OnStatChanged += Refresh;
    }
}
