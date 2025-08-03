using Cysharp.Threading.Tasks;
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
    public float criticalChance = 0;
    public float criticalDamage = 0;

    [Header("레벨")]
    public int level = 1;
    public float currentExp = 0;
    public float maxExp = 10;

    [Header("재화")]
    public int statPoint = 1;
    public float gold = 0;
    public int diamond = 0;
    public int enhanceStone = 0;
    public int skillGem = 0;

    [Header("보너스")]
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

    public int StatPoint
    {
        get => statPoint;
        set
        {
            statPoint = value;
            playerProgress[PlayerProgressType.StatPoint] = statPoint;
            OnCurrencyChanged?.Invoke();
        }
    }

    public int EnhanceStone
    {
        get => enhanceStone;
        set
        {
            enhanceStone = value;
            playerProgress[PlayerProgressType.EnhanceStone] = enhanceStone;
            OnCurrencyChanged?.Invoke();
        }
    }

    public int SkillGem
    {
        get => skillGem;
        set
        {
            skillGem = value;
            playerProgress[PlayerProgressType.SkillGem] = skillGem;
            OnCurrencyChanged?.Invoke();
        }
    }

    private Dictionary<StatUpgradeType, int> statLevels = new Dictionary<StatUpgradeType, int>();
    private Dictionary<GoldUpgradeType, int> upgradeLevels = new Dictionary<GoldUpgradeType, int>();
    private Dictionary<PlayerProgressType, float> playerProgress = new Dictionary<PlayerProgressType, float>();

    //StatPanelUI 업데이트용 액션
    public event Action OnStatChanged;
    //골드, 다이아 업데이트용 액션
    public event Action OnCurrencyChanged;
    //체력바, 경험치바 업데이트용 액션
    public event Action OnStatusChanged;

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
        //Debug.Log($"{exp} 경험치 획득함, {currentExp} / {maxExp}");
        playerProgress[PlayerProgressType.CurrentExp] = currentExp;

        //방치했을 때 한 번에 많은 경험치가 들어올 수 있음
        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            playerProgress[PlayerProgressType.CurrentExp] = currentExp;

            LevelUp();
        }
        OnStatusChanged?.Invoke();
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

        //Debug.Log($"레벨 상승함, 현재 레벨 : {level}, 다음 경험치 요구량 : {maxExp}");

        StatPoint++;
        playerProgress[PlayerProgressType.StatPoint] = StatPoint;

        GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();
    }

    public void AddStat(StatUpgradeType statType, int amount)
    {
        int currentLevel = GetStat(statType);
        int maxLevel = GetMaxStat(statType);
        //강화하려는 수치가 최대 레벨까지 남은 양과 보유한 스탯 포인트중 더 낮은 쪽까지만 사용하게 함.
        int possibleAmount = Mathf.Min(amount, maxLevel - currentLevel, StatPoint); //최대까지 남은 수치 계산

        //현재 레벨 280, 최대 300, 선택 수치100, 스탯포인트 200이면 실제 강화량 20

        //Debug.Log($"[강화] {statType} + {possibleAmount} (선택값: {amount})");

        //이미 최대치까지 강화된 상태라면
        if (possibleAmount <= 0)
        {
            Debug.LogWarning("강화할 수 없음");
            return;
        }

        StatPoint -= possibleAmount;
        statLevels[statType] += possibleAmount;

        RecalculateStats(); //스탯 재계산
        GameManager.Instance.statSaver.RequestSave(GetProgressSaveData()); //스탯 저장 요청
        OnStatChanged?.Invoke(); //UI 새로고침
        OnStatusChanged?.Invoke();
    }

    public bool AddStat(GoldUpgradeType type, int amount)
    {
        int currentLevel = GetUpgradeLevel(type);
        int maxLevel = GetMaxUpgradeLevel(type);
        GoldUpgradeData data = DataManager.Instance.GetGoldUpgradeData(type);

        int availableAmount = Mathf.Min(amount, maxLevel - currentLevel);

        if (availableAmount <= 0)
        {
            Debug.LogWarning("[PlayerStats] 강화할 수 있는 수치가 없음");
            return false;
        }

        float totalCost = 0f;
        int finalAmount = 0;

        for (int i = 0; i < availableAmount; i++)
        {
            //업그레이드 비용 계산
            float cost = data.Price + (data.PriceIncrease * (currentLevel + i));
            //현재 골드로 가능한만큼 강화할 수치 결정
            if (Gold >= totalCost + cost)
            {
                totalCost += cost;
                finalAmount++;
            }
            else
            {
                break;
            }
        }

        if (finalAmount <= 0)
        {
            Debug.LogWarning("골드 부족");
            return false;
        }

        Gold -= totalCost;
        upgradeLevels[type] -= finalAmount;

        RecalculateStats();
        GameManager.Instance.statSaver.RequestSave(GetProgressSaveData());
        OnStatChanged?.Invoke();
        OnStatusChanged?.Invoke();

        Debug.Log($"[PlayerStats] {type} 강화 + {finalAmount}, 총 비용 {totalCost:F1}");

        return true;
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
        GoldUpgradeData data = DataManager.Instance.GetGoldUpgradeData(type);
        return data.BaseValue + (data.BaseValueIncrease * GetUpgradeLevel(type));
    }

    public void RecalculateStats()
    {
        level = Mathf.Max((int)GetProgress(PlayerProgressType.Level), 1);
        currentExp = GetProgress(PlayerProgressType.CurrentExp);
        maxExp = maxExp = DataManager.Instance.GetExpData(level);
        StatPoint = (int)GetProgress(PlayerProgressType.StatPoint);
        Gold = (int)GetProgress(PlayerProgressType.Gold);
        Diamond = (int)GetProgress(PlayerProgressType.Diamond);

        damage = 5 + (GetStat(StatUpgradeType.Attack) * 3) + (GetUpgradeValue(GoldUpgradeType.Attack));
        maxHealth = 50 + (GetStat(StatUpgradeType.Health) * 10) + (GetUpgradeValue(GoldUpgradeType.Health));
        criticalChance = GetUpgradeLevel(GoldUpgradeType.CriticalChance);
        //TODO : 크리 공식 손봐야 하는데
        attackSpeed = 1 + (GetStat(StatUpgradeType.AttackSpeed) * 0.01f);
        moveSpeed = 5 + (GetStat(StatUpgradeType.MoveSpeed) * 0.01f);

        RecalculateItem();
        RecalculateSkill();
        OnStatusChanged?.Invoke();
    }

    private void RecalculateItem()
    {
        //아이템 영향 계산

        //보유 아이템
        foreach (InventoryItem item in InventoryManager.Instance.GetItemList())
        {
            //아이템을 보유중이라면
            if (item.IsUnlocked == true)
            {
                float value = item.Data.OwnedValue + (item.Data.OwnedValuePerLevel * item.Level);
                ApplyEffect(item.Data.OwnedEffectType, value);
            }
        }

        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            //장착중인 아이템 가져오기
            InventoryItem item = InventoryManager.Instance.GetEquippedItem(type);
            if (item == null)
                continue;

            float value = item.Data.BaseValue + (item.Data.BaseValuePerLevel * item.Level);
            ApplyEffect(item.Data.EquippedEffectType, value);
        }
    }

    private void RecalculateSkill()
    {
        //모든 스킬 데이터를 불러오고
        foreach (SkillData data in DataManager.Instance.GetAllSkillData())
        {
            //현재 얻은 스킬인지 확인하고
            if (SkillManager.Instance.IsUnlocked(data.SkillId))
            {
                //얻은 스킬이라면 계산에 활용함
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
                maxHealth += value;
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
        //어차피 스탯페이지는 열릴때마다 새로고침됨, 여기에 쓸 필요 없음
    }

    public PlayerProgressSaveData GetProgressSaveData()
    {
        //클래스 생성
        PlayerProgressSaveData data = new PlayerProgressSaveData();

        //foreach로 딕셔너리 순환
        foreach (KeyValuePair<PlayerProgressType, float> progress in playerProgress)
        {
            //Add로 ProgressEntry 리스트에 추가
            //Add내부에서 new ProgressEnety로 객체 생성 및 초기화
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

    public void InitializeFromProgressData(PlayerProgressSaveData data)
    {
        //넘어온 데이터의 ProgressEntry리스트와 StatLevelEntry리스트를 순회하며 딕셔너리 초기화

        //진행 상태 초기화
        foreach (ProgressEntry entry in data.progressValues)
        {
            playerProgress[entry.PlayerProgressType] = entry.Value;
            //Debug.Log($"[PlayerStats] {entry.PlayerProgressType} : {entry.Value}");
        }

        //스탯 강화 상태 초기회
        foreach (StatLevelEntry entry in data.statLevels)
        {
            statLevels[entry.StatUpgradeType] = entry.Level;
            //Debug.Log($"[PlayerStats] {entry.StatUpgradeType} : {entry.Level}");
        }

        foreach (GoldLevelEntry entry in data.goldUpgradeLevels)
        {
            upgradeLevels[entry.GoldUpgradeType] = entry.Level;
        }

        //결과 반영
        RecalculateStats();
        OnStatChanged?.Invoke(); //(UIStatPage) playerStats.OnStatChanged += Refresh;
    }


    public bool TrySpendItem(PlayerProgressType type, int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("[PlayerStats] 잘못된 차감 요청");
            return false;
        }

        if (playerProgress.TryGetValue(type, out float value) == false || value < amount)
        {
            return false;
        }

        switch (type)
        {
            case PlayerProgressType.Gold:
                Gold -= amount;
                break;
            case PlayerProgressType.Diamond:
                Diamond -= amount;
                break;
            case PlayerProgressType.StatPoint:
                StatPoint -= amount;
                break;
            case PlayerProgressType.EnhanceStone:
                EnhanceStone -= amount;
                break;
            case PlayerProgressType.SkillGem:
                SkillGem -= amount;
                break;
            default:
                playerProgress[type] -= amount;  
                OnCurrencyChanged?.Invoke();
                break;
        }

        return true;
    }



    public bool HasEnoughCurrency(PlayerProgressType type, int amount)
    {
        if (type == PlayerProgressType.MaxExp || type == PlayerProgressType.CurrentExp)
        {
            Debug.LogWarning($"[PlayerStats] 사용할 수 없는 타입, {type}");
        }


        if (playerProgress.TryGetValue(type, out float value) && value >= amount)
        {
            Debug.Log($"[PlayerStats] {type}, {playerProgress[type]}, {value:F1}, {amount}");

            return true;
        }
        return false;
    }

    public bool TryEnhanceItem(ItemData itemData, InventoryItem item)
    {
        //강화하려는 아이템 레벨이 최대 레벨 이상이라면
        if (item.Level >= itemData.MaxLevel)
        {
            Debug.LogWarning("[PlayerStats] 이미 최대 레벨에 도달함");
            return false;
        }

        //아이템 데이터에서 강화비용 불러오기
        int cost = itemData.UpgradePrice;

        //강화석으로 강화하려는데 실패했다면 리턴
        if (TrySpendItem(PlayerProgressType.EnhanceStone, cost) == false)
        {
            Debug.LogWarning($"[PlayerStats] 강화석이 부족함 {EnhanceStone} / {cost}");
            return false;
        }

        item.Level++;

        //저장 요청, 이후 ItemUI는 UIItemInfoPopup에서 갱신함
        GameManager.Instance.statSaver.RequestSave(GetProgressSaveData());
        GameManager.Instance.statSaver.RequestSave(InventoryManager.Instance.BuildSaveData());
        RecalculateStats();
        return true;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        OnStatusChanged?.Invoke();

    }

    public void Recovery()
    {
        health = maxHealth;
        OnStatusChanged?.Invoke();
    }

    public void ResetStats()
    {
        int totalRefund = 0;

        //foreach돌때 수정할거면 복사해서 수정해야함
        List<StatUpgradeType> keys = new List<StatUpgradeType>(statLevels.Keys);

        foreach (StatUpgradeType type in keys)
        {
            totalRefund += statLevels[type];
            statLevels[type] = 0;
        }

        StatPoint += totalRefund;

        RecalculateStats();
        GameManager.Instance.statSaver.RequestSave(GetProgressSaveData());
        OnStatChanged?.Invoke();
        OnStatusChanged?.Invoke();
    }

    public void AddCurrency(PlayerProgressType type, int amount)
    {
        switch (type)
        {
            case PlayerProgressType.Gold:
                Gold += amount;
                break;
            case PlayerProgressType.Diamond:
                Diamond += amount;
                break;
            case PlayerProgressType.StatPoint:
                StatPoint += amount;
                break;
            case PlayerProgressType.EnhanceStone:
                EnhanceStone += amount;
                break;
            case PlayerProgressType.SkillGem:
                SkillGem += amount;
                break;
            default:
                playerProgress[type] += amount;
                OnCurrencyChanged?.Invoke();
                break;
        }
    }

    [ContextMenu("다이아부자")]
    private void Test()
    {
        Diamond += 100000;
    }
}
