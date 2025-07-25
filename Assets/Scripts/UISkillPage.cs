using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillPage : UIPage
{
    //UI페이지에 닫기버튼이랑 열기닫기 기능은 있음
    //열기 버튼은 다른 UI에 붙어있음
    //그럼 여기서 해야할 일은 UIStatPage처럼 DataManager의 skillDataTable을 순회하며 오브젝트 생성
    //스킬 레벨에 변동이 있을 때마다 업데이트
    [SerializeField] private GameObject skillSlotPrefab;
    [SerializeField] private Transform activeContentRoot;
    [SerializeField] private Transform passiveContentRoot;
    [SerializeField] private GameObject activeSkillScroll;
    [SerializeField] private GameObject passiveSkillScroll;
    [SerializeField] private Button activeSkillButton;
    [SerializeField] private Button passiveSkillButton;
    [SerializeField] private Color selectedColor;
    [SerializeField]private Color unselectedColor;
    [SerializeField] private Button popupOpenButton;

    private List<UISkillSlot> slotUIs = new List<UISkillSlot>();

    private void OnEnable()
    {
        activeSkillButton.onClick.AddListener(ShowActiveSkills);
        passiveSkillButton.onClick.AddListener(ShowPassiveSkills);
        popupOpenButton.onClick.AddListener(()=>UIManager.Instance.PopupOpen<SkillEquipPopup>());

        RefreshAll();
        LanguageManager.OnLanguageChanged += RefreshAll; 
    }

    private void OnDisable()
    {
        activeSkillButton.onClick.RemoveListener(ShowActiveSkills);
        passiveSkillButton.onClick.RemoveListener(ShowActiveSkills);
        popupOpenButton.onClick.RemoveAllListeners();

        LanguageManager.OnLanguageChanged -= RefreshAll;
    }

    private void ShowActiveSkills()
    {
        activeSkillButton.image.color = selectedColor;
        passiveSkillButton.image.color = unselectedColor;
        activeSkillScroll.SetActive(true);
        passiveSkillScroll.SetActive(false);
    }

    private void ShowPassiveSkills()
    {
        activeSkillButton.image.color = unselectedColor;
        passiveSkillButton.image.color = selectedColor;
        activeSkillScroll.SetActive(false);
        passiveSkillScroll.SetActive(true);
    }

    private void Start()
    {
        foreach (SkillData data in DataManager.Instance.GetAllSkillData())
        {
            GameObject obj = Instantiate(skillSlotPrefab, data.Type == SkillType.Active ? activeContentRoot : passiveContentRoot);
            UISkillSlot ui = obj.GetComponent<UISkillSlot>();
            ui.Init(data);
            slotUIs.Add(ui);
        }

        Debug.Log("[UISkillPage] 스킬 슬롯 초기화됨");

        ShowActiveSkills(); 
    }

    public void RefreshAll()
    {
        //언어 바뀔때 모든 스킬 슬롯 UI 새로고침
        foreach (UISkillSlot ui in slotUIs)
        {
            ui.Refresh();
        }
    }
}
