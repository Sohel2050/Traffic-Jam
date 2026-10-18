using System;
using UnityEngine;
using Unity.Services.LevelPlay;

/// <summary>
/// Unity LevelPlay (ironSource) mediation wrapper.
/// Banner is active; Interstitial / Rewarded / MREC are fully coded but
/// switched off through AdsConfig flags until you enable them.
/// </summary>
public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    private bool _initialized;
    private LevelPlayBannerAd _banner;
    private LevelPlayBannerAd _mrec;
    private LevelPlayInterstitialAd _interstitial;
    private LevelPlayRewardedAd _rewarded;
    private Action<bool> _rewardCallback;
    private bool _rewardEarned;
    private Action _interstitialDone;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed  += OnInitFailed;
        LevelPlay.Init(AdsConfig.AppKey);
        Premium.Changed += OnPremiumChanged;
    }

    private void OnPremiumChanged()
    {
        if (Premium.IsPremium) HideBanner();
        else if (_initialized && AdsConfig.EnableBanner)
        {
            if (_banner == null) CreateBanner(); else _banner.ShowAd();
        }
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        LevelPlay.OnInitSuccess -= OnInitSuccess;
        LevelPlay.OnInitFailed  -= OnInitFailed;
        Premium.Changed -= OnPremiumChanged;
        _banner?.DestroyAd();
        _mrec?.DestroyAd();
        _interstitial?.DestroyAd();
        _rewarded?.DestroyAd();
    }

    // ------------------------------------------------------------------ init
    private void OnInitSuccess(LevelPlayConfiguration config)
    {
        _initialized = true;
        Debug.Log("[Ads] LevelPlay initialized");

        if (AdsConfig.EnableBanner && !Premium.IsPremium) CreateBanner();
        if (AdsConfig.EnableInterstitial) CreateInterstitial();
        if (AdsConfig.EnableRewarded)     CreateRewarded();
        if (AdsConfig.EnableMrec)         CreateMrec();
    }

    private void OnInitFailed(LevelPlayInitError error)
    {
        Debug.LogWarning("[Ads] LevelPlay init failed: " + error);
    }

    // ---------------------------------------------------------------- banner
    private void CreateBanner()
    {
        var bannerConfig = new LevelPlayBannerAd.Config.Builder()
            .SetSize(LevelPlayAdSize.CreateAdaptiveAdSize())
            .SetPosition(LevelPlayBannerPosition.BottomCenter)
            .SetDisplayOnLoad(true)
            .SetRespectSafeArea(true)
            .Build();
        _banner = new LevelPlayBannerAd(AdsConfig.BannerAdUnit, bannerConfig);
        _banner.OnAdLoadFailed += e => Debug.LogWarning("[Ads] Banner load failed: " + e);
        _banner.LoadAd();
    }

    public void ShowBanner() { if (Premium.IsPremium) return; if (AdsConfig.EnableBanner && _banner != null) _banner.ShowAd(); }
    public void HideBanner() { if (_banner != null) _banner.HideAd(); }

    // ------------------------------------------------------------------ MREC
    private void CreateMrec()
    {
        var mrecConfig = new LevelPlayBannerAd.Config.Builder()
            .SetSize(LevelPlayAdSize.MEDIUM_RECTANGLE)
            .SetPosition(LevelPlayBannerPosition.Center)
            .SetDisplayOnLoad(false) // shown manually via ShowMrec()
            .SetRespectSafeArea(true)
            .Build();
        _mrec = new LevelPlayBannerAd(AdsConfig.MrecAdUnit, mrecConfig);
        _mrec.OnAdLoadFailed += e => Debug.LogWarning("[Ads] MREC load failed: " + e);
        _mrec.LoadAd();
    }

    public void ShowMrec() { if (AdsConfig.EnableMrec && _mrec != null) _mrec.ShowAd(); }
    public void HideMrec() { if (_mrec != null) _mrec.HideAd(); }

    // ---------------------------------------------------------- interstitial
    private void CreateInterstitial()
    {
        _interstitial = new LevelPlayInterstitialAd(AdsConfig.InterstitialAdUnit);
        _interstitial.OnAdLoadFailed    += e => Debug.LogWarning("[Ads] Interstitial load failed: " + e);
        _interstitial.OnAdClosed        += i => { _interstitial.LoadAd(); FinishInterstitial(); };
        _interstitial.OnAdDisplayFailed += (i, e) => { _interstitial.LoadAd(); FinishInterstitial(); };
        _interstitial.LoadAd();
    }

    public bool IsInterstitialReady()
    {
        return AdsConfig.EnableInterstitial && _interstitial != null && _interstitial.IsAdReady();
    }

    private void FinishInterstitial()
    {
        var cb = _interstitialDone;
        _interstitialDone = null;
        if (cb != null) cb();
    }

    // Shows an interstitial and calls onDone afterwards (also when no ad is available, so nothing gets stuck).
    public void ShowInterstitial(Action onDone)
    {
        if (Premium.IsPremium || !AdsConfig.EnableInterstitial || _interstitial == null || !_interstitial.IsAdReady())
        {
            if (_interstitial != null && !Premium.IsPremium) _interstitial.LoadAd();
            if (onDone != null) onDone();
            return;
        }
        _interstitialDone = onDone;
        _interstitial.ShowAd();
    }

    public void ShowInterstitial()
    {
        if (Premium.IsPremium || !AdsConfig.EnableInterstitial || _interstitial == null) return;
        if (_interstitial.IsAdReady()) _interstitial.ShowAd();
        else _interstitial.LoadAd();
    }

    // -------------------------------------------------------------- rewarded
    private void CreateRewarded()
    {
        _rewarded = new LevelPlayRewardedAd(AdsConfig.RewardedAdUnit);
        _rewarded.OnAdLoadFailed    += e => Debug.LogWarning("[Ads] Rewarded load failed: " + e);
        _rewarded.OnAdRewarded      += (i, r) => _rewardEarned = true;
        _rewarded.OnAdDisplayFailed += (i, e) => FinishReward(false);
        _rewarded.OnAdClosed        += i => FinishReward(_rewardEarned);
        _rewarded.LoadAd();
    }

    public bool IsRewardedReady()
    {
        return AdsConfig.EnableRewarded && _rewarded != null && _rewarded.IsAdReady();
    }

    public void ShowRewarded(Action<bool> onFinished)
    {
        if (!IsRewardedReady())
        {
            Debug.Log("[Ads] Rewarded not available");
            onFinished?.Invoke(false);
            if (AdsConfig.EnableRewarded && _rewarded != null) _rewarded.LoadAd();
            return;
        }
        _rewardCallback = onFinished;
        _rewardEarned = false;
        _rewarded.ShowAd();
    }

    private void FinishReward(bool earned)
    {
        var cb = _rewardCallback;
        _rewardCallback = null;
        _rewardEarned = false;
        cb?.Invoke(earned);
        if (_rewarded != null) _rewarded.LoadAd();
    }

    // ------------------------------------------------- static convenience API
    public static void Interstitial() { if (Instance != null) Instance.ShowInterstitial(); }
    public static void Interstitial(Action onDone) { if (Instance != null) Instance.ShowInterstitial(onDone); else if (onDone != null) onDone(); }
    public static void Rewarded(Action<bool> cb)
    {
        if (Instance != null) Instance.ShowRewarded(cb); else cb?.Invoke(false);
    }

    private void OnApplicationPause(bool paused)
    {
        if (_initialized) LevelPlay.SetPauseGame(paused);
    }
}
