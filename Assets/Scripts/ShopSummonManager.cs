using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ShopManager
{
    private Dictionary<SummonSubCategory, int> levels = new Dictionary<SummonSubCategory, int>();
    private Dictionary<SummonSubCategory, int> exps = new Dictionary<SummonSubCategory, int>();
    private Dictionary<SummonSubCategory, HashSet<int>> claimedLevels = new Dictionary<SummonSubCategory, HashSet<int>>();
    private Dictionary<string, ShopPurchaseEntry> _purchaseEntries = new Dictionary<string, ShopPurchaseEntry>();


    public void InitSummonProgressData(SummonProgressData data)
    {
        levels.Clear();
        exps.Clear();
        claimedLevels.Clear();

        foreach (SummonProgressEntry entry in data.SummonProgressEntries)
        {
            levels[entry.Category] = entry.Level;
            exps[entry.Category] = entry.Exp;
        }

        foreach (SummonRewardClaimEntry entry in data.SummonRewardEntries)
        {
            claimedLevels[entry.Category] = new HashSet<int>(entry.Levels);
        }
    }

    public void InitPurchaseData(ShopPurchaseData data)
    {
        _purchaseEntries = data.PurchaseEntries;
    }

    public int GetLevel(SummonSubCategory category)
    {
        //레벨 딕셔너리에서 카테고리에 해당하는 레벨이 있는지 확인후 반환
        if (levels.TryGetValue(category, out int level))
        {
            return level;
        }

        return 1;
    }

    public int GetExp(SummonSubCategory category)
    {
        if (exps.TryGetValue(category, out int exp))
        {
            return exp;
        }

        return 0;
    }

    public void AddExp(SummonSubCategory category, int exp)
    {
        if (levels.ContainsKey(category) == false) { levels[category] = 1; }
        if (exps.ContainsKey(category) == false) { exps[category] = 0; }
        exps[category] += exp;

        while (true)
        {
            int currentLevel = levels[category];
            int maxExp = DataManager.Instance.GetSummonMaxExp(category, currentLevel);

            //exp가 올랐을 때 최대 exp보다 크다면
            if (exps[category] >= maxExp)
            {
                //다음 while문에서 maxExp 또 계산하니 혹여나 두 단계 건너뛰어도 괜찮음
                exps[category] -= maxExp;
                levels[category]++;
            }
            else
            {
                break;
            }
        }
    }

    public void SetLevel(SummonSubCategory category, int level)
    {
        levels[category] = level;
    }

    public SummonProgressData BuildSummonProgressData()
    {
        List<SummonProgressEntry> entries = new List<SummonProgressEntry>();
        List<SummonRewardClaimEntry> rewardEntries = new List<SummonRewardClaimEntry>();

        foreach (var kvp in levels)
        {
            entries.Add(new SummonProgressEntry
            {
                Category = kvp.Key,
                Level = kvp.Value,
                Exp = exps[kvp.Key],
                //Exp = exps.TryGetValue(kvp.Key, out int exp) ? exp : 0
            });
        }

        foreach (var kvp in claimedLevels)
        {
            rewardEntries.Add(new SummonRewardClaimEntry()
            {
                Category = kvp.Key,
                Levels = new List<int>(kvp.Value)
            });
        }

        SummonProgressData data = new SummonProgressData { SummonProgressEntries = entries, SummonRewardEntries = rewardEntries };

        return data;
    }

    public bool HasClaimed(SummonSubCategory category, int level)
    {
        if (claimedLevels.TryGetValue(category, out var levels) && levels.Contains(level))
        {
            return true;
        }

        return false;
    }

    public void ClaimReward(SummonSubCategory category, int level)
    {
        if (claimedLevels.TryGetValue(category, out var levels) == false)
        {
            levels = new HashSet<int>();
            claimedLevels[category] = levels;
        }

        levels.Add(level);
    }

    public bool IsLimitExceeded(string shopId, ShopLimitType limitType, int purchaseLimit)
    {
        ShopPurchaseEntry entry = GetOrCreateEntry(shopId);
        NormalizeForLimit(entry, limitType, DateTime.UtcNow);

        if (limitType == ShopLimitType.None)
        {
            return false;
        }

        //구매 제한이 0 이하인 경우는 구매 못하도록 true반환
        if (purchaseLimit <= 0)
        {
            return true;
        }


        if (limitType == ShopLimitType.Account)
        {
            return entry.PurchaseCount >= purchaseLimit;
        }

        //그 외 일일, 주간, 월간 제한
        return entry.PeriodCount >= purchaseLimit;
    }


    public bool IsLimitReset(DateTime lastPurchased, ShopLimitType limitType)
    {
        DateTime now = DateTime.UtcNow;

        switch (limitType)
        {
            //어제보다 오늘이 더 크면
            case ShopLimitType.Daily:
                return now.Day > lastPurchased.Day;
            case ShopLimitType.Weekly:
                //현재 문화권의 달력 계산 도구 가져오기
                CultureInfo culture = CultureInfo.InvariantCulture;
                Calendar calendar = culture.Calendar;

                //검사할 날짜, 첫 주를 언제로 볼지, 한 주의 시작 요일 설정
                int nowWeek = calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                int lastWeek = calendar.GetWeekOfYear(lastPurchased, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                //이번 주 != 저번에 구매한 주 || 올해 !=  저번에 구매한 해
                return nowWeek != lastWeek || now.Year != lastPurchased.Year;
            case ShopLimitType.Monthly:
                return now.Year != lastPurchased.Year || now.Month != lastPurchased.Month;

            case ShopLimitType.Account:
            case ShopLimitType.None:
            default:
                return false;
        }
    }

    private void SaveData()
    {
        ShopPurchaseData data = new ShopPurchaseData
        {
            PurchaseEntries = _purchaseEntries
        };

        GameManager.Instance.statSaver.SavePurchaseData(data).Forget();
    }

    public ShopPurchaseEntry GetOrCreateEntry(string shopId)
    {
        //비상키 생성
        if (string.IsNullOrEmpty(shopId))
        {
            Debug.LogError("[ShopManager] ShopId가 비어있음");
            shopId = "_INVALID_";
        }

        //이미 구매기록이 있다면 해당 기록 반환
        if (_purchaseEntries.TryGetValue(shopId, out ShopPurchaseEntry entry))
        {
            return entry;
        }

        entry = new ShopPurchaseEntry
        {
            PurchaseCount = 0,
            PeriodCount = 0,
            WindowKey = null,
            LastPurchased = null
        };

        _purchaseEntries[shopId] = entry;

        return entry;
    }

    private string GetWindowKey(ShopLimitType limitType, DateTime utcNow)
    {
        switch (limitType)
        {
            case ShopLimitType.Daily:
                return utcNow.ToString("yyyy-MM-dd");
            case ShopLimitType.Weekly:
                CultureInfo culture = CultureInfo.InvariantCulture;
                Calendar calendar = culture.Calendar;
                int week = calendar.GetWeekOfYear(utcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                return $"{utcNow:yyyy}-W{week:D2}";
            case ShopLimitType.Monthly:
                return utcNow.ToString("yyyy-MM");
            case ShopLimitType.Account:
            case ShopLimitType.None:
            default:
                return "ALL";
        }
    }

    private void NormalizeForLimit(ShopPurchaseEntry entry, ShopLimitType limitType, DateTime utcNow)
    {
        //현재 시간 키
        string currentKey = GetWindowKey(limitType, utcNow);

        if (entry.WindowKey != currentKey)
        {
            //저장된 키와 다르면 시간이 바뀐 것으로 취급
            entry.WindowKey = currentKey;
            entry.PeriodCount = 0;
        }
    }

    public void UpdatePurchase(string shopId, ShopLimitType limitType, int count)
    {
        if (count <= 0)
        {
            Debug.LogWarning($"[ShopManager] 잘못된 count : {count}");
            return;
        }

        ShopPurchaseEntry entry = GetOrCreateEntry(shopId);

        DateTime utcNow = DateTime.UtcNow;

        NormalizeForLimit(entry, limitType, utcNow);

        //누적 기록 증가
        entry.PurchaseCount += count;

        if (limitType == ShopLimitType.Daily || limitType == ShopLimitType.Weekly || limitType == ShopLimitType.Monthly)
        {
            //기간 제한이 있으면 기간 카운트도 증가
            entry.PeriodCount += count;
        }

        entry.LastPurchased = utcNow.ToString("o");

        SaveData();
    }

    public int GetRemainingForLimit(string shopId, ShopLimitType limitType, int purchaseLimit)
    {
        if (limitType == ShopLimitType.None)
        {
            return int.MaxValue;
        }


        if (purchaseLimit <= 0)
        {
            return 0; //구매 제한이 0 이하인 경우는 구매 불가능
        }

        ShopPurchaseEntry entry = GetOrCreateEntry(shopId);
        NormalizeForLimit(entry, limitType, DateTime.UtcNow);

        int used;

        switch (limitType)
        {
            case ShopLimitType.Account:
                used = entry.PurchaseCount;
                break;
            case ShopLimitType.Daily:
            case ShopLimitType.Weekly:
            case ShopLimitType.Monthly:
                used = entry.PeriodCount;
                break;
            default:
                return int.MaxValue;

        }

        int remaining = Mathf.Max(0, (purchaseLimit - used));

        return remaining;
    }
}