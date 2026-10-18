#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class IAPSettingsMenu
{
    private const string Path = "Assets/UnblockCar/Resources/IAPSettings.asset";

    // Top menu: Tools > Traffic Jam > IAP Settings  (selects the asset so it shows in the Inspector)
    [MenuItem("Tools/Traffic Jam/IAP Settings")]
    public static void Open()
    {
        var s = AssetDatabase.LoadAssetAtPath<IAPSettings>(Path);
        if (s == null)
        {
            s = ScriptableObject.CreateInstance<IAPSettings>();
            AssetDatabase.CreateAsset(s, Path);
            AssetDatabase.SaveAssets();
        }
        Selection.activeObject = s;
        EditorGUIUtility.PingObject(s);
    }
}
#endif
