using ChvjUnityInfra;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using DG.Tweening;

public class AdmobManager : CHSingletonStatic<AdmobManager>
{
    string _bannerAdUnitId = "ca-app-pub-7085378387310828/7475395879";
    string _interstitialAdUnitId = "ca-app-pub-7085378387310828/3699678689";
    string _rewardedAdUnitId = "";

    string _testBannerAdUnitId = "ca-app-pub-3940256099942544/9214589741";
    string _testInterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    string _testTewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";

    BannerView bannerView;
    InterstitialAd interstitialAd;
    RewardedAd rewardedAd;

    AdRequest adRequest = new AdRequest();

    bool _initialize = false;

    //# MobileAds SDK 초기화 완료 여부 (Init 가드용 _initialize 와 별개)
    bool _mobileAdsInitialized = false;

    //# 배너 상태 — 초기화 전 요청 지연 로드 / 중복 생성 방지 / 실패 재시도 카운트
    bool _bannerRequested = false;
    bool _bannerLoaded = false;
    AdPosition _bannerPosition = AdPosition.Bottom;
    int _bannerRetryCount = 0;

    //# 예약된 재시도 트윈 핸들 — 재생성 시 취소해 stale 재로드 방지
    Tween _bannerRetryTween;

    const int MaxBannerRetry = 2;
    const float BannerRetryBaseDelay = 4f;

    public event Action AcquireReward;
    public event Action CloseAD;

    public void Init()
    {
        if (_initialize == true)
            return;

        _initialize = true;

#if UNITY_EDITOR || TEST
        _bannerAdUnitId = _testBannerAdUnitId;
        _interstitialAdUnitId= _testInterstitialAdUnitId;
        _rewardedAdUnitId = _testTewardedAdUnitId;
#endif

        adRequest = new AdRequest(adRequest);

        //# 전체이용가 유틸 앱 — 광고 콘텐츠 등급을 G로 제한 (패밀리 정책 위반 방지)
        //# 아동 전용 앱이 아니므로 child-directed / under-age 는 Unspecified
        RequestConfiguration requestConfiguration = new RequestConfiguration
        {
            MaxAdContentRating = MaxAdContentRating.G,
            TagForChildDirectedTreatment = TagForChildDirectedTreatment.Unspecified,
            TagForUnderAgeOfConsent = TagForUnderAgeOfConsent.Unspecified,
        };
        MobileAds.SetRequestConfiguration(requestConfiguration);

        //# 광고 콜백을 메인 스레드에서 발생시킴 (기본 false — 백그라운드 콜백에서 DOTween 생성·필드 접근 레이스 방지)
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        //# 초기화 완료를 플래그로 추적 — 대기 중이던 배너 요청이 있으면 여기서 로드
        MobileAds.Initialize(initStatus =>
        {
            _mobileAdsInitialized = true;
            Debug.Log("[AdmobManager] MobileAds initialized.");

            if (_bannerRequested == true)
            {
                LoadBanner();
            }
        });
    }

    public void ShowBanner(AdPosition _position)
    {
        _bannerPosition = _position;
        _bannerRequested = true;

        //# 이미 로드되어 표시 중이면 재생성하지 않음 (중복 생성·누수 방지)
        if (_bannerLoaded == true && bannerView != null)
            return;

        //# SDK 초기화 전 호출이면 즉시 로드하지 않고 완료 콜백에서 로드
        if (_mobileAdsInitialized == false)
        {
            Debug.Log("[AdmobManager] Banner requested before MobileAds init; deferring load.");
            return;
        }

        LoadBanner();
    }

    void LoadBanner()
    {
        //# 예약된 재시도가 있으면 취소 (stale DelayedCall 이 새 배너에 이중 LoadAd 하는 것 방지)
        _bannerRetryTween?.Kill();
        _bannerRetryTween = null;

        //# 기존 배너가 있으면 파괴 후 재생성 (중복 인스턴스 누수 방지)
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }

        _bannerLoaded = false;
        _bannerRetryCount = 0;

        bannerView = new BannerView(_bannerAdUnitId, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth), _bannerPosition);
        RegisterBannerEventHandlers(bannerView);
        bannerView.LoadAd(adRequest);
    }

    void RegisterBannerEventHandlers(BannerView ad)
    {
        ad.OnBannerAdLoaded += () =>
        {
            _bannerLoaded = true;
            _bannerRetryCount = 0;
            Debug.Log("[AdmobManager] Banner loaded. size=" +
                      ad.GetWidthInPixels() + "x" + ad.GetHeightInPixels() +
                      " response=" + ad.GetResponseInfo());
        };
        ad.OnBannerAdLoadFailed += OnBannerAdLoadFailed;
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("[AdmobManager] Banner full screen content opened.");
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("[AdmobManager] Banner full screen content closed.");
        };
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("[AdmobManager] Banner recorded an impression.");
        };
        ad.OnAdClicked += () =>
        {
            Debug.Log("[AdmobManager] Banner was clicked.");
        };
        ad.OnAdPaid += (AdValue value) =>
        {
            Debug.Log("[AdmobManager] Banner paid " + value.Value + " " + value.CurrencyCode);
        };
    }

    void OnBannerAdLoadFailed(LoadAdError error)
    {
        _bannerLoaded = false;
        Debug.LogError("[AdmobManager] Banner failed to load with error : " + error);

        //# 재시도 소진 시 중단 (무한 재시도 금지)
        if (_bannerRetryCount >= MaxBannerRetry)
        {
            Debug.LogError("[AdmobManager] Banner retry exhausted (" + MaxBannerRetry + ").");
            return;
        }

        _bannerRetryCount += 1;

        //# 지수 백오프 — 4s, 8s. DOVirtual 은 메인 스레드 tick + timeScale 무시
        float delay = BannerRetryBaseDelay * Mathf.Pow(2f, _bannerRetryCount - 1);
        Debug.Log("[AdmobManager] Banner retry " + _bannerRetryCount + "/" + MaxBannerRetry +
                  " in " + delay + "s.");
        _bannerRetryTween = DOVirtual.DelayedCall(delay, RetryLoadBanner);
    }

    void RetryLoadBanner()
    {
        if (bannerView == null)
            return;

        bannerView.LoadAd(adRequest);
    }

    void LoadInterstitialAd(bool _show = false)
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        InterstitialAd.Load(_interstitialAdUnitId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    interstitialAd = ad;
                    RegisterEventHandlers(interstitialAd);

                    if (_show == true)
                    {
                        interstitialAd.Show();
                    }
                });
    }

    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd() == true)
        {
            interstitialAd.Show();
        }
        else
        {
            LoadInterstitialAd(true);
        }
    }

    void LoadRewardedAd(bool _show = false)
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        RewardedAd.Load(_rewardedAdUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    rewardedAd = ad;
                    RegisterEventHandlers(rewardedAd);

                    if (_show == true)
                    {
                        rewardedAd.Show(RewardHandler);
                    }
                });
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd() == true)
        {
            rewardedAd.Show(RewardHandler);
        }
        else
        {
            LoadRewardedAd(true);
        }
    }

    void RewardHandler(Reward _reward)
    {
        double currencyAmount = _reward.Amount;
        string currencyType = _reward.Type;

        if (AcquireReward != null)
        {
            AcquireReward.Invoke();
        }
    }

    void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");

            if (CloseAD != null)
            {
                CloseAD.Invoke();
            }

            LoadRewardedAd();
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

    void RegisterEventHandlers(InterstitialAd ad)
    {
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");

            if (CloseAD != null)
            {
                CloseAD.Invoke();
            }

            LoadInterstitialAd();
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
        };
    }
}
