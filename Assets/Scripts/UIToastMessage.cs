using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIToastMessage : PooledUI
{
    [SerializeField] private TextMeshProUGUI _messageText;

    private CanvasGroup _cg;
    private const float MOVE_TIME = 1.5f;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
    }

    public void Init(string message)
    {
        DontDestroyOnLoad(gameObject);

        _messageText.text = DataManager.Instance.GetLocalizedText(message);

        Transform root = UIManager.Instance.ToastRoot;

        if (root == null)
        {
            Debug.LogWarning("[UIToastMessage] ToastRoot가 없음");
        }
        else
        {
            transform.SetParent(root, false);
        }
        _cg.alpha = 0;
        UITweening.PlayToast(_cg, MOVE_TIME, () =>
        {
            transform.SetParent(ObjectPoolManager.Instance.uiPool.transform);
            ObjectPoolManager.Instance.uiPool.Return(this);
        });

        //DelayCallManager.Instance.CallLater(DURATION, () => ObjectPoolManager.Instance.uiPool.Return(this));
    }

    public void Log(string message)
    {
        DontDestroyOnLoad(gameObject);

        _messageText.text = message;

        Transform root = UIManager.Instance.ToastRoot;

        if (root == null)
        {
            Debug.LogWarning("[UIToastMessage] ToastRoot가 없음");
        }
        else
        {
            transform.SetParent(root, false);
        }
        _cg.alpha = 0;
        UITweening.PlayToast(_cg, MOVE_TIME, () =>
        {
            transform.SetParent(ObjectPoolManager.Instance.uiPool.transform);
            ObjectPoolManager.Instance.uiPool.Return(this);
        });

        //DelayCallManager.Instance.CallLater(DURATION, () => ObjectPoolManager.Instance.uiPool.Return(this));
    }

    public void LogError(string message)
    {
        DontDestroyOnLoad(gameObject);
        _messageText.color = Color.red; //에러 메시지 색상 변경
        _messageText.text = message;

        Transform root = UIManager.Instance.ToastRoot;

        if (root == null)
        {
            Debug.LogWarning("[UIToastMessage] ToastRoot가 없음");
        }
        else
        {
            transform.SetParent(root, false);
        }
        _cg.alpha = 0;
        UITweening.PlayToast(_cg, MOVE_TIME, () =>
        {
            transform.SetParent(ObjectPoolManager.Instance.uiPool.transform);
            ObjectPoolManager.Instance.uiPool.Return(this);
        });

        //DelayCallManager.Instance.CallLater(DURATION, () => ObjectPoolManager.Instance.uiPool.Return(this));
    }
}
