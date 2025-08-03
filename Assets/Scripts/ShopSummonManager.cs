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
    private Dictionary<string, ShopPurchaseEntry> purchaseEntries = new Dictionary<string, ShopPurchaseEntry>();


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
        purchaseEntries = data.PurchaseEntries;
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
        //shopId로 구매기록 조회
        //구매 기록이 없다면 -> 제한을 넘지 않음 -> false 리턴
        //구매 기록이 있다면 -> lastPurchased 조건 검사 
        Debug.Log($"ShopId : {shopId}");
        if (purchaseEntries.TryGetValue(shopId, out ShopPurchaseEntry entry) == false)
        {
            return false;
        }

        DateTime lastPurchasedTime = DateTime.Parse(entry.LastPurchased);

        bool isReset = IsLimitReset(lastPurchasedTime, limitType);

        if (isReset)
        {
            return false;
        }

        //제한 이상이면 도달했으므로 true반환
        return entry.PurchaseCount >= purchaseLimit;
    }


    public bool IsLimitReset(DateTime lastPurchased, ShopLimitType limitType)
    {
        //Daily 오늘이 마지막에 저장된 데이터보다 크면
        //Weekly 이번 주 != 구매한 주
        //Monthly 이번 달 != 구매한 달
        //Account, None false 반환

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
                int nowWeek = calendar.GetWeekOfYear(now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                int lastWeek = calendar.GetWeekOfYear(lastPurchased, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

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

    public ShopPurchaseEntry GetPurchaseEntry(string shopId)
    {
        if (purchaseEntries.TryGetValue(shopId, out ShopPurchaseEntry entry) == false)
        {
            Debug.LogWarning($"[ShopManager] 해당하는 구매 데이터가 없음 {shopId}");

            return new ShopPurchaseEntry()
            {
                PurchaseCount = 0,
                LastPurchased = null
            };
        }
        return entry;
    }

    public void UpdatePurchase(string shopId, int count)
    {
        if (purchaseEntries.TryGetValue(shopId, out var entry))
        {
            entry.PurchaseCount += count;
            entry.LastPurchased = DateTime.UtcNow.ToString("o");
        }
        else
        {
            purchaseEntries[shopId] = new ShopPurchaseEntry
            {
                PurchaseCount = count,
                LastPurchased = DateTime.UtcNow.ToString("o")
            };
        }

        SaveData();
    }
    private void SaveData()
    {
        ShopPurchaseData data = new ShopPurchaseData
        {
            PurchaseEntries = purchaseEntries
        };

        GameManager.Instance.statSaver.SavePurchaseData(data).Forget();
    }
}
