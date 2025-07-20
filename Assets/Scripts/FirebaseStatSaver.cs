using Cysharp.Threading.Tasks;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FirebaseStatSaver : MonoBehaviour
{
    //���̾�̽� �ǽð� �����ͺ��̽����� �ֻ��� ���
    private DatabaseReference dbRef;

    //��� ��ȣ�� ������ִ� ��Ʈ�ѷ� ��ü, async�۾��� �߰��� ����� �� �ְ� ����. 
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
        //Debug.Log("[FirebaseStatSaver] ���̾�̽� �ʱ�ȭ ��");
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
        //CancellationToken�� �� �۾��� ��ҵǾ������� üũ�ϴ� ��ȣ��ġ
        try
        {
            //2�ʰ� ����ϴٰ� token���� ��� ��ȣ�� ���� �ߴ�
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
            //SaveStatLevels(statLevels);
            SavePlayerProgressDataAsync(data).Forget();
        }
        catch (OperationCanceledException)
        {
            //�߰��� ���� ��û�� �� ������ �����ϱ�
            Debug.Log("[FirebaseStatSaver] ���� ��ҵ�");
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
            Debug.LogError($"[FirebaseStatSaver] ���� ���� ���� ����, {e}");
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
                    Debug.LogWarning($"[PlayerProgressData] ĳ�� ������ ����, ���û {i + 1}/{MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }


                PlayerProgressSaveData data = string.IsNullOrEmpty(json) ? new PlayerProgressSaveData() : JsonUtility.FromJson<PlayerProgressSaveData>(json);

                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[��õ� {i + 1}/{MAX_RETRY_COUNT}] ���� �ҷ����� ����: {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }

        await UniTask.SwitchToMainThread();
        throw new Exception($"���� �ҷ����� {MAX_RETRY_COUNT}ȸ ���� ������");
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
            Debug.LogError($"[FirebaseStatSaver] �������� ���� ����, {e}");

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
                    Debug.LogWarning($"[StageData] ĳ�� ������ ����, ���û {i + 1}/{MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }

                StageSaveData data = string.IsNullOrEmpty(json) ? new StageSaveData() : JsonUtility.FromJson<StageSaveData>(json);

                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[��õ� {i + 1}/{MAX_RETRY_COUNT}] �������� �ҷ����� ����, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }

        await UniTask.SwitchToMainThread();
        throw new Exception($"�������� �ҷ����� {MAX_RETRY_COUNT}ȸ ���� ������");
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
            Debug.LogError($"[FirebaseStatSaver] ��ų ���� ���� ����, {e}");
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
                    Debug.LogWarning($"[SkillEquipSaveData] ĳ�� ������ ����, ���û {i + 1}/{MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }

                SkillEquipSaveData data = string.IsNullOrEmpty(json) ? new SkillEquipSaveData() : JsonUtility.FromJson<SkillEquipSaveData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[��õ� {i + 1}/{MAX_RETRY_COUNT}] ��ų ���� �ҷ����� ����, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }
        await UniTask.SwitchToMainThread();
        throw new Exception($"��ų ���� �ҷ����� {MAX_RETRY_COUNT}ȸ ���� ������");
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
            Debug.LogError($"[FirebaseStatSaver] ��ų ���� ���� ����, {e}");
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
                    Debug.LogWarning($"[PlayerSkillSaveData] ĳ�� ������ ����, ���û {i + 1}/ {MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }

                PlayerSkillSaveData data = string.IsNullOrEmpty(json) ? new PlayerSkillSaveData() : JsonUtility.FromJson<PlayerSkillSaveData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[��õ� {i + 1}/{MAX_RETRY_COUNT}] �÷��̾� ��ų ���� �ҷ����� ����, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }
        await UniTask.SwitchToMainThread();
        throw new Exception($"��ų ���� �ҷ����� {MAX_RETRY_COUNT}ȸ ���� ������");
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
            Debug.Log("[FirebaseStatSaver] ���� ��ҵ�");
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
            Debug.LogError($"[FirebaseStatSaver] �κ��丮 ���� ����, {e}");
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
                    Debug.LogWarning($"[InventorySaveData] ĳ�� ������ ����, ���û {i + 1}/ {MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }
                InventorySaveData data = string.IsNullOrEmpty(json) ? new InventorySaveData() : JsonUtility.FromJson<InventorySaveData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[��õ� {i + 1}/{MAX_RETRY_COUNT}] �÷��̾� �κ��丮 �ҷ����� ����, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }

        await UniTask.SwitchToMainThread();
        throw new Exception($"�÷��̾� �κ��丮 �ҷ����� {MAX_RETRY_COUNT}ȸ ���� ������");
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
            Debug.LogError($"[FirebaseStatSaver] ��ȯ ���� ���� ����, {e}");
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
                    Debug.LogWarning($"[SummonProgressData] ĳ�� ������ ����, ���û {i + 1}/ {MAX_RETRY_COUNT}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                    continue;
                }

                SummonProgressData data = string.IsNullOrEmpty(json) ? new SummonProgressData() : JsonUtility.FromJson<SummonProgressData>(json);
                await UniTask.SwitchToMainThread();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[��õ� {i + 1}/{MAX_RETRY_COUNT}] �÷��̾� ��ȯ ���� �ҷ����� ����, {e}");
                await UniTask.Delay(RETRY_DELAY_MS);
            }
        }
        await UniTask.SwitchToMainThread();
        throw new Exception($"�÷��̾� ��ȯ ���� �ҷ����� {MAX_RETRY_COUNT}ȸ ���� ������");
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
    public SkillId skillId; //enum �ʵ�� ������
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