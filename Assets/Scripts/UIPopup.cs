using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIPopup : MonoBehaviour
{
    [SerializeField] protected Button closeButton;

    protected virtual void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() =>
            {
                UIManager.Instance.PopupClose();
            });
        }
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.HandleBack();
        }
    }
}
