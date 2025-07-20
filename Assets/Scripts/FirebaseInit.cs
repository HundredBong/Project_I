using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;

public class FirebaseInit : MonoBehaviour
{
    //TODO : �̱������� �ٲ�� �� �� ����. 
    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(authTask =>
                {
                    if (authTask.IsCompleted && authTask.IsFaulted == false && authTask.IsCanceled == false)
                    {
                        GameManager.Instance.firebaseReady = true;
                        //Debug.Log("���̾�̽� �غ��, �͸� �α��� ����");
                        //Debug.Log($"FirebaseInit : {FirebaseAuth.DefaultInstance.CurrentUser.UserId}" );
                    }
                    else
                    {
                        Debug.LogError($"�͸� �α��� ����, {authTask.Exception}");
                    }
                });
            }
            else
            {
                Debug.LogError($"���̾�̽� ����, {task.Result}");
            }
        });
    }
}
