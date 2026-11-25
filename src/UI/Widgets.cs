using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AtlyssTrainer
{
    internal interface IEditTarget
    {
        RectTransform FocusRect { get; }
        void Append(char c);
        void Backspace();
        void Submit();
        void Cancel();
    }

    /// <summary>Routes keyboard input to whichever inline field has focus.</summary>
    internal static class UiInput
    {
        public static IEditTarget Target;

        public static bool IsEditing
        {
            get { return Target != null; }
        }

        public static void Focus(IEditTarget target)
        {
            Target = target;
        }

        public static void Clear()
        {
            Target = null;
        }

        public static void Tick()
        {
            if (Target == null) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                IEditTarget t = Target;
                Target = null;
                t.Cancel();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                IEditTarget t = Target;
                Target = null;
                t.Submit();
                return;
            }

            string input = Input.inputString;
            if (string.IsNullOrEmpty(input)) return;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '\b')
                    Target.Backspace();
                else if (c == '\n' || c == '\r' || c == '\t')
                    continue;
                else if (c >= 32)
                    Target.Append(c);
            }
        }

        public static bool ConsumesKeyboard
        {
            get { return Target != null; }
        }

        public static void HandleClickOutside(Camera camera)
        {
            if (Target == null || !Input.GetMouseButtonDown(0))
                return;

            RectTransform rect = Target.FocusRect;
            if (rect == null)
            {
                Target = null;
                return;
            }

            if (!RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, camera))
                Target = null;
        }
    }

    /// <summary>Label + value with ASCII steppers and click-to-type.</summary>
    internal sealed class ValueRow : IEditTarget
    {
        private UiText _label;
        private UiText _value;
        private Func<int> _get;
        private Action<int> _set;
        private int _min;
        private int _max;
        private int _step;
        private string _buffer;
        private bool _editing;
        private string _suffix;

        public RectTransform Root;

        public RectTransform FocusRect
        {
            get { return Root; }
        }

        public ValueRow Build(Transform parent, float width, string label, Func<int> get, Action<int> set,
            int min, int max, int step, string suffix)
        {
            _get = get;
            _set = set;
            _min = min;
            _max = max;
            _step = step;
            _suffix = suffix ?? "";

            Image outer = Ui.Img("ValueRow", parent, Theme.Row, Ui.Square);
            Root = outer.rectTransform;
            Root.sizeDelta = new Vector2(width, 34f);

            Ui.Border(Root, Theme.Line, 2f);

            _label = Ui.Label("Label", outer.transform, label, 17f, Theme.Text, TextAlignmentOptions.Left);

            bool compact = width < 300f;
            float btn = compact ? 24f : 30f;
            float val = compact ? 64f : 106f;
            float gap = compact ? 6f : 8f;
            float xPlus = width - 6f - btn;
            float xValue = xPlus - gap - val;
            float xMinus = xValue - gap - btn;

            Ui.Stretch(_label.rectTransform, 12f, 0f, width - xMinus + 8f, 0f);

            Button minus = Ui.Button(outer.transform, "<", delegate { Step(-1); }, 18f);
            Ui.SetRect(minus.GetComponent<RectTransform>(), xMinus, 4f, btn, 24f);

            Button plus = Ui.Button(outer.transform, ">", delegate { Step(1); }, 18f);
            Ui.SetRect(plus.GetComponent<RectTransform>(), xPlus, 4f, btn, 24f);

            Image valueBg = Ui.Img("Value", outer.transform, new Color(0f, 0f, 0f, 0.5f), Ui.Square);
            Ui.Border(valueBg.rectTransform, Theme.Line, 1f);
            Ui.SetRect(valueBg.rectTransform, xValue, 4f, val, 24f);
            valueBg.raycastTarget = true;

            _value = Ui.Label("Text", valueBg.transform, "", 17f, Theme.Accent2, TextAlignmentOptions.Center);
            Ui.Stretch(_value.rectTransform, 0f, 0f, 0f, 0f);

            Button valueBtn = valueBg.gameObject.AddComponent<Button>();
            valueBtn.targetGraphic = valueBg;
            valueBtn.transition = Selectable.Transition.None;
            valueBtn.onClick.AddListener(new UnityEngine.Events.UnityAction(BeginEdit));

            Refresh();
            return this;
        }

        private void Step(int direction)
        {
            if (_editing)
            {
                UiInput.Clear();
                EndEdit(false);
            }
            int next = Mathf.Clamp(_get() + direction * _step, _min, _max);
            _set(next);
            Refresh();
        }

        private void BeginEdit()
        {
            _editing = true;
            _buffer = "";
            UiInput.Focus(this);
            Refresh();
        }

        private void EndEdit(bool apply)
        {
            _editing = false;
            if (apply)
            {
                int parsed;
                if (int.TryParse(_buffer, out parsed))
                    _set(Mathf.Clamp(parsed, _min, _max));
            }
            _buffer = null;
            Refresh();
        }

        public void Append(char c)
        {
            if (!_editing) return;
            if (char.IsDigit(c) || (c == '-' && _buffer.Length == 0))
            {
                if (_buffer == null) _buffer = "";
                if (_buffer.Length < 9) _buffer += c;
                Refresh();
            }
        }

        public void Backspace()
        {
            if (!_editing || string.IsNullOrEmpty(_buffer)) return;
            _buffer = _buffer.Substring(0, _buffer.Length - 1);
            Refresh();
        }

        public void Submit()
        {
            EndEdit(true);
        }

        public void Cancel()
        {
            EndEdit(false);
        }

        public void Refresh()
        {
            if (_value == null) return;

            if (_editing)
            {
                _value.color = Theme.Text;
                _value.text = (_buffer == null ? "" : _buffer) + "_";
            }
            else
            {
                _value.color = Theme.Accent2;
                _value.text = _get() + _suffix;
            }
        }
    }

    /// <summary>Clickable row with an ASCII checkbox.</summary>
    internal sealed class ToggleRow
    {
        private UiText _state;
        private UiText _label;
        private Image _fill;
        private Func<bool> _get;
        private Action<bool> _set;

        public RectTransform Root;

        public ToggleRow Build(Transform parent, float width, string label, Func<bool> get, Action<bool> set, string hint)
        {
            _get = get;
            _set = set;

            Image outer = Ui.Img("ToggleRow", parent, Theme.Row, Ui.Square);
            Root = outer.rectTransform;
            Root.sizeDelta = new Vector2(width, 34f);
            outer.raycastTarget = true;

            _fill = Ui.Img("Fill", Root, Theme.Row, Ui.Square);
            Ui.Stretch(_fill.rectTransform, 1f, 1f, 1f, 1f);

            Button btn = outer.gameObject.AddComponent<Button>();
            btn.targetGraphic = outer;
            btn.onClick.AddListener(new UnityEngine.Events.UnityAction(Toggle));

            _label = Ui.Label("Label", _fill.transform, label, 17f, Theme.Text, TextAlignmentOptions.Left);
            Ui.Stretch(_label.rectTransform, 12f, 0f, 90f, 0f);

            if (!string.IsNullOrEmpty(hint))
            {
                UiText h = Ui.Label("Hint", _fill.transform, hint, 12f, Theme.Muted, TextAlignmentOptions.Right);
                Ui.Stretch(h.rectTransform, 12f, 0f, 64f, 0f);
            }

            _state = Ui.Label("State", _fill.transform, "[ ]", 17f, Theme.Muted, TextAlignmentOptions.Right);
            Ui.SetRect(_state.rectTransform, width - 52f, 0f, 40f, 34f);

            HoverFx hover = outer.gameObject.AddComponent<HoverFx>();
            hover.Background = _fill;
            hover.BackgroundNormal = Theme.Row;
            hover.BackgroundHover = Theme.Accent;
            hover.TextNormal = Theme.Text;
            hover.TextHover = Color.black;
            hover.Label = _label;
            hover.Label2 = _state;

            Refresh();
            return this;
        }

        private void Toggle()
        {
            UiInput.Clear();
            _set(!_get());
            Refresh();
        }

        public void Refresh()
        {
            if (_state == null) return;

            bool on = _get();
            _state.text = on ? "[X]" : "[ ]";
            _state.color = on ? Theme.Accent : Theme.Muted;

            HoverFx hover = Root.GetComponent<HoverFx>();
            if (hover != null)
            {
                hover.TextNormal = Theme.Text;
                hover.TextNormal2 = on ? Theme.Accent : Theme.Muted;
                hover.TextHover2 = Color.black;
            }
        }
    }

    /// <summary>Text field with a ">" prompt.</summary>
    internal sealed class SearchBox : IEditTarget
    {
        private UiText _text;
        private string _value = "";
        private Action<string> _onChange;
        private string _placeholder;

        public RectTransform Root;

        public string Value
        {
            get { return _value; }
        }

        public RectTransform FocusRect
        {
            get { return Root; }
        }

        public SearchBox Build(Transform parent, float width, float height, string placeholder, Action<string> onChange)
        {
            _onChange = onChange;
            _placeholder = placeholder;

            Image outer = Ui.Img("Search", parent, new Color(0f, 0f, 0f, 0.35f), Ui.Square);
            Root = outer.rectTransform;
            Root.sizeDelta = new Vector2(width, height);
            outer.raycastTarget = true;

            Ui.Border(Root, Theme.Line, 2f);

            Button btn = outer.gameObject.AddComponent<Button>();
            btn.targetGraphic = outer;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(new UnityEngine.Events.UnityAction(StartEdit));

            UiText prompt = Ui.Label("Prompt", outer.transform, ">", 17f, Theme.Accent, TextAlignmentOptions.Left);
            Ui.SetRect(prompt.rectTransform, 10f, 0f, 14f, height);

            _text = Ui.Label("Text", outer.transform, placeholder, 17f, Theme.Muted, TextAlignmentOptions.Left);
            Ui.SetRect(_text.rectTransform, 28f, 0f, width - 38f, height);

            return this;
        }

        private void StartEdit()
        {
            UiInput.Focus(this);
            Refresh();
        }

        public void Append(char c)
        {
            if (_value.Length >= 40) return;
            _value += c;
            Changed();
        }

        public void Backspace()
        {
            if (_value.Length == 0) return;
            _value = _value.Substring(0, _value.Length - 1);
            Changed();
        }

        public void Submit()
        {
            Refresh();
        }

        public void Cancel()
        {
            Refresh();
        }

        private void Changed()
        {
            if (_onChange != null) _onChange(_value);
            Refresh();
        }

        public void Clear()
        {
            _value = "";
            Changed();
        }

        public void Refresh()
        {
            if (_text == null) return;

            bool focused = UiInput.Target == (IEditTarget)this;

            if (string.IsNullOrEmpty(_value))
            {
                _text.text = focused ? "_" : _placeholder;
                _text.color = Theme.Muted;
            }
            else
            {
                _text.text = _value + (focused ? "_" : "");
                _text.color = Theme.Accent2;
            }
        }
    }

    /// <summary>Simple hover relay so list rows can drive a details panel.</summary>
    internal sealed class PointerRelay : MonoBehaviour, IPointerEnterHandler
    {
        public Action OnEnter;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (OnEnter != null) OnEnter();
        }
    }

    /// <summary>Recycling list inside a real ScrollRect.</summary>
    internal sealed class VirtualList
    {
        private ScrollRect _scroll;
        private RectTransform _content;
        private float _rowH;
        private int _count;
        private int _slots;
        private RectTransform[] _rows;
        private int _first = -1;
        private float _width;
        private Action<int, int> _bind;
        private Action<int, RectTransform> _create;

        public RectTransform Root;

        public void Build(Transform parent, float x, float y, float width, float height,
            float rowHeight, int slots, Action<int, RectTransform> createRow, Action<int, int> bindRow)
        {
            _rowH = rowHeight;
            _slots = slots;
            _width = width;
            _create = createRow;
            _bind = bindRow;

            Image frame = Ui.Img("List", parent, new Color(0f, 0f, 0f, 0.4f), Ui.Square);
            Root = frame.rectTransform;
            Ui.SetRect(Root, x, y, width, height);
            frame.raycastTarget = true;

            Ui.Border(Root, Theme.Line, 2f);

            RectTransform viewport = Ui.Rect("Viewport", Root);
            Ui.Stretch(viewport, 2f, 2f, 2f, 2f);
            viewport.gameObject.AddComponent<RectMask2D>();

            Image vp = viewport.gameObject.AddComponent<Image>();
            vp.color = new Color(0f, 0f, 0f, 0.001f);
            vp.sprite = Ui.Square;
            vp.raycastTarget = true;

            _content = Ui.Rect("Content", viewport);
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.anchoredPosition = Vector2.zero;
            _content.sizeDelta = new Vector2(0f, 100f);

            _scroll = Root.gameObject.AddComponent<ScrollRect>();
            _scroll.content = _content;
            _scroll.viewport = viewport;
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.inertia = false;
            _scroll.scrollSensitivity = 45f;
            _scroll.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<Vector2>(OnScroll));

            _rows = new RectTransform[_slots];
            for (int i = 0; i < _slots; i++)
            {
                RectTransform row = Ui.Rect("Row" + i, _content);
                Ui.SetRect(row, 0f, i * rowHeight, width - 8f, rowHeight - 1f);
                row.gameObject.SetActive(false);
                _rows[i] = row;

                if (_create != null) _create(i, row);
            }
        }

        private void OnScroll(Vector2 value)
        {
            Refresh(false);
        }

        public void SetCount(int count)
        {
            _count = count;
            _content.sizeDelta = new Vector2(0f, Mathf.Max(_rowH * _slots, _count * _rowH) + 4f);
            _content.anchoredPosition = Vector2.zero;
            _first = -1;
            Refresh(true);
        }

        public void Refresh(bool force)
        {
            if (_rows == null) return;

            int first = Mathf.FloorToInt(_content.anchoredPosition.y / _rowH);
            int maxFirst = Mathf.Max(0, _count - _slots);
            if (first < 0) first = 0;
            if (first > maxFirst) first = maxFirst;

            if (!force && first == _first)
                return;

            _first = first;

            for (int slot = 0; slot < _slots; slot++)
            {
                RectTransform row = _rows[slot];
                int data = first + slot;

                if (data >= _count)
                {
                    row.gameObject.SetActive(false);
                    continue;
                }

                row.gameObject.SetActive(true);
                Ui.SetRect(row, 0f, data * _rowH, _width - 8f, _rowH - 1f);
                if (_bind != null) _bind(slot, data);
            }
        }
    }

    /// <summary>Captures the next pressed key for a rebind row.</summary>
    internal sealed class KeyCatcher : MonoBehaviour
    {
        private static KeyCatcher _instance;
        private Action<KeyCode> _callback;

        public static void Begin(Action<KeyCode> callback)
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AtlyssCheatKeyCatcher");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<KeyCatcher>();
            }

            _instance._callback = callback;
        }

        private void Update()
        {
            if (_callback == null || !Input.anyKeyDown) return;

            KeyCode[] all = (KeyCode[])Enum.GetValues(typeof(KeyCode));

            for (int i = 0; i < all.Length; i++)
            {
                KeyCode key = all[i];
                if (key == KeyCode.None) continue;
                if (key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6) continue;

                if (Input.GetKeyDown(key))
                {
                    Action<KeyCode> callback = _callback;
                    _callback = null;
                    if (callback != null) callback(key);
                    return;
                }
            }
        }
    }

    /// <summary>Label + key button. Click and press any key to rebind.</summary>
    internal sealed class KeyRow
    {
        public RectTransform Root;

        private UiText _value;
        private Func<KeyCode> _get;
        private Action<KeyCode> _set;
        private bool _waiting;

        public KeyRow Build(Transform parent, float width, string label, Func<KeyCode> get, Action<KeyCode> set)
        {
            _get = get;
            _set = set;

            Image outer = Ui.Img("KeyRow", parent, Theme.Row, Ui.Square);
            Root = outer.rectTransform;
            Root.sizeDelta = new Vector2(width, 34f);

            Ui.Border(Root, Theme.Line, 2f);

            UiText text = Ui.Label("Label", outer.transform, label, 17f, Theme.Text, TextAlignmentOptions.Left);
            Ui.Stretch(text.rectTransform, 12f, 0f, 230f, 0f);

            Image box = Ui.Img("Value", outer.transform, new Color(0f, 0f, 0f, 0.5f), Ui.Square);
            Ui.Border(box.rectTransform, Theme.Line, 1f);
            Ui.SetRect(box.rectTransform, width - 214f, 4f, 200f, 24f);
            box.raycastTarget = true;

            _value = Ui.Label("Text", box.transform, "", 16f, Theme.Accent2, TextAlignmentOptions.Center);
            Ui.Stretch(_value.rectTransform, 2f, 0f, 2f, 0f);

            Button btn = box.gameObject.AddComponent<Button>();
            btn.targetGraphic = box;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(new UnityEngine.Events.UnityAction(StartWait));

            Refresh();
            return this;
        }

        private void StartWait()
        {
            _waiting = true;
            UiInput.Clear();
            KeyCatcher.Begin(Assign);
            Refresh();
        }

        private void Assign(KeyCode key)
        {
            _waiting = false;
            if (_set != null) _set(key);
            Refresh();
        }

        public void Refresh()
        {
            if (_value == null) return;

            if (_waiting)
            {
                _value.text = L10n.T("press a key...", "нажми клавишу...");
                _value.color = Theme.Amber;
                return;
            }

            KeyCode key = _get();
            _value.text = key == KeyCode.None ? "-" : key.ToString();
            _value.color = Theme.Accent2;
        }
    }
}
