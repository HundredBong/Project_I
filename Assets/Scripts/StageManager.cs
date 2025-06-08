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
    [SerializeField] private int initalSpawnCount = 40; //초기 몬스터 수

    public bool isLoop; //현재 스테이지를 반복할 것인지에 대한 변수

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadStageData();
        StartStage();
    }

    private void LoadStageData()
    {
        //일단 대충하고 추후 파이어베이스에서 불러오기
        currentStage = 1;
        killCount = 0;
    }

    private void StartStage()
    {
        killCount = 0;

        SpawnManager.Instance.SpawnEnemiesForStage(GetStageType(currentStage), initalSpawnCount);
    }


    public void NotifyKill()
    {
        killCount++;

        //다음 스테이지로 넘어가기위한 최대 킬 수에 도달하지 않고, 현재 킬 카운트와 사이즈 연산 값이 0이면 
        //20 < 100 && 20 % 20 == 0 
        if (killCount < totalKillsRequired && killCount % spawnBatchSize == 0)
        {
            //사이즈만큼 몬스터 다시 생성
            Debug.Log("[StageManager] 몬스터 재생성");
            SpawnManager.Instance.SpawnEnemiesForStage(GetStageType(currentStage), spawnBatchSize);
        }

        //현재 킬 카운트가 스테이지 클리어에 필요한만큼 도달하면
        if (killCount >= totalKillsRequired)
        {
            OnStageClear();
        }
    }

    private void OnStageClear()
    {
        Debug.Log($"[StageManager] {currentStage}클리어 함");

        if (isLoop == false)
        {
            currentStage++;
        }

        SaveStageData();
        ResetStage();
    }

    private void SaveStageData()
    {
        //파이어베이스와 연동해야함
    }

    private void ResetStage()
    {
        Debug.Log($"[SpawnManager] 스테이지 리셋");
        //플레이어 위치 초기화, 기존 몬스터 제거
        killCount = 0;

        GameManager.Instance.player.transform.position = Vector3.zero;

        SpawnManager.Instance.SpawnEnemiesForStage(GetStageType(currentStage), initalSpawnCount);
    }

    private StageType GetStageType(int stageNumber)
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
}
