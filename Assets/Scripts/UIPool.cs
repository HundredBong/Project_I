using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPool : GenericPoolManager<PooledUI>
{
    public GameObject resultPrefab;
    public GameObject toastMessagePrefab;
    public GameObject toastRewardPrefab;
    public GameObject rewardSlotPrebfab;
    public GameObject rankingPrefab;
    public GameObject damageTextPrefab;

    public void Start()
    {
        Preload(resultPrefab, 30);
        Preload(toastMessagePrefab, 10);
        Preload(toastRewardPrefab, 10);
    }

    public T GetUI<T>(GameObject prefab) where T : PooledUI
    {
        return base.Get(prefab) as T;
    }

    public UIResultContent GetResult()
    {
        return base.Get(resultPrefab) as UIResultContent;
    }

    public UIToastMessage GetMessage()
    {
        return base.Get(toastMessagePrefab) as UIToastMessage;
    }

    public UIToastReward GetReward()
    {
        return base.Get(toastRewardPrefab) as UIToastReward;
    }

    public UIRewardSlot GetRewardSlot()
    {
        return base.Get(rewardSlotPrebfab) as UIRewardSlot;
    }

    public UIRankingSlot GetRankingSlot()
    {
        return base.Get(rankingPrefab) as UIRankingSlot;
    }

    public DamageText GetDamageText()
    {
        return base.Get(damageTextPrefab) as DamageText;
    }
}
