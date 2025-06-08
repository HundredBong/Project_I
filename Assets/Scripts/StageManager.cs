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
    [SerializeField] private int spawnBatchSize = 20; //생성할 몬스터 수
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
        SpawnManager.Instance.SpawnEnemiesForStage(currentStage, initalSpawnCount);
    }

    private void SpawnEnemies(int count)
    {
        SpawnManager.Instance.SpawnEnemiesForStage(currentStage, initalSpawnCount);
    }

    public void NotifyKill()
    {
        killCount++;

        //다음 스테이지로 넘어가기위한 최대 킬 수에 도달하지 않고, 현재 킬 카운트와 사이즈 연산 값이 0이면 
        if (killCount < totalKillsRequired && killCount % spawnBatchSize == 0)
        {
            //사이즈만큼 몬스터 다시 생성
            SpawnEnemies(spawnBatchSize);
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
        //플레이어 위치 초기화, 기존 몬스터 제거
        killCount = 0;
        GameManager.Instance.player.transform.position = Vector3.zero;

        SpawnEnemies(initalSpawnCount);
    }

    public int GetCurrentStage()
    {
        return currentStage;
    }
}
