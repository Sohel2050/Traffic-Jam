// All LevelPlay IDs and feature switches live here.
public static class AdsConfig
{
    // ---- LevelPlay IDs ----
    public const string AppKey            = "289f67ced";
    public const string BannerAdUnit      = "6sh1mi0447flwvyu";
    public const string InterstitialAdUnit= "8fhlbnrq7gj4lt6g";
    public const string MrecAdUnit        = "r7e5gdvmporhwarl";
    public const string NativeAdUnit      = "qg96wlh206gz9ko4";
    public const string RewardedAdUnit    = "visgs12g7biz0fzw";

    // ---- Feature switches ----
    // Only Banner is live right now. To enable another format later,
    // just change its flag to true (no other code change needed).
    public const bool EnableBanner       = true;
    public const bool EnableInterstitial = true; 
    public const bool EnableRewarded     = false;
    public const bool EnableMrec         = false;
    public const bool EnableNative       = false; // reserved: needs a native-ad UI layout first

    // Interstitial after finished levels / game over: OFF (only banner + hint interstitials are used)
    public const bool EnableLevelInterstitial = false;
    public const int InterstitialEveryLevels = 3;

    // Show an interstitial every N hint button presses
    public const int InterstitialEveryHints = 3;

    // Hints given after watching an interstitial when the hint counter is at 0 ("AD" button)
    public const int HintsPerInterstitial = 3;
}
