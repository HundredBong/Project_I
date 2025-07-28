using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIStageInfoPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI goToMaxStageText;
    [SerializeField] private Button bossChallengeButton;
    [SerializeField] private Button goToMaxStageButton;
    [SerializeField] private Button stageSelectButton;
    [SerializeField] private Button giveUpButton;
    [SerializeField] private Image stageProgressImage;

    private void OnEnable()
    {
        Debug.Log($"스테이지 매니저가 널일 수 있나 {(StageManager.Instance == null ? "그럴수도 있죵" : "그럴 수는 없어용")}");
        SubscribeAsync().Forget();
    }

    private async UniTaskVoid SubscribeAsync()
    {
        await UniTask.WaitUntil(() => StageManager.Instance != null);

        StageManager.Instance.OnKillUpdated += RefreshKill;
        StageManager.Instance.OnStageChanged += RefreshStage;
        StageManager.Instance.OnBossStageEntered += RefreshBossStage;

        goToMaxStageButton.onClick.RemoveAllListeners();
        goToMaxStageButton.onClick.AddListener(OnMaxStageButtonClicked);

        bossChallengeButton.onClick.RemoveAllListeners();
        bossChallengeButton.onClick.AddListener(OnChallengeButtonClicked);

        stageSelectButton.onClick.RemoveAllListeners();
        stageSelectButton.onClick.AddListener(OnStageSelectButtonClicked);

        giveUpButton.onClick.RemoveAllListeners();
        giveUpButton.onClick.AddListener(OnGiveUpButtonClicked);
    }

    private void OnDisable()
    {
        if (StageManager.Instance == null) { return; }

        StageManager.Instance.OnKillUpdated -= RefreshKill;
        StageManager.Instance.OnStageChanged -= RefreshStage;
        StageManager.Instance.OnBossStageEntered -= RefreshBossStage;

        goToMaxStageButton.onClick.RemoveAllListeners();
        bossChallengeButton.onClick.RemoveAllListeners();
        stageSelectButton.onClick.RemoveAllListeners();
        giveUpButton.onClick.RemoveAllListeners();
    }

    private void Start()
    {
        //각종 텍스트 초기화

        killText.text = $"0 / 100"; 
        int stage = StageManager.Instance.CurrentStage;
        bool canBoss = false; //즉시 보스에게 도전하지 못하도록 일단 false, 절대 프로퍼티 만들기 귀찮아서 그런거 아님

        RefreshStage(stage, canBoss);
        SetLocalizedText();
    }

    private void SetLocalizedText()
    {
        stageText.text = $"{DataManager.Instance.GetLocalizedText("UI_Stage")} {StageManager.Instance.CurrentStage}";
        goToMaxStageText.text = DataManager.Instance.GetLocalizedText("UI_GoMaxStage");
    }

    private void RefreshKill(int current, int required)
    {
        int count = Mathf.Min(current, required);
        killText.text = $"{count} / {required}";
        stageProgressImage.fillAmount = Mathf.Min(current / (float)required, 1f);
    }

    private void RefreshStage(int stage, bool canBoss)
    {
        //Debug.Log($"현재 스테이지 : {stage}, 맥스 : {StageManager.Instance.MaxClearedStage}, 보스 여부 : {canBoss}");

        stageText.text = $"{DataManager.Instance.GetLocalizedText("UI_Stage")} {stage}";

        bool climbing = stage > StageManager.Instance.MaxClearedStage;// || (stage>= StageManager.Instance.MaxClearedStage && canBoss == );
        ;


        if (climbing)
        {
            if (canBoss)
            {
                bossChallengeButton.gameObject.SetActive(true);
                bossChallengeButton.interactable = true;
                goToMaxStageButton.gameObject.SetActive(false);
                giveUpButton.gameObject.SetActive(false);
            }
            else
            {
                bossChallengeButton.gameObject.SetActive(true);
                bossChallengeButton.interactable = false;
                goToMaxStageButton.gameObject.SetActive(false);
                giveUpButton.gameObject.SetActive(false);
            }
        }
        else
        {
            bossChallengeButton.gameObject.SetActive(false);
            goToMaxStageButton.gameObject.SetActive(true);
            giveUpButton.gameObject.SetActive(false);
        }
    }

    private void RefreshBossStage(int currentStage)
    {
        bossChallengeButton.gameObject.SetActive(false);
        goToMaxStageButton.gameObject.SetActive(false);
        giveUpButton.gameObject.SetActive(true);

        killText.text = "";
        stageText.text = $"BOSS {currentStage}";
        stageProgressImage.fillAmount = 1f;
    }


    private void OnChallengeButtonClicked()
    {
        StageManager.Instance.StartBossChallenge();
    }

    private void OnMaxStageButtonClicked()
    {
        StageManager.Instance.GoToStage(StageManager.Instance.MaxClearedStage);
    }

    private void OnStageSelectButtonClicked()
    {
        UIManager.Instance.PopupOpen<UIStageSelectPopup>();
    }

    private void OnGiveUpButtonClicked()
    {
        StageManager.Instance.ResetStage();
    }
}

