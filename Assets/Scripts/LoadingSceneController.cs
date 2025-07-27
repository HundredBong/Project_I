using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private Image _progressBar;

    public static string _nextScene;
    private static bool _isLoading;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); //로딩 끝나고 씬 완전히 넘어가야 파괴되도록 함
    }

    public static void LoadScene(string sceneName)
    {
        if (_isLoading || sceneName == null)
        {
            return;
        }

        _nextScene = sceneName;
        _isLoading = true;
        SceneManager.LoadScene("LoadingScene");
    }

    private void Start()
    {
        LoadSceneProcess().Forget();
    }

    private async UniTaskVoid LoadSceneProcess()
    {
        try
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(_nextScene);
            op.allowSceneActivation = false;

            //0 ~ 90
            while (op.progress < 0.9f)
            {
                _progressBar.fillAmount = op.progress;
                await UniTask.Yield();
            }

            //90 ~ 100
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime;
                _progressBar.fillAmount = Mathf.Lerp(0.9f, 1f, t);
                await UniTask.Yield();
            }

            op.allowSceneActivation = true;
            await op;

            _isLoading = false;
            Destroy(gameObject);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Loading] 로딩 실패 : {e.Message}");
            _isLoading = false;
        }
    }
}
