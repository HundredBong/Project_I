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
        _messageText.text = DataManager.Instance.GetLocalizedText(message);

        Transform root = UIManager.Instance.ToastRoot;

        if (root == null)
        {
            Debug.LogWarning("[UIToastMessage] ToastRoot°¡ ¾øÀ½");
        }
        else
        {
            transform.SetParent(root);
        }
        _cg.alpha = 0;
        UITweening.PlayToast(_cg, MOVE_TIME, () => ObjectPoolManager.Instance.uiPool.Return(this));

        //DelayCallManager.Instance.CallLater(DURATION, () => ObjectPoolManager.Instance.uiPool.Return(this));
    }
}
