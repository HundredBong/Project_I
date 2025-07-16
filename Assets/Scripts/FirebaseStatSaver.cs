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
    //파이어베이스 실시간 데이터베이스에서 최상위 경로
    private DatabaseReference dbRef;

    //취소 신호를 만들어주는 컨트롤러 객체, async작업을 중간에 취소할 수 있게 해줌. 
    private CancellationTokenSource progressSaveCts;
    private CancellationTokenSource inventorySaveCts;

    private async void Start()
    {
        await UniTask.WaitUntil(() => GameManager.Instance.firebaseReady);

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("[FirebaseStatSaver] 파이어베이스 초기화 됨");
    }

    public void RequestSave(PlayerProgressSaveData data)
    {
        if (progressSaveCts != null)
        {
            progressSaveCts.Cancel();
        }
        progressSaveCts = new CancellationTokenSource();
        
        //비동기 함수를 기다릴 필요 없으니 Forget선언
        DelayAndSave(data, progressSaveCts.Token).Forget();

        Debug.Log("[FirebaseStatSaver] 저장 요청 들어옴");
        string json = JsonUtility.ToJson(data);
        Debug.Log($"[FirebaseStatSaver] 저장될 JSON : {json}");
    }

    private async UniTaskVoid DelayAndSave(PlayerProgressSaveData data, CancellationToken token)
    {
        //CancellationToken은 이 작업이 취소되었는지를 체크하는 신호장치
        try
        {
            //2초간 대기하다가 token에서 취소 신호가 오면 중단
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
            //SaveStatLevels(statLevels);
            SavePlayerProgressData(data);
        }
        catch (OperationCanceledException)
        {
            //중간에 저장 요청이 또 들어오면 무시하기
            Debug.Log("[FirebaseStatSaver] 저장 취소됨");
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
                Debug.Log("[FirebaseStatSaver] 플레이어 진행 상태 저장 성공");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] 진행 상태 저장 실패, {task.Exception}");
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
                Debug.LogError($"[FirebaseStatSaver] 진행 상태 불러오기 실패, {task.Exception}");
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
        //스탯은 enum 기반이므로 반복해서 저장 가능함,

        string userId = "test_user"; //추후 Firebase Auth로 대체하기
        string path = $"users/{userId}/stats"; //저장경로

        foreach (KeyValuePair<StatUpgradeType, int> stat in statLevels)
        {
            string statName = stat.Key.ToString(); //Attack이나 뭐 그런걸로 저장됨, 키 값 가져오기
            int level = stat.Value; //Attack의 레벨 가져오기

            //데이터베이스에서 users/userId/Stats/statName에 statValue를 저장하고, 끝났는지 확인후 로그 출력
            dbRef.Child(path).Child(statName).SetValueAsync(level).ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log($"[FirebaseStatSaver] {statName},{level} 저장됨");
                }
                else
                {
                    Debug.LogError($"[FirebaseStatSaver] {statName}저장에 실패함, {task.Exception}");
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

                //Debug.Log("[FirebaseStatSaver] 스탯 불러오기 성공");
                //onLoaded?.Invoke(loadedStats);
            }
            else
            {
                Debug.LogError("[FirebaseStatSaver] 스탯 불러오기 실패 : " + task.Exception);
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
                Debug.Log("[FirebaseStatSaver] 스테이지 데이터 저장 성공");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] 스테이지 데이터 저장 실패, {task.Exception}");
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
                Debug.LogError($"[FirebaseStatSaver] 스테이지 데이터 불러오기 실패, {task.Exception}");
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
                Debug.Log("[FirebaseStatSaver] 스킬 장착 데이터 저장 성공");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] 스킬 장착 데이터 저장 실패, {task.Exception}");
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
                Debug.LogError($"[FirebaseStatSaver] 스킬 장착 데이터 불러오기 실패, {task.Exception}");
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
                Debug.Log("[FirebaseStatSaver] 플레이어 스킬 데이터 저장 성공");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] 플레이어 스킬 데이터 저장 실패, {task.Exception}");
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
                Debug.LogError($"[FirebaseStatSaver] 플레이어 스킬 데이터 불러오기 실패, {task.Exception}");
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
            Debug.Log("[FirebaseStatSaver] 저장 취소됨");
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
                Debug.Log("[FirebaseStatSaver] 플레이어 인벤토리 데이터 저장 성공");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] 플레이어 인벤토리 데이터 저장 실패, {task.Exception}");
            }
        });
    }

    public void LoadInventoryData(Action<InventorySaveData> onLoaded)
    {
        Debug.Log("로드 인벤토리");
        string userId = "test_user";
        string path = $"users/{userId}/InventoryData";

        dbRef.Child(path).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                string json = task.Result.GetRawJsonValue();
                InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);
                MainThreadDispatcher(data, onLoaded);
                Debug.Log("인벤 불러오기 완료");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] 인벤토리 불러오기 실패, {task.Exception}");
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
                Debug.Log($"[FirebaseStatSaver] 플레이어 소환레벨 데이터 저장 성공");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] 플레이어 소환레벨 저장 실패, {task.Exception}");
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
                Debug.Log($"[FirebaseStatSaver] 소환레벨 불러오기 완료 {data.SummonProgressEntries.Count}");
            }
            else
            {
                Debug.LogError($"[FirebaseStatSaver] 소환레벨 불러오기 실패, {task.Exception}");
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