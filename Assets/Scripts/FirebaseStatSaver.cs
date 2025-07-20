using Cysharp.Threading.Tasks;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FirebaseStatSaver : MonoBehaviour
{
    //파이어베이스 실시간 데이터베이스에서 최상위 경로
    private DatabaseReference dbRef;

    //취소 신호를 만들어주는 컨트롤러 객체, async작업을 중간에 취소할 수 있게 해줌. 
    private CancellationTokenSource progressSaveCts;
    private CancellationTokenSource inventorySaveCts;

    private const int MAX_RETRY_COUNT = 5;
    private const int RETRY_DELAY_MS = 300;
    private const float DURATION_THRESHOLD = 0.1f;

    private async void Start()
    {
        await UniTask.WaitUntil(() => GameManager.Instance.firebaseReady);

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
        //Debug.Log("[FirebaseStatSaver] 파이어베이스 초기화 됨");
    }

    public void RequestSave(PlayerProgressSaveData data)
    {
        if (progressSaveCts != null)
        {
            progressSaveCts.Cancel();
            progressSaveCts.Dispose();
        }

        progressSaveCts = new CancellationTokenSource();
        DelayAndSave(data, progressSaveCts.Token).Forget();
    }

    private async UniTaskVoid DelayAndSave(PlayerProgressSaveData data, CancellationToken token)
    {
        //CancellationToken은 이 작업이 취소되었는지를 체크하는 신호장치
        try
        {
            //2초간 대기하다가 token에서 취소 신호가 오면 중단
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
            //SaveStatLevels(statLevels);
            SavePlayerProgressDataAsync(data).Forget();
        }
        catch (OperationCanceledException)
        {
            //중간에 저장 요청이 또 들어오면 무시하기
            Debug.Log("[FirebaseStatSaver] 저장 취소됨");
        }
    }

    public async UniTask SavePlayerProgressDataAsync(PlayerProgressSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = "test_user";
        string path = $"users/{userId}/progress";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 진행 상태 저장 실패, {e}");
        }
    }

    public async UniTask<PlayerProgressSaveData> LoadPlayerProgressDataAsync()
    {
        string userId = "test_user";
        string path = $"users/{userId}/progress";

        string firstResult = null;

        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;
            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string json = snapshot.GetRawJsonValue();
                float duration = Time.realtimeSinceStartup - start;

                if (firstResult == null)
                {
                    firstResult = json;
                }

                else if (duration < DURATION_THRESHOLD && json == firstResult)
                {
                    Debug.LogWarning($"[PlayerProgressData] 캐시 데이터 감지, 재요청 {i + 1}/{MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }


                PlayerProgressSaveData data = string.IsNullOrEmpty(json) ? new PlayerProgressSaveData() : JsonUtility.FromJson<PlayerProgressSaveData>(json);

                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 스탯 불러오기 실패: {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }

        await UniTask.SwitchToMainThread();
        throw new Exception($"스탯 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public async UniTask SaveStageDataAsync(StageSaveData data)
    {
        string json = JsonUtility.ToJson(data);

        string userId = "test_user";
        string path = $"users/{userId}/stage";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 스테이지 저장 실패, {e}");

        }
    }

    public async UniTask<StageSaveData> LoadStageDataAsync()
    {
        string userId = "test_user";
        string path = $"users/{userId}/stage/StageData";
        string firstResult = null;

        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;

            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string json = snapshot.GetRawJsonValue();
                float duration = Time.realtimeSinceStartup - start;

                if (firstResult == null)
                {
                    firstResult = json;
                }
                else if (duration < DURATION_THRESHOLD && json == firstResult)
                {
                    Debug.LogWarning($"[StageData] 캐시 데이터 감지, 재요청 {i + 1}/{MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }

                StageSaveData data = string.IsNullOrEmpty(json) ? new StageSaveData() : JsonUtility.FromJson<StageSaveData>(json);

                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 스테이지 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }

        await UniTask.SwitchToMainThread();
        throw new Exception($"스테이지 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public async UniTask SaveSkillEquipData(SkillEquipSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = "test_user";
        string path = $"users/{userId}/skillEquip";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 스킬 장착 저장 실패, {e}");
        }
    }

    public async UniTask<SkillEquipSaveData> LoadSkillEquipDataAsync()
    {
        string userId = "test_user";
        string path = $"users/{userId}/skillEquip";
        string firstResult = null;

        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;

            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string json = snapshot.GetRawJsonValue();
                float duration = Time.realtimeSinceStartup - start;

                if (firstResult == null)
                {
                    firstResult = json;
                }

                else if (duration < DURATION_THRESHOLD && json == firstResult)
                {
                    Debug.LogWarning($"[SkillEquipSaveData] 캐시 데이터 감지, 재요청 {i + 1}/{MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }

                SkillEquipSaveData data = string.IsNullOrEmpty(json) ? new SkillEquipSaveData() : JsonUtility.FromJson<SkillEquipSaveData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 스킬 장착 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }
        await UniTask.SwitchToMainThread();
        throw new Exception($"스킬 장착 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public async UniTask SavePlayerSkillDataAsync(PlayerSkillSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = "test_user";
        string path = $"users/{userId}/skillState";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 스킬 상태 저장 실패, {e}");
        }
    }

    public async UniTask<PlayerSkillSaveData> LoadPlayerSkillDataAsync()
    {
        string userId = "test_user";
        string path = $"users/{userId}/skillState";
        string firstResult = null;


        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;
            try
            {
                DataSnapshot snapShot = await dbRef.Child(path).GetValueAsync();
                string json = snapShot.GetRawJsonValue();
                float duration = Time.realtimeSinceStartup - start;

                if (firstResult == null)
                {
                    firstResult = json;
                }
                else if (duration < DURATION_THRESHOLD && json == firstResult)
                {
                    Debug.LogWarning($"[PlayerSkillSaveData] 캐시 데이터 감지, 재요청 {i + 1}/ {MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }

                PlayerSkillSaveData data = string.IsNullOrEmpty(json) ? new PlayerSkillSaveData() : JsonUtility.FromJson<PlayerSkillSaveData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 플레이어 스킬 상태 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }
        await UniTask.SwitchToMainThread();
        throw new Exception($"스킬 상태 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public void RequestSave(InventorySaveData data)
    {
        if (inventorySaveCts != null)
        {
            inventorySaveCts.Cancel();
            inventorySaveCts.Dispose();
        }

        inventorySaveCts = new CancellationTokenSource();
        DelayAndSave(data, inventorySaveCts.Token).Forget();
    }

    private async UniTaskVoid DelayAndSave(InventorySaveData data, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
            SaveInventoryDataAsync(data).Forget();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[FirebaseStatSaver] 저장 취소됨");
        }
    }

    public async UniTask SaveInventoryDataAsync(InventorySaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = "test_user";
        string path = $"users/{userId}/InventoryData";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 인벤토리 저장 실패, {e}");
        }
    }

    public async UniTask<InventorySaveData> LoadInventoryDataAsync()
    {
        string userId = "test_user";
        string path = $"users/{userId}/InventoryData";
        string firstResult = null;

        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;
            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string json = snapshot.GetRawJsonValue();
                float duration = Time.realtimeSinceStartup - start;

                if (firstResult == null)
                {
                    firstResult = json;
                }
                else if (duration < DURATION_THRESHOLD && firstResult == json)
                {
                    Debug.LogWarning($"[InventorySaveData] 캐시 데이터 감지, 재요청 {i + 1}/ {MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }
                InventorySaveData data = string.IsNullOrEmpty(json) ? new InventorySaveData() : JsonUtility.FromJson<InventorySaveData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 플레이어 인벤토리 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }

        await UniTask.SwitchToMainThread();
        throw new Exception($"플레이어 인벤토리 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }

    public async UniTask SaveSummonProgressAsync(SummonProgressData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = "test_user";
        string path = $"users/{userId}/SummonProgress";

        try
        {
            await dbRef.Child(path).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseStatSaver] 소환 레벨 저장 실패, {e}");
        }
    }

    public async UniTask<SummonProgressData> LoadSummonProgressDataAsync()
    {
        string userId = "test_user";
        string path = $"users/{userId}/SummonProgress";
        string firstResult = null;

        for (int i = 0; i < MAX_RETRY_COUNT; i++)
        {
            float start = Time.realtimeSinceStartup;

            try
            {
                DataSnapshot snapshot = await dbRef.Child(path).GetValueAsync();
                string json = snapshot.GetRawJsonValue();
                float duration = Time.realtimeSinceStartup - start;

                if (firstResult == null)
                {
                    firstResult = json;
                }
                else if (duration < DURATION_THRESHOLD && firstResult == json)
                {
                    Debug.LogWarning($"[SummonProgressData] 캐시 데이터 감지, 재요청 {i + 1}/ {MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }

                SummonProgressData data = string.IsNullOrEmpty(json) ? new SummonProgressData() : JsonUtility.FromJson<SummonProgressData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[재시도 {i + 1}/{MAX_RETRY_COUNT}] 플레이어 소환 레벨 불러오기 실패, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }
        await UniTask.SwitchToMainThread();
        throw new Exception($"플레이어 소환 레벨 불러오기 {MAX_RETRY_COUNT}회 연속 실패함");
    }
}

[System.Serializable]
public class StageSaveData
{
    public int CurrentStageId;
    public int MaxClearedStageId;
    public bool IsLoop;
    public bool[] BossDefeated;
    public bool[] StageClearedFlags;
}

[System.Serializable]
public class SkillEquipSaveData
{
    public SkillId[] equippedSkills = new SkillId[6];
}

[System.Serializable]
public class SkillStateSaveData
{
    public SkillId skillId; //enum 필드는 괜찮음
    public int level;
    public int ownedCount;
    public int awakenLevel;
}

[System.Serializable]
public class PlayerSkillSaveData
{
    public List<SkillStateSaveData> skillStates = new List<SkillStateSaveData>();
}

[System.Serializable]
public class StatLevelEntry
{
    public StatUpgradeType StatUpgradeType;
    public int Level;
}

[System.Serializable]
public class ProgressEntry
{
    public PlayerProgressType PlayerProgressType;
    public float Value;
}

[System.Serializable]
public class GoldLevelEntry
{
    public GoldUpgradeType GoldUpgradeType;
    public int Level;
}

[System.Serializable]
public class PlayerProgressSaveData
{
    public List<ProgressEntry> progressValues = new();
    public List<StatLevelEntry> statLevels = new();
    public List<GoldLevelEntry> goldUpgradeLevels = new();
}

[System.Serializable]
public class InventoryEntry
{
    public int Id;
    public int Level;
    public int Count;
    public bool IsEquipped;
    public bool IsUnlocked;
}

[System.Serializable]
public class InventorySaveData
{
    public List<InventoryEntry> InventoryEntries = new List<InventoryEntry>();
}


[System.Serializable]
public class SummonProgressEntry
{
    public SummonSubCategory Category;
    public int Level;
    public int Exp;
}

[System.Serializable]
public class SummonRewardClaimEntry
{
    public SummonSubCategory Category;
    public List<int> Levels = new List<int>();
}

[System.Serializable]
public class SummonProgressData
{
    public List<SummonProgressEntry> SummonProgressEntries = new List<SummonProgressEntry>();
    public List<SummonRewardClaimEntry> SummonRewardEntries = new List<SummonRewardClaimEntry>();
}