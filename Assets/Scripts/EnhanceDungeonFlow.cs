using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnhanceDungeonFlow : IStageFlow
{
    private StageManager _manager;
    private int _killCount = 0; 
    private int _requiredCount = 50; //스테이지 클리어에 필요한 킬수 및 스폰할 몬스터 수

    private const float FADE_DURATION = 2f;

    public EnhanceDungeonFlow(StageManager manager)
    {
        _manager = manager;
    }
     
    public void Start()
    {
        _killCount = 0;
        UIManager.Instance.FadeOut(FADE_DURATION / 2);

        //던전 UI 킬수 업데이트
        _manager.InvokeKillUpdated(_killCount, _requiredCount);

        DelayCallManager.Instance.CallLater(FADE_DURATION / 2, () => SpawnManager.Instance.SpawnEnemiesForEnhanceDungeon(_requiredCount));
    }

    public void OnEnemyDead()
    {

    }

    public void OnStageClear()
    {

    }

    public void OnPlayerDead()
    {

    }

    public void ResetStage()
    {

    }

    public void GiveReward()
    {

    }

    public void OnTimeOut()
    {

    }
}
