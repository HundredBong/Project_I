using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;

public class FirebaseInit : MonoBehaviour
{
    //TODO : 싱글톤으로 바꿔야 할 수 있음. 
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
                        //Debug.Log("파이어베이스 준비됨, 익명 로그인 성공");
                        //Debug.Log($"FirebaseInit : {FirebaseAuth.DefaultInstance.CurrentUser.UserId}" );
                    }
                    else
                    {
                        Debug.LogError($"익명 로그인 실패, {authTask.Exception}");
                    }
                });
            }
            else
            {
                Debug.LogError($"파이어베이스 에러, {task.Result}");
            }
        });
    }
}
