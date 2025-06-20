using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillPage : UIPage
{
    //UI�������� �ݱ��ư�̶� ����ݱ� ����� ����
    //���� ��ư�� �ٸ� UI�� �پ�����
    //�׷� ���⼭ �ؾ��� ���� UIStatPageó�� DataManager�� skillDataTable�� ��ȸ�ϸ� ������Ʈ ����
    //��ų ������ ������ ���� ������ ������Ʈ

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

    private List<SkillSlotUI> slotUIs = new List<SkillSlotUI>();

    private void OnEnable()
    {
        activeSkillButton.onClick.AddListener(ShowActiveSkills);
        passiveSkillButton.onClick.AddListener(ShowPassiveSkills);
        popupOpenButton.onClick.AddListener(()=>UIManager.Instance.PopupOpen<SkillEquipPopup>());

        LanguageManager.OnLanguageChanged += Refresh; 
    }

    private void OnDisable()
    {
        activeSkillButton.onClick.RemoveListener(ShowActiveSkills);
        passiveSkillButton.onClick.RemoveListener(ShowActiveSkills);

        LanguageManager.OnLanguageChanged -= Refresh;
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
        foreach (KeyValuePair<SkillId, SkillData> kvp in DataManager.Instance.skillDataTable)
        {
            GameObject obj = Instantiate(skillSlotPrefab, kvp.Value.Type == SkillType.Active ? activeContentRoot : passiveContentRoot);
            SkillSlotUI ui = obj.GetComponent<SkillSlotUI>();
            ui.Init(kvp.Value);
            slotUIs.Add(ui);
        }

        Debug.Log("[UISkillPage] ��ų ���� �ʱ�ȭ��");

        ShowActiveSkills(); 
    }

    public void Refresh()
    {
        //��� �ٲ� ��� ��ų ���� UI ���ΰ�ħ
        foreach (SkillSlotUI ui in slotUIs)
        {
            ui.Refresh();
        }
    }
}
