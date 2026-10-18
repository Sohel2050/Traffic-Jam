using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Reads update_config.json from GitHub.
///  - installed version &lt; min_version    =&gt; FORCE update (cannot be closed)
///  - installed version &lt; latest_version =&gt; optional update (LATER button)
/// Builds its own UI at runtime, so no scene changes are needed.
/// </summary>
public class UpdateChecker : MonoBehaviour
{
    [Serializable]
    private class Cfg
    {
        public string min_version;
        public string latest_version;
        public string update_message;
        public string play_store_url;
    }

    private Cfg _cfg;
    private GameObject _ui;
    private bool _optionalShown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        var go = new GameObject("UpdateChecker");
        DontDestroyOnLoad(go);
        go.AddComponent<UpdateChecker>();
    }

    private void Start() { StartCoroutine(Check()); }

    private void OnApplicationFocus(bool focus)
    {
        // user came back from the store without updating: re-check
        if (focus && _cfg != null && _ui == null) StartCoroutine(Check());
    }

    private IEnumerator Check()
    {
        using (var req = UnityWebRequest.Get(AppLinks.UpdateConfigUrl + "?t=" + DateTime.UtcNow.Ticks))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) yield break; // offline: let the user play

            Cfg cfg = null;
            try { cfg = JsonUtility.FromJson<Cfg>(req.downloadHandler.text); } catch (Exception) { }
            if (cfg == null) yield break;
            _cfg = cfg;
        }

        string cur = Application.version;
        if (!string.IsNullOrEmpty(_cfg.min_version) && Compare(cur, _cfg.min_version) < 0)
            Show(true);
        else if (!_optionalShown && !string.IsNullOrEmpty(_cfg.latest_version) && Compare(cur, _cfg.latest_version) < 0)
        {
            _optionalShown = true;
            Show(false);
        }
    }

    private static int Compare(string a, string b)
    {
        string[] pa = a.Trim().Split('.'), pb = b.Trim().Split('.');
        int n = Math.Max(pa.Length, pb.Length);
        for (int i = 0; i < n; i++)
        {
            int x = 0, y = 0;
            if (i < pa.Length) int.TryParse(pa[i], out x);
            if (i < pb.Length) int.TryParse(pb[i], out y);
            if (x != y) return x < y ? -1 : 1;
        }
        return 0;
    }

    // ------------------------------------------------------------------ UI
    private static readonly Color Dark = new Color32(74, 29, 107, 255);
    private static readonly Color Cream = new Color32(255, 246, 220, 255);
    private static readonly Color Yellow = new Color32(255, 199, 44, 255);
    private static readonly Color Pink = new Color32(255, 95, 162, 255);
    private static Sprite _rounded;

    private static Sprite Rounded()
    {
        if (_rounded != null) return _rounded;
        const int n = 64, r = 24;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = Mathf.Max(Mathf.Max(r - x - 0.5f, x + 0.5f - (n - r)), 0f);
                float dy = Mathf.Max(Mathf.Max(r - y - 0.5f, y + 0.5f - (n - r)), 0f);
                float a = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        tex.Apply();
        _rounded = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        return _rounded;
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>();
        r.sizeDelta = size;
        r.anchoredPosition = pos;
        return r;
    }

    private static Image Box(string name, Transform parent, Vector2 size, Vector2 pos, Color c)
    {
        var im = Rect(name, parent, size, pos).gameObject.AddComponent<Image>();
        im.sprite = Rounded();
        im.type = Image.Type.Sliced;
        im.color = c;
        return im;
    }

    private static Text Label(Transform parent, string s, int size, Vector2 box, Vector2 pos, FontStyle st)
    {
        var t = Rect("Text", parent, box, pos).gameObject.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = s;
        t.fontSize = size;
        t.fontStyle = st;
        t.color = Dark;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    private static void MakeButton(Transform parent, string label, Vector2 size, Vector2 pos, Color c, UnityEngine.Events.UnityAction onClick)
    {
        Box("BtnShadow", parent, size + new Vector2(0, 0), pos + new Vector2(0, -10), Dark);
        var img = Box("Button", parent, size, pos, c);
        var b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(onClick);
        Label(img.transform, label, 52, size, Vector2.zero, FontStyle.Bold);
    }

    private void Show(bool force)
    {
        if (_ui != null) Destroy(_ui);
        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }

        _ui = new GameObject("UpdateDialog", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        DontDestroyOnLoad(_ui);
        var canvas = _ui.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;
        var scaler = _ui.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        var dim = Rect("Dim", _ui.transform, Vector2.zero, Vector2.zero);
        dim.anchorMin = Vector2.zero; dim.anchorMax = Vector2.one; dim.offsetMin = Vector2.zero; dim.offsetMax = Vector2.zero;
        var dimImg = dim.gameObject.AddComponent<Image>();
        dimImg.color = new Color(0, 0, 0, 0.78f); // blocks touches behind

        float h = force ? 640f : 780f;
        Box("CardOutline", _ui.transform, new Vector2(912, h + 12), Vector2.zero, Dark);
        var card = Box("Card", _ui.transform, new Vector2(900, h), Vector2.zero, Cream);

        string title = force ? "UPDATE REQUIRED" : "NEW UPDATE";
        string msg = string.IsNullOrEmpty(_cfg.update_message)
            ? "A new version is available with bug fixes and improvements."
            : _cfg.update_message;
        float top = h / 2f;
        Label(card.transform, title, 66, new Vector2(820, 100), new Vector2(0, top - 90), FontStyle.Bold);
        Label(card.transform, msg, 42, new Vector2(780, 240), new Vector2(0, top - 290), FontStyle.Normal);

        string url = string.IsNullOrEmpty(_cfg.play_store_url) ? AppLinks.GamePage : _cfg.play_store_url;
        MakeButton(card.transform, "UPDATE NOW", new Vector2(640, 130), new Vector2(0, -top + (force ? 150f : 270f)), Yellow,
            () => Application.OpenURL(url));
        if (!force)
            MakeButton(card.transform, "LATER", new Vector2(360, 100), new Vector2(0, -top + 105f), Pink,
                () => { Destroy(_ui); _ui = null; });
    }
}
