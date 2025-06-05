using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;

public class FirebaseInit : MonoBehaviour
{
    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("파이어베이스 준비됨");
            }
            else
            {
                Debug.LogError($"파이어베이스 에러, {task.Result}");
            }
        });
    }
}
