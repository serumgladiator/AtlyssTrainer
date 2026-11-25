using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AtlyssTrainer
{
    internal static class Theme
    {
        public static Color Window = new Color(0f, 0f, 0f, 0.86f);
        public static Color Panel = new Color(0.02f, 0.03f, 0.02f, 0.9f);
        public static Color Row = new Color(0.05f, 0.07f, 0.05f, 0.92f);
        public static Color RowAlt = new Color(0.03f, 0.05f, 0.03f, 0.92f);
        public static Color Line = new Color(0.15f, 0.45f, 0.18f, 1f);
        public static Color Accent = new Color(0.35f, 1f, 0.45f, 1f);
        public static Color Accent2 = new Color(0.2f, 0.95f, 1f, 1f);
        public static Color Amber = new Color(1f, 0.78f, 0.25f, 1f);
        public static Color Text = new Color(0.82f, 0.92f, 0.82f, 1f);
        public static Color Muted = new Color(0.42f, 0.55f, 0.42f, 1f);
        public static Color Danger = new Color(1f, 0.3f, 0.3f, 1f);
        public static Color Good = new Color(0.4f, 1f, 0.5f, 1f);

        public static string Current = "green";

        public static readonly string[] ThemeNames = { "green", "amber", "cyan", "magenta", "black", "red" };

        public static void Apply(string name)
        {
            if (string.IsNullOrEmpty(name)) name = "green";
            Current = name;

            switch (name)
            {
                case "amber":
                    Accent = HexColor("FFB43C");
                    Accent2 = HexColor("FFE28A");
                    Text = HexColor("F2E2C4");
                    Muted = HexColor("8A7350");
                    Line = HexColor("7A4E14");
                    Good = HexColor("FFD27A");
                    Danger = HexColor("FF6B4A");
                    Row = HexColor("100B04", 0.92f);
                    RowAlt = HexColor("0A0702", 0.92f);
                    Panel = HexColor("0A0702", 0.9f);
                    break;

                case "cyan":
                    Accent = HexColor("45E6FF");
                    Accent2 = HexColor("C2F8FF");
                    Text = HexColor("D6F4FA");
                    Muted = HexColor("4E7E8A");
                    Line = HexColor("1E6B7A");
                    Good = HexColor("8DF7FF");
                    Danger = HexColor("FF5A7A");
                    Row = HexColor("03100F", 0.92f);
                    RowAlt = HexColor("020A0A", 0.92f);
                    Panel = HexColor("020A0A", 0.9f);
                    break;

                case "magenta":
                    Accent = HexColor("FF66D9");
                    Accent2 = HexColor("FFB3F0");
                    Text = HexColor("F7D9F0");
                    Muted = HexColor("8A547C");
                    Line = HexColor("7A2A63");
                    Good = HexColor("FF9BE8");
                    Danger = HexColor("FF4D6A");
                    Row = HexColor("10040D", 0.92f);
                    RowAlt = HexColor("0A0209", 0.92f);
                    Panel = HexColor("0A0209", 0.9f);
                    break;

                case "black":
                    Accent = HexColor("F2F2F2");
                    Accent2 = HexColor("BFBFBF");
                    Text = HexColor("EDEDED");
                    Muted = HexColor("7A7A7A");
                    Line = HexColor("4A4A4A");
                    Good = HexColor("FFFFFF");
                    Danger = HexColor("FF6666");
                    Row = HexColor("141414", 0.92f);
                    RowAlt = HexColor("0C0C0C", 0.92f);
                    Panel = HexColor("0A0A0A", 0.92f);
                    break;

                case "red":
                    Accent = HexColor("FF5A5A");
                    Accent2 = HexColor("FF9C8A");
                    Text = HexColor("F5D6D6");
                    Muted = HexColor("8A4A4A");
                    Line = HexColor("7A2020");
                    Good = HexColor("FF8A6A");
                    Danger = HexColor("FF3B3B");
                    Row = HexColor("100404", 0.92f);
                    RowAlt = HexColor("0A0202", 0.92f);
                    Panel = HexColor("0A0202", 0.9f);
                    break;

                default:
                    Accent = HexColor("5AFF73");
                    Accent2 = HexColor("33F2FF");
                    Text = HexColor("D1EBD1");
                    Muted = HexColor("6B8A6B");
                    Line = HexColor("26732E");
                    Good = HexColor("8AFF9C");
                    Danger = HexColor("FF4D4D");
                    Row = HexColor("050B05", 0.92f);
                    RowAlt = HexColor("030703", 0.92f);
                    Panel = HexColor("020602", 0.9f);
                    break;
            }

            float transparency = Mathf.Clamp01(Settings.MenuOpacity.Value);

            Window = new Color(0f, 0f, 0f, Mathf.Max(0.05f, 1f - transparency));
            Row.a = Mathf.Max(0.14f, 1f - transparency * 0.8f);
            RowAlt.a = Mathf.Max(0.10f, 1f - transparency * 0.85f);
            Panel.a = Mathf.Max(0.10f, 1f - transparency * 0.85f);
        }

        private static Color HexColor(string hex, float alpha)
        {
            Color c;
            if (ColorUtility.TryParseHtmlString("#" + hex, out c)) c.a = alpha;
            return c;
        }

        private static Color HexColor(string hex)
        {
            return HexColor(hex, 1f);
        }

        public static Color Rarity(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Rare: return new Color(0.45f, 0.7f, 1f);
                case ItemRarity.Exotic: return new Color(0.85f, 0.5f, 1f);
                case ItemRarity.Legendary: return new Color(1f, 0.72f, 0.3f);
                case ItemRarity.Cosmetic: return new Color(0.5f, 1f, 0.8f);
                case ItemRarity.Quest: return new Color(1f, 0.9f, 0.35f);
                default: return new Color(0.8f, 0.85f, 0.8f);
            }
        }

        public static Color Rainbow(float hue, float value)
        {
            return Color.HSVToRGB(Mathf.Repeat(hue, 1f), 1f, value);
        }

        public static string Hex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }
    }

    /// <summary>Thin wrapper over legacy UI.Text so the whole menu can use a monospace OS font.</summary>
    internal sealed class UiText
    {
        public Text Text;
        private bool _uppercase = true;

        public RectTransform rectTransform
        {
            get { return Text.rectTransform; }
        }

        public bool uppercase
        {
            get { return _uppercase; }
            set { _uppercase = value; }
        }

        public string text
        {
            get { return Text.text; }
            set { Text.text = _uppercase ? Upper(value) : (value ?? ""); }
        }

        public Color color
        {
            get { return Text.color; }
            set { Text.color = value; }
        }

        public float fontSize
        {
            get { return Text.fontSize; }
            set { Text.fontSize = Mathf.Max(6, Mathf.RoundToInt(value)); }
        }

        public bool raycastTarget
        {
            get { return Text.raycastTarget; }
            set { Text.raycastTarget = value; }
        }

        public bool enableWordWrapping
        {
            set
            {
                Text.horizontalOverflow = value ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
                Text.verticalOverflow = value ? VerticalWrapMode.Truncate : VerticalWrapMode.Overflow;
            }
        }

        public TextOverflowModes overflowMode
        {
            set { Text.verticalOverflow = value == TextOverflowModes.Overflow ? VerticalWrapMode.Overflow : VerticalWrapMode.Truncate; }
        }

        public TextAlignmentOptions alignment
        {
            set { Text.alignment = Ui.ToAnchor(value); }
        }

        public FontStyles fontStyle
        {
            set { Text.fontStyle = (value & FontStyles.Bold) != 0 ? FontStyle.Bold : FontStyle.Normal; }
        }

        public float characterSpacing
        {
            set { }
        }

        public float lineSpacing
        {
            set { Text.lineSpacing = value; }
        }

        public Vector4 margin
        {
            set { }
        }

        /// <summary>Uppercases only visible text, keeping rich text tags intact.</summary>
        private static string Upper(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            var sb = new System.Text.StringBuilder(value.Length);
            bool tag = false;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '<') tag = true;
                if (!tag) c = char.ToUpperInvariant(c);
                if (c == '>') tag = false;
                sb.Append(c);
            }

            return sb.ToString();
        }
    }

    /// <summary>Inverts a row's colours on hover, old-school style.</summary>
    internal sealed class HoverFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image Background;
        public UiText Label;
        public UiText Label2;
        public Color BackgroundNormal;
        public Color BackgroundHover;
        public Color TextNormal;
        public Color TextHover;
        public Color TextNormal2;
        public Color TextHover2;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Background != null) Background.color = BackgroundHover;
            if (Label != null) Label.color = TextHover;
            if (Label2 != null) Label2.color = TextHover2;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Background != null) Background.color = BackgroundNormal;
            if (Label != null) Label.color = TextNormal;
            if (Label2 != null) Label2.color = TextNormal2;
        }
    }

    internal static class Ui
    {
        public static Sprite Square;
        public static Font Font;
        public static Font StarFont;
        public static bool Ready;

        public static readonly string[] FontNames =
        {
            "Consolas", "Lucida Console", "Courier New", "Tahoma",
            "Verdana", "Segoe UI", "Arial", "Comic Sans MS"
        };

        public static void Reset()
        {
            Ready = false;
            Square = null;
        }

        public static void Init()
        {
            if (Ready) return;

            Square = MakeSolid(4);

            string fontName = Settings.MenuFont != null && !string.IsNullOrEmpty(Settings.MenuFont.Value)
                ? Settings.MenuFont.Value
                : "Consolas";

            try
            {
                Font = UnityEngine.Font.CreateDynamicFontFromOSFont(
                    new string[] { fontName, "Consolas", "Courier New", "Arial" }, 16);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("OS font unavailable: " + e.Message);
            }

            if (Font == null)
            {
                try { Font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                catch { Font = null; }
            }

            try
            {
                StarFont = UnityEngine.Font.CreateDynamicFontFromOSFont(
                    new string[] { "Consolas", "Courier New", "Lucida Console" }, 16);
            }
            catch { StarFont = null; }

            if (StarFont == null) StarFont = Font;

            Ready = true;
        }

        private static Sprite MakeSolid(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;

            Color32 white = new Color32(255, 255, 255, 255);
            Color32[] pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = white;

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;

            Sprite s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s.hideFlags = HideFlags.HideAndDontSave;
            return s;
        }

        internal static TextAnchor ToAnchor(TextAlignmentOptions alignment)
        {
            switch (alignment)
            {
                case TextAlignmentOptions.TopLeft: return TextAnchor.UpperLeft;
                case TextAlignmentOptions.Top: return TextAnchor.UpperCenter;
                case TextAlignmentOptions.TopRight: return TextAnchor.UpperRight;
                case TextAlignmentOptions.Left: return TextAnchor.MiddleLeft;
                case TextAlignmentOptions.Center: return TextAnchor.MiddleCenter;
                case TextAlignmentOptions.Right: return TextAnchor.MiddleRight;
                case TextAlignmentOptions.BottomLeft: return TextAnchor.LowerLeft;
                case TextAlignmentOptions.Bottom: return TextAnchor.LowerCenter;
                case TextAlignmentOptions.BottomRight: return TextAnchor.LowerRight;
                case TextAlignmentOptions.MidlineLeft: return TextAnchor.MiddleLeft;
                case TextAlignmentOptions.MidlineRight: return TextAnchor.MiddleRight;
                default: return TextAnchor.MiddleCenter;
            }
        }

        // ------------------------------------------------------------- objects

        public static RectTransform Rect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(100f, 30f);
            return rt;
        }

        public static Image Img(string name, Transform parent, Color color, Sprite sprite)
        {
            RectTransform rt = Rect(name, parent);
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite == null ? Square : sprite;
            img.type = Image.Type.Simple;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static UiText Label(string name, Transform parent, string text, float size, Color color,
            TextAlignmentOptions align)
        {
            RectTransform rt = Rect(name, parent);
            Text t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.fontSize = Mathf.Max(6, Mathf.RoundToInt(size));
            t.color = color;
            t.alignment = ToAnchor(align);
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            t.fontStyle = FontStyle.Normal;

            UiText ui = new UiText();
            ui.Text = t;
            ui.text = text;
            return ui;
        }

        public static void SetRect(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
        }

        public static void Stretch(RectTransform rt, float left, float top, float right, float bottom)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>Draws a real 2px frame out of four thin lines (keeps transparency working).</summary>
        public static void Border(RectTransform root, Color color, float thickness)
        {
            Image top = Img("BorderTop", root, color, Square);
            RectTransform tr = top.rectTransform;
            tr.anchorMin = new Vector2(0f, 1f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.sizeDelta = new Vector2(0f, thickness);
            tr.anchoredPosition = Vector2.zero;

            Image bottom = Img("BorderBottom", root, color, Square);
            RectTransform br = bottom.rectTransform;
            br.anchorMin = new Vector2(0f, 0f);
            br.anchorMax = new Vector2(1f, 0f);
            br.pivot = new Vector2(0.5f, 0f);
            br.sizeDelta = new Vector2(0f, thickness);
            br.anchoredPosition = Vector2.zero;

            Image left = Img("BorderLeft", root, color, Square);
            RectTransform lr = left.rectTransform;
            lr.anchorMin = new Vector2(0f, 0f);
            lr.anchorMax = new Vector2(0f, 1f);
            lr.pivot = new Vector2(0f, 0.5f);
            lr.sizeDelta = new Vector2(thickness, 0f);
            lr.anchoredPosition = Vector2.zero;

            Image right = Img("BorderRight", root, color, Square);
            RectTransform rr = right.rectTransform;
            rr.anchorMin = new Vector2(1f, 0f);
            rr.anchorMax = new Vector2(1f, 1f);
            rr.pivot = new Vector2(1f, 0.5f);
            rr.sizeDelta = new Vector2(thickness, 0f);
            rr.anchoredPosition = Vector2.zero;
        }

        public static Button Button(Transform parent, string label, Action onClick, float fontSize)
        {
            Image img = Img("Button", parent, Theme.Row, Square);
            img.raycastTarget = true;
            Border(img.rectTransform, Theme.Line, 2f);

            Button btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = Color.white;
            cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            cb.selectedColor = Color.white;
            cb.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.55f);
            cb.fadeDuration = 0f;
            btn.colors = cb;

            UiText text = Label("Label", img.transform, label, fontSize, Theme.Text, TextAlignmentOptions.Center);
            Stretch(text.rectTransform, 6f, 0f, 6f, 0f);

            HoverFx hover = img.gameObject.AddComponent<HoverFx>();
            hover.Background = img;
            hover.BackgroundNormal = Theme.Row;
            hover.BackgroundHover = Theme.Accent;
            hover.TextNormal = Theme.Text;
            hover.TextHover = Color.black;
            hover.Label = text;

            if (onClick != null)
                btn.onClick.AddListener(new UnityEngine.Events.UnityAction(onClick));

            return btn;
        }

        public static void SetLabel(Button btn, string label)
        {
            if (btn == null) return;
            Text t = btn.GetComponentInChildren<Text>();
            if (t != null)
            {
                UiText ui = new UiText();
                ui.Text = t;
                ui.text = label;
            }
        }

        public static void SetButtonColor(Button btn, Color color)
        {
            if (btn == null) return;

            Image img = btn.GetComponent<Image>();
            if (img != null) img.color = color;

            HoverFx hover = btn.GetComponent<HoverFx>();
            if (hover != null) hover.BackgroundNormal = color;
        }

        public static UiText FindLabel(Transform parent)
        {
            if (parent == null) return null;

            Text t = parent.GetComponentInChildren<Text>();
            if (t == null) return null;

            UiText ui = new UiText();
            ui.Text = t;
            return ui;
        }

        /// <summary>Vertical scroll area with a thin frame.</summary>
        public static RectTransform ScrollArea(RectTransform parent, float x, float y, float width, float height,
            out ScrollRect scroll)
        {
            Image frame = Img("ScrollArea", parent, new Color(0f, 0f, 0f, 0.001f), Square);
            SetRect(frame.rectTransform, x, y, width, height);
            frame.raycastTarget = true;

            RectTransform viewport = Rect("Viewport", frame.rectTransform);
            Stretch(viewport, 0f, 0f, 6f, 0f);
            viewport.gameObject.AddComponent<RectMask2D>();

            Image vp = viewport.gameObject.AddComponent<Image>();
            vp.color = new Color(0f, 0f, 0f, 0.001f);
            vp.sprite = Square;
            vp.raycastTarget = true;

            RectTransform content = Rect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 10f);

            scroll = frame.gameObject.AddComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = false;
            scroll.scrollSensitivity = 45f;

            return content;
        }

        public sealed class Stack
        {
            private readonly RectTransform _root;
            private float _y;
            private readonly float _width;

            public Stack(RectTransform root, float top, float width)
            {
                _root = root;
                _y = top;
                _width = width;
            }

            public float Y
            {
                get { return _y; }
                set { _y = value; }
            }

            public float Width
            {
                get { return _width; }
            }

            public RectTransform Next(float height, float gap)
            {
                RectTransform rt = Rect("Row", _root);
                SetRect(rt, 0f, _y, _width, height);
                _y += height + gap;
                return rt;
            }
        }
    }
}
