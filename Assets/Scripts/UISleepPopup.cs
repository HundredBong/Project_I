using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class UISleepPopup : UIPopup
{
    [SerializeField] private TextMeshProUGUI _clockText;
    [SerializeField] private TextMeshProUGUI _stageText;
    [SerializeField] private TextMeshProUGUI _batteryText;

    [SerializeField] private Image _batteryFillImage;

    [SerializeField] private Slider _releaseSleepSlider;

    private Coroutine _refreshCoroutine;
    private WaitForSeconds _sleepInterval;
    protected override void Awake()
    {
        base.Awake();

        _releaseSleepSlider.minValue = 0;
        _releaseSleepSlider.maxValue = 1;

        _sleepInterval = new WaitForSeconds(30f);
    }

    public override void Open()
    {
        base.Open();

        //게임매니저에서 슬립모드 활성화 후 열기
        GameManager.Instance.EnterSleepMode();

        if (_refreshCoroutine != null)
        {
            StopCoroutine(_refreshCoroutine);
            _refreshCoroutine = null;
        }

        _refreshCoroutine = StartCoroutine(RefreshCoroutine());

        _releaseSleepSlider.onValueChanged.AddListener(value => CheckReleaseSlider(value));
    }

    public override void Close()
    {
        _releaseSleepSlider.onValueChanged.RemoveListener(value => CheckReleaseSlider(value));

        if (_refreshCoroutine != null)
        {
            StopCoroutine(_refreshCoroutine);
            _refreshCoroutine = null;
        }

        base.Close();
    }

    private void CheckReleaseSlider(float value)
    {
        if (0.95f < value)
        {
            //게임매니저에서 슬립모드 해제후 닫기
            GameManager.Instance.ReleaseSleepMode();
            Close();
        }
    }

    private IEnumerator RefreshCoroutine()
    {
        while (true)
        {
            _clockText.text = DateTime.Now.ToString("HH:mm");
            _stageText.text = $"{StageManager.Instance.currentStage.ToString()} {DataManager.Instance.GetLocalizedText("UI_Stage")}";

            float battery = SystemInfo.batteryLevel;

            if (battery < 0)
            {
                _batteryFillImage.fillAmount = 1f;
                _batteryText.text = $"100%";
            }
            else
            {
                _batteryFillImage.fillAmount = battery;
                _batteryText.text = $"{(battery * 100f).ToString("F0")}%";
            }

            yield return _sleepInterval;
        }
    }

    public void ResetValue()
    {
        _releaseSleepSlider.value = 0;
    }
}
