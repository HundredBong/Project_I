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

    public IdleStageFlow(StageManager mamager)
    {
        _manager = mamager;
    }

    public void Start()
    {
        _killCount = 0;
        UIManager.Instance.FadeOut(FADE_DURATION / 2);

        _manager.InvokeKillUpdated(_killCount, _totalKillsRequired);
        _manager.InvokeStageChanged(DungeonType.None, _killCount, StageManager.Instance.bossChallengable[StageManager.Instance.currentStage - 1]);

        DelayCallManager.Instance.CallLater(FADE_DURATION / 2, () => SpawnManager.Instance.SpawnEnemiesForCurrentStage(_defaultSpawnCount));
    }

    public void OnEnemyDead()
    {
        _killCount++;

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
        _manager.bossDefeated[_manager.currentStage - 1] = true;
        GiveReward();
        _manager.maxClearedStage = Mathf.Max(_manager.maxClearedStage, _manager.currentStage);
        _manager.GoToStage(_manager.currentStage + 1);


        GameManager.Instance.statSaver.SaveStageDataAsync(_manager.BuildStageSaveData()).Forget();
        GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();
    }

    public void OnStageClear()
    {
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
    }

    public void OnTimeOut()
    {

    }

    public void OnPlayerDead()
    {
        _manager.currentStage = Mathf.Max(_manager.currentStage - 1, 1);
        _manager.GoToStage(_manager.currentStage);
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

            _manager.InvokeStageChanged(DungeonType.None, _manager.currentStage, _manager.bossChallengable[_manager.currentStage]);
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

        switch (stage.RewardType)
        {
            case RewardType.Diamond:
                GameManager.Instance.stats.Diamond += stage.BossRewardAmount;
                break;
            case RewardType.SkillGem:
                GameManager.Instance.stats.skillGem += stage.BossRewardAmount;
                break;
            case RewardType.EnhanceStone:
                GameManager.Instance.stats.enhanceStone += stage.BossRewardAmount;
                break;
            default:
                break;
        }
    }
}
