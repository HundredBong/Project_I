using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleStageFlow : IStageFlow
{
    private int _killCount;
    private int _totalKillsRequired = 100;
    private int _spawnBatchSize = 20;
    private int _defaultSpawnCount = 30;
    private const float FADE_DURATION = 2f;

    private StageManager _manager;

    private Dictionary<RewardType, PlayerProgressType> _rewardTypeToProgressType = new Dictionary<RewardType, PlayerProgressType>
    {
        { RewardType.Diamond, PlayerProgressType.Diamond },
        { RewardType.SkillGem, PlayerProgressType.SkillGem },
        { RewardType.EnhanceStone, PlayerProgressType.EnhanceStone }
    };

    public IdleStageFlow(StageManager mamager)
    {
        _manager = mamager;
    }

    public void Start()
    {
        _killCount = 0;
        UIManager.Instance.FadeOut(FADE_DURATION / 2);

        _manager.InvokeKillUpdated(_killCount, _totalKillsRequired);
        _manager.InvokeStageChanged(DungeonType.None, StageManager.Instance.currentStage, StageManager.Instance.bossChallengable[StageManager.Instance.currentStage - 1]);
        SkillManager.Instance.RequestResetAllCooldowns();

        DelayCallManager.Instance.CallLater(FADE_DURATION / 2, () => SpawnManager.Instance.SpawnEnemiesForCurrentStage(_defaultSpawnCount));
    }

    public void OnEnemyDead()
    {
        _killCount++;
        StageManager.Instance.StopTimer();

        if (_killCount < _totalKillsRequired && _killCount % _spawnBatchSize == 0)
        {
            SpawnManager.Instance.SpawnEnemiesForCurrentStage(_spawnBatchSize);
        }

        _manager.InvokeKillUpdated(_killCount, _totalKillsRequired);

        if (_killCount >= _totalKillsRequired)
        {
            OnStageClear();
        }
    }

    public void OnBossDead()
    {
        StageManager.Instance.StopTimer();

        _manager.bossDefeated[_manager.currentStage - 1] = true;
        GiveReward();
        _manager.maxClearedStage = Mathf.Max(_manager.maxClearedStage, _manager.currentStage);
        _manager.GoToStage(_manager.currentStage + 1, StageMoveType.Clear);


        GameManager.Instance.statSaver.SaveStageDataAsync(_manager.BuildStageSaveData()).Forget();
        GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();

        GameManager.Instance.statSaver.SaveRanking(_manager.BuildRankingSaveData()).Forget();
    }

    public void OnStageClear()
    {
        StageManager.Instance.StopTimer();

        bool canBose = _manager.bossChallengable[_manager.currentStage - 1];
        bool climbing = _manager.currentStage > _manager.maxClearedStage;

        if (climbing)
        {
            if (canBose)
            {
                StartBossChallenge();
            }
            else
            {
                _manager.bossChallengable[_manager.currentStage - 1] = true;
                ResetStage();
            }
        }
        else
        {
            ResetStage();
        }

        GameManager.Instance.statSaver.SaveStageDataAsync(_manager.BuildStageSaveData()).Forget();
        GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();
        GameManager.Instance.statSaver.SetLastActiveNowAsync().Forget();
    }

    public void OnTimeOut()
    {
        OnPlayerDead();
    }

    public void OnPlayerDead()
    {
        StageManager.Instance.StopTimer();

        _manager.currentStage = Mathf.Max(_manager.currentStage - 1, 1);
        _manager.GoToStage(_manager.currentStage, StageMoveType.Dead);


        GameManager.Instance.statSaver.SaveStageDataAsync(_manager.BuildStageSaveData()).Forget();
        GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();
    }

    public void ResetStage()
    {
        ObjectPoolManager.Instance.enemyPool.ReturnAllEnemies();

        DelayCallManager.Instance.CallLater(FADE_DURATION / 2f, () =>
        {
            _killCount = 0;

            GameManager.Instance.player.transform.position = Vector3.zero;
            GameManager.Instance.stats.Recovery();
            GameManager.Instance.player.StateMachine.ChangeState(StateType.Idle);
            SpawnManager.Instance.SpawnEnemiesForCurrentStage(_defaultSpawnCount);

            _manager.InvokeStageChanged(DungeonType.None, _manager.currentStage, _manager.bossChallengable[_manager.currentStage - 1]);
            _manager.InvokeKillUpdated(_killCount, _totalKillsRequired);
        });
    }

    public void StartBossChallenge()
    {
        GameManager.Instance.player.transform.position = Vector3.zero;
        ObjectPoolManager.Instance.enemyPool.ReturnAllEnemies();

        UIManager.Instance.FadeInOut(FADE_DURATION);

        DelayCallManager.Instance.CallLater(FADE_DURATION / 2f, () =>
        {
            SkillManager.Instance.RequestResetAllCooldowns();
            _manager.InvokeBossStageEntered(_manager.currentStage);
        });

        DelayCallManager.Instance.CallLater(FADE_DURATION, () =>
        {
            GameManager.Instance.stats.Recovery();
            SpawnManager.Instance.SpawnStageBoss();
        });
    }

    public void GiveReward()
    {
        StageData stage = DataManager.Instance.stageDataTable[_manager.currentStage];
        GameManager.Instance.stats.AddCurrency(_rewardTypeToProgressType[stage.RewardType], stage.BossRewardAmount);
        ObjectPoolManager.Instance.uiPool.GetReward().Init($"UI_{stage.RewardType}", stage.BossRewardAmount);
        GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();
    }

    public void GiveUp()
    {
        ResetStage();
    }
}
