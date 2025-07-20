using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Popup")]
    public List<UIPopup> popups = new List<UIPopup>();

    [Header("Page")]
    public List<UIPage> pages = new List<UIPage>();

    private Stack<UIPopup> openPopups = new Stack<UIPopup>();
    private UIPage currentPage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        //-------------------------------------------------------------------

        Init();
    }

    private void Init()
    {
        foreach (UIPopup p in popups)
        {
            p.gameObject.SetActive(false);
        }

        foreach (UIPage p in pages)
        {
            p.gameObject.SetActive(false);
        }
    }

    //T Ÿ���� �ް�, �� Ÿ���� ��ü�� ��ȯ
    //var page = UIManager.Instance.PageOpen<UIHome>(); �ϸ� T�� UIHome�� ��,
    //Where T : UIPage => T�� UIPage�� ��ӹ޾ƾ� ��. 
    public T PageOpen<T>() where T : UIPage
    {
        //�������� �׻� �ϳ��� ������. 

        //Debug.Log("[UIManager] ������ ����");

        if (currentPage != null)
        {
            currentPage.Close();
        }

        T page = pages.Find(p => p is T) as T;

        if (page != null)
        {
            page.Open();
            currentPage = page;
        }
      
        return page;
    }

    public void PageClose()
    {
        while(openPopups.Count > 0)
        {
            PopupClose();
        }

        if (currentPage != null)
        {
            currentPage.Close();
            currentPage = null;
        }
    }

    public T PopupOpen<T>() where T : UIPopup
    {
        //�˾��� ������ ���� ��, �˶� �˾��̳� ���� �˾�������, ���� �� �������� �����ź��� �ݾƾ� �ϴ� ���ÿ� ����

        //popup����Ʈ���� Ÿ���� ��ġ�ϴ� ù��° ��ü�� ã��, �� ��ü�� �ٿ�ĳ�����ؼ� ��ȯ
        T popup = popups.Find(p => p is T) as T;

        if (popup != null)
        {
            popup.Open();
            openPopups.Push(popup);
        }

        return popup;
    }

    public void PopupClose()
    {
        if (openPopups.Count > 0)
        {
            //�������� ���� �˾� ������� ����
            UIPopup popup = openPopups.Pop();
            popup.Close();
        }
    }

    public void HandleBack()
    {
        //�˾� ť�� �����ִ� �˾��� ������ �˾��� ����.
        if (openPopups.Count > 0) 
        {
            PopupClose();
        }
        //ť�� �������� �ʰ�, ���� �������� ������ �������� ����.
        else if (currentPage != null)
        {
            PageClose();
        }
    }

    public bool TryGetPage<T>(out T page) where T : UIPage
    {
        foreach (UIPage p in pages)
        {
            if (p is T target)
            {
                page = target;
                return true;
            }
        }

        page = null;
        return false;
    }
}
