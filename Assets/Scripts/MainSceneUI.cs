using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneUI : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;

    private void Awake()
    {
        gameStartButton.interactable = false;
    }

    private void OnEnable()
    {
        gameStartButton.onClick.AddListener(OnClickStartButton);
    }

    private void Start()
    {
        WaitForReadyAsync().Forget();
    }

    private async UniTaskVoid WaitForReadyAsync()
    {
        try
        {
            await UniTask.WaitUntil(() => GameManager.Instance.loadSceneReady); 
            gameStartButton.interactable = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[MainSceneUI] 기다리다 예외 발생함, {e.Message}");
        }
    }

    private void OnClickStartButton()
    {
        SceneManager.LoadScene("1_StageScene");
    }
}
