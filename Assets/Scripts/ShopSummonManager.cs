using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopSummonManager
{
    private Dictionary<SummonSubCategory, int> levels = new Dictionary<SummonSubCategory, int>();
    private Dictionary<SummonSubCategory, int> exps = new Dictionary<SummonSubCategory, int>();
    private Dictionary<SummonSubCategory, HashSet<int>> claimedLevels = new Dictionary<SummonSubCategory, HashSet<int>>();

    public void Init(SummonProgressData data)
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

}
