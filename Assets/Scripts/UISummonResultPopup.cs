using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UISummonResultPopup : UIPopup
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private TextMeshProUGUI closeButtonText;

    private Coroutine showResultCoroutine;
    private WaitForSeconds _commonWait;
    private WaitForSeconds _uncommonWait;

    private List<UIResultContent> _contents = new List<UIResultContent>();

    private Action _onSummon10;
    private Action _onSummon30;
    private Action _onSummon100;

    [SerializeField] private Button _summon10Button;
    [SerializeField] private Button _summon30Button;
    [SerializeField] private Button _summon100Button;

    [SerializeField] private TextMeshProUGUI _summon10ButtonText;
    [SerializeField] private TextMeshProUGUI _summon30ButtonText;
    [SerializeField] private TextMeshProUGUI _summon100ButtonText;

    [SerializeField] private TextMeshProUGUI _summon10PriceText;
    [SerializeField] private TextMeshProUGUI _summon30PriceText;
    [SerializeField] private TextMeshProUGUI _summon100PriceText;

    protected override void Awake()
    {
        base.Awake();

        _commonWait = new WaitForSeconds(0.05f);
        _uncommonWait = new WaitForSeconds(0.3f - 0.05f);

        SetButtonsActive(false);
        SetLocalizedText();
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += SetLocalizedText;

        _summon10Button.onClick.AddListener(() => _onSummon10?.Invoke());
        _summon30Button.onClick.AddListener(() => _onSummon30?.Invoke());
        _summon100Button.onClick.AddListener(() => _onSummon100?.Invoke());
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= SetLocalizedText;

        _onSummon10 = null;
        _onSummon30 = null;
        _onSummon100 = null;

        _summon10Button.onClick.RemoveAllListeners();
        _summon30Button.onClick.RemoveAllListeners();
        _summon100Button.onClick.RemoveAllListeners();
    }

    public void RegistActions(Action onSummon10, Action onSummon30, Action onSummon100, SummonSubCategory category)
    {
        _onSummon10 = null;
        _onSummon30 = null;
        _onSummon100 = null;

        _onSummon10 = onSummon10;
        _onSummon30 = onSummon30;
        _onSummon100 = onSummon100;

        _summon10PriceText.text = DataManager.Instance.GetSummonPriceData(category, 10).ToString("N0");
        _summon30PriceText.text = DataManager.Instance.GetSummonPriceData(category, 30).ToString("N0");
        _summon100PriceText.text = DataManager.Instance.GetSummonPriceData(category, 100).ToString("N0");
    }

    private void SetButtonsActive(bool active)
    {
        closeButton.gameObject.SetActive(active);

        if (_summon10Button != null)
        {
            _summon10Button.gameObject.SetActive(active && _onSummon10 != null);
        }

        if (_summon30Button != null)
        {
            _summon30Button.gameObject.SetActive(active && _onSummon30 != null);
        }

        if (_summon100Button != null)
        {
            _summon100Button.gameObject.SetActive(active && _onSummon100 != null);
        }
    }

    private void SetLocalizedText()
    {
        _summon10ButtonText.text = DataManager.Instance.GetLocalizedText("UI_Summon10");
        _summon30ButtonText.text = DataManager.Instance.GetLocalizedText("UI_Summon30");
        _summon100ButtonText.text = DataManager.Instance.GetLocalizedText("UI_Summon100");
    }

    public void StartDisplayingResult(Queue<ItemData> data)
    {
        closeButtonText.text = DataManager.Instance.GetLocalizedText("UI_ResultPopupClose");
        SetButtonsActive(false);

        if (showResultCoroutine != null)
        {
            StopDisplayingResult();
        }

        foreach (UIResultContent content in _contents)
        {
            content.transform.SetParent(ObjectPoolManager.Instance.uiPool.transform ?? null);
            ObjectPoolManager.Instance.uiPool.Return(content);
        }

        _contents.Clear();

        showResultCoroutine = StartCoroutine(StartDisplayingResultCoroutine(data));
    }

    public void StartDisplayingResult(Queue<SkillData> data)
    {
        closeButtonText.text = DataManager.Instance.GetLocalizedText("UI_ResultPopupClose");
        SetButtonsActive(false);


        if (showResultCoroutine != null)
        {
            StopDisplayingResult();
        }

        foreach (UIResultContent content in _contents)
        {
            content.transform.SetParent(ObjectPoolManager.Instance.uiPool.transform ?? null);
            ObjectPoolManager.Instance.uiPool.Return(content);
        }

        _contents.Clear();

        showResultCoroutine = StartCoroutine(StartDisplayingResultCoroutine(data));
    }

    public void StopDisplayingResult()
    {
        if (showResultCoroutine != null)
        {
            StopCoroutine(showResultCoroutine);
            showResultCoroutine = null;
        }
    }

    private IEnumerator StartDisplayingResultCoroutine(Queue<ItemData> data)
    {
        //1. 아이템 데이터의 타입을 받아서 이게 무기인지, 방어구인지 검사하고
        //2. 10회, 30회, 100회 짜리 버튼을 SetActive(true)로 만들고,
        //3. 그 버튼이 UISummonPanel의 Summon버튼과 동일한 역할을 하도록 해야 함.

        while (data.Count != 0)
        {
            yield return _commonWait;
            UIResultContent content = ObjectPoolManager.Instance.uiPool.GetResult();
            _contents.Add(content);

            content.transform.SetParent(contentRoot);
            ItemData itemData = data.Dequeue();
            content.Initialize(itemData);

            if (GradeType.Epic <= itemData.GradeType)
            {
                yield return _uncommonWait;
            }

        }

        SetButtonsActive(true);
    }

    private IEnumerator StartDisplayingResultCoroutine(Queue<SkillData> data)
    {
        while (data.Count != 0)
        {
            yield return _commonWait;
            UIResultContent content = ObjectPoolManager.Instance.uiPool.GetResult();
            _contents.Add(content);

            content.transform.SetParent(contentRoot);
            SkillData skillData = data.Dequeue();
            content.Initialize(skillData);

            if (GradeType.Epic <= skillData.Grade)
            {
                yield return _uncommonWait;
            }
        }

        SetButtonsActive(true);
    }
}
