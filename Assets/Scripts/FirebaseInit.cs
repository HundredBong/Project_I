using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;

public class FirebaseInit : MonoBehaviour
{
    private void Start()
    {
        //FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        //{
        //    if (task.Result == DependencyStatus.Available)
        //    {
        //        FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(authTask =>
        //        {
        //            if (authTask.IsCompleted && authTask.IsFaulted == false && authTask.IsCanceled == false)
        //            {
        //                GameManager.Instance.firebaseReady = true;
        //            }
        //            else
        //            {
        //                Debug.LogError($"익명 로그인 실패, {authTask.Exception}");
        //            }
        //        });
        //    }
        //    else
        //    {
        //        Debug.LogError($"파이어베이스 에러, {task.Result}");
        //    }
        //});

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("[FirebaseInit] Dependencies OK");
            }
            else
            {
                Debug.LogError($"[FirebaseInit] Dependency error : {task.Result}");
            }
        });
    }
}
