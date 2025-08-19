using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    //전면 광고 객체
    private InterstitialAd _interstitial;
    private RewardedAd _rewarded;
    private bool _hasShownThisLaunch = false; //앱 시작 시 광고를 보여줬는지 여부
    private bool _rewardEarned = false; //광고에서 보상을 받았는지 여부
    private CancellationTokenSource _cts;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        MobileAds.RaiseAdEventsOnUnityMainThread = true; //광고 이벤트를 Unity 메인 스레드에서 발생시키도록 설정

        //광고 SDK 초기화
        MobileAds.Initialize(_ =>
        {
            Debug.Log("[AdManager] Google Mobile Ads 초기화 완료");
        });
    }

    private void OnEnable()
    {
        _cts = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    public void ShowLaunchInterstitialOnce()
    {
        if (_hasShownThisLaunch)
        {
            return;
        }

        RunLaunchInterstitialFlowAsync(_cts.Token).Forget();
    }

    private async UniTaskVoid RunLaunchInterstitialFlowAsync(CancellationToken ct)
    {
#if UNITY_EDITOR
        ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] 에디터에서 전면 광고 스킵");
        Debug.Log("[AdManager] 에디터에서 전면 광고 스킵");
        ObjectPoolManager.Instance.audioPool.GetAudio().PlaySFX("GameStart");
        _hasShownThisLaunch = true;
        GameManager.Instance.loadSceneReady = true;
        return;
#elif UNITY_ANDROID || UNITY_IOS
        await WaitUntilConsentReadyAsync(10000, ct);

        bool loaded = await LoadInterstitialAsync(ct);

        if (loaded == false)
        {
            Debug.LogWarning("[AdManager] 전면 광고 로드 실패");
            _hasShownThisLaunch = true;
            return;
        }

        await UniTask.Delay(300, cancellationToken: ct);

        ShowInterstitialOnce();
#endif
    }

    private async UniTask WaitUntilConsentReadyAsync(int timeoutMs, CancellationToken ct)
    {
        ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] UMP 상태 업데이트중");
        //광고 보여주기전에 동의 처리 해주는 함수
        //Google UMP 사용함

        //GoogleMobileAds.Ump.Api
        ConsentRequestParameters requestParams = new ConsentRequestParameters()
        {
            //만 13세와 미만 같은 어린 사용자 여부, true면 맞춤형 광고 제한 걸림
            TagForUnderAgeOfConsent = false,
            ConsentDebugSettings = new ConsentDebugSettings()
            {
                //테스트용으로 사용
                DebugGeography = DebugGeography.EEA,//유럽 경제 지역(European Economic Area)으로 설정
                TestDeviceHashedIds = new List<string>()
                {
                    "A58D6EFF70C15B53A410BFEC76E0DE17"
                }
            }
        };

        //비동기 결과를 나중에 알려주도록
        UniTaskCompletionSource<bool> tcsUpdate = new UniTaskCompletionSource<bool>();
        ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] tcsUpdate 생성");

        //Google 서버나 로컬 저장된 데이터 기반으로 동의 상태 최신화, 
        ConsentInformation.Update(requestParams, (FormError updateError) =>
        {
            ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] ConsentInformation 업데이트중");

            if (updateError != null)
            {
                ObjectPoolManager.Instance.uiPool.GetMessage().Log($"[AdManager] UMP 상태 업데이트 실패 : {updateError.Message}");

                Debug.LogWarning($"[AdManager] UMP 상태 업데이트 실패 : {updateError.Message}");
            }
            //에러가 없다면 상태 업데이트 성공
            else
            {
                ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] ConsentInformation 업데이트 성공");

                tcsUpdate.TrySetResult(true);
            }
        });

        //IDisposable
        using (ct.Register(() => tcsUpdate.TrySetCanceled()))
        {
            //업데이트 요청이 완료될 때까지 대기
            await tcsUpdate.Task;
        }

        //업데이트가 완료될 때까지 대기
        ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] tcsUpdate 대기 완료");


        if (ConsentInformation.IsConsentFormAvailable())
        {
            UniTaskCompletionSource<ConsentForm> tcsLoad = new UniTaskCompletionSource<ConsentForm>();

            ConsentForm.Load((ConsentForm form, FormError loadError) =>
            {
                if (loadError != null || form == null)
                {
                    ObjectPoolManager.Instance.uiPool.GetMessage().LogError($"[AdManager] UMP 폼 로드 실패함 : {loadError.Message}");
                    Debug.LogWarning($"[AdManager] UMP 폼 로드 실패함 : {loadError.Message}");
                    tcsLoad.TrySetException(new System.Exception(loadError.Message ?? "ConsetnForm is null"));
                    return;
                }
                ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] consentForm Show 실행 전");
                tcsLoad.TrySetResult(form);
            });

            ConsentForm loadedForm;

            //중간에 취소될 수 있도록 CancellationToken을 사용하여 대기
            //ct가 취소되면 tcsLoad를 취소 처리
            using (ct.Register(() => tcsLoad.TrySetCanceled()))
            {
                //결과가 SetResult되거나 취소되거나 예외가 발생할 때까지 대기
                loadedForm = await tcsLoad.Task;
            }

            UniTaskCompletionSource<bool> tcsShow = new UniTaskCompletionSource<bool>();

            loadedForm.Show((FormError showError) =>
            {
                //폼이 닫히면 이 콜백이 호출됨

                if (showError != null)
                {
                    ObjectPoolManager.Instance.uiPool.GetMessage().LogError($"[AdManager] UMP 폼 표시 실패 : {showError.Message}");
                    Debug.LogWarning($"[AdManager] UMP 폼 표시 실패 : {showError.Message}");
                    tcsShow.TrySetResult(false);
                    return;
                }

                ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] UMP 폼 표시 완료");
                tcsShow.TrySetResult(true);
            });

            using (ct.Register(() => tcsShow.TrySetCanceled()))
            {
                bool showOk = await tcsShow.Task;

                var status = ConsentInformation.ConsentStatus;

                //switch (status)
                //{
                //    case ConsentStatus.Unknown: //초기상태
                //        break;
                //    case ConsentStatus.Required: //동의가 필요한 상태
                //        ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] 동의가 필요함");
                //        UIManager.Instance.PopupOpen<UIConfirmPopup>().Init(() =>
                //        {
                //            Application.Quit();
                //        }, "UI_ConsentConfirm");
                //        break;
                //    case ConsentStatus.NotRequired: //해당 지역, 상황에서는 동의 절차가 필요하지 않음
                //        break;
                //    case ConsentStatus.Obtained: //동의가 완료된 상태
                //        break;
                //    default:
                //        break;

                //}
            }
        }
        else
        {
            ObjectPoolManager.Instance.uiPool.GetMessage().LogError($"[AdManager] UMP 폼 표시 가능한 상태가 아님");
        }

        int waited = 0;
        const int step = 200;

        //최대 10초까지 200ms 간격으로 ConsentInformation.CanRequestAds()가 true가 될 때까지 대기
        while (ConsentInformation.CanRequestAds() == false && waited < timeoutMs && ct.IsCancellationRequested == false)
        {
            await UniTask.Delay(step, cancellationToken: ct);
            waited += step;
        }
    }

    private async UniTask<bool> LoadInterstitialAsync(CancellationToken ct)
    {
        if (_interstitial != null)
        {
            return true;
        }

        UniTaskCompletionSource<bool> tcs = new UniTaskCompletionSource<bool>();

        //플랫폼에 맞는 광고 유닛 Id 리턴, 테스트용 광고 유닛 Id 사용중
        string adUnitId = GetInterstitialUnitId();

        //광고 요청 옵션
        AdRequest request = new AdRequest();

        //Load로 전면 광고 로드
        InterstitialAd.Load(adUnitId, request, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogWarning($"[AdManager] 전면 광고 로드 실패: {error?.GetMessage()}");
                tcs.TrySetResult(false);
                return;
            }

            //에러가 없다면 광고 객체를 저장함, 전면 광고는 소모품이고 한 번 보여주면 끝이라서 저장해줘야 함
            _interstitial = ad;

            //전면 광고 이벤트 핸들러 등록
            _interstitial.OnAdFullScreenContentClosed += () =>
            {
                ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] 전면 광고 닫힘");
                GameManager.Instance.loadSceneReady = true;
                ObjectPoolManager.Instance.audioPool.GetAudio().PlaySFX("GameStart");
                _interstitial = null;
            };

            _interstitial.OnAdFullScreenContentFailed += (AdError adError) =>
            {
                ObjectPoolManager.Instance.uiPool.GetMessage().LogError($"[AdManager] 전면 광고 실패: {adError.GetMessage()}");
                GameManager.Instance.loadSceneReady = true;
                ObjectPoolManager.Instance.audioPool.GetAudio().PlaySFX("GameStart");
                _interstitial = null;
            };

            ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] 전면 광고 로드 성공");
            tcs.TrySetResult(true);
        });

        using (ct.Register(() => tcs.TrySetResult(false)))
        {
            return await tcs.Task;
        }
    }

    private async UniTask<bool> LoadRewardedAsync(CancellationToken ct)
    {
        if (_rewarded != null)
        {
            return true;
        }

        UniTaskCompletionSource<bool> tcs = new UniTaskCompletionSource<bool>();

        string adUnitId = GetRewardedUnitId(); //테스트용 광고 유닛 Id 사용중

        AdRequest request = new AdRequest();

        RewardedAd.Load(adUnitId, request, (RewardedAd ad, LoadAdError error) =>
        {
            //에러가 있거나 광고가 null이면 false 반환
            if (error != null || ad == null)
            {
                ObjectPoolManager.Instance.uiPool.GetMessage().LogError($"[AdManager] 보상형 광고 로드 실패: {error.GetMessage()}");
                tcs.TrySetResult(false);
            }

            _rewarded = ad;

            //광고는 소모품이므로 한 번 보여주면 폐기하고 다시 로드해야 함
            _rewarded.OnAdFullScreenContentClosed += () =>
            {
                ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] 보상형 광고 닫힘");
                _rewarded = null;
            };

            //
            _rewarded.OnAdFullScreenContentFailed += (AdError adError) =>
            {
                ObjectPoolManager.Instance.uiPool.GetMessage().LogError($"[AdManager] 보상형 광고 실패: {adError.GetMessage()}");
                _rewarded = null;
            };

            ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] 보상형 광고 로드 성공");
            tcs.TrySetResult(true);
        });

        using (ct.Register(() => tcs.TrySetCanceled()))
        {
            return await tcs.Task;
        }
    }

    public async UniTask<bool> ShowRewardedAsync(CancellationToken ct)
    {
#if UNITY_EDITOR
        ObjectPoolManager.Instance.uiPool.GetMessage().Log("[AdManager] 에디터에서 보상형 광고 스킵");
        Debug.Log("[AdManager] 에디터에서 보상형 광고 스킵");
        await UniTask.Yield();
        return true;
#elif UNITY_ANDROID || UNITY_IOS
        //광고 불러오기
        bool loaded = await LoadRewardedAsync(ct);

        if (loaded == false || _rewarded == null)
        {
            ObjectPoolManager.Instance.uiPool.GetMessage().LogError("[AdManager] 보상형 광고를 표시할 수 없음");
            Debug.LogWarning("[AdManager] 보상형 광고를 표시할 수 없음");
            return false;
        }

        _rewardEarned = false;

        UniTaskCompletionSource<bool> tcsShow = new UniTaskCompletionSource<bool>();

        //보상 콜백, 광고가 성공적으로 보여지고 사용자가 광고를 끝까지 시청했을 때 호출됨
        _rewarded.Show((Reward reward) =>
        {
            _rewardEarned = true;
        });

        _rewarded.OnAdFullScreenContentClosed += () =>
        {
            tcsShow.TrySetResult(_rewardEarned);
        };

        _rewarded.OnAdFullScreenContentFailed += (AdError adError) =>
        {
            ObjectPoolManager.Instance.uiPool.GetMessage().LogError($"[AdManager] 보상형 광고 실패: {adError.GetMessage()}");
            tcsShow.TrySetResult(false);
        };

        using (ct.Register(() => tcsShow.TrySetCanceled()))
        {
            //광고가 끝날 때까지 대기
            return await tcsShow.Task;
        }
#endif
    }

    private void ShowInterstitialOnce()
    {
        if (_hasShownThisLaunch)
        {
            return;
        }

        if (_interstitial == null || _interstitial.CanShowAd() == false)
        {
            Debug.Log("[AdManager] 전면 광고를 표시할 수 없음");
            _hasShownThisLaunch = true;
            return;
        }

        _hasShownThisLaunch = true;
        _interstitial.Show();
    }

    private string GetInterstitialUnitId()
    {
#if UNITY_ANDROID
        return "ca-app-pub-3940256099942544/1033173712"; //테스트 전면 광고
#elif UNITY_IOS
    return "ca-app-pub-3940256099942544/4411468910"; //테스트 전면 광고
#else
    return "unexpected_platform";
#endif
    }

    private string GetRewardedUnitId()
    {
#if UNITY_ANDROID
        return "ca-app-pub-3940256099942544/5224354917"; //테스트 보상형 광고
#elif UNITY_IOS
        return "ca-app-pub-3940256099942544/1712485313";
#else
        return "unexpected_platform";
#endif
    }

    private void ResetUMP()
    {
#if UNITY_ANDROID || UNITY_IOS
        ConsentInformation.Reset();
#endif
    }
}
