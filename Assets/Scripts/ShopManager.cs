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

    public bool IsLimitExceeded(string shopId, ShopLimitType limitType, int purchaseLimit, DateTime utcDate)
    {
        ShopPurchaseEntry entry = GetOrCreateEntry(shopId);
        NormalizeForLimit(entry, limitType, utcDate);

        if (limitType == ShopLimitType.None)
        {
            return false;
        }
        if (limitType == ShopLimitType.Account)
        {
            return entry.PurchaseCount >= purchaseLimit;
        }
        return entry.PeriodCount >= purchaseLimit;
    }

    //public bool IsLimitReset(DateTime lastPurchased, ShopLimitType limitType)
    //{
    //    DateTime now = DateTime.UtcNow;

    //    switch (limitType)
    //    {
    //        //어제보다 오늘이 더 크면
    //        case ShopLimitType.Daily:
    //            return now.Day > lastPurchased.Day;
    //        case ShopLimitType.Weekly:
    //            //현재 문화권의 달력 계산 도구 가져오기
    //            CultureInfo culture = CultureInfo.InvariantCulture;
    //            Calendar calendar = culture.Calendar;

    //            //검사할 날짜, 첫 주를 언제로 볼지, 한 주의 시작 요일 설정
    //            int nowWeek = calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    //            int lastWeek = calendar.GetWeekOfYear(lastPurchased, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

    //            //이번 주 != 저번에 구매한 주 || 올해 !=  저번에 구매한 해
    //            return nowWeek != lastWeek || now.Year != lastPurchased.Year;
    //        case ShopLimitType.Monthly:
    //            return now.Year != lastPurchased.Year || now.Month != lastPurchased.Month;

    //        case ShopLimitType.Account:
    //        case ShopLimitType.None:
    //        default:
    //            return false;
    //    }
    //}

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
            LastPurchased = 0L
        };

        _purchaseEntries[shopId] = entry;

        return entry;
    }

    public async UniTask UpdatePurchaseAsync(string shopId, ShopLimitType limitType, int count)
    {
        if (count <= 0)
        {
            Debug.LogWarning($"[ShopManager] 잘못된 count : {count}");
            return;
        }

        ShopPurchaseEntry entry = GetOrCreateEntry(shopId);

        //서버에서 받은 long을 DateTime으로 변환, ms를 날짜와 시간으로
        long nowMs = await GameManager.Instance.statSaver.GetServerNowMsAsync();
        DateTime utcNow = DateTimeOffset.FromUnixTimeMilliseconds(nowMs).UtcDateTime;

        NormalizeForLimit(entry, limitType, utcNow);

        //누적 기록 증가
        entry.PurchaseCount += count;

        if (limitType == ShopLimitType.Daily || limitType == ShopLimitType.Weekly || limitType == ShopLimitType.Monthly)
        {
            //기간 제한이 있으면 기간 카운트도 증가
            entry.PeriodCount += count;
        }

        entry.LastPurchased = nowMs;

        SaveData();
    }

    private void NormalizeForLimit(ShopPurchaseEntry entry, ShopLimitType limitType, DateTime utcNow)
    {
        //새로운 기간으로 넘어갔는지 확인하고, 넘어갔으면 구매 횟수 초기화

        //UpdatePurchase에서도 periodCount를 증가시키지 않기는 함
        if (limitType == ShopLimitType.None || limitType == ShopLimitType.Account)
        {
            return;
        }

        DateTime date = utcNow.Date;

        string currentKey = GetWindowKey(limitType, utcNow);

        if (string.Equals(entry.WindowKey, currentKey, StringComparison.Ordinal) == false)
        {
            //저장된 키와 다르면 시간이 바뀐 것으로 취급
            entry.WindowKey = currentKey;
            entry.PeriodCount = 0;
        }
    }

    private string GetWindowKey(ShopLimitType limitType, DateTime utcNow)
    {
        //현재 시간을 문자열로 만들어주고 WindowKey로 저장
        utcNow = utcNow.AddHours(9);
        switch (limitType)
        {
            case ShopLimitType.Daily:
                return utcNow.ToString("yyyy-MM-dd");
            case ShopLimitType.Weekly:
                int week = GetIsoWeekOfYear(utcNow);
                int year = GetIsoWeekYear(utcNow);
                return $"{year}-W{week:D2}";
            case ShopLimitType.Monthly:
                return utcNow.ToString("yyyy-MM");
            case ShopLimitType.Account:
            case ShopLimitType.None:
            default:
                return "ALL";
        }
    }

    private int GetIsoWeekOfYear(DateTime date)
    {
        CultureInfo culture = CultureInfo.InvariantCulture;
        Calendar calendar = culture.Calendar;
        int week = calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return week;
    }

    private int GetIsoWeekYear(DateTime date)
    {
        //단순히 date.Year쓰면 연말, 연초 경계에서 뭔가 꼬임, 그 주의 목요일이 포함된 해를 그 주의 연도로 설정

        //요일 가져오기
        DayOfWeek dayOfWeek = date.DayOfWeek;

        //이번 주의 월요일로 이동할 오프셋 계산
        int offsetToMonday = (dayOfWeek == DayOfWeek.Sunday) ? -6 : (DayOfWeek.Monday - dayOfWeek);

        //월요일 날짜 계산후 3을 더해 목요일로 설정
        DateTime monday = date.AddDays(offsetToMonday);
        DateTime thursday = monday.AddDays(3);

        //목요일이 속한 해를 반환
        return thursday.Year;

        //2024-12-31 화요일 기준
        //dayOfWeek = Tuesday
        //offsetToMonday = monday - tuesday -> 1 - 2 = -1
        //월요일 : 화요일 + (-1)
        //목요일 = 월요일 + 3

        //2024-01-01 수요일 기준
        //dayOfWeek = wednesday
        //offsetToMonday = monday - wedensday -> 1 - 3 = -2
        //월요일 : 수요일 + (-2)
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


    public int GetRemainingForLimit(string shopId, ShopLimitType limitType, int purchaseLimit, DateTime utcDate)
    {
        if (limitType == ShopLimitType.None)
        {
            return int.MaxValue;
        }
        if (purchaseLimit <= 0)
        {
            return 0;
        }

        ShopPurchaseEntry entry = GetOrCreateEntry(shopId);
        NormalizeForLimit(entry, limitType, utcDate);

        int used = (limitType == ShopLimitType.Account) ? entry.PurchaseCount : entry.PeriodCount;
        return Mathf.Max(0, purchaseLimit - used);
    }
}