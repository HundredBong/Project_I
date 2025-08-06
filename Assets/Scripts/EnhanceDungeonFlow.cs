using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnhanceDungeonFlow : IStageFlow
{
    private StageManager _manager;
    private DungeonLevelData _levelData;
    private int _killCount = 0;
    private int _requiredCount = 50; //스테이지 클리어에 필요한 킬수 및 스폰할 몬스터 수

    private const float FADE_DURATION = 2f;

    public EnhanceDungeonFlow(StageManager manager)
    {
        _manager = manager;

        _levelData = DataManager.Instance.GetDungeonLevelData(DungeonType.EnhanceDungeon, StageManager.Instance.enhanceDungeonLevel);
    }

    public void Start()
    {
        _killCount = 0;
        GameManager.Instance.player.transform.position = Vector2.zero;
        UIManager.Instance.FadeOut(FADE_DURATION / 2);

        //던전 UI 킬수 업데이트
        _manager.InvokeKillUpdated(_killCount, _requiredCount);

        DelayCallManager.Instance.CallLater(FADE_DURATION / 2, () => SpawnManager.Instance.SpawnEnemiesForEnhanceDungeon(_requiredCount));
    }

    public void OnEnemyDead()
    {
        _killCount++;

        //던전용 UI업데이트
        _manager.InvokeKillUpdated(_killCount,_requiredCount);

        if (_killCount >= _requiredCount)
        {
            OnStageClear();
        }
    }

    public void OnStageClear()
    {
        ObjectPoolManager.Instance.uiPool.GetMessage().Init("하하 멍청이");
        GiveReward(); //플레이어한테 보상 지급
        //다음 단계, 나가기가 있는 팝업 띄우기
    }

    public void OnPlayerDead()
    {
        //팝업 띄우기
    }

    public void ResetStage()
    {
        Start();
    }

    public void GiveReward()
    {
        
    }

    public void OnTimeOut()
    {

    }
}
