using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPool : GenericPoolManager<PooledUI>
{
    public GameObject resultPrefab;
    public GameObject toastMessagePrefab;
    public GameObject toastRewardPrefab;
    int index = 0;

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

    private void Test()
    {
        index++;
        UIToastMessage message = GetMessage();
        if (message == null) { Debug.LogError("³Î"); return; }
        message.Init($"TEST {index}");
    }

    private void Test2()
    {
        index++;
        UIToastReward reward = GetReward();
        if (reward == null) { Debug.LogError("³Î"); return; }
        reward.Init(DataManager.Instance.GetSpriteByKey("UI_EnhanceStone"), index);
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        Test2();
    //    }
    //}
}
