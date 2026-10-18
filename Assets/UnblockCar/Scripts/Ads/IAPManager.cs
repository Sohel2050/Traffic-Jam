using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine.Purchasing;

/// <summary>Premium status (cached on device so it works offline).</summary>
public static class Premium
{
    public static event Action Changed;
    public static bool IsPremium { get { return PlayerPrefs.GetInt("Premium", 0) == 1; } }
    public static void Set(bool value)
    {
        if (IsPremium == value) return;
        PlayerPrefs.SetInt("Premium", value ? 1 : 0);
        PlayerPrefs.Save();
        if (value && AdsManager.Instance != null) AdsManager.Instance.HideBanner();
        if (Changed != null) Changed();
    }
}

/// <summary>
/// Subscriptions (Google Play) with Unity IAP 5: monthly + yearly. Created automatically at startup.
/// Benefits: no ads, unlimited hints, all levels unlocked.
/// </summary>
public class IAPManager : MonoBehaviour
{
    // IDs are edited in Resources/IAPSettings.asset (Inspector)
    public static string Monthly { get { return IAPSettings.Monthly; } }
    public static string Yearly  { get { return IAPSettings.Yearly; } }

    public static IAPManager Instance { get; private set; }
    private StoreController _controller;
    private bool _connected;
    private bool _restoreRequested;
    private bool _buying;
    public bool Ready { get { return _controller != null && _connected; } }
    public event Action<string> OnMessage;
    public event Action OnReady;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        if (Instance != null) return;
        var go = new GameObject("IAPManager");
        DontDestroyOnLoad(go);
        go.AddComponent<IAPManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private async void Start()
    {
        try
        {
            try
            {
                if (UnityServices.State == ServicesInitializationState.Uninitialized)
                    await UnityServices.InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[IAP] Unity Services init skipped: " + e.Message);
            }

            _controller = UnityIAPServices.StoreController();
            Subscribe();
            await _controller.Connect();
            _connected = true;

            var products = new List<ProductDefinition>
            {
                new ProductDefinition(Monthly, ProductType.Subscription),
                new ProductDefinition(Yearly,  ProductType.Subscription)
            };
            _controller.FetchProducts(products);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[IAP] init failed: " + e.Message);
        }
    }

    private void Subscribe()
    {
        _controller.OnStoreDisconnected += f => { _connected = false; Debug.LogWarning("[IAP] store disconnected: " + f); };
        _controller.OnProductsFetched += products =>
        {
            if (OnReady != null) OnReady();
            _controller.FetchPurchases();
        };
        _controller.OnProductsFetchFailed += f => Debug.LogWarning("[IAP] products fetch failed: " + f);
        _controller.OnPurchasesFetched += OnPurchasesFetched;
        _controller.OnPurchasesFetchFailed += f =>
        {
            Debug.LogWarning("[IAP] purchases fetch failed: " + f);
            if (_restoreRequested) { _restoreRequested = false; Say("Could not check purchases. Try again."); }
        };
        _controller.OnPurchasePending += order =>
        {
            if (OrderHasSubscription(order)) Premium.Set(true);
            _controller.ConfirmPurchase(order);
        };
        _controller.OnPurchaseConfirmed += order =>
        {
            _buying = false;
            if (OrderHasSubscription(order)) { Premium.Set(true); Say("Premium unlocked. Thank you!"); }
        };
        _controller.OnPurchaseDeferred += order => { _buying = false; Say("Purchase is waiting for approval."); };
        _controller.OnPurchaseFailed += failed =>
        {
            _buying = false;
            Debug.LogWarning("[IAP] purchase failed: " + failed.FailureReason);
            if (failed.FailureReason != PurchaseFailureReason.UserCancelled) Say("Purchase failed. Please try again.");
        };
    }

    // Google Play only returns subscriptions that are still valid, so a confirmed/pending order = active premium.
    private void OnPurchasesFetched(Orders orders)
    {
        bool active = false;
        foreach (var o in orders.ConfirmedOrders) if (OrderHasSubscription(o)) active = true;
        foreach (var o in orders.PendingOrders)
        {
            if (OrderHasSubscription(o)) active = true;
        }
        Premium.Set(active);
        if (_restoreRequested)
        {
            _restoreRequested = false;
            Say(active ? "Premium restored." : "No active subscription found.");
        }
    }

    private static bool OrderHasSubscription(Order order)
    {
        if (order == null || order.CartOrdered == null) return false;
        foreach (var item in order.CartOrdered.Items())
        {
            if (item == null || item.Product == null) continue;
            string id = item.Product.definition.id;
            if (id == Monthly || id == Yearly) return true;
        }
        return false;
    }

    // ----------------------------------------------------------- public API
    public string Price(string id)
    {
        if (_controller == null) return "";
        var p = _controller.GetProductById(id);
        return p != null ? p.metadata.localizedPriceString : "";
    }

    public void Buy(string id)
    {
        if (!Ready) { Say("Store not ready. Check your internet and try again."); return; }
        if (_buying) return;
        var p = _controller.GetProductById(id);
        if (p == null) { Say("Product not available yet. Try again in a moment."); return; }
        _buying = true;
        _controller.PurchaseProduct(p);
    }

    public void Restore()
    {
        if (!Ready) { Say("Store not ready. Check your internet and try again."); return; }
        _restoreRequested = true;
        _controller.FetchPurchases();
    }

    private void Say(string m) { if (OnMessage != null) OnMessage(m); }
}

/// <summary>Premium (paywall) popup built at runtime.</summary>
public static class PremiumPopup
{
    private static GameObject _ui;
    private static Text _msg;
    private static readonly Color Dark = new Color(0.10f, 0.14f, 0.35f);
    private static readonly Color Cream = new Color(1f, 0.97f, 0.88f);
    private static readonly Color Orange = new Color(1f, 0.62f, 0.12f);
    private static readonly Color Green = new Color(0.25f, 0.72f, 0.35f);

    private static Text _monthlyTxt, _yearlyTxt;

    public static void Show()
    {
        if (_ui != null) return;
        _ui = new GameObject("PremiumPopup", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        UnityEngine.Object.DontDestroyOnLoad(_ui);
        var cv = _ui.GetComponent<Canvas>(); cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 5000;
        var sc = _ui.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1080, 1920);
        sc.matchWidthOrHeight = 0.5f;

        var dim = Box("Dim", _ui.transform, new Color(0, 0, 0, 0.75f));
        Stretch((RectTransform)dim.transform);
        var outline = Box("Outline", _ui.transform, Dark); Center((RectTransform)outline.transform, 940, 1340, 0);
        var card = Box("Card", outline.transform, Cream);
        var cr = (RectTransform)card.transform; Stretch(cr); cr.offsetMin = new Vector2(8, 8); cr.offsetMax = new Vector2(-8, -8);

        // layout measured from the card bottom (card is 1324 high)
        Label(card.transform, "GO PREMIUM", 84, FontStyle.Bold, 800, 1130, 130);
        Label(card.transform, "No ads\nUnlimited hints\nAll levels unlocked", 54, FontStyle.Bold, 800, 790, 280);

        var mb = MakeButton(card.transform, "", 780, 190, 560, Orange, () => { if (IAPManager.Instance != null) IAPManager.Instance.Buy(IAPManager.Monthly); });
        var yb = MakeButton(card.transform, "", 780, 190, 340, Green, () => { if (IAPManager.Instance != null) IAPManager.Instance.Buy(IAPManager.Yearly); });
        _monthlyTxt = mb.GetComponentInChildren<Text>(); _yearlyTxt = yb.GetComponentInChildren<Text>();
        _monthlyTxt.fontSize = 48; _yearlyTxt.fontSize = 48;
        RefreshPrices();

        _msg = Label(card.transform, "", 36, FontStyle.Normal, 800, 200, 120);
        var restore = MakeButton(card.transform, "RESTORE", 340, 100, 50, new Color(0.35f, 0.5f, 0.9f), () => { if (IAPManager.Instance != null) IAPManager.Instance.Restore(); });
        var close = MakeButton(card.transform, "CLOSE", 340, 100, 50, new Color(0.6f, 0.6f, 0.6f), Close);
        ((RectTransform)restore.transform).anchoredPosition = new Vector2(-190, 50);
        ((RectTransform)close.transform).anchoredPosition = new Vector2(190, 50);

        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OnMessage += OnMsg;
            IAPManager.Instance.OnReady += RefreshPrices;
        }
    }

    // "Locked" stage popup, same look as the paywall, with an Unlock-all (premium) shortcut
    public static void ShowLocked(string starsHave, int starsNeed)
    {
        if (_ui != null) return;
        _ui = new GameObject("LockedPopup", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        UnityEngine.Object.DontDestroyOnLoad(_ui);
        var cv = _ui.GetComponent<Canvas>(); cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 5000;
        var sc = _ui.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1080, 1920);
        sc.matchWidthOrHeight = 0.5f;

        var dim = Box("Dim", _ui.transform, new Color(0, 0, 0, 0.75f));
        Stretch((RectTransform)dim.transform);
        var outline = Box("Outline", _ui.transform, Dark); Center((RectTransform)outline.transform, 900, 900, 0);
        var card = Box("Card", outline.transform, Cream);
        var cr = (RectTransform)card.transform; Stretch(cr); cr.offsetMin = new Vector2(8, 8); cr.offsetMax = new Vector2(-8, -8);

        Label(card.transform, "LOCKED", 84, FontStyle.Bold, 760, 700, 130);
        Label(card.transform, "You have " + starsHave + " stars", 54, FontStyle.Bold, 760, 560, 90);
        Label(card.transform, "Collect " + starsNeed + " stars to open this stage,\nor unlock everything with Premium.", 40, FontStyle.Normal, 780, 400, 130);

        MakeButton(card.transform, "UNLOCK ALL WITH PREMIUM", 740, 120, 240, Green, () => { Close(); Show(); });
        MakeButton(card.transform, "CLOSE", 340, 100, 70, new Color(0.6f, 0.6f, 0.6f), Close);
    }

    private static void RefreshPrices()
    {
        var m = IAPManager.Instance;
        string mp = m != null ? m.Price(IAPManager.Monthly) : "";
        string yp = m != null ? m.Price(IAPManager.Yearly) : "";
        if (_monthlyTxt != null) _monthlyTxt.text = "MONTHLY" + (string.IsNullOrEmpty(mp) ? "" : "\n" + mp);
        if (_yearlyTxt != null) _yearlyTxt.text = "YEARLY (BEST VALUE)" + (string.IsNullOrEmpty(yp) ? "" : "\n" + yp);
    }

    private static void Close()
    {
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OnMessage -= OnMsg;
            IAPManager.Instance.OnReady -= RefreshPrices;
        }
        UnityEngine.Object.Destroy(_ui); _ui = null;
    }

    private static void OnMsg(string m)
    {
        if (_msg != null) _msg.text = m;
        if (Premium.IsPremium) Close();
    }

    // ------------------------------------------------------------- helpers
    private static Image Box(string n, Transform p, Color c)
    {
        var g = new GameObject(n, typeof(RectTransform), typeof(Image)); g.transform.SetParent(p, false);
        var i = g.GetComponent<Image>(); i.color = c; return i;
    }
    private static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
    private static void Center(RectTransform r, float w, float h, float y)
    { r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f); r.sizeDelta = new Vector2(w, h); r.anchoredPosition = new Vector2(0, y); }

    // y is measured from the card bottom
    private static Text Label(Transform p, string s, int size, FontStyle st, float w, float yFromBottom, float h)
    {
        var g = new GameObject("Label", typeof(RectTransform), typeof(Text)); g.transform.SetParent(p, false);
        var r = (RectTransform)g.transform;
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0f); r.sizeDelta = new Vector2(w, h); r.anchoredPosition = new Vector2(0, yFromBottom);
        var t = g.GetComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = s; t.fontSize = size; t.fontStyle = st; t.alignment = TextAnchor.MiddleCenter; t.color = Dark; t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    private static Button MakeButton(Transform p, string s, float w, float h, float yFromBottom, Color c, UnityEngine.Events.UnityAction click)
    {
        var img = Box("Btn", p, c);
        var r = (RectTransform)img.transform;
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0f); r.sizeDelta = new Vector2(w, h);
        r.anchoredPosition = new Vector2(0, yFromBottom);
        var b = img.gameObject.AddComponent<Button>(); b.targetGraphic = img; b.onClick.AddListener(click);
        var t = Label(img.transform, s, 54, FontStyle.Bold, w, 0, h);
        t.color = Color.white;
        var tr = (RectTransform)t.transform; tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
        return b;
    }
}
