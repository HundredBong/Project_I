using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup), typeof(TextMeshProUGUI))]
public class UIToastMessage : PooledUI
{
    private CanvasGroup _cg;
    private TextMeshProUGUI _messageText;
    private const float DURATION = 1.5f;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _messageText = GetComponent<TextMeshProUGUI>();
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
        UITweening.PlayToast(_cg, DURATION, () => ObjectPoolManager.Instance.uiPool.Return(this));

        //DelayCallManager.Instance.CallLater(DURATION, () => ObjectPoolManager.Instance.uiPool.Return(this));
    }
}
