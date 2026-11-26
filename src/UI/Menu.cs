using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AtlyssTrainer
{
    internal abstract class Tab
    {
        public RectTransform Root;
        public abstract string Title { get; }
        public abstract string Hint { get; }
        public virtual void Build(RectTransform area, float width, float height) { }
        public virtual void Refresh() { }
        public virtual void OnShow() { }
    }

    internal sealed class WindowDrag : MonoBehaviour, IDragHandler
    {
        public RectTransform Target;
        public Canvas Canvas;

        public void OnDrag(PointerEventData eventData)
        {
            if (Target == null) return;
            float scale = Canvas == null ? 1f : Canvas.scaleFactor;
            if (scale <= 0f) scale = 1f;
            Target.anchoredPosition += eventData.delta / scale;
        }
    }

    /// <summary>Animated rainbow ASCII banner in the spirit of 2000s cracktros.</summary>
    internal sealed class CracktroBanner : MonoBehaviour
    {
        public UiText Title;
        public UiText Stars;
        public float AreaWidth = 1032f;

        private const string TitleText = "ambiheimo's atlyss ultimate trainer!!!";
        private const string StarChars = ".:+*";

        private float _timer;
        private int _columns;
        private int _rows;
        private int _builtWidth;
        private int _builtHeight;
        private char[] _chars;
        private float[] _phase;
        private float[] _speed;
        private string[,] _palette;
        private readonly StringBuilder _line = new StringBuilder(8192);
        private readonly StringBuilder _title = new StringBuilder(2048);
        private bool _staticRendered;

        private void Update()
        {
            bool animated = Settings.BannerAnimate.Value;

            if (!animated)
            {
                if (!_staticRendered)
                {
                    if (!EnsureGrid()) return;
                    Render(0f, false);
                    _staticRendered = true;
                }
                return;
            }

            _staticRendered = false;

            float interval = 1f / Mathf.Clamp(Settings.BannerFps.Value, 1, 30);
            _timer -= Time.unscaledDeltaTime;
            if (_timer > 0f) return;
            _timer = interval;

            if (!EnsureGrid()) return;

            Render(Time.unscaledTime, true);
        }

        private bool EnsureGrid()
        {
            if (Stars == null) return false;

            float width = Stars.rectTransform.rect.width;
            float height = Stars.rectTransform.rect.height;

            if (width < 20f) width = AreaWidth;
            if (height < 20f) height = 80f;

            int widthKey = Mathf.RoundToInt(width);
            int heightKey = Mathf.RoundToInt(height);

            if (_chars != null && widthKey == _builtWidth && heightKey == _builtHeight)
                return true;

            _builtWidth = widthKey;
            _builtHeight = heightKey;

            float advance = 7.2f;
            float lineHeight = 14f;

            TextGenerator gen = new TextGenerator();
            TextGenerationSettings settings = Stars.Text.GetGenerationSettings(Vector2.zero);
            settings.generationExtents = Vector2.zero;
            settings.horizontalOverflow = HorizontalWrapMode.Overflow;
            settings.verticalOverflow = VerticalWrapMode.Overflow;

            float measuredAdvance = gen.GetPreferredWidth(new string('M', 40), settings) / 40f;
            float measuredLine = gen.GetPreferredHeight("M\nM", settings) - gen.GetPreferredHeight("M", settings);

            if (measuredAdvance > 0.5f) advance = measuredAdvance;
            if (measuredLine > 0.5f) lineHeight = measuredLine;

            _columns = Mathf.Max(10, Mathf.CeilToInt(width / advance) + 2);
            _rows = Mathf.Max(2, Mathf.CeilToInt(height / lineHeight) + 1);

            int total = _columns * _rows;

            _chars = new char[total];
            _phase = new float[total];
            _speed = new float[total];

            for (int i = 0; i < total; i++)
            {
                _chars[i] = UnityEngine.Random.value < 0.74f
                    ? ' '
                    : StarChars[UnityEngine.Random.Range(0, StarChars.Length)];

                _phase[i] = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                _speed[i] = UnityEngine.Random.Range(0.3f, 2.2f);
            }

            _palette = new string[32, 6];

            for (int h = 0; h < 32; h++)
            {
                for (int v = 0; v < 6; v++)
                {
                    float value = 0.25f + v * 0.15f;
                    _palette[h, v] = Theme.Hex(Theme.Rainbow(h / 32f, value));
                }
            }

            return true;
        }

        private void Render(float t, bool animated)
        {
            if (Title != null)
            {
                _title.Length = 0;

                for (int i = 0; i < TitleText.Length; i++)
                {
                    float hue = animated ? i * 0.045f - t * 0.35f : i * 0.045f;
                    _title.Append("<color=#").Append(Theme.Hex(Theme.Rainbow(hue, 1f))).Append('>');
                    _title.Append(TitleText[i] == ' ' ? " " : TitleText[i].ToString());
                    _title.Append("</color>");
                }

                Title.text = _title.ToString();
            }

            if (Stars == null) return;

            _line.Length = 0;

            for (int row = 0; row < _rows; row++)
            {
                if (row > 0) _line.Append('\n');

                for (int x = 0; x < _columns; x++)
                {
                    int index = row * _columns + x;
                    char glyph = _chars[index];

                    if (glyph == ' ')
                    {
                        _line.Append(' ');
                        continue;
                    }

                    int hueIndex;
                    int valueIndex;

                    if (animated)
                    {
                        float wave = Mathf.Sin(t * _speed[index] + _phase[index]);
                        float value = 0.2f + 0.8f * Mathf.Clamp01(wave * wave);
                        hueIndex = (x + (int)(t * 14f)) % 32;
                        valueIndex = Mathf.Clamp((int)(value * 5f), 0, 5);
                    }
                    else
                    {
                        hueIndex = (x + row * 9) % 32;
                        valueIndex = 2 + ((x + row * 3) % 4);
                    }

                    _line.Append("<color=#").Append(_palette[hueIndex, valueIndex]).Append('>');
                    _line.Append(glyph);
                    _line.Append("</color>");
                }
            }

            Stars.text = _line.ToString();
        }
    }
    internal sealed class Menu : MonoBehaviour
    {
        private const float WindowWidth = 1060f;
        private const float WindowHeight = 660f;
        private const float NavWidth = 176f;
        private const float BannerHeight = 84f;
        private const float FooterHeight = 28f;

        private Canvas _canvas;
        private GameObject _root;
        private RectTransform _holder;
        private RectTransform _contentArea;
        private UiText _footer;
        private UiText _headerLevel;

        private readonly List<Tab> _tabs = new List<Tab>();
        private readonly List<Button> _navButtons = new List<Button>();
        private int _active = -1;
        private bool _open;
        private float _refreshTimer;

        private bool _hadPlayer;
        private bool _prevInUI;
        private CursorLockMode _prevLock;
        private bool _prevCursorVisible;
        private string _toast;
        private float _toastTimer;

        private bool _prevLockControl;
        private bool _prevLockMidair;
        private bool _moveLockSaved;

        public static Menu Create()
        {
            GameObject go = new GameObject("AtlyssTrainer");
            DontDestroyOnLoad(go);
            return go.AddComponent<Menu>();
        }

        public static bool IsOpenNow;

        public bool IsOpen
        {
            get { return _open; }
        }

        public void Toggle()
        {
            if (_open) Close();
            else Open();
        }

        public void Open()
        {
            EnsureEventSystem();
            Ui.Init();
            Build();

            _open = true;
            IsOpenNow = true;
            _root.SetActive(true);

            Player p = GameApi.LocalPlayer;
            if (p != null)
            {
                _hadPlayer = true;
                _prevInUI = p._inUI;
                p._inUI = true;

                if (p._pMove != null)
                {
                    _prevLockControl = p._pMove._lockControl;
                    _prevLockMidair = p._pMove._lockControlMidair;
                    _moveLockSaved = true;
                }

                ReleaseCombat(p);
            }

            _prevLock = Cursor.lockState;
            _prevCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            ApplyScale(Settings.UiScale.Value);
            Select(Mathf.Max(0, _active));
        }

        private static void ReleaseCombat(Player p)
        {
            try
            {
                if (p._pCombat != null)
                {
                    p._pCombat._isBlocking = false;
                    p._pCombat.Network_isBlocking = false;
                    p._pCombat._localBlockingInput = false;
                    p._pCombat.Network_inParryWindow = false;
                }

                if (p._pTargeting != null)
                {
                    p._pTargeting._isTargetingEntity = false;
                    p._pTargeting._targetedEntity = null;
                    p._pTargeting.Network_syncSelectedEntity = null;
                }

                ResetAimCamera();
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("ReleaseCombat: " + e.Message);
            }
        }

        // The game keeps the aim camera flag set after the target is cleared, and its
        // Handle_CameraControl is blocked while the menu is open, so it never resets itself.
        private static void ResetAimCamera()
        {
            CameraFunction cam = CameraFunction._current;
            if (cam != null) cam._unlockedCamera = false;
        }

        public void Close()
        {
            _open = false;
            IsOpenNow = false;
            if (_root != null) _root.SetActive(false);
            UiInput.Clear();

            if (_hadPlayer)
            {
                Player p = GameApi.LocalPlayer;
                if (p != null)
                {
                    p._inUI = _prevInUI;

                    if (_moveLockSaved && p._pMove != null)
                    {
                        p._pMove._lockControl = _prevLockControl;
                        p._pMove._lockControlMidair = _prevLockMidair;
                    }
                }
            }

            _moveLockSaved = false;

            ResetAimCamera();

            Cursor.lockState = _prevLock;
            Cursor.visible = _prevCursorVisible;
        }

        public void DestroyMenu()
        {
            if (_root != null) Destroy(_root);
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null) return;

            GameObject go = new GameObject("AtlyssTrainerEventSystem");
            DontDestroyOnLoad(go);
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        // ------------------------------------------------------------- build

        private void Build()
        {
            if (_root != null) return;

            Ui.Init();

            _navButtons.Clear();
            _tabs.Clear();

            _root = new GameObject("AtlyssTrainerCanvas", typeof(RectTransform));
            _root.layer = 5;
            DontDestroyOnLoad(_root);

            _canvas = _root.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 5000;

            CanvasScaler scaler = _root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            _root.AddComponent<GraphicRaycaster>();

            // invisible input blocker: no dimming, but clicks do not reach the game
            Image blocker = Ui.Img("Blocker", _root.transform, new Color(0f, 0f, 0f, 0.001f), Ui.Square);
            Ui.Stretch(blocker.rectTransform, 0f, 0f, 0f, 0f);
            blocker.raycastTarget = true;

            _holder = Ui.Rect("Holder", _root.transform);
            Center(_holder, WindowWidth, WindowHeight, Vector2.zero);

            // chunky offset shadow, attached to the window
            Image shadow2 = Ui.Img("Shadow2", _holder, new Color(0f, 0f, 0f, 0.35f), Ui.Square);
            Center(shadow2.rectTransform, WindowWidth, WindowHeight, new Vector2(8f, -8f));
            shadow2.raycastTarget = false;

            Image shadow1 = Ui.Img("Shadow1", _holder, new Color(0f, 0f, 0f, 0.55f), Ui.Square);
            Center(shadow1.rectTransform, WindowWidth, WindowHeight, new Vector2(4f, -4f));
            shadow1.raycastTarget = false;

            _window = Ui.Img("Window", _holder, Theme.Window, Ui.Square);
            Ui.Stretch(_window.rectTransform, 0f, 0f, 0f, 0f);
            _window.raycastTarget = true;
            Ui.Border(_window.rectTransform, Theme.Accent, 2f);

            BuildCorners(_window.transform);
            BuildBanner(_window.transform);
            BuildNav(_window.transform);
            BuildContent(_window.transform);

            _window.gameObject.AddComponent<Button>().transition = Selectable.Transition.None;
        }

        private Image _window;

        private static void Center(RectTransform rt, float width, float height, Vector2 offset)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = offset;
        }

        private void BuildCorners(Transform parent)
        {
            for (int i = 0; i < 4; i++)
            {
                Image corner = Ui.Img("Corner" + i, parent, Theme.Accent, Ui.Square);
                float x = (i % 2 == 0) ? 0f : WindowWidth - 9f;
                float y = (i < 2) ? 0f : WindowHeight - 9f;
                Ui.SetRect(corner.rectTransform, x, y, 8f, 8f);
            }
        }

        private void BuildBanner(Transform parent)
        {
            float bannerAlpha = Mathf.Clamp(1f - Settings.MenuOpacity.Value * 0.4f, 0.55f, 1f);
            Image banner = Ui.Img("Banner", parent, new Color(0f, 0f, 0f, bannerAlpha), Ui.Square);
            Ui.SetRect(banner.rectTransform, 8f, 8f, WindowWidth - 16f, BannerHeight);
            banner.raycastTarget = true;

            Canvas bannerCanvas = banner.gameObject.AddComponent<Canvas>();
            bannerCanvas.overrideSorting = false;

            banner.gameObject.AddComponent<RectMask2D>();

            // starfield covers the whole banner, text sits on top with a black outline
            UiText stars = Ui.Label("Stars", banner.transform, "", 12f, Theme.Muted, TextAlignmentOptions.TopLeft);
            Ui.SetRect(stars.rectTransform, 0f, 0f, WindowWidth - 16f, BannerHeight);
            stars.Text.font = Ui.StarFont != null ? Ui.StarFont : Ui.Font;
            stars.uppercase = false;
            stars.fontStyle = FontStyles.Normal;
            stars.characterSpacing = 0f;
            stars.lineSpacing = 1f;

            UiText title = Ui.Label("Title", banner.transform,
                "ambiheimo's atlyss ultimate trainer!!!", 21f, Theme.Accent, TextAlignmentOptions.Center);
            Ui.SetRect(title.rectTransform, 12f, 24f, WindowWidth - 40f, 30f);
            title.uppercase = false;
            title.fontStyle = FontStyles.Bold;
            title.characterSpacing = 4f;

            Outline titleOutline = title.Text.gameObject.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0f, 0f, 0f, 0.95f);
            titleOutline.effectDistance = new Vector2(2f, -2f);

            UiText sub = Ui.Label("Sub", banner.transform,
                ":: trainer " + Plugin.Version + " :: game " + GameApi.GameVersion +
                " :: bepinex :: @serumgladiator :: insert = close ::",
                12f, Theme.Muted, TextAlignmentOptions.Center);
            Ui.SetRect(sub.rectTransform, 12f, 57f, WindowWidth - 40f, 18f);

            Outline subOutline = sub.Text.gameObject.AddComponent<Outline>();
            subOutline.effectColor = new Color(0f, 0f, 0f, 0.95f);
            subOutline.effectDistance = new Vector2(2f, -2f);

            Image line = Ui.Img("BannerLine", banner.transform, Theme.Accent, Ui.Square);
            Ui.SetRect(line.rectTransform, 0f, BannerHeight - 1f, WindowWidth - 16f, 1f);

            CracktroBanner anim = banner.gameObject.AddComponent<CracktroBanner>();
            anim.Title = title;
            anim.Stars = stars;
            anim.AreaWidth = WindowWidth - 28f;
        }

        private void BuildNav(Transform parent)
        {
            float navHeight = WindowHeight - BannerHeight - FooterHeight - 22f;

            Image nav = Ui.Img("Nav", parent, new Color(0f, 0f, 0f, Mathf.Max(0.1f, Theme.Panel.a * 0.8f)), Ui.Square);
            Ui.SetRect(nav.rectTransform, 8f, BannerHeight + 8f, NavWidth, navHeight);

            _tabs.Clear();
            _tabs.Add(new CheatsTab());
            _tabs.Add(new CharacterTab());
            _tabs.Add(new WorldTab());
            _tabs.Add(new TeleportTab());
            _tabs.Add(new ItemsTab());
            _tabs.Add(new EnchantTab());
            _tabs.Add(new CreepsTab());
            _tabs.Add(new SettingsTab());
            _tabs.Add(new ExperimentalTab());

            float y = 8f;
            for (int i = 0; i < _tabs.Count; i++)
            {
                int index = i;
                Button b = Ui.Button(nav.transform, "", delegate { Select(index); }, 12f);
                Ui.SetRect(b.GetComponent<RectTransform>(), 6f, y, NavWidth - 12f, 32f);
                _navButtons.Add(b);
                y += 36f;
            }

            UiText version = Ui.Label("Version", nav.transform, "v" + Plugin.Version + " :: @serumgladiator",
                9f, Theme.Muted, TextAlignmentOptions.BottomLeft);
            Ui.SetRect(version.rectTransform, 8f, navHeight - 20f, NavWidth - 16f, 14f);

            _headerLevel = Ui.Label("Level", nav.transform, "", 11f, Theme.Text, TextAlignmentOptions.BottomLeft);
            Ui.SetRect(_headerLevel.rectTransform, 8f, navHeight - 40f, NavWidth - 16f, 14f);
        }

        private void BuildContent(Transform parent)
        {
            float x = NavWidth + 18f;
            float width = WindowWidth - x - 10f;
            float height = WindowHeight - BannerHeight - FooterHeight - 14f;

            RectTransform content = Ui.Rect("Content", parent);
            Ui.SetRect(content, x, BannerHeight + 8f, width, height);
            _contentArea = content;

            for (int i = 0; i < _tabs.Count; i++)
            {
                Tab tab = _tabs[i];

                RectTransform area = Ui.Rect(tab.GetType().Name, _contentArea);
                Ui.Stretch(area, 0f, 0f, 0f, 0f);

                try
                {
                    tab.Root = area;
                    tab.Build(area, width, height);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Tab build " + tab.GetType().Name + ": " + e);
                }

                area.gameObject.SetActive(false);
                Ui.SetLabel(_navButtons[i], tab.Title);
            }

            _footer = Ui.Label("Footer", parent, "", 11f, Theme.Muted, TextAlignmentOptions.Left);
            Ui.SetRect(_footer.rectTransform, x, WindowHeight - FooterHeight + 2f, width, 18f);
        }

        // ------------------------------------------------------------- state

        private void Select(int index)
        {
            if (index < 0 || index >= _tabs.Count) return;
            _active = index;

            for (int i = 0; i < _tabs.Count; i++)
            {
                bool on = i == index;
                if (_tabs[i].Root != null) _tabs[i].Root.gameObject.SetActive(on);

                if (i < _navButtons.Count)
                {
                    Ui.SetButtonColor(_navButtons[i], on
                        ? new Color(Theme.Accent.r * 0.22f, Theme.Accent.g * 0.22f, Theme.Accent.b * 0.22f, 1f)
                        : Theme.Row);
                    UiText label = Ui.FindLabel(_navButtons[i].transform);
                    if (label != null)
                    {
                        label.color = on ? Theme.Accent : Theme.Text;
                        label.text = (on ? "> " : "  ") + _tabs[i].Title;
                    }
                }
            }

            try
            {
                _tabs[index].OnShow();
                _tabs[index].Refresh();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Tab show: " + e);
            }

            RefreshFooter();
        }

        private void RefreshFooter()
        {
            if (_footer == null) return;

            if (_toastTimer > 0f && !string.IsNullOrEmpty(_toast))
            {
                _footer.text = "  " + _toast;
                _footer.color = Theme.Good;
                return;
            }

            _footer.color = Theme.Muted;

            Tab tab = _active >= 0 && _active < _tabs.Count ? _tabs[_active] : null;
            string host = GameApi.IsHost ? L10n.T("host", "хост") : L10n.T("client", "клиент");
            string mode = GameApi.InGame ? host : L10n.T("no character", "нет персонажа");

            _footer.text = "  " + (tab == null ? "" : tab.Hint) +
                           "   ::   " + mode + "   ::   " + L10n.T("wheel = scroll", "колесо = прокрутка");
        }

        public void Toast(string message)
        {
            _toast = message;
            _toastTimer = 2.5f;
            RefreshFooter();
        }

        public void ApplyScale(float scale)
        {
            if (_root == null) return;
            CanvasScaler scaler = _root.GetComponent<CanvasScaler>();
            if (scaler == null) return;

            scale = Mathf.Clamp(scale, 0.5f, 2f);
            scaler.referenceResolution = new Vector2(1920f / scale, 1080f / scale);
        }

        public void SetFont(string fontName)
        {
            Settings.MenuFont.Value = fontName;
            Ui.Reset();
            Ui.Init();
            Rebuild();
        }

        public void Rebuild()
        {
            int active = _active;

            if (_root != null)
            {
                Destroy(_root);
                _root = null;
                _canvas = null;
                _window = null;
                _contentArea = null;
                _footer = null;
                _headerLevel = null;
                _navButtons.Clear();
                _tabs.Clear();
            }

            Build();
            if (_root != null) _root.SetActive(true);
            Select(Mathf.Max(0, active));
        }

        public void Tick()
        {
            if (!_open) return;

            bool editing = UiInput.ConsumesKeyboard;
            UiInput.HandleClickOutside(null);
            UiInput.Tick();

            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.unscaledDeltaTime;
                RefreshFooter();
            }

            if (Input.GetKeyDown(KeyCode.Escape) && !editing)
            {
                Close();
                return;
            }

            _refreshTimer -= Time.unscaledDeltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = 0.25f;

                if (_active >= 0 && _active < _tabs.Count)
                {
                    try { _tabs[_active].Refresh(); }
                    catch (Exception e) { Plugin.Log.LogError("Refresh: " + e); }
                }

                if (_headerLevel != null)
                {
                    PlayerStats st = GameApi.Stats;
                    if (st != null)
                    {
                        string cls = st._class != null ? L10n.Game(st._class._className) : "";
                        _headerLevel.text = L10n.T("lv ", "ур. ") + st._currentLevel + "  " + cls;
                    }
                    else
                    {
                        _headerLevel.text = "";
                    }
                }

                RefreshFooter();
            }
        }

        public void LateTick()
        {
            if (!_open) return;

            EnsureInputModule();

            Player p = GameApi.LocalPlayer;
            if (p != null)
            {
                p._inUI = true;
                p._inChat = false;

                if (Settings.LockInput.Value && p._pMove != null)
                {
                    p._pMove._lockControl = true;
                    p._pMove._lockControlMidair = true;
                }
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private static BaseInputModule _module;
        private static float _moduleCheck;

        private static void EnsureInputModule()
        {
            _moduleCheck -= Time.unscaledDeltaTime;
            if (_moduleCheck > 0f) return;
            _moduleCheck = 1f;

            EventSystem es = EventSystem.current;
            if (es == null) return;

            if (_module != null && _module.isActiveAndEnabled) return;

            BaseInputModule[] modules = es.GetComponents<BaseInputModule>();
            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i] != null && modules[i].isActiveAndEnabled)
                {
                    _module = modules[i];
                    return;
                }
            }

            if (modules.Length == 0)
            {
                _module = es.gameObject.AddComponent<StandaloneInputModule>();
            }
            else
            {
                modules[0].enabled = true;
                _module = modules[0];
            }
        }
    }
}
