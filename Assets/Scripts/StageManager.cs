using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [SerializeField] private int currentStage = 1; //���� ��������
    [SerializeField] private int killCount = 0; //���� ų ī��Ʈ
    [SerializeField] private int totalKillsRequired = 100; //�������� Ŭ��� �ʿ��� ų ī��Ʈ
    [SerializeField] private int spawnBatchSize = 20; //������� ���� ��
    [SerializeField] private int defaultSpawnCount = 30; //�⺻ ���� ���� ��
    [SerializeField]private int maxClearedStage = 1; //�ִ� Ŭ���� ��������
    [SerializeField]private bool isLoop; //���� ���������� �ݺ��� �������� ���� ����
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
            Debug.LogWarning("[StageManager] �ε�� �����Ͱ� null��");
            return;
        }

        currentStage = data.CurrentStageId;
        maxClearedStage = data.MaxClearedStageId;
        stageClearedFlags = data.StageClearedFlags ?? new bool[DataManager.Instance.stageDataTable.Count];
        bossDefeated = data.BossDefeated ?? new bool[DataManager.Instance.stageDataTable.Count];
        isLoop = data.IsLoop;

        //Debug.Log($"[StageManager] �������� ������ �ҷ����� �Ϸ�, ���� �������� : {currentStage}");
    }

    public void StartStage()
    {
        //Debug.Log($"[StageManager] {currentStage}�������� ����");
        killCount = 0;

        SpawnManager.Instance.SpawnEnemiesForCurrentStage(defaultSpawnCount);
    }


    public void NotifyKill()
    {
        killCount++;

        //���� ���������� �Ѿ������ �ִ� ų ���� �������� �ʰ�, ���� ų ī��Ʈ�� ������ ���� ���� 0�̸� 
        //20 < 100 && 20 % 20 == 0 
        if (killCount < totalKillsRequired && killCount % spawnBatchSize == 0)
        {
            //�����ŭ ���� �ٽ� ����
            //Debug.Log("[StageManager] ���� �����");
            SpawnManager.Instance.SpawnEnemiesForCurrentStage(spawnBatchSize);
        }

        //���� ų ī��Ʈ�� �������� Ŭ��� �ʿ��Ѹ�ŭ �����ϸ�
        if (killCount >= totalKillsRequired)
        {
            OnStageClear();
        }
    }

    private void OnStageClear()
    {
        //1. ���������� Ŭ���� �߰� ������ ���� ���°� �ƴ϶��
        //1-1. ���������� Ŭ���������� �������� �ƴ϶�� �ݺ�
        //1-2. ���������� Ŭ�����߰�, �������̶�� ������ �Ѿ
        //2. ���������� Ŭ���� �߰� ������ ���� ���¶��
        //2-1. �ݺ����̶�� ���� ���������� �Ѿ
        //Debug.Log($"[StageManager] {currentStage}�������� Ŭ����");

        if (isLoop == false)
        {
            currentStage++;

            maxClearedStage = Mathf.Max(maxClearedStage, currentStage); //�ִ� Ŭ���� �������� ������Ʈ, if�� �Ƚᵵ �� 
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

        //Debug.Log($"[StageManager] �������� ������ ���� ��û, ���� �������� {data.CurrentStageId}, �ִ� Ŭ���� �������� {data.MaxClearedStageId}");
        GameManager.Instance.statSaver.SaveStageData(data);
    }

    private void ResetStage()
    {
        //Debug.Log($"[SpawnManager] �������� ����");
        //�÷��̾� ��ġ �ʱ�ȭ, ���� ���� ����
        killCount = 0;

        GameManager.Instance.player.transform.position = Vector3.zero;
        ObjectPoolManager.Instance.enemyPool.ReturnAllEnemies();
        SpawnManager.Instance.SpawnEnemiesForCurrentStage(30);
    }

    public StageType GetStageType(int stageNumber)
    {
        //1 ~ 100 ������ 0 -> Forest ��ȯ
        //101~200 ������ 1 -> �ٸ� �������� ��ȯ
        //��� �̰� �������� 10�� ������ �� ���� �´°ǰ� �Ӹ��� �ȵ��� ��
        //881 ������ 880 / (100 % 2) => 880 / 0 = 0
        //151 ������ 150 / 100 % 2 => 200 / 0 = 0
        //�� 0�� ���� �ʳ�
        //�ƴ� ���� 
        //880 / 100 % 2 => 8 % 2 = 0 ??
        //150 / 100 % 2 => 1 % 2 = 1 ��

        return (StageType)(((stageNumber - 1) / 100) % Enum.GetValues(typeof(StageType)).Length);
    }

    public int GetCurrentStage()
    {
        return currentStage;
    }
}
