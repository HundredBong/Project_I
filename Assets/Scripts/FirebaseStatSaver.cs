using Firebase.Database;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

public class FirebaseStatSaver : MonoBehaviour
{
    //���̾�̽� �ǽð� �����ͺ��̽����� �ֻ��� ���
    private DatabaseReference dbRef;

    //��� ��ȣ�� ������ִ� ��Ʈ�ѷ� ��ü, async�۾��� �߰��� ����� �� �ְ� ����. 
    private CancellationTokenSource saveCts;

    private async void Start()
    {
        await UniTask.WaitUntil(() => GameManager.Instance.firebaseReady);

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("[FirebaseStatSaver] ���̾�̽� �ʱ�ȭ ��");
    }

    public void RequestSave(PlayerProgressSaveData data)
    {
        if (saveCts != null)
        {
            saveCts.Cancel();
        }
        saveCts = new CancellationTokenSource();

        //�񵿱� �Լ��� ��ٸ� �ʿ� ������ Forget����
        DelayAndSave(data, saveCts.Token).Forget();

        Debug.Log("[FirebaseStatSaver] ���� ��û ����");
        string json = JsonUtility.ToJson(data);
        Debug.Log($"[FirebaseStatSaver] ����� JSON : {json}");
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
            Debug.Log("[FirebaseStatSaver] ���� ��ҵ�");
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
                Debug.Log("[FirebaseStatSaver] �÷��̾� ���� ���� ���� ����");
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
                    Debug.Log($"[FirebaseStatSaver] {statName},{level} �����");
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

        dbRef.Child(path).Child("StageData").SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("[FirebaseStatSaver] �������� ������ ���� ����");
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
    }

    public void LoadSkillEquipData(Action<SkillEquipSaveData> onLoaded)
    {
        string userId = "test_user";
        string path = $"users/{userId}/skillEquip";

        dbRef.Child(path).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {

                string json = task.Result.GetRawJsonValue();
                SkillEquipSaveData data = JsonUtility.FromJson<SkillEquipSaveData>(json);
                MainThreadDispatcher(data, onLoaded);
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] ��ų ���� ������ �ҷ����� ����, {task.Exception}");
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

    public void SaveInventoryData(InventorySaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string userId = "test_user";
        string path = $"users/{userId}/InventoryData";

        dbRef.Child(path).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("[FirebaseStatSaver] �÷��̾� �κ��丮 ������ ���� ����");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] �÷��̾� �κ��丮 ������ ���� ����, {task.Exception}");
            }
        });
    }

    public void LoadInventoryData(Action<InventorySaveData> onLoaded)
    {
        string userId = "test_user";
        string path = $"users/{userId}/InventoryData";

        dbRef.Child(path).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                string json = task.Result.GetRawJsonValue();
                InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);
                MainThreadDispatcher(data, onLoaded);
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] ���� ���� �ҷ����� ����, {task.Exception}");
            }
        });
    }

    public async void MainThreadDispatcher(InventorySaveData data, Action<InventorySaveData> onLoaded)
    {
        await UniTask.SwitchToMainThread();
        onLoaded?.Invoke(data);
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
    public List<InventoryEntry> inventoryEntries = new List<InventoryEntry>();
}