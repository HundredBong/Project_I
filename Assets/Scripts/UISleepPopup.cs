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

    [SerializeField] private Image _batteryIcon;

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

        ResetValue();

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
                _batteryIcon.sprite = DataManager.Instance.GetSpriteByKey("Batteries_3");
                _batteryText.text = $"100%";
            }
            else
            {
                float percent = battery * 100f;
                _batteryText.text = $"{percent:F0}%";

                if (percent <= 20f)
                {
                    _batteryIcon.sprite = DataManager.Instance.GetSpriteByKey("Batteries_0");
                }
                else if (percent <= 50f)
                {
                    _batteryIcon.sprite = DataManager.Instance.GetSpriteByKey("Batteries_1");
                }
                else if (percent <= 80f)
                {
                    _batteryIcon.sprite = DataManager.Instance.GetSpriteByKey("Batteries_2");
                }
                else
                {
                    _batteryIcon.sprite = DataManager.Instance.GetSpriteByKey("Batteries_3");
                }
            }

            yield return _sleepInterval;
        }
    }

    public void ResetValue()
    {
        //Slider의 Event Trigger로도 실행됨
        _releaseSleepSlider.value = 0;
    }
}
