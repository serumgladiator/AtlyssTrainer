using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AtlyssTrainer
{
    internal static class TabUi
    {
        public static void Section(RectTransform parent, float x, float y, float width, string title)
        {
            UiText t = Ui.Label("Section", parent, title, 14f, Theme.Accent, TextAlignmentOptions.Left);
            Ui.SetRect(t.rectTransform, x, y, width, 20f);

            Image line = Ui.Img("Line", parent, Theme.Line, Ui.Square);
            Ui.SetRect(line.rectTransform, x, y + 22f, width, 1f);
        }

        public static Button Action(RectTransform parent, float x, float y, float width, float height,
            string label, Action onClick)
        {
            Button b = Ui.Button(parent, label, onClick, 17f);
            Ui.SetRect(b.GetComponent<RectTransform>(), x, y, width, height);
            return b;
        }

        public static ValueRow Value(RectTransform parent, float x, float y, float width, string label,
            Func<int> get, Action<int> set, int min, int max, int step, string suffix)
        {
            ValueRow row = new ValueRow();
            row.Build(parent, width, label, get, set, min, max, step, suffix);
            Ui.SetRect(row.Root, x, y, width, 38f);
            return row;
        }

        public static ToggleRow Toggle(RectTransform parent, float x, float y, float width, string label,
            Func<bool> get, Action<bool> set, string hint)
        {
            ToggleRow row = new ToggleRow();
            row.Build(parent, width, label, get, set, hint);
            Ui.SetRect(row.Root, x, y, width, 40f);
            return row;
        }
    }

    // ------------------------------------------------------------------ cheats

    internal sealed class CheatsTab : Tab
    {
        private ToggleRow _god, _oneHit, _mana, _stamina, _freeze, _keepBuffs, _dash;
        private ToggleRow _crit, _accuracy, _parry, _reflect, _noCd, _instant, _purge, _noclip;
        private ValueRow _damage, _speed, _jump, _jumps;
        private ValueRow _incoming, _reflectPercent, _absorb, _gravity;
        private UiText _mode;

        public override string Title
        {
            get { return L10n.T("Cheats", "Читы"); }
        }

        public override string Hint
        {
            get { return L10n.T("Combat, survival and movement", "Бой, выживание и передвижение"); }
        }

        public override void Build(RectTransform area, float width, float height)
        {
            RectTransform content = Ui.ScrollArea(area, 0f, 0f, width, height, out _scroll);
            float w = width - 20f;
            float y = 0f;

            TabUi.Section(content, 0f, y, w, L10n.T("COMBAT", "БОЙ"));
            y += 32f;

            _god = TabUi.Toggle(content, 0f, y, w,
                L10n.T("God mode", "Бессмертие"),
                delegate { return Settings.GodMode.Value; },
                delegate (bool v) { Settings.GodMode.Value = v; },
                L10n.T("damage is ignored", "урон игнорируется"));
            y += 46f;

            _oneHit = TabUi.Toggle(content, 0f, y, w,
                L10n.T("One hit kill", "Ваншот"),
                delegate { return Settings.OneHitKill.Value; },
                delegate (bool v) { Settings.OneHitKill.Value = v; },
                L10n.T("host only", "только хост"));
            y += 46f;

            _damage = TabUi.Value(content, 0f, y, w,
                L10n.T("Damage multiplier", "Множитель урона"),
                delegate { return Mathf.RoundToInt(Settings.DamageMultiplier.Value); },
                delegate (int v) { Settings.DamageMultiplier.Value = v; },
                1, 100, 1, "x");
            y += 46f;

            _incoming = TabUi.Value(content, 0f, y, w,
                L10n.T("Incoming damage", "Получаемый урон"),
                delegate { return Mathf.RoundToInt(Settings.IncomingMultiplier.Value * 100f); },
                delegate (int v) { Settings.IncomingMultiplier.Value = v / 100f; },
                0, 200, 5, "%");
            y += 46f;

            _crit = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Always critical", "Всегда крит"),
                delegate { return Settings.AlwaysCrit.Value; },
                delegate (bool v) { Settings.AlwaysCrit.Value = v; }, null);
            y += 46f;

            _accuracy = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Never miss", "Без промахов"),
                delegate { return Settings.PerfectAccuracy.Value; },
                delegate (bool v) { Settings.PerfectAccuracy.Value = v; }, null);
            y += 46f;

            _parry = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Auto parry", "Автопарирование"),
                delegate { return Settings.AutoParry.Value; },
                delegate (bool v) { Settings.AutoParry.Value = v; }, null);
            y += 46f;

            _reflect = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Reflect damage", "Отражать урон"),
                delegate { return Settings.ReflectDamage.Value; },
                delegate (bool v) { Settings.ReflectDamage.Value = v; }, null);
            y += 46f;

            _reflectPercent = TabUi.Value(content, 0f, y, w,
                L10n.T("Reflect percent", "Процент отражения"),
                delegate { return Mathf.RoundToInt(Settings.ReflectPercent.Value); },
                delegate (int v) { Settings.ReflectPercent.Value = v; },
                0, 1000, 10, "%");
            y += 46f;

            _absorb = TabUi.Value(content, 0f, y, w,
                L10n.T("Damage absorption", "Поглощение урона"),
                delegate { return Mathf.RoundToInt(Settings.DamageAbsorption.Value * 100f); },
                delegate (int v) { Settings.DamageAbsorption.Value = v / 100f; },
                0, 100, 5, "%");
            y += 46f;

            _noCd = TabUi.Toggle(content, 0f, y, w,
                L10n.T("No skill cooldowns", "Без перезарядки навыков"),
                delegate { return Settings.NoCooldowns.Value; },
                delegate (bool v) { Settings.NoCooldowns.Value = v; }, null);
            y += 46f;

            _instant = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Instant cast", "Мгновенный каст"),
                delegate { return Settings.InstantCast.Value; },
                delegate (bool v) { Settings.InstantCast.Value = v; }, null);
            y += 46f;

            _purge = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Purge debuffs", "Снимать дебаффы"),
                delegate { return Settings.PurgeConditions.Value; },
                delegate (bool v) { Settings.PurgeConditions.Value = v; }, null);
            y += 56f;

            _mana = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Infinite mana", "Бесконечная мана"),
                delegate { return Settings.InfiniteMana.Value; },
                delegate (bool v) { Settings.InfiniteMana.Value = v; }, null);
            y += 46f;

            _stamina = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Infinite stamina", "Бесконечная выносливость"),
                delegate { return Settings.InfiniteStamina.Value; },
                delegate (bool v) { Settings.InfiniteStamina.Value = v; }, null);
            y += 46f;

            _keepBuffs = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Health lock", "Блокировка здоровья"),
                delegate { return Settings.KeepBuffs.Value; },
                delegate (bool v) { Settings.KeepBuffs.Value = v; },
                L10n.T("works for clients", "работает и у клиента"));
            y += 56f;

            TabUi.Section(content, 0f, y, w, L10n.T("MOVEMENT", "ПЕРЕДВИЖЕНИЕ"));
            y += 32f;

            _speed = TabUi.Value(content, 0f, y, w,
                L10n.T("Movement speed", "Скорость движения"),
                delegate { return Mathf.RoundToInt(Settings.SpeedMultiplier.Value * 100f); },
                delegate (int v) { Settings.SpeedMultiplier.Value = v / 100f; },
                100, 1000, 10, "%");
            y += 46f;

            _jump = TabUi.Value(content, 0f, y, w,
                L10n.T("Jump height", "Высота прыжка"),
                delegate { return Mathf.RoundToInt(Settings.JumpMultiplier.Value * 100f); },
                delegate (int v) { Settings.JumpMultiplier.Value = v / 100f; },
                100, 1000, 10, "%");
            y += 46f;

            _jumps = TabUi.Value(content, 0f, y, w,
                L10n.T("Max jumps", "Прыжков подряд"),
                delegate { return Settings.MaxJumps.Value; },
                delegate (int v) { Settings.MaxJumps.Value = v; },
                1, 20, 1, "");
            y += 46f;

            _dash = TabUi.Toggle(content, 0f, y, w,
                L10n.T("No dash cooldown", "Без перезарядки рывка"),
                delegate { return Settings.NoDashCooldown.Value; },
                delegate (bool v) { Settings.NoDashCooldown.Value = v; }, null);
            y += 46f;

            _gravity = TabUi.Value(content, 0f, y, w,
                L10n.T("Gravity", "Гравитация"),
                delegate { return Mathf.RoundToInt(Settings.GravityScale.Value * 100f); },
                delegate (int v) { Settings.GravityScale.Value = v / 100f; },
                0, 500, 10, "%");
            y += 46f;

            _noclip = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Noclip / fly", "Noclip / полёт"),
                delegate { return Settings.Noclip.Value; },
                delegate (bool v) { Settings.Noclip.Value = v; },
                L10n.T("WASD + space, host only", "WASD + пробел, только хост"));
            y += 56f;

            TabUi.Section(content, 0f, y, w, L10n.T("WORLD", "МИР"));
            y += 32f;

            _freeze = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Freeze time", "Остановить время"),
                delegate { return Settings.FreezeTime.Value; },
                delegate (bool v) { Settings.FreezeTime.Value = v; },
                L10n.T("host only", "только хост"));
            y += 46f;

            _mode = Ui.Label("Mode", content, "", 13f, Theme.Muted, TextAlignmentOptions.Left);
            Ui.SetRect(_mode.rectTransform, 0f, y, w, 36f);
            y += 46f;

            float bw = (w - 12f) * 0.5f;
            TabUi.Action(content, 0f, y, bw, 42f,
                L10n.T("Restore all", "Восстановить всё"),
                delegate
                {
                    string result = GameApi.Replenish();
                    if (Plugin.MainMenu != null) Plugin.MainMenu.Toast(result);
                });

            TabUi.Action(content, bw + 12f, y, bw, 42f,
                L10n.T("Kill nearby enemies", "Убить врагов рядом"),
                delegate
                {
                    int killed = GameApi.KillNearbyCreeps(25f);
                    if (Plugin.MainMenu != null)
                        Plugin.MainMenu.Toast(L10n.T("Enemies killed: ", "Убито врагов: ") + killed);
                });

            y += 50f;

            TabUi.Action(content, 0f, y, w, 42f,
                L10n.T("Kill every enemy on the map", "Убить всех врагов на карте"),
                delegate
                {
                    int killed = GameApi.KillAllCreeps();
                    if (Plugin.MainMenu != null)
                        Plugin.MainMenu.Toast(L10n.T("Enemies killed: ", "Убито врагов: ") + killed);
                });

            y += 50f;
            content.sizeDelta = new Vector2(0f, y + 8f);
        }

        private ScrollRect _scroll;

        public override void Refresh()
        {
            if (_god != null) _god.Refresh();
            if (_oneHit != null) _oneHit.Refresh();
            if (_mana != null) _mana.Refresh();
            if (_stamina != null) _stamina.Refresh();
            if (_freeze != null) _freeze.Refresh();
            if (_keepBuffs != null) _keepBuffs.Refresh();
            if (_dash != null) _dash.Refresh();
            if (_damage != null) _damage.Refresh();
            if (_speed != null) _speed.Refresh();
            if (_jump != null) _jump.Refresh();
            if (_jumps != null) _jumps.Refresh();
            if (_crit != null) _crit.Refresh();
            if (_accuracy != null) _accuracy.Refresh();
            if (_parry != null) _parry.Refresh();
            if (_reflect != null) _reflect.Refresh();
            if (_noCd != null) _noCd.Refresh();
            if (_instant != null) _instant.Refresh();
            if (_purge != null) _purge.Refresh();
            if (_noclip != null) _noclip.Refresh();
            if (_incoming != null) _incoming.Refresh();
            if (_reflectPercent != null) _reflectPercent.Refresh();
            if (_absorb != null) _absorb.Refresh();
            if (_gravity != null) _gravity.Refresh();

            if (_mode != null)
            {
                _mode.text = GameApi.IsHost
                    ? L10n.T("Multiplayer rules apply: host powers are live.", "Вы хост: серверные читы активны.")
                    : L10n.T("You are a client: only local values can be changed.",
                        "Вы клиент: доступны только локальные изменения.");
            }
        }
    }

    // ------------------------------------------------------------------ character

    internal sealed class CharacterTab : Tab
    {
        private ScrollRect _scroll;
        private UiText _info;
        private ValueRow _level, _exp, _gold, _attr, _skills;
        private readonly System.Collections.Generic.List<ValueRow> _stats = new System.Collections.Generic.List<ValueRow>();
        private readonly System.Collections.Generic.List<Button> _classButtons = new System.Collections.Generic.List<Button>();
        private readonly System.Collections.Generic.List<ScriptablePlayerBaseClass> _classes =
            new System.Collections.Generic.List<ScriptablePlayerBaseClass>();
        private readonly System.Collections.Generic.List<ValueRow> _professionRows =
            new System.Collections.Generic.List<ValueRow>();
        private readonly System.Collections.Generic.List<ValueRow> _appearanceRows =
            new System.Collections.Generic.List<ValueRow>();
        private readonly System.Collections.Generic.List<ToggleRow> _hideRows =
            new System.Collections.Generic.List<ToggleRow>();
        private ValueRow _tier;

        private static void Toast(string message)
        {
            if (Plugin.MainMenu != null) Plugin.MainMenu.Toast(message);
        }

        private static readonly string[,] StatRows =
        {
            { "hp", "Health", "Здоровье" },
            { "mana", "Mana", "Мана" },
            { "stamina", "Stamina", "Выносливость" },
            { "attack", "Attack power", "Сила атаки" },
            { "magic", "Magic power", "Сила магии" },
            { "dex", "Dexterity power", "Ловкость" },
            { "defense", "Defense", "Защита" },
            { "mdefense", "Magic defense", "Маг. защита" },
            { "crit", "Critical rate", "Крит. шанс" },
            { "mcrit", "Magic crit rate", "Маг. крит" },
            { "evasion", "Evasion", "Уклонение" },
            { "fireres", "Fire resist", "Сопр. огню" },
            { "waterres", "Water resist", "Сопр. воде" },
            { "natureres", "Nature resist", "Сопр. природе" },
            { "earthres", "Earth resist", "Сопр. земле" },
            { "holyres", "Holy resist", "Сопр. свету" },
            { "shadowres", "Shadow resist", "Сопр. тьме" }
        };

        public override string Title
        {
            get { return L10n.T("Character", "Персонаж"); }
        }

        public override string Hint
        {
            get { return L10n.T("Level, currency and every stat", "Уровень, валюта и все характеристики"); }
        }

        public override void Build(RectTransform area, float width, float height)
        {
            RectTransform content = Ui.ScrollArea(area, 0f, 0f, width, height, out _scroll);
            float w = width - 20f;
            float y = 4f;

            _info = Ui.Label("Info", content, "", 15f, Theme.Text, TextAlignmentOptions.Left);
            Ui.SetRect(_info.rectTransform, 0f, y, w, 40f);
            y += 46f;

            TabUi.Section(content, 0f, y, w, L10n.T("CLASS", "КЛАСС"));
            y += 32f;

            _classes.Clear();
            _classes.AddRange(GameApi.Classes);

            if (_classes.Count > 0)
            {
                int count = Mathf.Min(_classes.Count, 4);
                float cbw = (w - (count - 1) * 10f) / count;

                for (int i = 0; i < count; i++)
                {
                    ScriptablePlayerBaseClass cls = _classes[i];
                    Button b = TabUi.Action(content, i * (cbw + 10f), y, cbw, 38f,
                        L10n.Game(cls._className),
                        delegate
                        {
                            string msg = GameApi.SetClass(cls);
                            if (Plugin.MainMenu != null) Plugin.MainMenu.Toast(msg);
                        });
                    _classButtons.Add(b);
                }

                y += 46f;
            }
            else
            {
                UiText none = Ui.Label("NoClasses", content,
                    L10n.T("Classes are not loaded yet", "Классы ещё не загружены"),
                    13f, Theme.Muted, TextAlignmentOptions.Left);
                Ui.SetRect(none.rectTransform, 0f, y, w, 20f);
                y += 28f;
            }

            _tier = TabUi.Value(content, 0f, y, w, L10n.T("Class tier", "Ранг класса"),
                delegate { PlayerStats s = GameApi.Stats; return s == null ? 0 : s._syncClassTier; },
                delegate (int v) { Toast(GameApi.SetClassTier(v)); },
                0, 9, 1, "");
            y += 50f;

            TabUi.Section(content, 0f, y, w, L10n.T("PROGRESSION", "ПРОГРЕСС"));
            y += 32f;

            _level = TabUi.Value(content, 0f, y, w, L10n.T("Level", "Уровень"),
                delegate { PlayerStats s = GameApi.Stats; return s == null ? 1 : s._currentLevel; },
                delegate (int v) { GameApi.SetLevel(v); },
                1, 999, 1, "");
            y += 44f;

            _exp = TabUi.Value(content, 0f, y, w, L10n.T("Experience", "Опыт"),
                delegate { PlayerStats s = GameApi.Stats; return s == null ? 0 : s._currentExp; },
                delegate (int v) { GameApi.SetExp(v); },
                0, 99999999, 100, "");
            y += 44f;

            _gold = TabUi.Value(content, 0f, y, w, L10n.T("Gold", "Золото"),
                delegate { PlayerInventory inv = GameApi.Inventory; return inv == null ? 0 : inv._heldCurrency; },
                delegate (int v) { GameApi.SetCurrency(v); },
                0, 99999999, 100, "");
            y += 44f;

            _attr = TabUi.Value(content, 0f, y, w, L10n.T("Attribute points", "Очки атрибутов"),
                delegate { PlayerStats s = GameApi.Stats; return s == null ? 0 : s._currentAttributePoints; },
                delegate (int v) { GameApi.SetAttributePoints(v); },
                0, 9999, 1, "");
            y += 44f;

            _skills = TabUi.Value(content, 0f, y, w, L10n.T("Skill points", "Очки навыков"),
                delegate { PlayerStats s = GameApi.Stats; return s == null ? 0 : s._currentSkillPoints; },
                delegate (int v) { GameApi.SetSkillPoints(v); },
                0, 9999, 1, "");
            y += 54f;

            TabUi.Section(content, 0f, y, w, L10n.T("STATS", "ХАРАКТЕРИСТИКИ"));
            y += 26f;

            UiText note = Ui.Label("StatNote", content,
                L10n.T("Values are live; equipment or level changes recalculate them.",
                    "Значения применяются сразу; смена экипировки или уровня пересчитает их."),
                12f, Theme.Muted, TextAlignmentOptions.Left);
            Ui.SetRect(note.rectTransform, 0f, y, w, 18f);
            y += 22f;

            float col = (w - 12f) * 0.5f;
            for (int i = 0; i < StatRows.GetLength(0); i++)
            {
                string key = StatRows[i, 0];
                string label = L10n.T(StatRows[i, 1], StatRows[i, 2]);

                bool percent = key == "crit" || key == "mcrit" || key == "evasion";
                bool health = key == "hp" || key == "mana" || key == "stamina";

                ValueRow row = TabUi.Value(content, (i % 2) * (col + 12f), y, col, label,
                    MakeGetter(key),
                    MakeSetter(key),
                    0, health ? 999999 : (percent ? 100 : 99999),
                    health ? 100 : (percent ? 1 : 10),
                    percent ? "%" : "");
                _stats.Add(row);

                if (i % 2 == 1) y += 44f;
            }

            if (StatRows.GetLength(0) % 2 == 1) y += 44f;

            TabUi.Section(content, 0f, y, w, L10n.T("PROFESSIONS", "ПРОФЕССИИ"));
            y += 32f;

            System.Collections.Generic.List<ProfessionStruct> professions = GameApi.Professions;

            if (professions.Count == 0)
            {
                UiText noProf = Ui.Label("NoProf", content,
                    L10n.T("Professions are not loaded yet", "Профессии ещё не загружены"),
                    13f, Theme.Muted, TextAlignmentOptions.Left);
                Ui.SetRect(noProf.rectTransform, 0f, y, w, 20f);
                y += 28f;
            }
            else
            {
                for (int i = 0; i < professions.Count; i++)
                {
                    int index = i;
                    ValueRow row = TabUi.Value(content, 0f, y, w,
                        L10n.T("Profession #", "Профессия #") + professions[i]._professionID,
                        delegate
                        {
                            var list = GameApi.Professions;
                            return index < list.Count ? list[index]._professionLvl : 0;
                        },
                        delegate (int v) { GameApi.SetProfessionLevel(index, v); },
                        0, 99, 1, "");
                    _professionRows.Add(row);
                    y += 44f;
                }
            }

            y += 6f;
            TabUi.Section(content, 0f, y, w, L10n.T("APPEARANCE", "ВНЕШНОСТЬ"));
            y += 32f;

            _appearanceRows.Add(TabUi.Value(content, 0f, y, w, L10n.T("Height", "Рост"),
                delegate { return Mathf.RoundToInt(GameApi.Appearance._heightWeight * 100f); },
                delegate (int v) { var a = GameApi.Appearance; a._heightWeight = v / 100f; GameApi.SetAppearance(a); },
                0, 300, 5, "%"));
            y += 42f;

            _appearanceRows.Add(TabUi.Value(content, 0f, y, w, L10n.T("Width", "Ширина"),
                delegate { return Mathf.RoundToInt(GameApi.Appearance._widthWeight * 100f); },
                delegate (int v) { var a = GameApi.Appearance; a._widthWeight = v / 100f; GameApi.SetAppearance(a); },
                0, 300, 5, "%"));
            y += 42f;

            _appearanceRows.Add(TabUi.Value(content, 0f, y, w, L10n.T("Torso", "Торс"),
                delegate { return Mathf.RoundToInt(GameApi.Appearance._torsoWeight * 100f); },
                delegate (int v) { var a = GameApi.Appearance; a._torsoWeight = v / 100f; GameApi.SetAppearance(a); },
                0, 300, 5, "%"));
            y += 42f;

            _appearanceRows.Add(TabUi.Value(content, 0f, y, w, L10n.T("Arms", "Руки"),
                delegate { return Mathf.RoundToInt(GameApi.Appearance._armWeight * 100f); },
                delegate (int v) { var a = GameApi.Appearance; a._armWeight = v / 100f; GameApi.SetAppearance(a); },
                0, 300, 5, "%"));
            y += 42f;

            _appearanceRows.Add(TabUi.Value(content, 0f, y, w, L10n.T("Head", "Голова"),
                delegate { return Mathf.RoundToInt(GameApi.Appearance._headWidth * 100f); },
                delegate (int v) { var a = GameApi.Appearance; a._headWidth = v / 100f; GameApi.SetAppearance(a); },
                0, 300, 5, "%"));
            y += 42f;

            _appearanceRows.Add(TabUi.Value(content, 0f, y, w, L10n.T("Belly", "Живот"),
                delegate { return Mathf.RoundToInt(GameApi.Appearance._bellyWeight * 100f); },
                delegate (int v) { var a = GameApi.Appearance; a._bellyWeight = v / 100f; GameApi.SetAppearance(a); },
                0, 300, 5, "%"));
            y += 42f;

            _appearanceRows.Add(TabUi.Value(content, 0f, y, w, L10n.T("Bottom", "Низ"),
                delegate { return Mathf.RoundToInt(GameApi.Appearance._bottomWeight * 100f); },
                delegate (int v) { var a = GameApi.Appearance; a._bottomWeight = v / 100f; GameApi.SetAppearance(a); },
                0, 300, 5, "%"));
            y += 42f;

            _appearanceRows.Add(TabUi.Value(content, 0f, y, w, L10n.T("Chest", "Грудь"),
                delegate { return Mathf.RoundToInt(GameApi.Appearance._boobWeight * 100f); },
                delegate (int v) { var a = GameApi.Appearance; a._boobWeight = v / 100f; GameApi.SetAppearance(a); },
                0, 300, 5, "%"));
            y += 46f;

            _hideRows.Add(TabUi.Toggle(content, 0f, y, w, L10n.T("Hide helm", "Скрыть шлем"),
                delegate { return GameApi.Appearance._hideHelm; },
                delegate (bool v) { var a = GameApi.Appearance; a._hideHelm = v; GameApi.SetAppearance(a); }, null));
            y += 44f;

            _hideRows.Add(TabUi.Toggle(content, 0f, y, w, L10n.T("Hide cape", "Скрыть плащ"),
                delegate { return GameApi.Appearance._hideCape; },
                delegate (bool v) { var a = GameApi.Appearance; a._hideCape = v; GameApi.SetAppearance(a); }, null));
            y += 44f;

            _hideRows.Add(TabUi.Toggle(content, 0f, y, w, L10n.T("Hide chest", "Скрыть нагрудник"),
                delegate { return GameApi.Appearance._hideChest; },
                delegate (bool v) { var a = GameApi.Appearance; a._hideChest = v; GameApi.SetAppearance(a); }, null));
            y += 44f;

            _hideRows.Add(TabUi.Toggle(content, 0f, y, w, L10n.T("Hide leggings", "Скрыть штаны"),
                delegate { return GameApi.Appearance._hideLeggings; },
                delegate (bool v) { var a = GameApi.Appearance; a._hideLeggings = v; GameApi.SetAppearance(a); }, null));
            y += 50f;

            y += 10f;
            float bw = (w - 24f) / 3f;            TabUi.Action(content, 0f, y, bw, 42f, L10n.T("Recalculate", "Пересчитать"),
                delegate
                {
                    GameApi.RecalculateStats();
                    if (Plugin.MainMenu != null)
                        Plugin.MainMenu.Toast(L10n.T("Stats recalculated", "Статы пересчитаны"));
                });

            TabUi.Action(content, bw + 12f, y, bw, 42f, L10n.T("Save profile", "Сохранить профиль"),
                delegate
                {
                    GameApi.SaveProfile();
                    if (Plugin.MainMenu != null)
                        Plugin.MainMenu.Toast(L10n.T("Profile saved", "Профиль сохранён"));
                });

            TabUi.Action(content, (bw + 12f) * 2f, y, bw, 42f, L10n.T("Full heal", "Полное лечение"),
                delegate
                {
                    GameApi.Replenish();
                    if (Plugin.MainMenu != null)
                        Plugin.MainMenu.Toast(L10n.T("Restored", "Восстановлено"));
                });

            y += 46f;
            content.sizeDelta = new Vector2(0f, y + 8f);
        }

        private static Func<int> MakeGetter(string key)
        {
            return delegate { return GameApi.GetStat(key); };
        }

        private static Action<int> MakeSetter(string key)
        {
            return delegate (int v) { GameApi.SetStat(key, v); };
        }

        public override void Refresh()
        {
            PlayerStats st = GameApi.Stats;
            if (_info != null)
            {
                if (st == null)
                {
                    _info.text = L10n.T("Load a character to edit stats.", "Загрузите персонажа, чтобы менять статы.");
                }
                else
                {
                    string cls = st._class != null ? L10n.Game(st._class._className) : "?";
                    _info.text = L10n.T("Class: ", "Класс: ") + cls +
                                 "    " + L10n.T("Tier: ", "Ранг: ") + st._syncClassTier +
                                 "    " + L10n.T("Name: ", "Имя: ") + (GameApi.LocalPlayer == null ? "" : GameApi.LocalPlayer._nickname);
                }
            }

            if (_level != null) _level.Refresh();
            if (_exp != null) _exp.Refresh();
            if (_gold != null) _gold.Refresh();
            if (_attr != null) _attr.Refresh();
            if (_skills != null) _skills.Refresh();

            for (int i = 0; i < _stats.Count; i++)
                _stats[i].Refresh();

            if (_tier != null) _tier.Refresh();

            for (int i = 0; i < _professionRows.Count; i++) _professionRows[i].Refresh();
            for (int i = 0; i < _appearanceRows.Count; i++) _appearanceRows[i].Refresh();
            for (int i = 0; i < _hideRows.Count; i++) _hideRows[i].Refresh();

            for (int i = 0; i < _classButtons.Count && i < _classes.Count; i++)
            {
                bool current = st != null && st._class == _classes[i];
                Ui.SetButtonColor(_classButtons[i], current ? new Color(0.24f, 0.2f, 0.12f) : Theme.Row);

                UiText label = Ui.FindLabel(_classButtons[i].transform);
                if (label != null) label.color = current ? Theme.Accent : Theme.Text;
            }
        }
    }

    // ------------------------------------------------------------------ world

    internal sealed class WorldTab : Tab
    {
        private UiText _clock;
        private UiText _note;
        private ValueRow _hour;
        private ToggleRow _freeze;
        private ToggleRow _weather;

        public override string Title
        {
            get { return L10n.T("World", "Мир"); }
        }

        public override string Hint
        {
            get { return L10n.T("Time of day and world state", "Время суток и состояние мира"); }
        }

        public override void Build(RectTransform area, float width, float height)
        {
            float y = 0f;

            TabUi.Section(area, 0f, y, width, L10n.T("CLOCK", "ЧАСЫ"));
            y += 32f;

            _clock = Ui.Label("Clock", area, "", 34f, Theme.Accent, TextAlignmentOptions.Left);
            Ui.SetRect(_clock.rectTransform, 0f, y, width, 48f);
            y += 56f;

            _hour = TabUi.Value(area, 0f, y, width, L10n.T("Hour (0-23)", "Час (0-23)"),
                delegate { return GameApi.GetHour24(); },
                delegate (int v) { GameApi.SetTime(v, true); },
                0, 23, 1, "");
            y += 48f;

            float bw = (width - 36f) / 4f;
            TabUi.Action(area, 0f, y, bw, 40f, L10n.T("Dawn 6", "Рассвет 6"),
                delegate { GameApi.SetTime(6, true); });
            TabUi.Action(area, bw + 12f, y, bw, 40f, L10n.T("Noon 12", "Полдень 12"),
                delegate { GameApi.SetTime(12, true); });
            TabUi.Action(area, (bw + 12f) * 2f, y, bw, 40f, L10n.T("Dusk 19", "Закат 19"),
                delegate { GameApi.SetTime(19, true); });
            TabUi.Action(area, (bw + 12f) * 3f, y, bw, 40f, L10n.T("Night 22", "Ночь 22"),
                delegate { GameApi.SetTime(22, true); });
            y += 56f;

            _freeze = TabUi.Toggle(area, 0f, y, width,
                L10n.T("Freeze the cycle", "Остановить цикл"),
                delegate { return Settings.FreezeTime.Value; },
                delegate (bool v) { Settings.FreezeTime.Value = v; },
                L10n.T("host only", "только хост"));
            y += 56f;

            TabUi.Section(area, 0f, y, width, L10n.T("WEATHER AND SPAWNERS", "ПОГОДА И СПАВН"));
            y += 32f;

            _weather = TabUi.Toggle(area, 0f, y, width,
                L10n.T("Weather enabled", "Погода включена"),
                delegate { return Settings.Weather.Value; },
                delegate (bool v)
                {
                    Settings.Weather.Value = v;
                    GameApi.SetWeather(v);
                },
                L10n.T("local map instance", "локальный инстанс"));
            y += 46f;

            float sw = (width - 12f) * 0.5f;
            TabUi.Action(area, 0f, y, sw, 38f, L10n.T("Disable spawners", "Отключить спавн"),
                delegate { Toast(L10n.T("Spawners changed: ", "Спавнеров изменено: ") + GameApi.SetSpawners(true)); });
            TabUi.Action(area, sw + 12f, y, sw, 38f, L10n.T("Enable spawners", "Включить спавн"),
                delegate { Toast(L10n.T("Spawners changed: ", "Спавнеров изменено: ") + GameApi.SetSpawners(false)); });
            y += 50f;

            _note = Ui.Label("Note", area, "", 14f, Theme.Muted, TextAlignmentOptions.TopLeft);
            Ui.SetRect(_note.rectTransform, 0f, y, width, 90f);
            _note.enableWordWrapping = true;
        }

        private static void Toast(string message)
        {
            if (Plugin.MainMenu != null) Plugin.MainMenu.Toast(message);
        }

        public override void Refresh()
        {
            if (_hour != null) _hour.Refresh();
            if (_freeze != null) _freeze.Refresh();
            if (_weather != null) _weather.Refresh();

            GameWorldManager world = GameApi.World;
            if (_clock != null)
            {
                int hour = GameApi.GetHour24();
                string suffix = hour < 12 ? "AM" : "PM";
                int h12 = hour % 12;
                if (h12 == 0) h12 = 12;

                _clock.text = string.Format("{0:00}:00  {1}", hour, suffix);
                if (world != null)
                    _clock.text += "   " + (world._worldTime == WorldTime.NIGHT
                        ? L10n.T("night", "ночь")
                        : L10n.T("day", "день"));
            }

            if (_note != null)
            {
                _note.text = GameApi.IsHost
                    ? L10n.T("As host the clock is synced to every player in the lobby.",
                        "Как хост вы меняете время для всех игроков в лобби.")
                    : L10n.T("As a client the clock changes only for you. Lighting and the HUD update locally.",
                        "Как клиент вы меняете время только у себя: освещение и HUD обновятся локально.");
            }
        }
    }

    // ------------------------------------------------------------------ teleport

    internal sealed class TeleportTab : Tab
    {
        private VirtualList _list;
        private UiText _info;
        private SearchBox _search;
        private Button _pointsButton;
        private Button _mapsButton;

        private readonly System.Collections.Generic.List<UiText> _labels =
            new System.Collections.Generic.List<UiText>();

        private System.Collections.Generic.List<GameApi.SpawnTag> _allSpawns =
            new System.Collections.Generic.List<GameApi.SpawnTag>();

        private System.Collections.Generic.List<GameApi.SpawnTag> _filteredSpawns =
            new System.Collections.Generic.List<GameApi.SpawnTag>();

        private System.Collections.Generic.List<GameApi.MapEntry> _allMaps =
            new System.Collections.Generic.List<GameApi.MapEntry>();

        private System.Collections.Generic.List<GameApi.MapEntry> _filteredMaps =
            new System.Collections.Generic.List<GameApi.MapEntry>();

        private GameApi.SpawnTag[] _slotTags = new GameApi.SpawnTag[0];
        private GameApi.MapEntry[] _slotMaps = new GameApi.MapEntry[0];

        private int _mode;

        public override string Title
        {
            get { return L10n.T("Teleport", "Телепорт"); }
        }

        public override string Hint
        {
            get { return L10n.T("Waypoints and map travel", "Точки карты и переходы между картами"); }
        }

        public override void Build(RectTransform area, float width, float height)
        {
            _search = new SearchBox();
            _search.Build(area, 340f, 40f, L10n.T("Search...", "Поиск..."),
                delegate { Rebuild(); });
            Ui.SetRect(_search.Root, 0f, 0f, 340f, 40f);

            _pointsButton = Ui.Button(area, L10n.T("Waypoints", "Точки карты"), delegate { SetMode(0); }, 15f);
            Ui.SetRect(_pointsButton.GetComponent<RectTransform>(), 352f, 0f, 240f, 40f);

            _mapsButton = Ui.Button(area, L10n.T("World maps", "Карты мира"), delegate { SetMode(1); }, 15f);
            Ui.SetRect(_mapsButton.GetComponent<RectTransform>(), 604f, 0f, 252f, 40f);

            _info = Ui.Label("Info", area, "", 13f, Theme.Muted, TextAlignmentOptions.Left);
            Ui.SetRect(_info.rectTransform, 0f, 44f, width, 16f);

            _list = new VirtualList();
            _list.Build(area, 0f, 64f, width, height - 70f, 34f, 14, CreateRow, BindRow);

            Rebuild();
        }

        private void SetMode(int mode)
        {
            _mode = mode;
            Rebuild();
        }

        private void CreateRow(int slot, RectTransform row)
        {
            Image bg = row.gameObject.AddComponent<Image>();
            bg.sprite = Ui.Square;
            bg.type = Image.Type.Sliced;
            bg.color = slot % 2 == 0 ? Theme.Row : Theme.RowAlt;
            bg.raycastTarget = true;

            Button btn = row.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;

            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.25f, 1.2f, 1.05f, 1f);
            cb.pressedColor = new Color(0.7f, 0.68f, 0.6f, 1f);
            btn.colors = cb;

            int index = slot;
            btn.onClick.AddListener(new UnityEngine.Events.UnityAction(delegate { Activate(index); }));

            UiText label = Ui.Label("Tag", row, "", 15f, Theme.Text, TextAlignmentOptions.Left);
            Ui.Stretch(label.rectTransform, 14f, 0f, 14f, 0f);

            _labels.Add(label);
        }

        private int CurrentCount
        {
            get { return _mode == 0 ? _filteredSpawns.Count : _filteredMaps.Count; }
        }

        private void BindRow(int slot, int index)
        {
            if (slot >= _labels.Count) return;
            if (slot >= _slotTags.Length) _slotTags = new GameApi.SpawnTag[Mathf.Max(slot + 1, 14)];
            if (slot >= _slotMaps.Length) _slotMaps = new GameApi.MapEntry[Mathf.Max(slot + 1, 14)];

            if (_mode == 0)
            {
                _slotMaps[slot] = null;

                GameApi.SpawnTag tag = index >= 0 && index < _filteredSpawns.Count ? _filteredSpawns[index] : null;
                _slotTags[slot] = tag;

                if (tag == null)
                {
                    _labels[slot].text = "";
                    return;
                }

                Player player = GameApi.LocalPlayer;
                bool attuned = player != null && player._waypointAttunements != null &&
                               player._waypointAttunements.Contains(tag.Tag);

                float distance = player == null ? 0f : Vector3.Distance(player.transform.position, tag.Position);

                _labels[slot].text = (attuned ? "★  " : "    ") + tag.Tag +
                                     "      " + Mathf.RoundToInt(distance) + "m";
                _labels[slot].color = attuned ? Theme.Accent : Theme.Text;
            }
            else
            {
                _slotTags[slot] = null;

                GameApi.MapEntry map = index >= 0 && index < _filteredMaps.Count ? _filteredMaps[index] : null;
                _slotMaps[slot] = map;

                if (map == null)
                {
                    _labels[slot].text = "";
                    return;
                }

                _labels[slot].text = L10n.Game(map.Caption) + "      " + map.SubScene;
                _labels[slot].color = Theme.Text;
            }
        }

        private void Activate(int slot)
        {
            string result;

            if (_mode == 0)
            {
                if (slot < 0 || slot >= _slotTags.Length || _slotTags[slot] == null) return;
                result = GameApi.TeleportTo(_slotTags[slot].Tag);
            }
            else
            {
                if (slot < 0 || slot >= _slotMaps.Length || _slotMaps[slot] == null) return;
                result = GameApi.TravelTo(_slotMaps[slot]);
            }

            if (Plugin.MainMenu != null) Plugin.MainMenu.Toast(result);
        }

        private void Rebuild()
        {
            string query = _search == null ? "" : _search.Value.Trim();

            _allSpawns = GameApi.SpawnTags();
            _filteredSpawns.Clear();

            for (int i = 0; i < _allSpawns.Count; i++)
            {
                GameApi.SpawnTag tag = _allSpawns[i];
                if (tag == null) continue;
                if (query.Length > 0 && tag.Tag.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0) continue;
                _filteredSpawns.Add(tag);
            }

            _allMaps = GameApi.Maps();
            _filteredMaps.Clear();

            for (int i = 0; i < _allMaps.Count; i++)
            {
                GameApi.MapEntry map = _allMaps[i];
                if (map == null) continue;

                if (query.Length > 0)
                {
                    string caption = L10n.Game(map.Caption);
                    if (caption.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0 &&
                        map.SubScene.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                _filteredMaps.Add(map);
            }

            if (_list != null) _list.SetCount(CurrentCount);

            if (_pointsButton != null)
                Ui.SetButtonColor(_pointsButton, _mode == 0 ? new Color(0.24f, 0.2f, 0.12f) : Theme.Row);
            if (_mapsButton != null)
                Ui.SetButtonColor(_mapsButton, _mode == 1 ? new Color(0.24f, 0.2f, 0.12f) : Theme.Row);

            if (_info != null)
            {
                if (_mode == 0)
                {
                    _info.text = (GameApi.IsHost
                        ? L10n.T("Click a point to teleport", "Клик по точке — телепорт")
                        : L10n.T("Teleport is host only", "Телепорт доступен только хосту")) +
                        "   •   " + _filteredSpawns.Count;
                    _info.color = GameApi.IsHost ? Theme.Muted : Theme.Danger;
                }
                else
                {
                    _info.text = L10n.T("Map travel reloads the zone", "Переход загружает другую зону") +
                                 "   •   " + _filteredMaps.Count;
                    _info.color = Theme.Muted;
                }
            }
        }

        public override void OnShow()
        {
            Rebuild();
        }

        public override void Refresh()
        {
            if (_search != null) _search.Refresh();
        }
    }
}