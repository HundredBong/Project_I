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
    [SerializeField]private int maxClearedStage = 1; //최대 클리어 스테이지
    [SerializeField]private bool isLoop; //현재 스테이지를 반복할 것인지에 대한 변수
    [SerializeField]private bool[] stageClearedFlags;
    [SerializeField]private bool[] bossDefeated;


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

        if (stageClearedFlags == null || stageClearedFlags.Length == 0)
        {
            stageClearedFlags = new bool[DataManager.Instance.stageDataTable.Count];
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
        stageClearedFlags = data.StageClearedFlags ?? new bool[DataManager.Instance.stageDataTable.Count];
        bossDefeated = data.BossDefeated ?? new bool[DataManager.Instance.stageDataTable.Count];
        isLoop = data.IsLoop;

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
            //Debug.Log("[StageManager] 몬스터 재생성");
            SpawnManager.Instance.SpawnEnemiesForCurrentStage(spawnBatchSize);
        }

        //현재 킬 카운트가 스테이지 클리어에 필요한만큼 도달하면
        if (killCount >= totalKillsRequired)
        {
            OnStageClear();
        }
    }

    private void OnStageClear()
    {
        //1. 스테이지를 클리어 했고 보스를 잡은 상태가 아니라면
        //1-1. 스테이지를 클리어했으나 도전중이 아니라면 반복
        //1-2. 스테이지를 클리어했고, 도전중이라면 보스로 넘어감
        //2. 스테이지를 클리어 했고 보스를 잡은 상태라면
        //2-1. 반복중이라면 다음 스테이지로 넘어감
        //Debug.Log($"[StageManager] {currentStage}스테이지 클리어");

        if (isLoop == false)
        {
            currentStage++;

            maxClearedStage = Mathf.Max(maxClearedStage, currentStage); //최대 클리어 스테이지 업데이트, if문 안써도 됨 
            stageClearedFlags[currentStage - 1] = true;
        }

        SaveStageData();
        ResetStage();
        GameManager.Instance.statSaver.SavePlayerProgressData(GameManager.Instance.stats.GetProgressSaveData());
    }

    private void SaveStageData()
    {
        StageSaveData data = new StageSaveData
        {
            CurrentStageId = this.currentStage,
            MaxClearedStageId = this.maxClearedStage,
            StageClearedFlags = this.stageClearedFlags,
            BossDefeated = this.bossDefeated,
            IsLoop = this.isLoop
        };

        //Debug.Log($"[StageManager] 스테이지 데이터 저장 요청, 현재 스테이지 {data.CurrentStageId}, 최대 클리어 스테이지 {data.MaxClearedStageId}");
        GameManager.Instance.statSaver.SaveStageData(data);
    }

    private void ResetStage()
    {
        //Debug.Log($"[SpawnManager] 스테이지 리셋");
        //플레이어 위치 초기화, 기존 몬스터 제거
        killCount = 0;

        GameManager.Instance.player.transform.position = Vector3.zero;
        ObjectPoolManager.Instance.enemyPool.ReturnAllEnemies();
        SpawnManager.Instance.SpawnEnemiesForCurrentStage(30);
    }

    public StageType GetStageType(int stageNumber)
    {
        //1 ~ 100 넣으면 0 -> Forest 반환
        //101~200 넣으면 1 -> 다른 스테이지 반환
        //잠깐 이거 스테이지 10개 만들어야 이 연산 맞는건가 머리가 안도네 아
        //881 넣으면 880 / (100 % 2) => 880 / 0 = 0
        //151 넣으면 150 / 100 % 2 => 200 / 0 = 0
        //다 0이 뜨지 않나
        //아니 서순 
        //880 / 100 % 2 => 8 % 2 = 0 ??
        //150 / 100 % 2 => 1 % 2 = 1 오

        return (StageType)(((stageNumber - 1) / 100) % Enum.GetValues(typeof(StageType)).Length);
    }

    public int GetCurrentStage()
    {
        return currentStage;
    }
}
