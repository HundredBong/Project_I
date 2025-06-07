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
        #region ������ ����
        //T target = null;

        //foreach (UIPage p in pages)
        //{
        //    //���� ������ ���������� Ȯ����, ���� �������� UIHome�̸� true, �ƴϸ� false�� ��ȯ
        //    //XRRayInteractor Ȯ�������� �����ϸ� �򰥸� �� ����
        //    //if(p.GetType() == typeof(UIHome)) 
        //    bool isActive = p is T;

        //    //������ �Ѱ�, �ƴϸ� ����
        //    p.gameObject.SetActive(isActive);

        //    if (isActive == true)
        //    {
        //        //�ٿ�ĳ���� Page�� T�� ��ȯ
        //        target = p as T;
        //    }
        //}

        //return target;
        #endregion
    }

    public void PageClose()
    {
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
}
