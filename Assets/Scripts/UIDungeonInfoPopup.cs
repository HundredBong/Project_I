using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class UIDungeonInfoPopup : UIPopup
{
    [SerializeField] private TextMeshProUGUI _dungeonNameText;
    [SerializeField] private TextMeshProUGUI _dungeonLevelText;
    [SerializeField] private TextMeshProUGUI _requiredTicketText;
    [SerializeField] private TextMeshProUGUI _entryTicketText;
    [SerializeField] private TextMeshProUGUI _rewardText;
    [SerializeField] private TextMeshProUGUI _sweepText;
    [SerializeField] private TextMeshProUGUI _enterText;
    [SerializeField] private TextMeshProUGUI _currentTicketCountText;

    [Space(20)]
    [SerializeField] private Button _sweepButton;
    [SerializeField] private Button _enterButton;
    [SerializeField] private Button _positiveButton;
    [SerializeField] private Button _negetiveButton;

    [Space(20)]
    [SerializeField] private Image[] _rewardImages;
    [SerializeField] private TextMeshProUGUI[] _rewardAmountTexts;
    [SerializeField] private GameObject[] _rewardPrefabs;

    [Space(20)]
    [SerializeField] private Image _dungeonImage;
    [SerializeField] private Image _ticketImage;

    private DungeonData _data;
    private DungeonLevelData _levelData;
    private int _currentLevel;
    private int _maxClearedLevel;
    private int _maxDungeonLevel;
    private int _currentTicket;

    protected override void Awake()
    {
        base.Awake();

        int length = _rewardImages.Length;

        if (_rewardAmountTexts.Length != length || _rewardPrefabs.Length != length)
        {
            Debug.LogError("[UIDungeonInfoPopup] Reward 배열 길이가 맞지않음");
        }
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += Refresh;

        _positiveButton.onClick.AddListener(OnClickPositiveButton);
        _negetiveButton.onClick.AddListener(OnClickNegativeButton);
        _sweepButton.onClick.AddListener(OnClickSweep);
        _enterButton.onClick.AddListener(OnClickEnter);
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= Refresh;

        _positiveButton.onClick.RemoveListener(OnClickPositiveButton);
        _negetiveButton.onClick.RemoveListener(OnClickNegativeButton);
        _sweepButton.onClick.RemoveListener(OnClickSweep);
        _enterButton.onClick.RemoveListener(OnClickEnter);
    }

    public void Init(DungeonData dungeonData)
    {
        _data = dungeonData;
        _currentLevel = StageManager.Instance.GetMaxClearedLevel(_data.DungeonType);
        Debug.Log($"current : {_currentLevel}");
        _maxDungeonLevel = DataManager.Instance.GetMaxDungeonLevel(_data.DungeonType);
        _maxClearedLevel = StageManager.Instance.GetMaxClearedLevel(_data.DungeonType);
        _currentLevel = _maxClearedLevel;
        _levelData = DataManager.Instance.GetDungeonLevelData(_data.DungeonType, _currentLevel);

        _dungeonImage.sprite = DataManager.Instance.GetSpriteByKey(_data.DungeonSpriteKey);
        _ticketImage.sprite = DataManager.Instance.GetSpriteByKey($"UI_{_data.TicketType}");

        Refresh();
    }

    private void Refresh()
    {
        _currentTicket = GameManager.Instance.stats.GetCurrency(_data.TicketType);

        _dungeonNameText.text = DataManager.Instance.GetLocalizedText($"UI_{_data.NameKey}");
        _dungeonLevelText.text = $"{_currentLevel}{DataManager.Instance.GetLocalizedText("UI_Stage")}";
        _requiredTicketText.text = DataManager.Instance.GetLocalizedText($"UI_RequiredTicket");
        _entryTicketText.text = DataManager.Instance.GetLocalizedText($"UI_{_data.TicketType}");
        _rewardText.text = DataManager.Instance.GetLocalizedText($"UI_ClearReward");
        _sweepText.text = DataManager.Instance.GetLocalizedText($"UI_Sweep");
        _enterText.text = DataManager.Instance.GetLocalizedText($"UI_Enter");
        _currentTicketCountText.text = _currentTicket.ToString();

        int count = _levelData.Amounts.Count;

        foreach (GameObject obj in _rewardPrefabs)
        {
            obj.SetActive(false);
        }

        for (int i = 0; i < count; i++)
        {
            _rewardPrefabs[i].SetActive(true);
            _rewardImages[i].sprite = DataManager.Instance.GetSpriteByKey(_levelData.SpriteKeys[i]);
            _rewardAmountTexts[i].text = _levelData.Amounts[i].ToString();
        }
    }

    private void OnClickPositiveButton()
    {
        _currentLevel++;

        //최대 던전 레벨과 최대 클리어한 던전 레벨중 더 작은걸로 설정
        int max = Mathf.Min(_maxDungeonLevel, _maxClearedLevel);

        //현재 레벨이 최대 레벨을 초과하면 1로 초기화
        if (_currentLevel > max)
        {
            _currentLevel = 1;
        }

        Refresh();
    }

    private void OnClickNegativeButton()
    {
        _currentLevel--;

        //현재 스테이지가 0이하가 되면 최대레벨로 초기화 
        if (_currentLevel <= 0)
        {
            int max = Mathf.Min(_maxDungeonLevel, _maxClearedLevel);
            _currentLevel = max;
        }

        Refresh();
    }

    private void OnClickSweep()
    {
        if (_currentTicket <= 0)
        {
            ObjectPoolManager.Instance.uiPool.GetMessage().Init("UI_NotEnoughTickets");
            return;
        }

        //던전 보상데이터[0].스프라이트 키, 주요 보상키, 최대 티켓, Sweep, UI_CheckSweep, UI_Sweep, UI_Cancel
        UIManager.Instance.PopupOpen<UISliderPopup>().Init(_levelData.DungeonType, _currentLevel
            , _levelData.SpriteKeys[0], $"UI_{_levelData.Currencies[0]}", _currentTicket, (int selected) =>
        {
            Sweep(selected);
        }, "UI_CheckSweep", "UI_Sweep", "UI_Cancel");

    }

    private void Sweep(int count)
    {
        if (count <= 0)
        {
            return;
        }

        //기존 티켓 차감 
        if (GameManager.Instance.stats.TrySpendItem(_data.TicketType, count))
        {
            //보상 UI 출력
            Sprite[] sprites = new Sprite[_levelData.SpriteKeys.Count];
            int[] amounts = new int[_levelData.Amounts.Count];

            for (int i = 0; i < _levelData.SpriteKeys.Count; i++)
            {
                sprites[i] = DataManager.Instance.GetSpriteByKey(_levelData.SpriteKeys[i]);
            }
            for (int k = 0; k < amounts.Length; k++)
            {
                amounts[k] = _levelData.Amounts[k] * count;
            }

            ObjectPoolManager.Instance.uiPool.GetReward().Init(sprites, amounts);

            //보상 지급 및 저장
            for (int i = 0; i < _levelData.Currencies.Count; i++)
            {
                PlayerProgressType currency = _levelData.Currencies[i];
                int total = _levelData.Amounts[i] * count;

                GameManager.Instance.stats.AddCurrency(currency, total);
            }

            GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();
        }

        Refresh();
    }


    private void OnClickEnter()
    {
        //씬 만들고 이름 넣어야 함
        //선택한 레벨정보를 다음 씬에 넘겨줘야 함

        switch (_levelData.DungeonType)
        {
            case DungeonType.EnhanceDungeon:
                StageManager.Instance.enhanceDungeonLevel = _currentLevel;
                LoadingSceneController.LoadScene("2_EnhanceDungeonScene");
                break;
            case DungeonType.SkillDungeon:
                StageManager.Instance.skillGemDungeonLevel = _currentLevel;
                break;
            case DungeonType.None:
            default:
                break;
        }
    }

    [ContextMenu("테스트")]
    private void Test()
    {
        StageManager.Instance.enhanceDungeonLevel = 1;
        LoadingSceneController.LoadScene("2_EnhanceDungeonScene");
    }
}
