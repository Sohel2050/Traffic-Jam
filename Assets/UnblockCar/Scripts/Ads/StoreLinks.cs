using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds the home screen at runtime (no scene edits):
///   top: Settings | Title, Ribbon
///   middle: parking board art      bottom: PLAY, then MORE | RATE | PRIVACY
/// Also hides the MUSIC button (background music removed).
/// </summary>
public class StoreLinks : MonoBehaviour
{
    private readonly HashSet<int> _done = new HashSet<int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        var go = new GameObject("StoreLinks");
        DontDestroyOnLoad(go);
        go.AddComponent<StoreLinks>();
    }

    private IEnumerator Start()
    {
        yield return null;
        yield return null;
        Setup();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    private void OnSceneLoaded(Scene s, LoadSceneMode m) { StartCoroutine(Delayed()); }
    private IEnumerator Delayed() { yield return null; yield return null; Setup(); }

    private void Setup()
    {
        Button gear = null, play = null;
        foreach (var b in Resources.FindObjectsOfTypeAll<Button>())
        {
            if (b == null || !b.gameObject.scene.IsValid()) continue;
            var img = b.targetGraphic as Image;
            if (img == null || img.sprite == null) continue;
            int id = b.GetInstanceID();
            if (_done.Contains(id)) continue;
            string n = img.sprite.name;
            if (n.StartsWith("music_")) { _done.Add(id); b.gameObject.SetActive(false); }
            else if (n.StartsWith("btn_setting")) gear = b;
            else if (n.StartsWith("btn_play")) play = b;
        }
        if (gear != null && play != null) BuildHome(gear, play);
    }

    private static RectTransform Anchor(RectTransform rt, Vector2 anchor, Vector2 size, Vector2 pos)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        rt.localScale = Vector3.one;
        return rt;
    }

    private Image Art(RectTransform menu, string name, string sprite, Vector2 anchor, Vector2 size, Vector2 pos)
    {
        var sp = Resources.Load<Sprite>(sprite);
        if (sp == null) return null;
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(menu, false);
        Anchor((RectTransform)go.transform, anchor, size, pos);
        var im = go.GetComponent<Image>();
        im.sprite = sp; im.preserveAspect = true; im.raycastTarget = false;
        return im;
    }

    private Button Clone(Button src, string name, string sprite, Vector2 anchor, Vector2 size, Vector2 pos, UnityAction click)
    {
        var clone = Instantiate(src.gameObject, src.transform.parent, false);
        clone.name = name;
        var btn = clone.GetComponent<Button>();
        for (int i = 0; i < btn.onClick.GetPersistentEventCount(); i++)
            btn.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(click);
        _done.Add(btn.GetInstanceID());
        var sp = Resources.Load<Sprite>(sprite);
        var img = btn.targetGraphic as Image;
        if (sp != null && img != null) { img.sprite = sp; img.preserveAspect = true; }
        Anchor((RectTransform)clone.transform, anchor, size, pos);
        return btn;
    }

    private static GameObject _overlay;
    private static RectTransform _menu;
    private static Canvas _cv;
    private static int _rootOrder;

    private static bool AnyPopupOpen()
    {
        var pm = PopUpFrameWork.PopupManager.instance;
        if (pm == null) return false;
        if (pm.popupInfos != null)
            foreach (var pi in pm.popupInfos)
                if (pi != null && pi.popup != null && pi.popup.gameObject.activeInHierarchy) return true;
        if (pm.PopupLock != null && pm.PopupLock.activeInHierarchy) return true;
        if (pm.PopupGDPR != null && pm.PopupGDPR.activeInHierarchy) return true;
        return false;
    }

    private void Update()
    {
        if (_overlay == null || _menu == null) return;
        // all game screens stay active (hidden by alpha), so ask the ScreenManager which one is really showing
        bool on = _menu.gameObject.activeInHierarchy
                  && ScreenFrameWork.ScreenManager.Exists() && ScreenFrameWork.ScreenManager.Instance.MainScreen;
        if (_overlay.activeSelf != on) _overlay.SetActive(on);
        if (on && _cv != null)
        {
            // above the game canvas so every tap reaches our buttons; drops behind while a game popup is open
            int want = AnyPopupOpen() ? _rootOrder - 1 : _rootOrder + 1;
            if (_cv.sortingOrder != want) _cv.sortingOrder = want;
        }
    }

    // Home screen = its own full-screen canvas, fixed 540x1200 layout (same as the reference), scaled to fit any phone.
    private void BuildHome(Button gear, Button play)
    {
        _done.Add(gear.GetInstanceID());
        _done.Add(play.GetInstanceID());
        var menu = gear.transform.parent as RectTransform;
        if (menu == null || _overlay != null) return;
        var rootCanvas = menu.GetComponentInParent<Canvas>().rootCanvas;
        _menu = menu;

        _overlay = new GameObject("HomeOverlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var cv = _overlay.GetComponent<Canvas>();
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceCamera && rootCanvas.worldCamera != null)
        {
            cv.renderMode = RenderMode.ScreenSpaceCamera; cv.worldCamera = rootCanvas.worldCamera; cv.planeDistance = rootCanvas.planeDistance;
        }
        else cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingLayerID = rootCanvas.sortingLayerID;
        _cv = cv; _rootOrder = rootCanvas.sortingOrder;
        cv.sortingOrder = _rootOrder + 1;
        var sc = _overlay.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(540f, 1200f);
        sc.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        var root = (RectTransform)_overlay.transform;
        // background fills the whole screen
        var bgImg = Layer(root, "BG", "home_bg");
        if (bgImg != null)
        {
            var r = bgImg.rectTransform; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            bgImg.preserveAspect = false;
        }
        // 540x1200 design area, centered
        var cgo = new GameObject("Content", typeof(RectTransform));
        cgo.transform.SetParent(root, false);
        var content = (RectTransform)cgo.transform;
        content.anchorMin = content.anchorMax = new Vector2(0.5f, 0.5f);
        content.sizeDelta = new Vector2(540f, 1200f);
        content.anchoredPosition = new Vector2(0f, 14f); // a little up: leaves room for the banner

        Place(content, "Title", "home_title", 270, 232, 420, 189, null);
        Place(content, "Ribbon", "home_ribbon", 270, 348, 280, 50, null);
        Place(content, "Board", "home_board", 270, 600, 480, 480, null);
        Place(content, "Settings", "home_settings", 68, 84, 70, 76, () => gear.onClick.Invoke());
        Place(content, "Play", "home_play", 270, 880, 360, 130, () => play.onClick.Invoke());
        Place(content, "More", "home_more", 160, 1030, 80, 108, () => Application.OpenURL(AppLinks.DeveloperPage));
        Place(content, "Rate", "home_rate", 270, 1030, 80, 108, () => Application.OpenURL(AppLinks.GamePage));
        Place(content, "Privacy", "home_privacy", 380, 1030, 80, 108, () => OpenPrivacy());

        // PREMIUM button (top-right), hidden once subscribed
        var pgo = new GameObject("PremiumButton", typeof(RectTransform), typeof(Image), typeof(Button));
        pgo.transform.SetParent(content, false);
        var prt = (RectTransform)pgo.transform;
        prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f); prt.sizeDelta = new Vector2(170f, 56f);
        prt.anchoredPosition = new Vector2(440f - 270f, 600f - 84f);
        var pgImg = pgo.GetComponent<Image>(); pgImg.color = new Color(1f, 0.62f, 0.12f);
        pgo.GetComponent<Button>().targetGraphic = pgImg;
        pgo.GetComponent<Button>().onClick.AddListener(() => PremiumPopup.Show());
        var tgo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        tgo.transform.SetParent(pgo.transform, false);
        var tr = (RectTransform)tgo.transform; tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
        var t = tgo.GetComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.text = "PREMIUM"; t.fontStyle = FontStyle.Bold;
        t.fontSize = 26; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white; t.raycastTarget = false;
        pgo.SetActive(!Premium.IsPremium);
        Premium.Changed += () => { if (pgo != null) pgo.SetActive(!Premium.IsPremium); };

        _overlay.SetActive(false); // Update() shows it while the main screen is the active screen
    }

    private static Image Layer(RectTransform parent, string name, string sprite)
    {
        var sp = Resources.Load<Sprite>(sprite);
        if (sp == null) return null;
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var im = go.GetComponent<Image>(); im.sprite = sp; im.raycastTarget = false;
        return im;
    }

    // cx, cy = centre in the 540x1200 design (origin top-left)
    private static void Place(RectTransform parent, string name, string sprite, float cx, float cy, float w, float h, UnityAction click)
    {
        var im = Layer(parent, name, sprite);
        if (im == null) return;
        var r = im.rectTransform;
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(w, h);
        r.anchoredPosition = new Vector2(cx - 270f, 600f - cy);
        im.preserveAspect = true;
        if (click != null)
        {
            im.raycastTarget = true;
            var b = im.gameObject.AddComponent<Button>(); b.targetGraphic = im; b.onClick.AddListener(click);
        }
    }

    // ------------------------------------------------------------ privacy
    public static void OpenPrivacy()
    {
        if (!string.IsNullOrEmpty(AppLinks.PrivacyPolicyUrl)) { Application.OpenURL(AppLinks.PrivacyPolicyUrl); return; }
        PrivacyPopup.Show();
    }
}

/// <summary>Shows the total stars earned on the home screen.</summary>
public class HomeStarsText : MonoBehaviour
{
    private Text _t; private float _next;
    public void Init(Text t) { _t = t; Refresh(); }
    private void OnEnable() { Refresh(); }
    private void Update() { if (Time.unscaledTime >= _next) { _next = Time.unscaledTime + 0.5f; Refresh(); } }
    private void Refresh() { if (_t != null && GameManager.instance != null) _t.text = GameManager.instance.GetTotalStarEarnAllMode.ToString(); }
}

/// <summary>Built-in scrollable privacy policy (no URL needed).</summary>
public static class PrivacyPopup
{
    private static GameObject _ui;
    private static Sprite _round;
    private static readonly Color Dark = new Color32(13, 59, 102, 255);
    private static readonly Color Cream = new Color32(255, 252, 240, 255);
    private static readonly Color Orange = new Color32(255, 138, 61, 255);

    private const string Policy =
"PRIVACY POLICY\n" +
"\n" +
"Last updated: October 2026\n" +
"\n" +
"This Privacy Policy explains what information the game \"Traffic Jam\" (the \"Game\", \"we\", \"us\") handles, why, and what choices you have. By using the Game you agree to this policy.\n" +
"\n" +
"1. Information we collect yourself\n" +
"We do not ask you to create an account and we do not collect your name, email address, phone number, contacts, photos, files or precise location. Game data such as your level progress, stars, daily bonus status and sound settings is saved only on your device and is removed when you uninstall the Game or clear its data.\n" +
"\n" +
"2. Information collected automatically by third-party SDKs\n" +
"The Game is free and supported by advertising. It uses Unity LevelPlay (ad mediation) and the SDKs of advertising partners connected through it, which may include AppLovin, Meta Audience Network, Unity Ads, Liftoff (Vungle), InMobi, Chartboost, Mintegral, MobileFuse, Moloco, Ogury, PubMatic, Smaato, Verve and Yandex. When ads are requested or shown, these companies may automatically collect:\n" +
"- Advertising ID (Google Advertising ID) and other device identifiers\n" +
"- IP address and approximate location derived from it\n" +
"- Device and app information (model, manufacturer, OS version, language, screen size, battery and network type, app version)\n" +
"- Ad interaction data (ads requested, viewed, clicked, installs)\n" +
"- Diagnostic and performance data\n" +
"\n" +
"3. How this information is used\n" +
"It is used to deliver, personalise and measure ads, to prevent fraud and abuse, to maintain and improve the Game, and to comply with legal obligations. We do not sell your personal information ourselves and we do not receive your name or contact details from these partners.\n" +
"\n" +
"4. Update check\n" +
"On start the Game downloads a small configuration file (a version number and message) from GitHub to tell you when a new version is available. Your IP address is visible to that server as with any internet request. No personal data is sent by the Game.\n" +
"\n" +
"5. Sharing\n" +
"We do not share personal information with anyone except the advertising and service providers described above, when required by law, or to protect rights and safety. Each partner processes data under its own privacy policy, which you can find on the partner's website.\n" +
"\n" +
"6. Your choices and rights\n" +
"- Open Android Settings > Google > Ads (or Settings > Privacy > Ads) to delete your Advertising ID or opt out of personalised ads.\n" +
"- Depending on where you live (for example the EEA, UK, California and other regions), you may have the right to access, correct, delete or restrict the use of your data, to object to processing, and to withdraw consent. Because we do not hold your personal data, requests about advertising data should be sent to the relevant ad partner; you may also contact us and we will help where we can.\n" +
"- You can stop all data collection by uninstalling the Game.\n" +
"\n" +
"7. Data retention and security\n" +
"Local game data stays on your device until you delete it. Data handled by ad partners is kept according to their own retention policies. We use reasonable measures to protect information, but no method of transmission or storage is completely secure.\n" +
"\n" +
"8. International transfers\n" +
"Ad partners may process data in countries other than your own. They are responsible for protecting it under applicable law.\n" +
"\n" +
"9. Children\n" +
"The Game is intended for a general audience and is not directed to children under 13 (or the minimum age in your country). We do not knowingly collect personal information from children. If you believe a child has provided information, contact us and we will help remove it.\n" +
"\n" +
"10. Permissions\n" +
"The Game uses internet and network-state access (ads and update check) and the Advertising ID permission. It does not use your camera, microphone, contacts, storage or precise location.\n" +
"\n" +
"11. Changes to this policy\n" +
"We may update this policy from time to time. The date at the top shows the latest version, and the updated policy will be available in the Game and on our Google Play listing.\n" +
"\n" +
"12. Contact\n" +
"Developer page on Google Play:\n" +
"play.google.com/store/apps/dev?id=5662539472191775885\n" +
"";

    private static Sprite Rounded()
    {
        if (_round != null) return _round;
        const int n = 64, r = 24;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = Mathf.Max(Mathf.Max(r - x - 0.5f, x + 0.5f - (n - r)), 0f);
                float dy = Mathf.Max(Mathf.Max(r - y - 0.5f, y + 0.5f - (n - r)), 0f);
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f)));
            }
        tex.Apply();
        _round = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        return _round;
    }

    private static RectTransform R(string n, Transform p)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        return (RectTransform)go.transform;
    }

    private static Image Box(string n, Transform p, Color c)
    {
        var im = R(n, p).gameObject.AddComponent<Image>();
        im.sprite = Rounded(); im.type = Image.Type.Sliced; im.color = c;
        return im;
    }

    private static Text Txt(Transform p, string s, int size, FontStyle st, TextAnchor al)
    {
        var t = R("Text", p).gameObject.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = s; t.fontSize = size; t.fontStyle = st; t.color = Dark; t.alignment = al;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    public static void Show()
    {
        if (_ui != null) Object.Destroy(_ui);
        if (UnityEngine.EventSystems.EventSystem.current == null)
            DontDestroy(new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule)));

        _ui = new GameObject("PrivacyPopup", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Object.DontDestroyOnLoad(_ui);
        var canvas = _ui.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 31000;
        var sc = _ui.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1080, 1920); sc.matchWidthOrHeight = 0.5f;

        var dim = R("Dim", _ui.transform);
        dim.anchorMin = Vector2.zero; dim.anchorMax = Vector2.one; dim.offsetMin = dim.offsetMax = Vector2.zero;
        dim.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.75f);

        var outline = Box("Outline", _ui.transform, Dark);
        Fit((RectTransform)outline.transform, 984, 1500);
        var card = Box("Card", outline.transform, Cream);
        var cr = (RectTransform)card.transform;
        cr.anchorMin = Vector2.zero; cr.anchorMax = Vector2.one; cr.offsetMin = new Vector2(6, 6); cr.offsetMax = new Vector2(-6, -6);

        // scroll area
        var scrollGo = R("Scroll", card.transform);
        scrollGo.anchorMin = Vector2.zero; scrollGo.anchorMax = Vector2.one;
        scrollGo.offsetMin = new Vector2(36, 190); scrollGo.offsetMax = new Vector2(-36, -40);
        var viewport = R("Viewport", scrollGo);
        viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one; viewport.offsetMin = viewport.offsetMax = Vector2.zero;
        viewport.gameObject.AddComponent<RectMask2D>();
        viewport.gameObject.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f);
        var content = R("Content", viewport);
        content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(0.5f, 1);
        content.offsetMin = content.offsetMax = Vector2.zero;
        var text = Txt(content, Policy, 38, FontStyle.Normal, TextAnchor.UpperLeft);
        var tr = (RectTransform)text.transform;
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
        var sr = scrollGo.gameObject.AddComponent<ScrollRect>();
        sr.viewport = viewport; sr.content = content; sr.horizontal = false; sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped; sr.scrollSensitivity = 40f;
        content.gameObject.AddComponent<TextHeightSync>().Init(text, content);

        // close button
        var shadow = Box("CloseShadow", card.transform, Dark);
        Pin((RectTransform)shadow.transform, 520, 120, 70 - 8);
        var btnImg = Box("Close", card.transform, Orange);
        Pin((RectTransform)btnImg.transform, 520, 120, 70);
        var b = btnImg.gameObject.AddComponent<Button>(); b.targetGraphic = btnImg;
        b.onClick.AddListener(() => { Object.Destroy(_ui); _ui = null; });
        var label = Txt(btnImg.transform, "CLOSE", 54, FontStyle.Bold, TextAnchor.MiddleCenter);
        var lr = (RectTransform)label.transform; lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = lr.offsetMax = Vector2.zero;
    }

    private static void DontDestroy(GameObject g) { Object.DontDestroyOnLoad(g); }

    private static void Fit(RectTransform r, float w, float h)
    {
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f); r.sizeDelta = new Vector2(w, h); r.anchoredPosition = Vector2.zero;
    }

    private static void Pin(RectTransform r, float w, float h, float bottom)
    {
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0f); r.pivot = new Vector2(0.5f, 0f);
        r.sizeDelta = new Vector2(w, h); r.anchoredPosition = new Vector2(0, bottom);
    }
}

/// <summary>Keeps the scroll content as tall as its text.</summary>
public class TextHeightSync : MonoBehaviour
{
    private Text _text; private RectTransform _content;
    public void Init(Text t, RectTransform c) { _text = t; _content = c; }
    private void LateUpdate()
    {
        if (_text == null || _content == null) return;
        float w = ((RectTransform)_content.parent).rect.width;
        var settings = _text.GetGenerationSettings(new Vector2(w, 0));
        float h = _text.cachedTextGeneratorForLayout.GetPreferredHeight(_text.text, settings) / _text.pixelsPerUnit;
        _content.sizeDelta = new Vector2(0, h + 40);
    }
}
