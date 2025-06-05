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
                Debug.Log("���̾�̽� �غ��");
            }
            else
            {
                Debug.LogError($"���̾�̽� ����, {task.Result}");
            }
        });
    }
}
