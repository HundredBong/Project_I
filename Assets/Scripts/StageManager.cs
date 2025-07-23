using Cysharp.Threading.Tasks;
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
    [SerializeField] private int maxClearedStage = 1; //�ִ� Ŭ���� ��������
    public bool IsLoop { get; set; } //���� ���������� �ݺ��� �������� ���� ����

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
            Debug.LogWarning("[StageManager] �ε�� �����Ͱ� null��");
            return;
        }
        Debug.Log(data.CurrentStageId);
        currentStage = data.CurrentStageId;
        maxClearedStage = data.MaxClearedStageId;
        bossChallengable = data.BossChallengable ?? new bool[DataManager.Instance.stageDataTable.Count];
        bossDefeated = data.BossDefeated ?? new bool[DataManager.Instance.stageDataTable.Count];
        IsLoop = LocalSetting.LoadStageLoop();

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
            SpawnManager.Instance.SpawnEnemiesForCurrentStage(spawnBatchSize);
        }

        //���� ų ī��Ʈ�� �������� Ŭ��� �ʿ��Ѹ�ŭ �����ϸ�
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
        maxClearedStage = Mathf.Max(maxClearedStage, currentStage); //�ִ� Ŭ���� �������� ������Ʈ

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
        //�ݺ����̶�� �������� Ŭ����
        if (IsLoop == true)
        {
            ResetStage();
        }

        //�ݺ����� �ƴϰ�, ������ Ŭ�������� �ʾ�����, �������� ���� �Ұ����� ���¿��� Ŭ���� �ߴٸ�
        if (IsLoop == false && bossChallengable[currentStage] == false && bossDefeated[currentStage] == false)
        {
            //�������� ���� �����ϵ��� ��,
            bossChallengable[currentStage] = true;
            maxClearedStage = currentStage;
        }

        //�ݺ����� �ƴϰ�, �������� ������ �����ϴٸ�
        if (IsLoop == false && bossChallengable[currentStage] == true && bossDefeated[currentStage] == false)
        {
            //�������� �����ϴ� ���������� �ʱ�ȭ
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
        //�÷��̾� ��ġ �ʱ�ȭ, ���� ���� ����
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
