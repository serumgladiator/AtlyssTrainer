using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AtlyssTrainer
{
    /// <summary>Risky actions live here and stay locked until the user explicitly confirms.</summary>
    internal sealed class ExperimentalTab : Tab
    {
        private RectTransform _area;
        private float _width;
        private float _height;

        private RectTransform _warning;
        private ToggleRow _ack;
        private Button _unlock;
        private ValueRow _scale;
        private UiText _status;

        public override string Title
        {
            get { return L10n.T("Experimental", "Экспериментально"); }
        }

        public override string Hint
        {
            get { return L10n.T("Risky actions. Backup first.", "Рискованные действия. Сначала бэкап."); }
        }

        public override void Build(RectTransform area, float width, float height)
        {
            _area = area;
            _width = width;
            _height = height;

            BuildWarning();
        }

        private void BuildWarning()
        {
            _warning = Ui.Rect("Warning", _area);
            Ui.Stretch(_warning, 0f, 0f, 0f, 0f);

            float y = 0f;
            TabUi.Section(_warning, 0f, y, _width, L10n.T("EXPERIMENTAL", "ЭКСПЕРИМЕНТАЛЬНО"));
            y += 34f;

            Image panel = Ui.Img("Danger", _warning, new Color(0.19f, 0.09f, 0.1f, 1f), Ui.Square);
            Ui.SetRect(panel.rectTransform, 0f, y, _width, 170f);

            Image line = Ui.Img("DangerLine", panel.transform, Theme.Danger, Ui.Square);
            Ui.SetRect(line.rectTransform, 0f, 0f, 3f, 170f);

            UiText text = Ui.Label("Text", panel.transform,
                L10n.T(
                    "These functions write into the character profile and can corrupt the save.\n" +
                    "Make a backup first. The mod cannot undo the result.\n" +
                    "Unlock lasts only for this game session.",
                    "Эти функции пишут в профиль персонажа и могут повредить сейв.\n" +
                    "Сначала сделай бэкап. Мод не сможет отменить результат.\n" +
                    "Разблокировка действует только до перезапуска игры."),
                15f, new Color(0.98f, 0.82f, 0.8f), TextAlignmentOptions.TopLeft);
            Ui.SetRect(text.rectTransform, 18f, 14f, _width - 36f, 92f);
            text.enableWordWrapping = true;

            Button backup = Ui.Button(_warning, L10n.T("Backup profile now", "Сделать бэкап профиля"), DoBackup, 15f);
            Ui.SetRect(backup.GetComponent<RectTransform>(), 16f, 118f, 300f, 36f);

            y += 182f;

            _ack = TabUi.Toggle(_warning, 0f, y, _width,
                L10n.T("I understand the risk and made a backup", "Я понимаю риск и сделал бэкап"),
                delegate { return _ackValue; },
                delegate (bool v) { _ackValue = v; }, null);
            y += 52f;

            _unlock = TabUi.Action(_warning, 0f, y, _width, 44f,
                L10n.T("Unlock experimental functions", "Разблокировать экспериментальные функции"),
                Unlock);
            Ui.SetButtonColor(_unlock, new Color(0.28f, 0.12f, 0.13f));

            y += 56f;

            _status = Ui.Label("Status", _warning, "", 13f, Theme.Muted, TextAlignmentOptions.TopLeft);
            Ui.SetRect(_status.rectTransform, 0f, y, _width, 40f);
            _status.enableWordWrapping = true;
        }

        private bool _ackValue;

        private void DoBackup()
        {
            string path = Backup.Profiles();

            if (_status != null)
            {
                _status.text = path == null
                    ? L10n.T("Backup failed, see the log.", "Бэкап не удался, смотри лог.")
                    : L10n.T("Backup saved: ", "Бэкап сохранён: ") + path;
                _status.color = path == null ? Theme.Danger : Theme.Good;
            }

            if (Plugin.MainMenu != null)
            {
                Plugin.MainMenu.Toast(path == null
                    ? L10n.T("Backup failed", "Бэкап не удался")
                    : L10n.T("Backup saved", "Бэкап сохранён"));
            }
        }

        private void Unlock()
        {
            if (!_ackValue) return;

            if (_warning != null)
                UnityEngine.Object.Destroy(_warning.gameObject);

            BuildContent();
        }

        private void BuildContent()
        {
            RectTransform content = Ui.ScrollArea(_area, 0f, 0f, _width, _height, out _unusedScroll);
            float w = _width - 20f;
            float y = 0f;

            TabUi.Section(content, 0f, y, w, L10n.T("DANGEROUS ACTIONS", "ОПАСНЫЕ ДЕЙСТВИЯ"));
            y += 34f;

            AddAction(content, ref y, w,
                L10n.T("Give every item (fills inventory)", "Выдать все предметы (забивает инвентарь)"),
                delegate { Report(GameApi.GiveAllItems()); });

            AddAction(content, ref y, w,
                L10n.T("Complete active quests", "Завершить активные квесты"),
                delegate { Report(GameApi.CompleteActiveQuests()); });

            AddAction(content, ref y, w,
                L10n.T("Unlock every skill", "Открыть все навыки"),
                delegate { Report(GameApi.UnlockAllSkills()); });

            AddAction(content, ref y, w,
                L10n.T("Unlock every waypoint", "Открыть все вейпоинты"),
                delegate { Report(GameApi.UnlockAllWaypoints()); });

            AddAction(content, ref y, w,
                L10n.T("Force profile save", "Принудительно сохранить профиль"),
                delegate
                {
                    GameApi.SaveProfile();
                    Report(L10n.T("Profile saved", "Профиль сохранён"));
                });

            y += 10f;
            TabUi.Section(content, 0f, y, w, L10n.T("CHARACTER SCALE", "МАСШТАБ ПЕРСОНАЖА"));
            y += 34f;

            _scale = TabUi.Value(content, 0f, y, w, L10n.T("Scale", "Масштаб"),
                delegate
                {
                    Player p = GameApi.LocalPlayer;
                    return p == null ? 100 : Mathf.RoundToInt(p.transform.localScale.x * 100f);
                },
                delegate (int v) { Report(GameApi.SetCharacterScale(v / 100f)); },
                30, 300, 10, "%");
            y += 50f;

            UiText note = Ui.Label("Note", content,
                L10n.T("Changing the scale also changes the hitbox height; the game may clip you into geometry.",
                    "Изменение масштаба меняет высоту хитбокса; игра может засунуть тебя в геометрию."),
                12f, Theme.Muted, TextAlignmentOptions.TopLeft);
            Ui.SetRect(note.rectTransform, 0f, y, w, 40f);
            note.enableWordWrapping = true;
            y += 46f;

            content.sizeDelta = new Vector2(0f, y + 8f);
        }

        private ScrollRect _unusedScroll;

        private void AddAction(RectTransform parent, ref float y, float width, string label, Action action)
        {
            Button b = TabUi.Action(parent, 0f, y, width, 40f, label, delegate
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Experimental action: " + e);
                    Report(L10n.T("Action failed, see log", "Действие не удалось, смотри лог"));
                }
            });

            Ui.SetButtonColor(b, new Color(0.2f, 0.13f, 0.14f));
            y += 48f;
        }

        private static void Report(string message)
        {
            if (Plugin.MainMenu != null) Plugin.MainMenu.Toast(message);
        }

        public override void Refresh()
        {
            if (_ack != null) _ack.Refresh();
            if (_scale != null) _scale.Refresh();

            if (_unlock != null)
            {
                bool acked = _ackValue;
                _unlock.interactable = acked;
                Ui.SetLabel(_unlock, acked
                    ? L10n.T("Unlock experimental functions", "Разблокировать экспериментальные функции")
                    : L10n.T("Check the box to unlock", "Поставь галочку для разблокировки"));
            }
        }
    }
}
