using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [SerializeField] private int currentStage = 1; //현재 스테이지
    [SerializeField] private int killCount = 0; //현재 킬 카운트
    [SerializeField] private int totalKillsRequired = 100; //스테이지 클리어에 필요한 킬 카운트
    [SerializeField] private int spawnBatchSize = 20; //재생성할 몬스터 수
    [SerializeField] private int defaultSpawnCount = 30; //기본 몬스터 생성 수
    [SerializeField] private int maxClearedStage = 1; //최대 클리어 스테이지
    public bool IsLoop { get; set; } //현재 스테이지를 반복할 것인지에 대한 변수

    [SerializeField] private bool[] bossChallengable;
    [SerializeField] private bool[] bossDefeated;


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
        Debug.Log(data.CurrentStageId);
        currentStage = data.CurrentStageId;
        maxClearedStage = data.MaxClearedStageId;
        bossChallengable = data.BossChallengable ?? new bool[DataManager.Instance.stageDataTable.Count];
        bossDefeated = data.BossDefeated ?? new bool[DataManager.Instance.stageDataTable.Count];
        IsLoop = LocalSetting.LoadStageLoop();

        //Debug.Log($"[StageManager] 스테이지 데이터 불러오기 완료, 현재 스테이지 : {currentStage}");
    }

    public void StartStage()
    {
        //Debug.Log($"[StageManager] {currentStage}스테이지 시작");
        killCount = 0;

        SpawnManager.Instance.SpawnEnemiesForCurrentStage(defaultSpawnCount);
    }


    public void NotifyKill()
    {
        killCount++;

        //다음 스테이지로 넘어가기위한 최대 킬 수에 도달하지 않고, 현재 킬 카운트와 사이즈 연산 값이 0이면 
        //20 < 100 && 20 % 20 == 0 
        if (killCount < totalKillsRequired && killCount % spawnBatchSize == 0)
        {
            //사이즈만큼 몬스터 다시 생성
            SpawnManager.Instance.SpawnEnemiesForCurrentStage(spawnBatchSize);
        }

        //현재 킬 카운트가 스테이지 클리어에 필요한만큼 도달하면
        if (killCount >= totalKillsRequired)
        {
            OnStageClear();
        }
    }

    public void NotifyKillBoss()
    {
        bossDefeated[currentStage] = true;
        GiveBossReward();
        currentStage++;
        maxClearedStage = Mathf.Max(maxClearedStage, currentStage); //최대 클리어 스테이지 업데이트

        GameManager.Instance.statSaver.SaveStageDataAsync(BuildStageSaveData()).Forget();
        GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();

        ResetStage();
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
        //반복중이라면 스테이지 클리어
        if (IsLoop == true)
        {
            ResetStage();
        }

        //반복중이 아니고, 보스를 클리어하지 않았으며, 보스에게 도전 불가능한 상태에서 클리어 했다면
        if (IsLoop == false && bossChallengable[currentStage] == false && bossDefeated[currentStage] == false)
        {
            //보스에게 도전 가능하도록 함,
            bossChallengable[currentStage] = true;
            maxClearedStage = currentStage;
        }

        //반복중이 아니고, 보스에게 도전이 가능하다면
        if (IsLoop == false && bossChallengable[currentStage] == true && bossDefeated[currentStage] == false)
        {
            //보스에게 도전하는 스테이지로 초기화
            GameManager.Instance.player.transform.position = Vector3.zero;
            ObjectPoolManager.Instance.enemyPool.ReturnAllEnemies();
            SpawnManager.Instance.SpawnStageBoss();
        }

        GameManager.Instance.statSaver.SaveStageDataAsync(BuildStageSaveData()).Forget();
        GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();
    }

    private StageSaveData BuildStageSaveData()
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

    private void ResetStage()
    {
        //플레이어 위치 초기화, 기존 몬스터 제거
        killCount = 0;

        GameManager.Instance.player.transform.position = Vector3.zero;
        ObjectPoolManager.Instance.enemyPool.ReturnAllEnemies();
        SpawnManager.Instance.SpawnEnemiesForCurrentStage(30);
    }

    public StageType GetStageType(int stageNumber)
    {
        return (StageType)(((stageNumber - 1) / 100) % Enum.GetValues(typeof(StageType)).Length);
    }

    public int GetCurrentStage()
    {
        return currentStage;
    }
}
