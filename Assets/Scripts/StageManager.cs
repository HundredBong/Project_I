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
    private const float FADE_DURATION = 2f;

    [SerializeField] public int currentStage = 1; //현재 스테이지///////////


    public int MaxClearedStage { get; private set; }

    [SerializeField] public bool[] bossChallengable; ////////////////////////
    [SerializeField] public bool[] bossDefeated;

    public event Action<int, int> OnKillUpdated; //현재 킬, 필요 킬
    public event Action<int, bool> OnStageChanged; //현재 스테이지,canBoss
    public event Action<int> OnBossStageEntered; //current

    private Dictionary<DungeonType, int> _dungeonClearedLevelData = new Dictionary<DungeonType, int>();

    //

    private IStageFlow _stageFlow;

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

        //Debug.Log(data.CurrentStageId);
        currentStage = data.CurrentStageId;
        MaxClearedStage = data.MaxClearedStageId;
        bossChallengable = data.BossChallengable ?? new bool[DataManager.Instance.stageDataTable.Count];
        bossDefeated = data.BossDefeated ?? new bool[DataManager.Instance.stageDataTable.Count];
    }

    public void SetDungeonData(DungeonSaveData data)
    {
        //서버에서 최대 클리어 레벨 불러와서 세팅
        _dungeonClearedLevelData = data.DungeonClearedData;
    }

    public IStageFlow CreateFlow(DungeonType type)
    {
        return type switch
        {
            DungeonType.None => new IdleStageFlow(this),
            _ => null
        };
    }

    public void StartStage(DungeonType type = DungeonType.None)
    {
        _stageFlow = CreateFlow(type);

        _stageFlow.Start();

        //killCount = 0;
        //UIManager.Instance.FadeOut(FADE_DURATION / 2);
        //OnStageChanged?.Invoke(currentStage, bossChallengable[currentStage - 1]); //스테이지 시작할 때마다 현재 스테이지 갱신
        //OnKillUpdated?.Invoke(killCount, totalKillsRequired);
        //DelayCallManager.Instance.CallLater(FADE_DURATION / 2, () => SpawnManager.Instance.SpawnEnemiesForCurrentStage(defaultSpawnCount));
    }


    public void NotifyKill()
    {
        _stageFlow.OnEnemyDead();
        //killCount++;

        ////Debug.Log($"킬 카운트 {killCount}, 연산 : {killCount % spawnBatchSize}, bool : {killCount < totalKillsRequired && killCount % spawnBatchSize == 0}");
        //////다음 스테이지로 넘어가기위한 최대 킬 수에 도달하지 않고, 현재 킬 카운트와 사이즈 연산 값이 0이면 
        //if (killCount < totalKillsRequired && killCount % spawnBatchSize == 0)
        //{
        //    SpawnManager.Instance.SpawnEnemiesForCurrentStage(spawnBatchSize);
        //}

        //OnKillUpdated?.Invoke(killCount, totalKillsRequired);

        ////현재 킬 카운트가 스테이지 클리어에 필요한만큼 도달하면
        //if (killCount >= totalKillsRequired)
        //{
        //    OnStageClear();
        //}
    }

    public void NotifyKillBoss()
    {
        bossDefeated[currentStage - 1] = true;
        GiveBossReward();
        Debug.Log($"Max : {MaxClearedStage}, current : {currentStage}, bool {currentStage > MaxClearedStage}");
        MaxClearedStage = Mathf.Max(MaxClearedStage, currentStage); //최대 스테이지 업데이트
        GoToStage(currentStage + 1);


        GameManager.Instance.statSaver.SaveStageDataAsync(BuildStageSaveData()).Forget();
        GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();
    }

    private void GiveBossReward()
    {
        StageData stage = DataManager.Instance.stageDataTable[currentStage];

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

    private void OnStageClear()
    {
        
        //Debug.Log($"current : {currentStage}, max : {MaxClearedStage}, bossChallengable : {bossChallengable[currentStage - 1]}");

        //bool canBose = bossChallengable[currentStage - 1];
        //bool climbing = currentStage > MaxClearedStage;// || (currentStage >= MaxClearedStage && canBose);


        //if (climbing)
        //{
        //    if (canBose)
        //    {
        //        StartBossChallenge();
        //    }
        //    else
        //    {
        //        bossChallengable[currentStage - 1] = true;
        //        ResetStage();
        //    }
        //}
        //else
        //{
        //    ResetStage();
        //}

        //GameManager.Instance.statSaver.SaveStageDataAsync(BuildStageSaveData()).Forget();
        //GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();
    }

    public void StartBossChallenge()
    {
        GameManager.Instance.player.transform.position = Vector3.zero;
        ObjectPoolManager.Instance.enemyPool.ReturnAllEnemies();

        UIManager.Instance.FadeInOut(FADE_DURATION);

        DelayCallManager.Instance.CallLater(FADE_DURATION / 2f, () =>
        {
            OnBossStageEntered?.Invoke(currentStage);
        });

        DelayCallManager.Instance.CallLater(FADE_DURATION, () =>
        {
            GameManager.Instance.stats.Recovery();
            SpawnManager.Instance.SpawnStageBoss();
        });
    }

    public StageSaveData BuildStageSaveData()
    {
        StageSaveData data = new StageSaveData
        {
            CurrentStageId = this.currentStage,
            MaxClearedStageId = this.MaxClearedStage,
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
        if (stage == MaxClearedStage && bossDefeated[stage - 1] == true)
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
