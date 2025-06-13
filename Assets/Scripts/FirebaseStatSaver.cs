using Firebase.Database;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

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

    public void RequestSave(Dictionary<StatType, int> statLevels)
    {
        if (saveCts != null)
        {
            saveCts.Cancel();
        }
        saveCts = new CancellationTokenSource();

        //�񵿱� �Լ��� ��ٸ� �ʿ� ������ Forget����
        DelayAndSave(statLevels, saveCts.Token).Forget();
    }

    private async UniTaskVoid DelayAndSave(Dictionary<StatType, int> statLevels, CancellationToken token)
    {
        //CancellationToken�� �� �۾��� ��ҵǾ������� üũ�ϴ� ��ȣ��ġ
        try
        {
            //2�ʰ� ����ϴٰ� token���� ��� ��ȣ�� ���� �ߴ�
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
            SaveStatLevels(statLevels);
        }
        catch (OperationCanceledException)
        {
            //�߰��� ���� ��û�� �� ������ �����ϱ�
            Debug.Log("[FirebaseStatSaver] ���� ��ҵ�");
        }
    }

    public void SaveStatLevels(Dictionary<StatType, int> statLevels)
    {
        //������ enum ����̹Ƿ� �ݺ��ؼ� ���� ������,

        string userId = "test_user"; //���� Firebase Auth�� ��ü�ϱ�
        string path = $"users/{userId}/stats"; //������

        foreach (KeyValuePair<StatType, int> stat in statLevels)
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

                //Debug.Log("[FirebaseStatSaver] ���� �ҷ����� ����");
                //onLoaded?.Invoke(loadedStats);
            }
            else
            {
                Debug.LogError("[FirebaseStatSaver] ���� �ҷ����� ���� : " + task.Exception);
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
