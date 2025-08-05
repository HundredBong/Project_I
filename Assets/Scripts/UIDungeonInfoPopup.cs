using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIDungeonInfoPopup : UIPopup
{
    [SerializeField] private TextMeshProUGUI _dungeonNameText;
    [SerializeField] private TextMeshProUGUI _dungeonLevelText;
    [SerializeField] private TextMeshProUGUI _requiredTicketText;
    [SerializeField] private TextMeshProUGUI _dungeonTicketText;
    [SerializeField] private TextMeshProUGUI _rewardText;
    [SerializeField] private TextMeshProUGUI _sweepText;
    [SerializeField] private TextMeshProUGUI _enterText;

    [Space(20)]
    [SerializeField] private Button _sweepButton;
    [SerializeField] private Button _enterButton;
    [SerializeField] private Button _positiveButton;
    [SerializeField] private Button _negetiveButton;

    [Space(20)]
    [SerializeField] private Image[] _rewardImages;
    [SerializeField] private TextMeshProUGUI[] _rewardAmountTexts;
    [SerializeField] private GameObject[] _rewardPrefabs;

    private DungeonData _data;
    private int _currentStage;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += Refresh;

        _positiveButton.onClick.AddListener(OnClickPositiveButton);
        _negetiveButton.onClick.AddListener(OnClickNegativeButton);
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= Refresh;

        _positiveButton.onClick.RemoveListener(OnClickPositiveButton);
        _negetiveButton.onClick.RemoveListener(OnClickNegativeButton);
    }

    public void Init(DungeonData dungeonData)
    {
        _data = dungeonData;
        _currentStage = 0; //파이어베이스에서 불러온 값으로 적용

        Refresh();
    }

    private void Refresh()
    {
        _dungeonNameText.text = DataManager.Instance.GetLocalizedText($"UI_{_data.NameKey}");

        //파이어베이스에서 레벨 불러오기 필요, DataManager에서 마지막으로 클리어한 레벨, 데이터매니저에서 보상 데이터 길이 불러오기로 제한
        _dungeonLevelText.text = $"{0}{DataManager.Instance.GetLocalizedText("UI_Stage")}";

        _requiredTicketText.text = DataManager.Instance.GetLocalizedText($"UI_RequiredTicket");
        _dungeonTicketText.text = DataManager.Instance.GetLocalizedText($"UI_{_data.TicketType}");
        _rewardText.text = DataManager.Instance.GetLocalizedText($"UI_ClearReward");
        _sweepText.text = DataManager.Instance.GetLocalizedText($"UI_Sweep");
        _enterText.text = DataManager.Instance.GetLocalizedText($"UI_Enter");
    }

    private void OnClickPositiveButton()
    {
        _currentStage++;
        int max = 0; //Mathf.Max(파이어베이스에서 최대 스테이지, 보상데이터 길이)로 설정
        if (_currentStage >= max)
        {
            _currentStage = 1;
        }
    }

    private void OnClickNegativeButton()
    {
        _currentStage--;
        if (_currentStage >= 0)
        {
            int max = 0; //Mathf.Max(파이어베이스에서 최대 스테이지, 보상데이터 길이)로 설정
            _currentStage = max;
        }
    }

    private void OnClickSweep()
    {
        //던전 보상데이터[0].스프라이트 키, 주요 보상키, 최대 소탕권 갯수 키, Sweep, UI_CheckSweep, UI_Sweep, UI_Cancel
        //UIManager.Instance.PopupOpen<UISliderPopup>().Init();
    }

    private void Sweep(int count)
    {
        //count만큼 던전 클리어한걸로 취급한 후 보상 지급
        DungeonType type = _data.DungeonType;
    }

    private void OnClickEnter()
    {
        //씬 만들고 이름 넣어야 함
        //선택한 레벨정보를 다음 씬에 넘겨줘야 함
        LoadingSceneController.LoadScene("");
        StageManager.Instance.SetStageType(_data.DungeonType);
    }
}
