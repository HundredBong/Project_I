using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;

public class FirebaseInit : MonoBehaviour
{
    //TODO : �̱������� �ٲ�� �� �� ����. 
    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                GameManager.Instance.firebaseReady = true;
                Debug.Log("���̾�̽� �غ��");
            }
            else
            {
                Debug.LogError($"���̾�̽� ����, {task.Result}");
            }
        });
    }
}
