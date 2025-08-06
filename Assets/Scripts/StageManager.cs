using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    //DungeonType.None
    private const float FADE_DURATION = 2f;
    public int currentStage = 1;
    public int maxClearedStage;
    public bool[] bossChallengable;
    public bool[] bossDefeated;

    public event Action<int, int> OnKillUpdated; //현재 킬, 필요 킬
    public event Action<int, bool> OnStageChanged; //현재 스테이지,canBoss
    public event Action<int> OnBossStageEntered; //current

    private Dictionary<DungeonType, int> _dungeonClearedLevelData = new Dictionary<DungeonType, int>();

    private IStageFlow _stageFlow;

    public int enhanceDungeonLevel;  //DungeonInfoPopup에서 초기화
    public int skillGemDungeonLevel; //DungeonInfoPopup에서 초기화

    public IdleStageFlow CurrentIdleStageFlow
    {
        get
        {
            return _stageFlow as IdleStageFlow;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        //---------------------------------------------

        if (bossChallengable == null || bossChallengable.Length == 0)
        {
            bossChallengable = new bool[DataManager.Instance.stageDataTable.Count];
        }

        if (bossDefeated == null || bossDefeated.Length == 0)
        {
            bossDefeated = new bool[DataManager.Instance.stageDataTable.Count];
        }
    }

    public void SetStageData(StageSaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[StageManager] 로드된 데이터가 null임");
            return;
        }

        currentStage = data.CurrentStageId;
        maxClearedStage = data.MaxClearedStageId;
        bossChallengable = data.BossChallengable ?? new bool[DataManager.Instance.stageDataTable.Count];
        bossDefeated = data.BossDefeated ?? new bool[DataManager.Instance.stageDataTable.Count];
    }

    public void SetDungeonData(DungeonSaveData data)
    {
        _dungeonClearedLevelData.Clear();
        _dungeonClearedLevelData = data.DungeonClearedData;
    }

    public IStageFlow CreateFlow(DungeonType type)
    {
        return type switch
        {
            DungeonType.None => new IdleStageFlow(this),
            DungeonType.EnhanceDungeon => new EnhanceDungeonFlow(this),
            _ => null
        };
    }

    public void StartStage(DungeonType type)
    {
        _stageFlow = CreateFlow(type);

        _stageFlow.Start();
    }


    public void NotifyKill()
    {
        _stageFlow.OnEnemyDead();
    }

    public void NotifyKillBoss()
    {
        CurrentIdleStageFlow?.OnBossDead();
    }

    public void StartBossChallenge()
    {
        CurrentIdleStageFlow?.StartBossChallenge();
    }

    public StageSaveData BuildStageSaveData()
    {
        StageSaveData data = new StageSaveData
        {
            CurrentStageId = this.currentStage,
            MaxClearedStageId = this.maxClearedStage,
            BossChallengable = this.bossChallengable,
            BossDefeated = this.bossDefeated,
        };

        return data;
    }

    public void ResetStage()
    {
        _stageFlow.ResetStage();
    }

    public StageType GetStageType(int stageNumber)
    {
        return (StageType)(((stageNumber - 1) / 100) % Enum.GetValues(typeof(StageType)).Length);
    }

    public int GetCurrentStage()
    {
        return currentStage;
    }

    public void GoToStage(int stage)
    {
        if (stage == maxClearedStage && bossDefeated[stage - 1] == true)
        {
            stage++;
        }

        UIManager.Instance.FadeInOut(FADE_DURATION);

        DelayCallManager.Instance.CallLater(FADE_DURATION / 2f, () =>
        {
            currentStage = stage;
            ResetStage();
        });
    }

    public int GetMaxClearedLevel(DungeonType type)
    {
        return _dungeonClearedLevelData[type];
    }

    public void InvokeKillUpdated(int killCount, int required)
    {
        OnKillUpdated?.Invoke(killCount, required);
    }

    public void InvokeStageChanged(int killCount, bool canBoss)
    {
        OnStageChanged?.Invoke(killCount, canBoss);
    }

    public void InvokeBossStageEntered(int stage)
    {
        OnBossStageEntered?.Invoke(stage);
    }
}
