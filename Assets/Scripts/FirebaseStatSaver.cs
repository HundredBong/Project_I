using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using System.Threading.Tasks;
using System;

public class FirebaseStatSaver : MonoBehaviour
{
    //파이어베이스 실시간 데이터베이스에서 최상위 경로
    private DatabaseReference dbRef;
    private bool isReady = false;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                //현재 프로젝트와 연결된 기본 파이어베이스 인스턴스의 최상위 경로
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                isReady = true;
                Debug.Log("[FirebaseStatSaver] 파이어베이스 초기화 됨");
            }
            else
            {
                Debug.Log("[FirebaseStatSaver] 파이어베이스 초기화 실패");
            }
        });
    }

    public void SaveStatLevels(Dictionary<StatType, int> statLevels)
    {
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

                Debug.Log("[FirebaseStatSaver] 스탯 불러오기 성공");
                onLoaded?.Invoke(loadedStats);
            }
            else
            {
                Debug.LogError("[FirebaseStatSaver] 스탯 불러오기 실패 : " + task.Exception);
            }
        });
    }
}
