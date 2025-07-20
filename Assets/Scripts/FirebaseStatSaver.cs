using Firebase.Database;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEditor.Experimental.GraphView;
using JetBrains.Annotations;

public class FirebaseStatSaver : MonoBehaviour
{
    //���̾�̽� �ǽð� �����ͺ��̽����� �ֻ��� ���
    private DatabaseReference dbRef;

    //��� ��ȣ�� ������ִ� ��Ʈ�ѷ� ��ü, async�۾��� �߰��� ����� �� �ְ� ����. 
    private CancellationTokenSource progressSaveCts;
    private CancellationTokenSource inventorySaveCts;

    private bool skillEquipListenerRegistered = false;

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
        }
        progressSaveCts = new CancellationTokenSource();

        //�񵿱� �Լ��� ��ٸ� �ʿ� ������ Forget����
        DelayAndSave(data, progressSaveCts.Token).Forget();

        //Debug.Log("[FirebaseStatSaver] ���� ��û ����");
        string json = JsonUtility.ToJson(data);
        //Debug.Log($"[FirebaseStatSaver] ����� JSON : {json}");
    }

    private async UniTaskVoid DelayAndSave(PlayerProgressSaveData data, CancellationToken token)
    {
        //CancellationToken�� �� �۾��� ��ҵǾ������� üũ�ϴ� ��ȣ��ġ
        try
        {
            //2�ʰ� ����ϴٰ� token���� ��� ��ȣ�� ���� �ߴ�
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
            //SaveStatLevels(statLevels);
            SavePlayerProgressData(data);
        }
        catch (OperationCanceledException)
        {
            //�߰��� ���� ��û�� �� ������ �����ϱ�
            //Debug.Log("[FirebaseStatSaver] ���� ��ҵ�");
        }
    }

    public void SavePlayerProgressData(PlayerProgressSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = "test_user";
        string path = $"users/{userId}/progress";

        dbRef.Child(path).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                //Debug.Log("[FirebaseStatSaver] �÷��̾� ���� ���� ���� ����");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] ���� ���� ���� ����, {task.Exception}");
            }
        });
    }

    public void LoadPlayerProgressData(Action<PlayerProgressSaveData> onLoaded)
    {
        string userId = "test_user";
        string path = $"users/{userId}/progress";

        dbRef.Child(path).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                string json = task.Result.GetRawJsonValue();
                PlayerProgressSaveData data = JsonUtility.FromJson<PlayerProgressSaveData>(json);
                MainThreadDispatcher(data, onLoaded);
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] ���� ���� �ҷ����� ����, {task.Exception}");
            }
        });
    }

    private async void MainThreadDispatcher(PlayerProgressSaveData data, Action<PlayerProgressSaveData> onLoaded)
    {
        await UniTask.SwitchToMainThread();
        onLoaded?.Invoke(data);
    }

    public void SaveStatLevels(Dictionary<StatUpgradeType, int> statLevels)
    {
        //������ enum ����̹Ƿ� �ݺ��ؼ� ���� ������,

        string userId = "test_user"; //���� Firebase Auth�� ��ü�ϱ�
        string path = $"users/{userId}/stats"; //������

        foreach (KeyValuePair<StatUpgradeType, int> stat in statLevels)
        {
            string statName = stat.Key.ToString(); //Attack�̳� �� �׷��ɷ� �����, Ű �� ��������
            int level = stat.Value; //Attack�� ���� ��������

            //�����ͺ��̽����� users/userId/Stats/statName�� statValue�� �����ϰ�, �������� Ȯ���� �α� ���
            dbRef.Child(path).Child(statName).SetValueAsync(level).ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    //Debug.Log($"[FirebaseStatSaver] {statName},{level} �����");
                }
                else
                {
                    Debug.LogError($"[FirebaseStatSaver] {statName}���忡 ������, {task.Exception}");
                }
            });
        }
    }

    public void LoadStatLevels(Action<Dictionary<StatUpgradeType, int>> onLoaded)
    {
        string userId = "test_user";
        string path = $"users/{userId}/stats";

        dbRef.Child(path).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DataSnapshot snapShot = task.Result;
                Dictionary<StatUpgradeType, int> loadedStats = new Dictionary<StatUpgradeType, int>();

                foreach (DataSnapshot child in snapShot.Children)
                {
                    string statName = child.Key;
                    string valueStr = child.Value.ToString();

                    if (Enum.TryParse(statName, out StatUpgradeType statType))
                    {
                        if (int.TryParse(valueStr, out int level))
                        {
                            loadedStats[statType] = level;
                        }
                    }
                }

                MainThreadDispatcher(loadedStats, onLoaded);

                //Debug.Log("[FirebaseStatSaver] ���� �ҷ����� ����");
                //onLoaded?.Invoke(loadedStats);
            }
            else
            {
                Debug.LogError("[FirebaseStatSaver] ���� �ҷ����� ���� : " + task.Exception);
            }
        });
    }

    private async void MainThreadDispatcher(Dictionary<StatUpgradeType, int> loadedStats, Action<Dictionary<StatUpgradeType, int>> onLoaded)
    {
        await UniTask.SwitchToMainThread();
        onLoaded?.Invoke(loadedStats);
    }

    public void SaveStageData(StageSaveData data)
    {
        string json = JsonUtility.ToJson(data);

        string userId = "test_user";
        string path = $"users/{userId}/stage";
        dbRef.Child(path).KeepSynced(true);
        dbRef.Child(path).Child("StageData").SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                //Debug.Log("[FirebaseStatSaver] �������� ������ ���� ����");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] �������� ������ ���� ����, {task.Exception}");
            }
        });
    }

    public void LoadStageData(Action<StageSaveData> onLoaded)
    {
        string userId = "test_user";
        string path = $"users/{userId}/stage/StageData";


        dbRef.Child(path).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                string json = task.Result.GetRawJsonValue();
                StageSaveData data = JsonUtility.FromJson<StageSaveData>(json);
                MainThreadDispatcher(data, onLoaded);
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] �������� ������ �ҷ����� ����, {task.Exception}");
            }
        });
    }

    private async void MainThreadDispatcher(StageSaveData data, Action<StageSaveData> onLoaded)
    {
        await UniTask.SwitchToMainThread();
        onLoaded?.Invoke(data);
    }

    public void SaveSkillEquipData(SkillEquipSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = "test_user";
        string path = $"users/{userId}/skillEquip";


        dbRef.Child(path).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("[FirebaseStatSaver] ��ų ���� ������ ���� ����");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] ��ų ���� ������ ���� ����, {task.Exception}");
            }
        });

        Debug.Log($"[FirebaseStatSaver] ���� �õ��ϴ� equipData ������ : {json}");
    }

    public void LoadSkillEquipData(Action<SkillEquipSaveData> onLoaded)
    {
        string userId = "test_user";
        string path = $"users/{userId}/skillEquip";

        Debug.Log($"[LoadEquip][START t={Time.realtimeSinceStartup:F3}]");

        dbRef.Child(path).GetValueAsync().ContinueWith(task =>
        {
            Debug.Log($"[LoadEquip][TASK DONE t={Time.realtimeSinceStartup:F3}] Success={task.IsCompletedSuccessfully} Fault={task.IsFaulted} Cancel={task.IsCanceled}");

            if (task.IsCompletedSuccessfully)
            {
                var snap = task.Result;
                string json = snap.GetRawJsonValue();
                Debug.Log($"[LoadEquip][RAW t={Time.realtimeSinceStartup:F3}] {json}");
                SkillEquipSaveData data = string.IsNullOrEmpty(json) ? new SkillEquipSaveData() : JsonUtility.FromJson<SkillEquipSaveData>(json);
                MainThreadDispatcher(data, onLoaded);
            }
            else
            {
                Debug.LogError($"[LoadEquip][ERROR] {task.Exception}");
            }
        });
    }

    private async void MainThreadDispatcher(SkillEquipSaveData data, Action<SkillEquipSaveData> onLoaded)
    {
        await UniTask.SwitchToMainThread();
        onLoaded?.Invoke(data);
    }

    public void SavePlayerSkillData(PlayerSkillSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = "test_user";
        string path = $"users/{userId}/skillState";


        dbRef.Child(path).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("[FirebaseStatSaver] �÷��̾� ��ų ������ ���� ����");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] �÷��̾� ��ų ������ ���� ����, {task.Exception}");
            }
        });

        Debug.Log($"[FirebaseStatSaver] ���� �õ��ϴ� skillState ������ : {json}");

    }

    public void LoadPlayerSkillData(Action<PlayerSkillSaveData> onLoaded)
    {
        string userId = "test_user";
        string path = $"users/{userId}/skillState";


        dbRef.Child(path).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                string json = task.Result.GetRawJsonValue();
                PlayerSkillSaveData data = JsonUtility.FromJson<PlayerSkillSaveData>(json);
                MainThreadDispatcher(data, onLoaded);
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] �÷��̾� ��ų ������ �ҷ����� ����, {task.Exception}");
            }
        });
    }

    public async void MainThreadDispatcher(PlayerSkillSaveData data, Action<PlayerSkillSaveData> onLoaded)
    {
        await UniTask.SwitchToMainThread();
        onLoaded?.Invoke(data);
    }

    public void RequestSave(InventorySaveData data)
    {
        if (inventorySaveCts != null)
        {
            inventorySaveCts.Cancel();
        }
        inventorySaveCts = new CancellationTokenSource();
        DelayAndSave(data, inventorySaveCts.Token).Forget();
        string json = JsonUtility.ToJson(data);
    }

    private async UniTaskVoid DelayAndSave(InventorySaveData data, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
            SaveInventoryData(data);
        }
        catch (OperationCanceledException)
        {
            //Debug.Log("[FirebaseStatSaver] ���� ��ҵ�");
        }
    }

    public void SaveInventoryData(InventorySaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = "test_user";
        string path = $"users/{userId}/InventoryData";


        dbRef.Child(path).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                //Debug.Log("[FirebaseStatSaver] �÷��̾� �κ��丮 ������ ���� ����");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] �÷��̾� �κ��丮 ������ ���� ����, {task.Exception}");
            }
        });
    }

    public void LoadInventoryData(Action<InventorySaveData> onLoaded)
    {
        //Debug.Log("�ε� �κ��丮");
        string userId = "test_user";
        string path = $"users/{userId}/InventoryData";


        dbRef.Child(path).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                string json = task.Result.GetRawJsonValue();
                InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);
                MainThreadDispatcher(data, onLoaded);
                //Debug.Log("�κ� �ҷ����� �Ϸ�");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] �κ��丮 �ҷ����� ����, {task.Exception}");
            }
        });
    }

    private async void MainThreadDispatcher(InventorySaveData data, Action<InventorySaveData> onLoaded)
    {
        await UniTask.SwitchToMainThread();
        onLoaded?.Invoke(data);
    }

    public void SaveSummonProgress(SummonProgressData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = "test_user";
        string path = $"users/{userId}/SummonProgress";


        dbRef.Child(path).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                //Debug.Log($"[FirebaseStatSaver] �÷��̾� ��ȯ���� ������ ���� ����");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] �÷��̾� ��ȯ���� ���� ����, {task.Exception}");
            }
        });
    }

    public void LoadSummonProgressData(Action<SummonProgressData> onLoaded)
    {
        string userId = "test_user";
        string path = $"users/{userId}/SummonProgress";

        dbRef.Child(path).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                string json = task.Result.GetRawJsonValue();
                SummonProgressData data = JsonUtility.FromJson<SummonProgressData>(json);
                MainThreadDispatcher(data, onLoaded);
                //Debug.Log($"[FirebaseStatSaver] ��ȯ���� �ҷ����� �Ϸ� {data.SummonProgressEntries.Count}");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] ��ȯ���� �ҷ����� ����, {task.Exception}");
            }
        });
    }

    private async void MainThreadDispatcher(SummonProgressData data, Action<SummonProgressData> onLoaded)
    {
        await UniTask.SwitchToMainThread();
        onLoaded?.Invoke(data);
    }

    public UniTask<PlayerSkillSaveData> LoadPlayerSkillDataAsync()
    {
        var tcs = new UniTaskCompletionSource<PlayerSkillSaveData>();
        LoadPlayerSkillData(data => tcs.TrySetResult(data));
        return tcs.Task;
    }

    public UniTask<SkillEquipSaveData> LoadSkillEquipDataAsync()
    {
        var tcs = new UniTaskCompletionSource<SkillEquipSaveData>();
        LoadSkillEquipData(data => tcs.TrySetResult(data));
        return tcs.Task;
    }


    public void ListenSkillEquip(Action<SkillEquipSaveData> onChanged)
    {
        if (skillEquipListenerRegistered)
        {
            Debug.Log("[SkillEquipListener] �̹� ��ϵ�, �ߺ� ��� ����");
            return;
        }

        string userId = "test_user";
        string path = $"users/{userId}/skillEquip";

        DatabaseReference target = FirebaseDatabase.DefaultInstance.RootReference.Child(path);

        Debug.Log("[SkillEquipListener] ��� ����");

        target.ValueChanged += (object sender, ValueChangedEventArgs e) =>
        {
            if (e.DatabaseError != null)
            {
                Debug.LogError($"[SkillEquipListener] DB Error : {e.DatabaseError.Message}");
                return;
            }

            string raw = e.Snapshot.GetRawJsonValue();
            Debug.Log($"[SkillEquipListener] ���ŵ� raw : {raw}");

            SkillEquipSaveData data;

            if (string.IsNullOrEmpty(raw))
            {
                data = new SkillEquipSaveData();
            }
            else
            {
                data = JsonUtility.FromJson<SkillEquipSaveData>(raw);

                if (data.equippedSkills == null || data.equippedSkills.Length != 6)
                {
                    SkillId[] safe = new SkillId[6];

                    if (data.equippedSkills != null)
                    {
                        int copy = Mathf.Min(6, data.equippedSkills.Length);
                        for (int i = 0; i < copy; i++)
                        {
                            safe[i] = data.equippedSkills[i];
                        }
                    }
                    data.equippedSkills = safe;
                }
            }

            onChanged?.Invoke(data);
        };

        skillEquipListenerRegistered = true;
        Debug.Log("[SkillEquipListener] ��� �Ϸ�");
    }

    public async UniTask<SkillEquipSaveData> LoadDataWithCheck()
    {
        string userId = "test_user"; 
        string path = $"users/{userId}/skillEquip";

        var db = FirebaseDatabase.DefaultInstance;
        var node = db.GetReference(path);

        float start = Time.realtimeSinceStartup;
        var task = node.GetValueAsync();
        await task;
        float duration = Time.realtimeSinceStartup - start;


        string result = task.IsCompletedSuccessfully ? task.Result.GetRawJsonValue() : null;
        string firstResult = result;

        Debug.Log($"duration = {duration:F3}");
        Debug.Log($"result : {result}");

        if (string.IsNullOrEmpty(result))
        {
            Debug.LogWarning("[SkillEquipLoad] ������� �������");
            return new SkillEquipSaveData(); 
        }

        //ĳ�÷� �ǽɵǸ� �ִ� 5ȸ �ݺ� ��õ�
        if (duration < 0.1f)
        {
            Debug.LogWarning("[SkillEquipLoad] ĳ�� ������ ����, ���û ����");

            for (int i = 0; i < 5; i++)
            {
                await UniTask.Delay(200);
                var retryTask = node.GetValueAsync();
                await retryTask;

                string newResult = retryTask.IsCompletedSuccessfully ? retryTask.Result.GetRawJsonValue() : null;
                Debug.Log($"[SkillEquipLoad] {i + 1}ȸ ��õ� ������ : {newResult}");

                //���� �޶������� break 
                if (!string.IsNullOrEmpty(newResult) && newResult != firstResult)
                {
                    result = newResult;
                    Debug.Log("[SkillEquipLoad] �ٸ� ������ ����, �ݺ� ����");
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(result))
        {
            Debug.LogWarning("[SkillEquipLoad] �ݺ� �Ŀ��� ������ ����, �⺻�� ��ȯ");
            return new SkillEquipSaveData();
        }

        SkillEquipSaveData data = null;
        try
        {
            data = JsonUtility.FromJson<SkillEquipSaveData>(result);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SkillEquipLoad] �Ľ� ����: {ex.Message}");
            data = new SkillEquipSaveData();
        }

        if (data == null)
        {
            Debug.LogWarning("[SkillEquipLoad] �Ľ� ����� null, �⺻�� ��ȯ");
            return new SkillEquipSaveData();
        }
        //�迭 ���� ����
        if (data.equippedSkills == null || data.equippedSkills.Length != 6)
        {
            Debug.LogWarning("[SkillEquipLoad] �迭 ���� ����");
            data.equippedSkills = new SkillId[6];
        }

        return data;
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