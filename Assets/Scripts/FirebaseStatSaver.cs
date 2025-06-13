using Firebase.Database;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

public class FirebaseStatSaver : MonoBehaviour
{
    //파이어베이스 실시간 데이터베이스에서 최상위 경로
    private DatabaseReference dbRef;

    //취소 신호를 만들어주는 컨트롤러 객체, async작업을 중간에 취소할 수 있게 해줌. 
    private CancellationTokenSource saveCts;

    private async void Start()
    {
        await UniTask.WaitUntil(() => GameManager.Instance.firebaseReady);

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("[FirebaseStatSaver] 파이어베이스 초기화 됨");
    }

    public void RequestSave(Dictionary<StatType, int> statLevels)
    {
        if (saveCts != null)
        {
            saveCts.Cancel();
        }
        saveCts = new CancellationTokenSource();

        //비동기 함수를 기다릴 필요 없으니 Forget선언
        DelayAndSave(statLevels, saveCts.Token).Forget();
    }

    private async UniTaskVoid DelayAndSave(Dictionary<StatType, int> statLevels, CancellationToken token)
    {
        //CancellationToken은 이 작업이 취소되었는지를 체크하는 신호장치
        try
        {
            //2초간 대기하다가 token에서 취소 신호가 오면 중단
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
            SaveStatLevels(statLevels);
        }
        catch (OperationCanceledException)
        {
            //중간에 저장 요청이 또 들어오면 무시하기
            Debug.Log("[FirebaseStatSaver] 저장 취소됨");
        }
    }

    public void SaveStatLevels(Dictionary<StatType, int> statLevels)
    {
        //스탯은 enum 기반이므로 반복해서 저장 가능함,

        string userId = "test_user"; //추후 Firebase Auth로 대체하기
        string path = $"users/{userId}/stats"; //저장경로

        foreach (KeyValuePair<StatType, int> stat in statLevels)
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

    public void LoadStatLevels(Action<Dictionary<StatType, int>> onLoaded)
    {
        string userId = "test_user";
        string path = $"users/{userId}/stats";

        dbRef.Child(path).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DataSnapshot snapShot = task.Result;
                Dictionary<StatType, int> loadedStats = new Dictionary<StatType, int>();

                foreach (DataSnapshot child in snapShot.Children)
                {
                    string statName = child.Key;
                    string valueStr = child.Value.ToString();

                    if (Enum.TryParse(statName, out StatType statType))
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

    private async void MainThreadDispatcher(Dictionary<StatType, int> loadedStats, Action<Dictionary<StatType, int>> onLoaded)
    {
        await Cysharp.Threading.Tasks.UniTask.SwitchToMainThread();
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
        await Cysharp.Threading.Tasks.UniTask.SwitchToMainThread();
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
