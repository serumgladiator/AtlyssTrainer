using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AtlyssTrainer
{
    /// <summary>Small always-on-top readout of the cheats that are currently active.</summary>
    internal sealed class Overlay : MonoBehaviour
    {
        private static Overlay _instance;

        private GameObject _root;
        private Image _background;
        private UiText _text;
        private float _timer;

        public static void Tick()
        {
            if (!Settings.ShowOverlay.Value || GameApi.LocalPlayer == null)
            {
                if (_instance != null) _instance.Hide();
                return;
            }

            if (_instance == null)
            {
                GameObject go = new GameObject("AtlyssCheatOverlay");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<Overlay>();
                _instance.Build();
            }

            _instance._timer -= Time.unscaledDeltaTime;
            if (_instance._timer > 0f) return;
            _instance._timer = 0.25f;

            _instance.Refresh();
        }

        private void Build()
        {
            Ui.Init();

            _root = new GameObject("OverlayCanvas", typeof(RectTransform));
            _root.layer = 5;
            DontDestroyOnLoad(_root);

            Canvas canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5100;

            CanvasScaler scaler = _root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            _background = Ui.Img("OverlayBg", _root.transform, new Color(0.04f, 0.035f, 0.055f, 0.82f), Ui.Square);
            _background.raycastTarget = false;
            Ui.SetRect(_background.rectTransform, 24f, 24f, 300f, 120f);

            Image accent = Ui.Img("Accent", _background.transform, Theme.Accent, Ui.Square);
            accent.raycastTarget = false;
            Ui.SetRect(accent.rectTransform, 0f, 0f, 2.5f, 120f);

            _text = Ui.Label("OverlayText", _background.transform, "", 15f, Theme.Text,
                TextAlignmentOptions.TopLeft);
            _text.raycastTarget = false;
            _text.enableWordWrapping = false;
            Ui.SetRect(_text.rectTransform, 14f, 10f, 274f, 100f);
        }

        private void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        private void Refresh()
        {
            if (_root == null) return;

            List<string> lines = new List<string>();

            if (Settings.GodMode.Value) lines.Add(L10n.T("GOD MODE", "БЕССМЕРТИЕ"));
            if (Settings.OneHitKill.Value) lines.Add(L10n.T("ONE HIT", "ВАНШОТ"));
            if (Settings.DamageMultiplier.Value > 1.001f)
                lines.Add(L10n.T("DAMAGE x", "УРОН x") + Mathf.RoundToInt(Settings.DamageMultiplier.Value));
            if (Settings.InfiniteMana.Value) lines.Add(L10n.T("INFINITE MANA", "БЕСК. МАНА"));
            if (Settings.InfiniteStamina.Value) lines.Add(L10n.T("INFINITE STAMINA", "БЕСК. ВЫНОСЛИВОСТЬ"));
            if (Settings.NoCooldowns.Value) lines.Add(L10n.T("NO COOLDOWNS", "БЕЗ КД"));
            if (Settings.InstantCast.Value) lines.Add(L10n.T("INSTANT CAST", "МГНОВЕННЫЙ КАСТ"));
            if (Settings.FreezeTime.Value) lines.Add(L10n.T("TIME FROZEN", "ВРЕМЯ СТОП"));
            if (Settings.SpeedMultiplier.Value > 1.001f)
                lines.Add(L10n.T("SPEED ", "СКОРОСТЬ ") + Mathf.RoundToInt(Settings.SpeedMultiplier.Value * 100f) + "%");
            if (Settings.JumpMultiplier.Value > 1.001f)
                lines.Add(L10n.T("JUMP ", "ПРЫЖОК ") + Mathf.RoundToInt(Settings.JumpMultiplier.Value * 100f) + "%");
            if (Settings.Noclip.Value) lines.Add("NOCLIP");
            if (Settings.FreezeAI.Value) lines.Add(L10n.T("ENEMIES FROZEN", "ВРАГИ ЗАМОРОЖЕНЫ"));
            if (Settings.NoCurrencyLoss.Value) lines.Add(L10n.T("FREE SHOPPING", "БЕСПЛАТНЫЕ ПОКУПКИ"));

            if (lines.Count == 0)
            {
                Hide();
                return;
            }

            _root.SetActive(true);

            StringBuilder sb = new StringBuilder();
            sb.Append("<color=#E0BC6E>").Append(L10n.T("ACTIVE CHEATS", "АКТИВНЫЕ ЧИТЫ")).Append("</color>\n");
            for (int i = 0; i < lines.Count; i++)
                sb.Append(lines[i]).Append('\n');

            _text.text = sb.ToString();

            float height = 34f + lines.Count * 21f;
            _background.rectTransform.sizeDelta = new Vector2(300f, height);
            _background.transform.Find("Accent").GetComponent<RectTransform>().sizeDelta = new Vector2(2.5f, height);
        }
    }
}
