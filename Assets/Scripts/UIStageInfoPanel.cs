using Cysharp.Threading.Tasks;
using System;
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
    [SerializeField] private GameObject timerGroup;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerImage;

    private Coroutine _timerCoroutine;

    private const float TIMER_DURATION = 30f;

    private void OnEnable()
    {
        if (StageManager.Instance == null) { return; }

        StageManager.Instance.OnKillUpdated += RefreshKill;
        StageManager.Instance.OnStageChanged += RefreshStage;
        StageManager.Instance.OnBossStageEntered += RefreshBossStage;
        StageManager.Instance.stopTimer += StopTimer;

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
        StageManager.Instance.stopTimer -= StopTimer;

        goToMaxStageButton.onClick.RemoveAllListeners();
        bossChallengeButton.onClick.RemoveAllListeners();
        stageSelectButton.onClick.RemoveAllListeners();
        giveUpButton.onClick.RemoveAllListeners();
    }

    private void Start()
    {
        //각종 텍스트 초기화
        stageProgressImage.fillAmount = 0f;
        SetLocalizedText();
    }

    private void SetLocalizedText()
    {
        goToMaxStageText.text = DataManager.Instance.GetLocalizedText("UI_GoMaxStage");
    }

    private void RefreshKill(int current, int required)
    {
        int count = Mathf.Min(current, required);
        killText.text = $"{count} / {required}";
        stageProgressImage.fillAmount = Mathf.Min(current / (float)required, 1f);
    }

    private void RefreshStage(DungeonType type, int stage, bool canBoss)
    {
        stageText.text = $"{DataManager.Instance.GetLocalizedText($"UI_{type}")} {stage}";
        Debug.Log($"타입 : {DataManager.Instance.GetLocalizedText($"UI_{type}")} 스테 : {stage}");
        StopTimer();

        if (type == DungeonType.None)
        {
            bool climbing = stage > StageManager.Instance.maxClearedStage;

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
        else
        {
            bossChallengeButton.gameObject.SetActive(false);
            goToMaxStageButton.gameObject.SetActive(false);
            stageSelectButton.gameObject.SetActive(false);
            StartTimer();
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
        StartTimer();
    }


    private void OnChallengeButtonClicked()
    {
        StageManager.Instance.StartBossChallenge();
    }

    private void OnMaxStageButtonClicked()
    {
        StageManager.Instance.GoToStage(StageManager.Instance.maxClearedStage);
    }

    private void OnStageSelectButtonClicked()
    {
        UIManager.Instance.PopupOpen<UIStageSelectPopup>();
    }

    private void OnGiveUpButtonClicked()
    {
        StageManager.Instance.ResetStage();
    }

    private void StartTimer()
    {
        timerGroup.SetActive(true);
        timerImage.fillAmount = 0f;

        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
        }

        _timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    private void StopTimer()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        timerGroup.SetActive(false);
    }

    private IEnumerator TimerCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < TIMER_DURATION)
        {
            elapsed += Time.deltaTime;
            timerImage.fillAmount = Mathf.Clamp01(elapsed / TIMER_DURATION);
            timerText.text = Mathf.Max(0, TIMER_DURATION - elapsed).ToString("F1");
            yield return null;
        }

        StageManager.Instance.OnTimeOut();
    }
}

