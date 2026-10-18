using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

// Put this file in  Assets/Editor/  then use the menu:  Tools > Traffic Jam > Apply Player Settings
public static class ApplyPlayerSettings
{
    // ---- edit these if needed ----
    const string Company        = "Albonik LLC";
    const string Product        = "Traffic Jam";
    const string VersionName    = "1.0.2";
    const int    VersionCode    = 2;            // must be higher than the last upload on Play Console
    const string PackageName    = "com.albonik.trafficjam";
    const string KeystorePath   = "/Volumes/Extra/Coded/keys/Bonik/boniktrafficjam.keystore";
    const string KeyAlias       = "boniktrafficjam";
    // --------------------------------

    [MenuItem("Tools/Traffic Jam/Apply Player Settings")]
    public static void Apply()
    {
        PlayerSettings.companyName = Company;
        PlayerSettings.productName = Product;
        PlayerSettings.bundleVersion = VersionName;

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageName);
        PlayerSettings.Android.bundleVersionCode = VersionCode;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Android, ApiCompatibilityLevel.NET_Standard);
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });

        // Keystore (passwords are NOT set here: enter them in Player > Publishing Settings)
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = KeystorePath;
        PlayerSettings.Android.keyaliasName = KeyAlias;

        // Default icon
        var logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UnblockCar/Images/logos.png");
        if (logo != null)
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { logo }, IconKind.Application);
        else
            Debug.LogWarning("logos.png not found");

        // Splash logo
        var title = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UnblockCar/Images/title 2.png");
        if (title != null)
            PlayerSettings.SplashScreen.logos = new[] { PlayerSettings.SplashScreenLogo.Create(2f, title) };
        else
            Debug.LogWarning("title 2.png not found");

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Done",
            "Player settings applied.\n\nStill do by hand:\n- Icon > Adaptive: Background = ic_adaptive_bg, Foreground = ic_adaptive_fg\n- Publishing Settings: keystore passwords\n- Active Input Handling = Input Manager (Old)",
            "OK");
    }

    // Run this LAST to test whether Gamma brings back the white boxes.
    [MenuItem("Tools/Traffic Jam/Set Color Space Gamma")]
    public static void SetGamma()
    {
        PlayerSettings.colorSpace = ColorSpace.Gamma;
        AssetDatabase.SaveAssets();
    }
}
