using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using System.Threading.Tasks;
using System;

public class FirebaseStatSaver : MonoBehaviour
{
    //���̾�̽� �ǽð� �����ͺ��̽����� �ֻ��� ���
    private DatabaseReference dbRef;
    private bool isReady = false;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                //���� ������Ʈ�� ����� �⺻ ���̾�̽� �ν��Ͻ��� �ֻ��� ���
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                isReady = true;
                Debug.Log("[FirebaseStatSaver] ���̾�̽� �ʱ�ȭ ��");
            }
            else
            {
                Debug.Log("[FirebaseStatSaver] ���̾�̽� �ʱ�ȭ ����");
            }
        });
    }

    public void SaveStatLevels(Dictionary<StatType, int> statLevels)
    {
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

    public void LoadStatLevels(System.Action<Dictionary<StatType, int>> onLoaded)
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

                Debug.Log("[FirebaseStatSaver] ���� �ҷ����� ����");
                onLoaded?.Invoke(loadedStats);
            }
            else
            {
                Debug.LogError("[FirebaseStatSaver] ���� �ҷ����� ���� : " + task.Exception);
            }
        });
    }
}
