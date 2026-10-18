using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PopUpFrameWork;
using TMPro;

// Pause popup: gold-bordered blue ribbon title, subtitle, divider and button labels.
// Everything is built at runtime, so no scene/prefab edit is needed.
// Drop this over Assets/UnblockCar/Scripts/PopUp/PopupPause.cs
public class PopupPause : Popup
{
    private static readonly Color32 Navy  = new Color32(26, 36, 90, 255);
    private static readonly Color32 Blue  = new Color32(47, 111, 219, 255);
    private static readonly Color32 Gold  = new Color32(255, 190, 40, 255);

    private static Sprite roundedSprite;

    private RectTransform ribbon;
    private bool built;

    public override void OnShowing(object[] inData)
    {
        EnsureUI();
        base.OnShowing(inData);
        StopAllCoroutines();
        StartCoroutine(PopIn());
    }

    // ---------------------------------------------------------------- build

    private void EnsureUI()
    {
        if (built) return;
        built = true;

        Transform parent = animContainer != null ? (Transform)animContainer : transform;

        // Ribbon (gold border -> blue body -> shine) straddling the top edge of the card
        var border = MakeImage("PauseRibbon", parent, Gold);
        ribbon = border.rectTransform;
        ribbon.anchorMin = ribbon.anchorMax = new Vector2(0.5f, 1f);
        ribbon.pivot = new Vector2(0.5f, 0.5f);
        ribbon.sizeDelta = new Vector2(520f, 140f);
        ribbon.anchoredPosition = new Vector2(0f, -10f);

        var body = MakeImage("Body", ribbon, Blue);
        Stretch(body.rectTransform, 9f);

        var shine = MakeImage("Shine", ribbon, new Color(1f, 1f, 1f, 0.18f));
        shine.rectTransform.anchorMin = new Vector2(0f, 1f);
        shine.rectTransform.anchorMax = new Vector2(1f, 1f);
        shine.rectTransform.pivot = new Vector2(0.5f, 1f);
        shine.rectTransform.offsetMin = new Vector2(22f, -62f);
        shine.rectTransform.offsetMax = new Vector2(-22f, -16f);

        var title = MakeText("Title", ribbon, "PAUSE", 82f, Color.white, FontStyles.Bold);
        Stretch(title.rectTransform, 0f);
        title.outlineWidth = 0.22f;
        title.outlineColor = Navy;

        // Subtitle + divider
        var sub = MakeText("Subtitle", parent, "Take a short break", 44f, new Color32(26, 36, 90, 200), FontStyles.Bold);
        sub.rectTransform.anchorMin = sub.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        sub.rectTransform.pivot = new Vector2(0.5f, 1f);
        sub.rectTransform.sizeDelta = new Vector2(600f, 60f);
        sub.rectTransform.anchoredPosition = new Vector2(0f, -110f);

        var line = MakeImage("Divider", parent, new Color32(26, 36, 90, 38));
        line.rectTransform.anchorMin = line.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        line.rectTransform.pivot = new Vector2(0.5f, 1f);
        line.rectTransform.sizeDelta = new Vector2(440f, 6f);
        line.rectTransform.anchoredPosition = new Vector2(0f, -185f);

        // Labels under the existing buttons
        AddLabel("Btn_Close", "RESUME");
        AddLabel("Btn_Chapter", "HOME");
    }

    private void AddLabel(string buttonName, string text)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name != buttonName) continue;
            var label = MakeText("Label", t, text, 46f, Navy, FontStyles.Bold);
            var r = label.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0f);
            r.pivot = new Vector2(0.5f, 1f);
            r.sizeDelta = new Vector2(280f, 56f);
            r.anchoredPosition = new Vector2(0f, -6f);
            return;
        }
    }

    // ------------------------------------------------------------ animation

    private IEnumerator PopIn()
    {
        if (ribbon == null) yield break;
        const float dur = 0.35f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            // ease-out-back
            const float c1 = 1.70158f, c3 = c1 + 1f;
            float e = 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
            ribbon.localScale = Vector3.one * Mathf.LerpUnclamped(0.5f, 1f, e);
            yield return null;
        }
        ribbon.localScale = Vector3.one;
    }

    // -------------------------------------------------------------- helpers

    private static Image MakeImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = GetRounded();
        img.type = Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI MakeText(string name, Transform parent, string text, float size, Color color, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    private static void Stretch(RectTransform r, float inset)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(inset, inset);
        r.offsetMax = new Vector2(-inset, -inset);
    }

    // 9-sliced white rounded rectangle generated in code (tinted via Image.color)
    private static Sprite GetRounded()
    {
        if (roundedSprite != null) return roundedSprite;

        const int size = 64, radius = 24;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        var px = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - (x + 0.5f), 0f, (x + 0.5f) - (size - radius));
                float dy = Mathf.Max(radius - (y + 0.5f), 0f, (y + 0.5f) - (size - radius));
                float a = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                px[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        }
        tex.SetPixels32(px);
        tex.Apply();

        roundedSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return roundedSprite;
    }
}
