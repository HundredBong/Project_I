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
    public float criticalChance = 0;
    public float criticalDamage = 0;

    [Header("����")]
    public int level = 1;
    public float currentExp = 0;
    public float maxExp = 10;

    [Header("��ȭ")]
    public int statPoint = 0;
    public float gold = 0;
    public int diamond = 0;
    public int enhanceStone = 0;

    [Header("���ʽ�")]
    [SerializeField] private float goldBonus = 1;
    [SerializeField] private float expBonus = 1;

    public float Gold
    {
        get => gold;
        set
        {
            //if (gold != value)
            //{
            gold = value;
            playerProgress[PlayerProgressType.Gold] = gold;
            OnCurrencyChanged?.Invoke();
            //}
        }
    }

    public int Diamond
    {
        get => diamond;
        set
        {
            //if (diamond != value)
            //{
            diamond = value;
            playerProgress[PlayerProgressType.Diamond] = diamond;
            OnCurrencyChanged?.Invoke();
            //}
        }
    }


    private Dictionary<StatUpgradeType, int> statLevels = new Dictionary<StatUpgradeType, int>();
    private Dictionary<GoldUpgradeType, int> upgradeLevels = new Dictionary<GoldUpgradeType, int>();
    private Dictionary<PlayerProgressType, float> playerProgress = new Dictionary<PlayerProgressType, float>();

    //StatPanelUI ������Ʈ�� �׼�
    public event Action OnStatChanged;
    //���, ���̾� ������Ʈ�� �׼�
    public event Action OnCurrencyChanged;


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

        foreach (GoldUpgradeType type in Enum.GetValues(typeof(GoldUpgradeType)))
        {
            upgradeLevels[type] = 0;
        }
    }

    public void GetExp(float exp)
    {
        currentExp += (exp * expBonus);
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

    public void GetGold(float gold)
    {
        Gold += (gold * goldBonus);
    }

    public int GetGold() { return (int)Gold; }

    public int GetDiamond() { return (int)Diamond; }

    private void LevelUp()
    {
        level++;
        maxExp = DataManager.Instance.GetExpData(level);
        playerProgress[PlayerProgressType.Level] = level;

        //Debug.Log($"���� �����, ���� ���� : {level}, ���� ����ġ �䱸�� : {maxExp}");

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

        //Debug.Log($"[��ȭ] {statType} + {possibleAmount} (���ð�: {amount})");

        //�̹� �ִ�ġ���� ��ȭ�� ���¶��
        if (possibleAmount <= 0)
        {
            Debug.LogWarning("��ȭ�� �� ����");
            return;
        }

        statPoint -= possibleAmount;
        statLevels[statType] += possibleAmount;

        RecalculateStats(); //���� ����
        GameManager.Instance.statSaver.RequestSave(GetProgressSaveData()); //���� ���� ��û
        OnStatChanged?.Invoke(); //UI ���ΰ�ħ
    }

    public void AddStat(GoldUpgradeType type, int amount)
    {
        int currentLevel = GetUpgradeLevel(type);
        int maxLevel = GetMaxUpgradeLevel(type);
        int targetLevel = currentLevel + amount;

        if (targetLevel > maxLevel)
        {
            Debug.LogWarning($"[PlayerStats] {type} �ִ� ���� �ʰ�");
            return;
        }

        GoldUpgradeData data = DataManager.Instance.GetGoldUpgradeData(type);
        float price = data.Price + (data.PriceIncrease * currentLevel);

        if (Gold < price)
        {
            Debug.LogWarning($"[PlayerStats] ��尡 ������ {Gold} / {price}");
            return;
        }

        Gold -= price;
        upgradeLevels[type] = targetLevel;

        RecalculateStats();
        GameManager.Instance.statSaver.RequestSave(GetProgressSaveData());
        OnStatChanged?.Invoke(); //UI �� ��� ���ΰ�ħ ����� ��
    }

    public int GetStat(StatUpgradeType statType)
    {
        return statLevels.TryGetValue(statType, out int level) ? level : 0;
    }

    public int GetUpgradeLevel(GoldUpgradeType type)
    {
        return upgradeLevels.TryGetValue(type, out int level) ? level : 0;
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
            StatUpgradeType.AttackSpeed => 300,
            StatUpgradeType.MoveSpeed => 300,
            _ => 0
        };
    }

    public int GetMaxUpgradeLevel(GoldUpgradeType type)
    {
        return type switch
        {
            GoldUpgradeType.Attack => DataManager.Instance.GetGoldUpgradeData(type).MaxLevel,
            GoldUpgradeType.Health => DataManager.Instance.GetGoldUpgradeData(type).MaxLevel,
            GoldUpgradeType.CriticalChance => DataManager.Instance.GetGoldUpgradeData(type).MaxLevel,
            GoldUpgradeType.CriticalDamage => DataManager.Instance.GetGoldUpgradeData(type).MaxLevel,
            _ => 0
        };
    }

    private float GetUpgradeValue(GoldUpgradeType type)
    {
        return DataManager.Instance.GetGoldUpgradeData(type).BaseValue + (DataManager.Instance.GetGoldUpgradeData(type).BaseValueIncrease * GetUpgradeLevel(type));
    }

    public void RecalculateStats()
    {
        level = Mathf.Max((int)GetProgress(PlayerProgressType.Level), 1);
        currentExp = GetProgress(PlayerProgressType.CurrentExp);
        maxExp = maxExp = DataManager.Instance.GetExpData(level);
        statPoint = (int)GetProgress(PlayerProgressType.StatPoint);
        Gold = (int)GetProgress(PlayerProgressType.Gold);
        Diamond = (int)GetProgress(PlayerProgressType.Diamond);

        damage = 5 + (GetStat(StatUpgradeType.Attack) * 3) + (GetUpgradeValue(GoldUpgradeType.Attack));
        maxHealth = 50 + (GetStat(StatUpgradeType.Health) * 10) + (GetUpgradeValue(GoldUpgradeType.Health));
        criticalChance = GetUpgradeLevel(GoldUpgradeType.CriticalChance);
        //TODO : ũ�� ���� �պ��� �ϴµ� �켱���� �ſ쳷��
        attackSpeed = 1 + (GetStat(StatUpgradeType.AttackSpeed) * 0.01f);
        moveSpeed = 5 + (GetStat(StatUpgradeType.MoveSpeed) * 0.01f);

        RecalculateItem();
        RecalculateSkill();
    }

    private void RecalculateItem()
    {
        //������ ���� ���

        //���� ������
        foreach (InventoryItem item in InventoryManager.Instance.GetItemList())
        {
            //�������� �������̶��
            if (item.IsUnlocked == true)
            {
                float value = item.Data.OwnedValue + (item.Data.OwnedValuePerLevel * item.Level);
                ApplyEffect(item.Data.OwnedEffectType, value);
            }
        }

        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            //�������� ������ ��������
            InventoryItem item = InventoryManager.Instance.GetEquippedItem(type);
            if (item == null)
                continue;

            float value = item.Data.BaseValue + (item.Data.BaseValuePerLevel * item.Level);
            ApplyEffect(item.Data.EquippedEffectType, value);
        }
    }

    private void RecalculateSkill()
    {
        //��� ��ų �����͸� �ҷ�����
        foreach (SkillData data in DataManager.Instance.GetAllSkillData())
        {
            //���� ���� ��ų���� Ȯ���ϰ�
            if (SkillManager.Instance.IsUnlocked(data.SkillId))
            {
                //���� ��ų�̶�� ��꿡 Ȱ����
                ApplyEffect(data.EffectType, data.PassiveValue + (data.PassiveValuePerLevel * SkillManager.Instance.GetSkillState(data.SkillId).Level));
            }

        }
    }

    private void ApplyEffect(SkillEffectType type, float value)
    {
        switch (type)
        {
            case SkillEffectType.GoldBonus:
                goldBonus += value;
                break;
            case SkillEffectType.ExpBonus:
                expBonus += value;
                break;
            case SkillEffectType.DamageBonus:
                damage += value;
                break;
            case SkillEffectType.HealthBonus:
                health += value;
                break;
            case SkillEffectType.CriticalDamageBonus:
                criticalDamage += value; 
                break;
            case SkillEffectType.CriticalChanceBonus:
                criticalChance += value; 
                break;
        }
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
            //Debug.Log($"[GetProgressSaveData] {progress.Key} = {progress.Value}");
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

        foreach (KeyValuePair<GoldUpgradeType, int> upgrade in upgradeLevels)
        {
            data.goldUpgradeLevels.Add(new GoldLevelEntry
            {
                GoldUpgradeType = upgrade.Key,
                Level = upgrade.Value
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
            //Debug.Log($"[PlayerStats] {entry.PlayerProgressType} : {entry.Value}");
        }

        //���� ��ȭ ���� �ʱ�ȸ
        foreach (StatLevelEntry entry in data.statLevels)
        {
            statLevels[entry.StatUpgradeType] = entry.Level;
            //Debug.Log($"[PlayerStats] {entry.StatUpgradeType} : {entry.Level}");
        }

        foreach (GoldLevelEntry entry in data.goldUpgradeLevels)
        {
            upgradeLevels[entry.GoldUpgradeType] = entry.Level;
        }

        //��� �ݿ�
        RecalculateStats();
        OnStatChanged?.Invoke(); //(UIStatPage) playerStats.OnStatChanged += Refresh;
    }

    //��ȭ��, ��� ����
    public bool TrySpendItem(PlayerProgressType type, int amount)
    {
        if (type == PlayerProgressType.Gold || type == PlayerProgressType.MaxExp || type == PlayerProgressType.CurrentExp)
        {
            Debug.LogWarning($"[PlayerStats] ����� �� ���� Ÿ��, {type}");
        }


        if (playerProgress.TryGetValue(type, out float value) && value >= amount)
        {
            //Debug.Log($"[PlayerStats] {type}, {playerProgress[type]}, {value:F1}, {amount}");

            playerProgress[type] -= amount;
            return true;
        }
        return false;
    }

    public bool TryEnhanceItem(ItemData itemData, InventoryItem item)
    {
        //��ȭ�Ϸ��� ������ ������ �ִ� ���� �̻��̶��
        if (item.Level >= itemData.MaxLevel)
        {
            Debug.LogWarning("[PlayerStats] �̹� �ִ� ������ ������");
            return false;
        }

        //������ �����Ϳ��� ��ȭ��� �ҷ�����
        int cost = itemData.UpgradePrice;

        //��ȭ������ ��ȭ�Ϸ��µ� �����ߴٸ� ����
        if (TrySpendItem(PlayerProgressType.EnhanceStone, cost) == false)
        {
            Debug.LogWarning($"[PlayerStats] ��ȭ���� ������ {enhanceStone} / {cost}");
            return false;
        }

        item.Level++;

        //���� ��û, ���� ItemUI�� UIItemInfoPopup���� ������
        GameManager.Instance.statSaver.RequestSave(GetProgressSaveData());
        GameManager.Instance.statSaver.RequestSave(InventoryManager.Instance.GetSaveData());
        //OnStatChanged?.Invoke();
        return true;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    [ContextMenu("���̾ƺ���")]
    private void Test()
    {
        Diamond += 100000;
    }
}
