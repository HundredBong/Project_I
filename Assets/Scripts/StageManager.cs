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

    [SerializeField] private int currentStage = 1; //현재 스테이지
    [SerializeField] private int killCount = 0; //현재 킬 카운트
    [SerializeField] private int totalKillsRequired = 100; //스테이지 클리어에 필요한 킬 카운트
    [SerializeField] private int spawnBatchSize = 20; //재생성할 몬스터 수
    [SerializeField] private int defaultSpawnCount = 30; //기본 몬스터 생성 수
    [SerializeField] private Image _fadeImage;
    private const float FADE_DURATION = 2f;

    public int MaxClearedStage { get; private set; }
    public int CurrentStage => currentStage;

    [SerializeField] private bool[] bossChallengable;
    [SerializeField] private bool[] bossDefeated;

    public event Action<int, int> OnKillUpdated; //현재 킬, 필요 킬
    public event Action<int, bool> OnStageChanged; //현재 스테이지,canBoss
    public event Action<int> OnBossStageEntered; //current


    private bool isStageTransitioning = false;
    public bool IsChallenging { get; private set; }
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
        MaxClearedStage = data.MaxClearedStageId;
        bossChallengable = data.BossChallengable ?? new bool[DataManager.Instance.stageDataTable.Count];
        bossDefeated = data.BossDefeated ?? new bool[DataManager.Instance.stageDataTable.Count];
    }

    public void StartStage()
    {
        killCount = 0;
        UIManager.Instance.FadeIn(FADE_DURATION / 2);
        OnStageChanged?.Invoke(currentStage, bossChallengable[currentStage - 1]); //스테이지 시작할 때마다 현재 스테이지 갱신
        OnKillUpdated?.Invoke(killCount, totalKillsRequired);
        DelayCallManager.Instance.CallLater(FADE_DURATION / 2, () => SpawnManager.Instance.SpawnEnemiesForCurrentStage(defaultSpawnCount));
    }


    public void NotifyKill()
    {
        killCount++;

        //Debug.Log($"킬 카운트 {killCount}, 연산 : {killCount % spawnBatchSize}, bool : {killCount < totalKillsRequired && killCount % spawnBatchSize == 0}");
        ////다음 스테이지로 넘어가기위한 최대 킬 수에 도달하지 않고, 현재 킬 카운트와 사이즈 연산 값이 0이면 
        if (killCount < totalKillsRequired && killCount % spawnBatchSize == 0)
        {
            SpawnManager.Instance.SpawnEnemiesForCurrentStage(spawnBatchSize);
        }

        OnKillUpdated?.Invoke(killCount, totalKillsRequired);

        //현재 킬 카운트가 스테이지 클리어에 필요한만큼 도달하면
        if (killCount >= totalKillsRequired)
        {
            OnStageClear();
        }
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

        bool canBose = bossChallengable[currentStage - 1];
        bool climbing = currentStage > MaxClearedStage;// || (currentStage >= MaxClearedStage && canBose);


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
        if (IsChallenging) { return; }

        IsChallenging = true;

        GameManager.Instance.player.transform.position = Vector3.zero;
        ObjectPoolManager.Instance.enemyPool.ReturnAllEnemies();

        UIManager.Instance.FadeInOut(FADE_DURATION);

        DelayCallManager.Instance.CallLater(FADE_DURATION / 2f, () =>
        {
            OnBossStageEntered?.Invoke(currentStage);
        });

        DelayCallManager.Instance.CallLater(FADE_DURATION, () =>
        {
            SpawnManager.Instance.SpawnStageBoss();
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

        DelayCallManager.Instance.CallLater(FADE_DURATION, () =>
        {
            //플레이어 위치 초기화, 기존 몬스터 제거
            killCount = 0;

            GameManager.Instance.player.transform.position = Vector3.zero;

            SpawnManager.Instance.SpawnEnemiesForCurrentStage(defaultSpawnCount);

            OnStageChanged?.Invoke(currentStage, bossChallengable[currentStage - 1]);
            OnKillUpdated?.Invoke(killCount, totalKillsRequired);
        });

        isStageTransitioning = false;
        IsChallenging = false;
    }

    public StageType GetStageType(int stageNumber)
    {
        return (StageType)(((stageNumber - 1) / 100) % Enum.GetValues(typeof(StageType)).Length);
    }

    public int GetCurrentStage()
    {
        return currentStage;
    }

    public void GoToStage(int stage) //2
    {
        //2가 2+1보다 크다면 리턴,
        if (stage > MaxClearedStage + 1 || stage < 1 || stage == currentStage)
        {
            Debug.LogWarning("[StageManager] 잘못된 스테이지 접근");
            return;
        }

        if (IsChallenging || isStageTransitioning)
        {
            Debug.LogWarning("[StageManager] 현재 스테이지를 변경할 수 없는 상태임");
            return;
        }

        isStageTransitioning = true;

        Debug.Log($"stage : {stage}, Max : {MaxClearedStage}, bool : {bossDefeated[stage]}");
        //2가 2랑 같고, 2스테를 클리어 했다면
        if (stage == MaxClearedStage && bossDefeated[stage - 1] == true)
        {
            stage++;
        }
        UIManager.Instance.FadeInOut(FADE_DURATION);

        DelayCallManager.Instance.CallLater(FADE_DURATION / 2f, () =>
        {
            currentStage = stage;
            ResetStage();
            OnStageChanged?.Invoke(stage, bossChallengable[stage - 1]);
        });
    }
}
