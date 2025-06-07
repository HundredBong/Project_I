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

    //T 타입을 받고, 그 타입의 객체를 반환
    //var page = UIManager.Instance.PageOpen<UIHome>(); 하면 T는 UIHome이 됨,
    //Where T : UIPage => T는 UIPage를 상속받아야 함. 
    public T PageOpen<T>() where T : UIPage
    {
        //페이지는 항상 하나만 떠야함. 

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
        #region 예전에 쓴거
        //T target = null;

        //foreach (UIPage p in pages)
        //{
        //    //내가 열려는 페이지인지 확인함, 지금 페이지가 UIHome이면 true, 아니면 false를 반환
        //    //XRRayInteractor 확장했을때 생각하면 헷갈릴 일 없음
        //    //if(p.GetType() == typeof(UIHome)) 
        //    bool isActive = p is T;

        //    //맞으면 켜고, 아니면 닫음
        //    p.gameObject.SetActive(isActive);

        //    if (isActive == true)
        //    {
        //        //다운캐스팅 Page를 T로 변환
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
        //팝업은 여러개 떠도 됨, 알람 팝업이나 설정 팝업같은거, 닫을 때 마지막에 켜진거부터 닫아야 하니 스택에 저장

        //popup리스트에서 타입이 일치하는 첫번째 객체를 찾고, 그 객체를 다운캐스팅해서 반환
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
            //마지막에 켜진 팝업 순서대로 꺼짐
            UIPopup popup = openPopups.Pop();
            popup.Close();
        }
    }
}
