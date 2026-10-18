using UnityEngine;

/// <summary>Store / update links in one place.</summary>
public static class AppLinks
{
    // Your developer page (More Games)
    public const string DeveloperPage = "https://play.google.com/store/apps/dev?id=5662539472191775885";

    // Force-update config hosted on GitHub (public repo)
    public const string UpdateConfigUrl =
        "https://raw.githubusercontent.com/Sohel2050/Traffic-Jam-Version-Controller/main/update_config.json";

    // Optional: host privacy_policy.html (e.g. GitHub Pages) and paste its URL here.
    // Empty = the Privacy Policy button shows the built-in policy popup.
    public const string PrivacyPolicyUrl = "";

    // This game's Play Store page (uses the real package name automatically)
    public static string GamePage
    {
        get { return "https://play.google.com/store/apps/details?id=" + Application.identifier; }
    }
}
