using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [SerializeField] private int currentStage = 1; //���� ��������
    [SerializeField] private int killCount = 0; //���� ų ī��Ʈ
    [SerializeField] private int totalKillsRequired = 100; //�������� Ŭ��� �ʿ��� ų ī��Ʈ
    [SerializeField] private int spawnBatchSize = 20; //������� ���� ��
    [SerializeField] private int defaultSpawnCount = 30; //�⺻ ���� ���� ��

    public int MaxClearedStage { get; private set; }

    [SerializeField] private bool[] bossChallengable;
    [SerializeField] private bool[] bossDefeated;

    public event Action<int, int> OnKillUpdated; //���� ų, �ʿ� ų
    public event Action<int, bool> OnStageChanged; //���� ��������,canBoss
    public event Action<int> OnBossStageEntered; //current

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
        MaxClearedStage = data.MaxClearedStageId;
        bossChallengable = data.BossChallengable ?? new bool[DataManager.Instance.stageDataTable.Count];
        bossDefeated = data.BossDefeated ?? new bool[DataManager.Instance.stageDataTable.Count];
    }

    public void StartStage()
    {
        killCount = 0;
        OnStageChanged?.Invoke(currentStage, bossChallengable[currentStage - 1]); //�������� ������ ������ ���� �������� ����
        OnKillUpdated?.Invoke(killCount, totalKillsRequired);

        SpawnManager.Instance.SpawnEnemiesForCurrentStage(defaultSpawnCount);
    }


    public void NotifyKill()
    {
        killCount++;


        Debug.Log($"ų ī��Ʈ {killCount}, ���� : {killCount % spawnBatchSize}, bool : {killCount < totalKillsRequired && killCount % spawnBatchSize == 0}");
        ////���� ���������� �Ѿ������ �ִ� ų ���� �������� �ʰ�, ���� ų ī��Ʈ�� ������ ���� ���� 0�̸� 
        if (killCount < totalKillsRequired && killCount % spawnBatchSize == 0)
        {
            SpawnManager.Instance.SpawnEnemiesForCurrentStage(spawnBatchSize);
        }

        OnKillUpdated?.Invoke(killCount, totalKillsRequired);

        //���� ų ī��Ʈ�� �������� Ŭ��� �ʿ��Ѹ�ŭ �����ϸ�
        if (killCount >= totalKillsRequired)
        {
            OnStageClear();
        }
    }

    public void NotifyKillBoss()
    {
        bossDefeated[currentStage - 1] = true;
        GiveBossReward();
        MaxClearedStage = Mathf.Max(MaxClearedStage, currentStage); //�ִ� �������� ������Ʈ
        currentStage++;
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
        Debug.Log($"current : {currentStage}, max : {MaxClearedStage}, bossChallengable : {bossChallengable[currentStage - 1]}");

        bool canBose = bossChallengable[currentStage - 1];
        bool climbing = currentStage > MaxClearedStage;


        if (climbing)
        {
            if (canBose)
            {
                StartBossChallenge();
            }
            else
            {
                bossChallengable[currentStage - 1] = true;
            }
        }
        else
        {
            ResetStage();
        }

        GameManager.Instance.statSaver.SaveStageDataAsync(BuildStageSaveData()).Forget();
        GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();
    }

    public void StartBossChallenge()
    {
        GameManager.Instance.player.transform.position = Vector3.zero;
        ObjectPoolManager.Instance.enemyPool.ReturnAllEnemies();

        //ĵ�� �̽��� 1�� ������ ��
        DelayCallManager.Instance.CallLater(1.5f, () =>
        {
            SpawnManager.Instance.SpawnStageBoss();
            OnBossStageEntered?.Invoke(currentStage);
        });
    }

    private StageSaveData BuildStageSaveData()
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
        ObjectPoolManager.Instance.enemyPool.ReturnAllEnemies();

        DelayCallManager.Instance.CallLater(1.5f, () =>
        {
            //�÷��̾� ��ġ �ʱ�ȭ, ���� ���� ����
            killCount = 0;

            GameManager.Instance.player.transform.position = Vector3.zero;

            SpawnManager.Instance.SpawnEnemiesForCurrentStage(30);

            OnStageChanged?.Invoke(currentStage, bossChallengable[currentStage - 1]);
            OnKillUpdated?.Invoke(killCount, totalKillsRequired);
        });
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
        if (stage > MaxClearedStage)
        {
            Debug.LogWarning("[StageManager] �߸��� �������� ����");
            return;
        }

        currentStage = stage;
        ResetStage();
        OnStageChanged?.Invoke(stage, bossChallengable[stage - 1]);
    }

    [ContextMenu("GO 1")]
    private void Test()
    {
        GoToStage(1);
    }
}
