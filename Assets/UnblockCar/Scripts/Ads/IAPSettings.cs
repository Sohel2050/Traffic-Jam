using UnityEngine;

/// <summary>
/// Subscription product IDs. Edit in the Unity Inspector:
/// Project window > Assets/UnblockCar/Resources/IAPSettings.asset
/// </summary>
[CreateAssetMenu(fileName = "IAPSettings", menuName = "Traffic Jam/IAP Settings")]
public class IAPSettings : ScriptableObject
{
    [Tooltip("Google Play monthly subscription product ID")]
    public string monthlyId = "alboniktraffic30days";
    [Tooltip("Google Play yearly subscription product ID")]
    public string yearlyId = "alboniktraffic365days";

    private static IAPSettings _inst;
    private static IAPSettings Inst
    {
        get
        {
            if (_inst == null) _inst = Resources.Load<IAPSettings>("IAPSettings");
            if (_inst == null) _inst = CreateInstance<IAPSettings>(); // fallback to defaults
            return _inst;
        }
    }

    public static string Monthly { get { return Inst.monthlyId; } }
    public static string Yearly  { get { return Inst.yearlyId; } }
}
