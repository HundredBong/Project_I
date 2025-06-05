using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;

public class FirebaseStatSaver : MonoBehaviour
{
    private DatabaseReference dbRef;
    private bool isReady = false;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
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
}
