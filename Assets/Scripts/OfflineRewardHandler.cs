using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineRewardHandler : MonoBehaviour
{
    //300,000 : 5분 = 60 * 5 * 1000 / 초 분 ms
    //86,400,000 : 24시간 = 60 * 60 * 24 * 1000 / 초 분 시간 ms

    private const long MIN_OFFLINE_MS = 5L * 60L * 1000L;
    private const long MAX_OFFLINE_MS = 60L * 60L * 24L * 1000L;

    private string[] _spriteKey = new string[2];
    private int[] _amount = new int[2];

    private bool _busy = false;

    private void Start()
    {
        CheckAndGiveRewards().Forget();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        //if (hasFocus)
        //{
        //    CheckAndGiveRewards().Forget();
        //}
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            GameManager.Instance.statSaver.SetLastActiveNowAsync().Forget();
        }
    }

    private async UniTask CheckAndGiveRewards()
    {
        if (_busy == true) { return; }

        _busy = true;

        try
        {
            //서버 시간
            long nowMs = await GameManager.Instance.statSaver.GetServerNowMsAsync();
            //마지막 활동 시간
            long lastMs = await GameManager.Instance.statSaver.GetLastActiveMsAsync();

            //기록이 없거나 이상한 값이 들어오면 기준 갱신
            if (lastMs <= 0 || nowMs <= lastMs)
            {
                ObjectPoolManager.Instance.uiPool.GetMessage().Log($"기록이 없거나, 이상한 값이 들어옴 {lastMs}, {nowMs}, 이상한값 여부 : {nowMs <= lastMs}");
                Debug.LogWarning($"기록이 없거나, 이상한 값이 들어옴 {lastMs}, {nowMs}, 이상한값 여부 : {nowMs <= lastMs}");
                await GameManager.Instance.statSaver.SaveLastActiveMsAsync(nowMs);
                return;
            }

            long offlineMs = nowMs - lastMs;

            //5분 미만일 경우 기준 갱신
            if (offlineMs < MIN_OFFLINE_MS)
            {
                ObjectPoolManager.Instance.uiPool.GetMessage().Log($"방치 시간이 5분 미만임, {offlineMs / 60000.0:F2}");
                Debug.LogWarning($"방치 시간이 5분 미만임, {offlineMs}");
                await GameManager.Instance.statSaver.SaveLastActiveMsAsync(nowMs);
                return;
            }

            //24시간을 넘길경우 고정
            offlineMs = offlineMs > MAX_OFFLINE_MS ? MAX_OFFLINE_MS : offlineMs;

            //분 구하기
            double minutes = offlineMs / 60000.0;
            int offlineMinutes = Convert.ToInt32(minutes);
            int currentStage = StageManager.Instance.currentStage;
            float rewardRate = DataManager.Instance.stageDataTable[currentStage].RewardRate;

            float reward = offlineMinutes * rewardRate;

            //이거 제대로 하려면 오프라인 보상 CSV도 파야할 거 같은데, 일단 하드코딩 \^o^/

            _spriteKey[0] = "UI_Gold";
            _spriteKey[1] = "UI_Exp";

            _amount[0] = (int)reward;
            _amount[1] = (int)reward;

            UIManager.Instance.PopupOpen<UIOfflineRewardPopup>().Init(offlineMinutes, () =>
            {
                GameManager.Instance.stats.GetExp(reward);
                GameManager.Instance.stats.GetGold(reward);

                ObjectPoolManager.Instance.uiPool.GetReward().Init(_spriteKey, _amount);
            }, _spriteKey, _amount);




            ObjectPoolManager.Instance.uiPool.GetMessage().Log($"방치 시간 : {minutes}, 획득한 보상 : {(int)reward}");
            Debug.Log($"방치 시간 : {minutes}, 획득한 보상 : {(int)reward}");

            //중복 지급 방지용 시간 저장
            await GameManager.Instance.statSaver.SaveLastActiveMsAsync(nowMs);
        }
        finally
        {
            _busy = false;
        }
    }
}
