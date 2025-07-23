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
        Debug.Log($"�������� �Ŵ����� ���� �� �ֳ� {(StageManager.Instance == null ? "��" : "�ƴϿ�")}");
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
        //���� �ؽ�Ʈ �ʱ�ȭ
        goToMaxStageText.text = DataManager.Instance.GetLocalizedText("UI_GoMaxStage");
    }

    private void RefreshKill(int current, int required)
    {
        killText.text = $"{current} / {required}";
        stageProgressImage.fillAmount = Mathf.Min(current / (float)required, 1f);
    }

    private void RefreshStage(int stage, bool canBoss)
    {
        Debug.Log($"���� �������� : {stage}, �ƽ� : {StageManager.Instance.MaxClearedStage}, ���� ���� : {canBoss}");

        stageText.text = $"{DataManager.Instance.GetLocalizedText("UI_Stage")} {stage}";

        bool climbing = stage > StageManager.Instance.MaxClearedStage;


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
        //UIManager.Instance.PopupOpen
    }

    private void OnGiveUpButtonClicked()
    {
        StageManager.Instance.ResetStage();
    }
}

