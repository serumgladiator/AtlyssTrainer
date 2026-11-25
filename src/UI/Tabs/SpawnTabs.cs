using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AtlyssTrainer
{
    // ------------------------------------------------------------------ items

    internal sealed class ItemsTab : Tab
    {
        private sealed class Row
        {
            public Image Background;
            public Image Strip;
            public Image Icon;
            public UiText Name;
            public UiText Meta;
            public ScriptableItem Item;
        }

        private readonly List<Row> _rows = new List<Row>();
        private readonly List<ScriptableItem> _filtered = new List<ScriptableItem>();

        private SearchBox _search;
        private VirtualList _list;
        private ValueRow _qty;
        private Button _filterButton;
        private UiText _count;
        private int _filter;
        private int _sort;
        private Button _sortButton;
        private int _qtyValue = 1;

        private RectTransform _detailsRoot;
        private UiText _detailsName;
        private UiText _detailsMeta;
        private UiText _detailsStats;
        private UiText _detailsDesc;
        private UiText _detailsCost;
        private Button _detailsAdd;
        private Button _detailsEquip;
        private Button _detailsDup;
        private Button _detailsDrop;
        private ScriptableItem _detailsItem;

        private const float ListWidth = 512f;

        public override string Title
        {
            get { return L10n.T("Items", "Предметы"); }
        }

        public override string Hint
        {
            get { return L10n.T("Click a row to add the item to your inventory", "Клик по строке — добавить предмет в инвентарь"); }
        }

        public override void Build(RectTransform area, float width, float height)
        {
            _search = new SearchBox();
            _search.Build(area, ListWidth, 40f, L10n.T("Search items...", "Поиск предметов..."),
                delegate { Rebuild(); });
            Ui.SetRect(_search.Root, 0f, 0f, ListWidth, 40f);

            _filterButton = Ui.Button(area, FilterLabel(), delegate
            {
                _filter = (_filter + 1) % 4;
                Ui.SetLabel(_filterButton, FilterLabel());
                Rebuild();
            }, 16f);
            Ui.SetRect(_filterButton.GetComponent<RectTransform>(), 0f, 48f, 170f, 38f);

            _sortButton = Ui.Button(area, SortLabel(), delegate
            {
                _sort = (_sort + 1) % 6;
                Ui.SetLabel(_sortButton, SortLabel());
                Rebuild();
            }, 15f);
            Ui.SetRect(_sortButton.GetComponent<RectTransform>(), 180f, 48f, 170f, 38f);

            _qty = TabUi.Value(area, 360f, 48f, ListWidth - 360f, L10n.T("Qty", "Шт."),
                delegate { return _qtyValue; },
                delegate (int v) { _qtyValue = Mathf.Clamp(v, 1, 999); },
                1, 999, 1, "");
            Ui.SetRect(_qty.Root, 360f, 49f, ListWidth - 360f, 38f);

            _count = Ui.Label("Count", area, "", 13f, Theme.Muted, TextAlignmentOptions.Left);
            Ui.SetRect(_count.rectTransform, 2f, 90f, ListWidth, 16f);

            _list = new VirtualList();
            _list.Build(area, 0f, 112f, ListWidth, height - 112f, 38f, 13, CreateRow, BindRow);

            BuildDetails(area, ListWidth + 16f, 0f, width - ListWidth - 16f, height);

            Rebuild();
        }

        private void BuildDetails(RectTransform area, float x, float y, float width, float height)
        {
            Image outer = Ui.Img("Details", area, Theme.Panel, Ui.Square);
            _detailsRoot = outer.rectTransform;
            Ui.SetRect(_detailsRoot, x, y, width, height);
            Ui.Border(_detailsRoot, Theme.Line, 2f);

            Image accent = Ui.Img("Accent", _detailsRoot, Theme.Line, Ui.Square);
            Ui.SetRect(accent.rectTransform, 0f, 0f, width, 2f);

            _detailsName = Ui.Label("Name", _detailsRoot, "", 20f, Theme.Text, TextAlignmentOptions.TopLeft);
            Ui.SetRect(_detailsName.rectTransform, 14f, 14f, width - 28f, 52f);
            _detailsName.enableWordWrapping = true;

            _detailsMeta = Ui.Label("Meta", _detailsRoot, "", 13f, Theme.Muted, TextAlignmentOptions.TopLeft);
            Ui.SetRect(_detailsMeta.rectTransform, 14f, 70f, width - 28f, 34f);
            _detailsMeta.enableWordWrapping = true;

            Image line = Ui.Img("Line", _detailsRoot, Theme.Line, Ui.Square);
            Ui.SetRect(line.rectTransform, 14f, 110f, width - 28f, 1f);

            _detailsStats = Ui.Label("Stats", _detailsRoot, "", 14f, Theme.Text, TextAlignmentOptions.TopLeft);
            Ui.SetRect(_detailsStats.rectTransform, 14f, 118f, width - 28f, 130f);
            _detailsStats.enableWordWrapping = true;

            _detailsDesc = Ui.Label("Desc", _detailsRoot, "", 12.5f, Theme.Muted, TextAlignmentOptions.TopLeft);
            Ui.SetRect(_detailsDesc.rectTransform, 14f, 256f, width - 28f, height - 378f);
            _detailsDesc.enableWordWrapping = true;
            _detailsDesc.overflowMode = TextOverflowModes.Ellipsis;

            _detailsCost = Ui.Label("Cost", _detailsRoot, "", 13f, Theme.Accent, TextAlignmentOptions.Left);
            Ui.SetRect(_detailsCost.rectTransform, 14f, height - 116f, width - 28f, 18f);

            float halfWidth = (width - 34f) * 0.5f;

            _detailsDup = Ui.Button(_detailsRoot, L10n.T("Duplicate", "Дублировать"),
                delegate { DupDetailsItem(); }, 14f);
            Ui.SetRect(_detailsDup.GetComponent<RectTransform>(), 14f, height - 80f, halfWidth, 30f);

            _detailsDrop = Ui.Button(_detailsRoot, L10n.T("Drop", "Выбросить"),
                delegate { DropDetailsItem(); }, 14f);
            Ui.SetRect(_detailsDrop.GetComponent<RectTransform>(), 14f + halfWidth + 6f, height - 80f, halfWidth, 30f);

            _detailsAdd = Ui.Button(_detailsRoot, L10n.T("Add to inventory", "Добавить в инвентарь"),
                delegate { AddDetailsItem(); }, 15f);
            Ui.SetRect(_detailsAdd.GetComponent<RectTransform>(), 14f, height - 42f, halfWidth, 30f);

            _detailsEquip = Ui.Button(_detailsRoot, L10n.T("Equip", "Надеть"),
                delegate { EquipDetailsItem(); }, 15f);
            Ui.SetRect(_detailsEquip.GetComponent<RectTransform>(), 14f + halfWidth + 6f, height - 42f, halfWidth, 30f);

            ShowDetails(null);
        }

        private string FilterLabel()
        {
            switch (_filter)
            {
                case 1: return L10n.T("Gear", "Снаряжение");
                case 2: return L10n.T("Consumables", "Расходники");
                case 3: return L10n.T("Trade goods", "Товары");
                default: return L10n.T("All items", "Все предметы");
            }
        }

        private string SortLabel()
        {
            switch (_sort)
            {
                case 1: return L10n.T("Price ^", "Цена ^");
                case 2: return L10n.T("Price v", "Цена v");
                case 3: return L10n.T("Level ^", "Ур. ^");
                case 4: return L10n.T("Level v", "Ур. v");
                case 5: return L10n.T("Rarity", "Редкость");
                default: return L10n.T("Name", "Имя");
            }
        }

        private void SortItems()
        {
            int mode = _sort;

            _filtered.Sort(delegate (ScriptableItem a, ScriptableItem b)
            {
                switch (mode)
                {
                    case 1: return a._vendorCost.CompareTo(b._vendorCost);
                    case 2: return b._vendorCost.CompareTo(a._vendorCost);
                    case 3: return CompareLevel(a, b, false);
                    case 4: return CompareLevel(a, b, true);
                    case 5: return ((int)a._itemRarity).CompareTo((int)b._itemRarity);
                    default:
                        return string.Compare(L10n.Game(a._itemName), L10n.Game(b._itemName),
                            StringComparison.CurrentCultureIgnoreCase);
                }
            });
        }

        private static int CompareLevel(ScriptableItem a, ScriptableItem b, bool descending)
        {
            int levelA = EquipmentLevel(a);
            int levelB = EquipmentLevel(b);

            if (levelA < 0 && levelB < 0) return 0;
            if (levelA < 0) return 1;
            if (levelB < 0) return -1;

            return descending ? levelB.CompareTo(levelA) : levelA.CompareTo(levelB);
        }

        private static int EquipmentLevel(ScriptableItem item)
        {
            ScriptableEquipment gear = item as ScriptableEquipment;
            return gear == null ? -1 : gear._equipmentLevel;
        }

        private void CreateRow(int slot, RectTransform row)
        {
            Row r = new Row();

            Image bg = row.gameObject.AddComponent<Image>();
            bg.sprite = Ui.Square;
            bg.type = Image.Type.Sliced;
            bg.color = slot % 2 == 0 ? Theme.Row : Theme.RowAlt;
            bg.raycastTarget = true;
            r.Background = bg;

            Button btn = row.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.25f, 1.2f, 1.05f, 1f);
            cb.pressedColor = new Color(0.7f, 0.68f, 0.6f, 1f);
            btn.colors = cb;
            btn.onClick.AddListener(new UnityEngine.Events.UnityAction(delegate { Give(r); }));

            int hoverSlot = slot;
            PointerRelay relay = row.gameObject.AddComponent<PointerRelay>();
            relay.OnEnter = delegate
            {
                if (hoverSlot < _rows.Count)
                    ShowDetails(_rows[hoverSlot].Item);
            };

            Image strip = Ui.Img("Strip", row, Theme.Accent, Ui.Square);
            Ui.SetRect(strip.rectTransform, 4f, 4f, 4f, 30f);
            r.Strip = strip;

            r.Icon = Ui.Img("Icon", row, Color.white, null);
            r.Icon.preserveAspect = true;
            Ui.SetRect(r.Icon.rectTransform, 14f, 4f, 28f, 28f);

            r.Name = Ui.Label("Name", row, "", 17f, Theme.Text, TextAlignmentOptions.Left);
            Ui.Stretch(r.Name.rectTransform, 50f, 0f, 150f, 0f);

            r.Meta = Ui.Label("Meta", row, "", 13f, Theme.Muted, TextAlignmentOptions.Right);
            Ui.Stretch(r.Meta.rectTransform, 0f, 0f, 14f, 0f);
            r.Meta.alignment = TextAlignmentOptions.MidlineRight;

            _rows.Add(r);
        }

        private void ShowDetails(ScriptableItem item)
        {
            _detailsItem = item;

            if (_detailsName == null) return;

            if (item == null)
            {
                _detailsName.text = L10n.T("No item selected", "Предмет не выбран");
                _detailsName.color = Theme.Muted;
                _detailsMeta.text = L10n.T("Hover a row to inspect it", "Наведи курсор на строку");
                _detailsStats.text = "";
                _detailsDesc.text = "";
                _detailsCost.text = "";
                if (_detailsAdd != null) _detailsAdd.gameObject.SetActive(false);
                if (_detailsEquip != null) _detailsEquip.gameObject.SetActive(false);
                if (_detailsDup != null) _detailsDup.gameObject.SetActive(false);
                if (_detailsDrop != null) _detailsDrop.gameObject.SetActive(false);
                return;
            }

            Color color = Theme.Rarity(item._itemRarity);
            _detailsName.text = L10n.Game(item._itemName);
            _detailsName.color = color;
            _detailsMeta.text = BuildMeta(item);
            _detailsStats.text = BuildStats(item);
            _detailsDesc.text = L10n.Game(item._itemDescription);
            _detailsCost.text = item._vendorCost > 0
                ? L10n.T("Value: ", "Цена: ") + item._vendorCost
                : "";

            if (_detailsAdd != null) _detailsAdd.gameObject.SetActive(true);
            if (_detailsEquip != null) _detailsEquip.gameObject.SetActive(item is ScriptableEquipment);
            if (_detailsDup != null) _detailsDup.gameObject.SetActive(true);
            if (_detailsDrop != null)
            {
                _detailsDrop.gameObject.SetActive(true);
                _detailsDrop.interactable = GameApi.IsHost;
            }
        }

        private static string BuildMeta(ScriptableItem item)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(SlotName(item));
            sb.Append("   •   ").Append(RarityName(item._itemRarity));

            if (item._maxStackAmount > 1)
                sb.Append("   •   x").Append(item._maxStackAmount);

            ScriptableEquipment gear = item as ScriptableEquipment;
            if (gear != null)
            {
                if (gear._equipmentLevel > 0)
                    sb.Append('\n').Append(L10n.T("Level ", "Уровень ")).Append(gear._equipmentLevel);

                if (gear._classRequirement != null)
                    sb.Append("    ").Append(L10n.Game(gear._classRequirement._className));
            }

            ScriptableConsumable consumable = item as ScriptableConsumable;
            if (consumable != null)
            {
                if (consumable._levelRequirement > 0)
                    sb.Append('\n').Append(L10n.T("Level ", "Уровень ")).Append(consumable._levelRequirement);

                if (consumable._consumableCooldown > 0f)
                    sb.Append("    ").Append(L10n.T("Cooldown ", "Перезарядка ")).Append(consumable._consumableCooldown.ToString("0.#")).Append("s");
            }

            return sb.ToString();
        }

        private static string BuildStats(ScriptableItem item)
        {
            var sb = new System.Text.StringBuilder();

            ScriptableWeapon weapon = item as ScriptableWeapon;
            if (weapon != null && weapon._weaponDamage > 0)
            {
                sb.Append(L10n.T("Damage: ", "Урон: ")).Append(weapon._weaponDamage);
                if (weapon._damageBonus > 0)
                    sb.Append(" (+").Append(weapon._damageBonus).Append(')');
                sb.Append('\n');
            }

            ScriptableEquipment gear = item as ScriptableEquipment;
            if (gear != null)
                AppendStatLines(sb, gear._statArray);

            if (sb.Length == 0)
                sb.Append(L10n.T("No stat bonuses", "Нет бонусов к характеристикам"));

            return sb.ToString();
        }

        private static void AppendStatLines(System.Text.StringBuilder sb, StatStruct st)
        {
            AddLine(sb, L10n.T("Health", "Здоровье"), st._maxHealth, false);
            AddLine(sb, L10n.T("Mana", "Мана"), st._maxMana, false);
            AddLine(sb, L10n.T("Stamina", "Выносливость"), st._maxStamina, false);
            AddLine(sb, L10n.T("Attack", "Атака"), st._attackPower, false);
            AddLine(sb, L10n.T("Magic", "Магия"), st._magicPower, false);
            AddLine(sb, L10n.T("Dexterity", "Ловкость"), st._dexPower, false);
            AddLine(sb, L10n.T("Defense", "Защита"), st._defense, false);
            AddLine(sb, L10n.T("Magic defense", "Маг. защита"), st._magicDefense, false);
            AddLine(sb, L10n.T("Critical", "Крит"), Mathf.RoundToInt(st._criticalRate * 100f), true);
            AddLine(sb, L10n.T("Magic crit", "Маг. крит"), Mathf.RoundToInt(st._magicCriticalRate * 100f), true);
            AddLine(sb, L10n.T("Evasion", "Уклонение"), Mathf.RoundToInt(st._evasion * 100f), true);
            AddLine(sb, L10n.T("Fire resist", "Сопр. огню"), st._fireResist, false);
            AddLine(sb, L10n.T("Water resist", "Сопр. воде"), st._waterResist, false);
            AddLine(sb, L10n.T("Nature resist", "Сопр. природе"), st._natureResist, false);
            AddLine(sb, L10n.T("Earth resist", "Сопр. земле"), st._earthResist, false);
            AddLine(sb, L10n.T("Holy resist", "Сопр. свету"), st._holyResist, false);
            AddLine(sb, L10n.T("Shadow resist", "Сопр. тьме"), st._shadowResist, false);
        }

        private static void AddLine(System.Text.StringBuilder sb, string label, int value, bool percent)
        {
            if (value == 0) return;

            sb.Append(value > 0 ? "+" : "").Append(value);
            if (percent) sb.Append('%');
            sb.Append("  ").Append(label).Append('\n');
        }

        private static string SlotName(ScriptableItem item)
        {
            if (item is ScriptableHelm) return L10n.T("Helm", "Шлем");
            if (item is ScriptableChestpiece) return L10n.T("Chest", "Нагрудник");
            if (item is ScriptableLeggings) return L10n.T("Leggings", "Штаны");
            if (item is ScriptableCape) return L10n.T("Cape", "Плащ");
            if (item is ScriptableRing) return L10n.T("Ring", "Кольцо");
            if (item is ScriptableWeapon) return L10n.T("Weapon", "Оружие");
            if (item is ScriptableShield) return L10n.T("Shield", "Щит");
            if (item is ScriptableConsumable) return L10n.T("Consumable", "Расходник");
            if (item is ScriptableTradeItem) return L10n.T("Trade good", "Товар");

            switch (item._itemType)
            {
                case ItemType.GEAR: return L10n.T("Gear", "Снаряжение");
                case ItemType.CONSUMABLE: return L10n.T("Consumable", "Расходник");
                default: return L10n.T("Item", "Предмет");
            }
        }

        private void AddDetailsItem()
        {
            if (_detailsItem == null) return;

            if (!GameApi.InGame)
            {
                if (Plugin.MainMenu != null)
                    Plugin.MainMenu.Toast(L10n.T("Load a character first", "Сначала загрузите персонажа"));
                return;
            }

            bool ok = GameApi.GiveItem(_detailsItem, _qtyValue);
            if (Plugin.MainMenu != null)
            {
                Plugin.MainMenu.Toast(ok
                    ? L10n.T("Added: ", "Добавлено: ") + L10n.Game(_detailsItem._itemName) + " x" + _qtyValue
                    : L10n.T("Failed to add item", "Не удалось добавить предмет"));
            }
        }

        private void EquipDetailsItem()
        {
            if (_detailsItem == null) return;

            string result = GameApi.EquipItem(_detailsItem);
            if (Plugin.MainMenu != null) Plugin.MainMenu.Toast(result);
        }

        private void DupDetailsItem()
        {
            if (_detailsItem == null) return;

            string result = GameApi.DuplicateItem(_detailsItem);
            if (Plugin.MainMenu != null) Plugin.MainMenu.Toast(result);
        }

        private void DropDetailsItem()
        {
            if (_detailsItem == null) return;

            string result = GameApi.DropItem(_detailsItem, _qtyValue);
            if (Plugin.MainMenu != null) Plugin.MainMenu.Toast(result);
        }

        private void BindRow(int slot, int index)
        {
            if (slot >= _rows.Count) return;
            Row r = _rows[slot];
            ScriptableItem item = index >= 0 && index < _filtered.Count ? _filtered[index] : null;
            r.Item = item;

            if (item == null)
            {
                r.Name.text = "";
                r.Meta.text = "";
                return;
            }

            Color color = Theme.Rarity(item._itemRarity);
            r.Name.text = L10n.Game(item._itemName);
            r.Name.color = color;

            if (r.Strip != null) r.Strip.color = color;

            r.Icon.enabled = item._itemIcon != null;
            r.Icon.sprite = item._itemIcon;

            r.Meta.text = RarityName(item._itemRarity);
            r.Meta.color = new Color(color.r, color.g, color.b, 0.85f);
        }

        private static string RarityName(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Rare: return L10n.T("Rare", "Редкий");
                case ItemRarity.Exotic: return L10n.T("Exotic", "Экзотический");
                case ItemRarity.Legendary: return L10n.T("Legendary", "Легендарный");
                case ItemRarity.Cosmetic: return L10n.T("Cosmetic", "Косметика");
                case ItemRarity.Quest: return L10n.T("Quest", "Квестовый");
                default: return L10n.T("Common", "Обычный");
            }
        }

        private void Give(Row r)
        {
            if (r.Item == null) return;

            if (!GameApi.InGame)
            {
                if (Plugin.MainMenu != null) Plugin.MainMenu.Toast(L10n.T("Load a character first", "Сначала загрузите персонажа"));
                return;
            }

            bool ok = GameApi.GiveItem(r.Item, _qtyValue);
            if (Plugin.MainMenu != null)
            {
                Plugin.MainMenu.Toast(ok
                    ? L10n.T("Added: ", "Добавлено: ") + L10n.Game(r.Item._itemName) + " x" + _qtyValue
                    : L10n.T("Failed to add item", "Не удалось добавить предмет"));
            }
        }

        private void Rebuild()
        {
            _filtered.Clear();

            List<ScriptableItem> all = GameApi.Items;
            string query = _search == null ? "" : _search.Value.Trim();

            for (int i = 0; i < all.Count; i++)
            {
                ScriptableItem item = all[i];
                if (item == null) continue;

                if (_filter != 0 && (int)item._itemType != _filter - 1) continue;

                if (query.Length > 0)
                {
                    string display = L10n.Game(item._itemName);
                    if (display.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0 &&
                        item._itemName.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                _filtered.Add(item);
            }

            SortItems();

            if (_list != null) _list.SetCount(_filtered.Count);

            if (_count != null)
            {
                _count.text = L10n.T("found: ", "найдено: ") + _filtered.Count;
            }
        }

        public override void OnShow()
        {
            if (GameApi.Items.Count == 0)
                GameApi.InvalidateCatalogs();

            Rebuild();
        }

        public override void Refresh()
        {
            if (_qty != null) _qty.Refresh();
            if (_search != null) _search.Refresh();
        }
    }

    // ------------------------------------------------------------------ creeps

    internal sealed class CreepsTab : Tab
    {
        private sealed class Row
        {
            public Image Background;
            public UiText Name;
            public UiText Meta;
            public ScriptableCreep Creep;
        }

        private readonly List<Row> _rows = new List<Row>();
        private readonly List<ScriptableCreep> _filtered = new List<ScriptableCreep>();

        private SearchBox _search;
        private VirtualList _list;
        private ValueRow _level;
        private ValueRow _count;
        private ValueRow _modifier;
        private ToggleRow _freezeAI;
        private UiText _found;
        private int _levelValue;
        private int _countValue = 1;

        public override string Title
        {
            get { return L10n.T("Enemies", "Противники"); }
        }

        public override string Hint
        {
            get { return L10n.T("Click a row to spawn the enemy next to you (host)", "Клик по строке — заспавнить врага рядом (хост)"); }
        }

        public override void Build(RectTransform area, float width, float height)
        {
            _search = new SearchBox();
            _search.Build(area, 460f, 40f, L10n.T("Search enemies...", "Поиск противников..."),
                delegate { Rebuild(); });
            Ui.SetRect(_search.Root, 0f, 0f, 460f, 40f);

            _level = TabUi.Value(area, 0f, 47f, 200f, L10n.T("Level", "Ур."),
                delegate { return _levelValue; },
                delegate (int v) { _levelValue = Mathf.Clamp(v, 0, 999); },
                0, 999, 1, "");
            Ui.SetRect(_level.Root, 0f, 47f, 200f, 38f);

            _count = TabUi.Value(area, 424f, 47f, 200f, L10n.T("Amount", "Кол-во"),
                delegate { return _countValue; },
                delegate (int v) { _countValue = Mathf.Clamp(v, 1, 25); },
                1, 25, 1, "");
            Ui.SetRect(_count.Root, 424f, 47f, 200f, 38f);

            _modifier = TabUi.Value(area, 212f, 47f, 200f, L10n.T("Modifier", "Мод."),
                delegate { return Settings.SpawnModifier.Value; },
                delegate (int v) { Settings.SpawnModifier.Value = Mathf.Clamp(v, 0, 99); },
                0, 99, 1, "");
            Ui.SetRect(_modifier.Root, 212f, 47f, 200f, 38f);

            _freezeAI = TabUi.Toggle(area, 636f, 47f, width - 636f,
                L10n.T("Freeze AI", "Заморозить ИИ"),
                delegate { return Settings.FreezeAI.Value; },
                delegate (bool v) { Settings.FreezeAI.Value = v; }, null);

            _found = Ui.Label("Found", area, "", 14f, Theme.Muted, TextAlignmentOptions.Right);
            Ui.SetRect(_found.rectTransform, 470f, 8f, width - 470f, 24f);

            _list = new VirtualList();
            _list.Build(area, 0f, 94f, width, height - 100f, 38f, 13, CreateRow, BindRow);

            Rebuild();
        }

        private void CreateRow(int slot, RectTransform row)
        {
            Row r = new Row();

            Image bg = row.gameObject.AddComponent<Image>();
            bg.sprite = Ui.Square;
            bg.type = Image.Type.Sliced;
            bg.color = slot % 2 == 0 ? Theme.Row : Theme.RowAlt;
            bg.raycastTarget = true;
            r.Background = bg;

            Button btn = row.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.25f, 1.2f, 1.05f, 1f);
            cb.pressedColor = new Color(0.7f, 0.68f, 0.6f, 1f);
            btn.colors = cb;
            btn.onClick.AddListener(new UnityEngine.Events.UnityAction(delegate { Spawn(r); }));

            r.Name = Ui.Label("Name", row, "", 17f, Theme.Text, TextAlignmentOptions.Left);
            Ui.Stretch(r.Name.rectTransform, 16f, 0f, 320f, 0f);

            r.Meta = Ui.Label("Meta", row, "", 13f, Theme.Muted, TextAlignmentOptions.Right);
            Ui.Stretch(r.Meta.rectTransform, 0f, 0f, 16f, 0f);
            r.Meta.alignment = TextAlignmentOptions.MidlineRight;

            _rows.Add(r);
        }

        private void BindRow(int slot, int index)
        {
            if (slot >= _rows.Count) return;
            Row r = _rows[slot];
            ScriptableCreep creep = index >= 0 && index < _filtered.Count ? _filtered[index] : null;
            r.Creep = creep;

            if (creep == null)
            {
                r.Name.text = "";
                r.Meta.text = "";
                return;
            }

            r.Name.text = L10n.Game(creep._creepName);
            r.Name.color = creep._isElite ? new Color(1f, 0.62f, 0.38f) : Theme.Text;

            string meta = L10n.T("Lv ", "Ур. ") + creep._creepLevel;
            if (creep._isElite) meta += "  " + L10n.T("ELITE", "ЭЛИТА");
            if (creep._unkillable) meta += "  " + L10n.T("UNKILLABLE", "НЕУБИВАЕМЫЙ");
            r.Meta.text = meta;
        }

        private void Spawn(Row r)
        {
            if (r.Creep == null) return;

            if (!GameApi.IsHost)
            {
                if (Plugin.MainMenu != null)
                    Plugin.MainMenu.Toast(L10n.T("Spawning enemies requires hosting", "Спавн врагов доступен только хосту"));
                return;
            }

            bool ok = GameApi.SpawnCreep(r.Creep, _countValue, _levelValue);
            if (Plugin.MainMenu != null)
            {
                Plugin.MainMenu.Toast(ok
                    ? L10n.T("Spawned: ", "Заспавнено: ") + L10n.Game(r.Creep._creepName) + " x" + _countValue
                    : L10n.T("Spawn failed", "Не удалось заспавнить"));
            }
        }

        private void Rebuild()
        {
            _filtered.Clear();

            List<ScriptableCreep> all = GameApi.Creeps;
            string query = _search == null ? "" : _search.Value.Trim();

            for (int i = 0; i < all.Count; i++)
            {
                ScriptableCreep creep = all[i];
                if (creep == null) continue;

                if (query.Length > 0)
                {
                    string display = L10n.Game(creep._creepName);
                    if (display.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0 &&
                        creep._creepName.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                _filtered.Add(creep);
            }

            if (_list != null) _list.SetCount(_filtered.Count);
            if (_found != null) _found.text = L10n.T("found: ", "найдено: ") + _filtered.Count;
        }

        public override void OnShow()
        {
            if (GameApi.Creeps.Count == 0)
                GameApi.InvalidateCatalogs();

            Rebuild();
        }

        public override void Refresh()
        {
            if (_level != null) _level.Refresh();
            if (_count != null) _count.Refresh();
            if (_modifier != null) _modifier.Refresh();
            if (_freezeAI != null) _freezeAI.Refresh();
            if (_search != null) _search.Refresh();
        }
    }

    // ------------------------------------------------------------------ settings

    internal sealed class SettingsTab : Tab
    {
        private ValueRow _scale, _opacity, _bannerFps;
        private Button _themeButton, _fontButton;
        private ToggleRow _animateBanner;
        private ToggleRow _russian, _lockInput, _overlay;
        private readonly System.Collections.Generic.List<KeyRow> _keys = new System.Collections.Generic.List<KeyRow>();
        private readonly Button[] _loadButtons = new Button[3];
        private UiText _note;

        public override string Title
        {
            get { return L10n.T("Settings", "Настройки"); }
        }

        public override string Hint
        {
            get { return L10n.T("Menu options, hotkeys and presets", "Параметры меню, клавиши и пресеты"); }
        }

        public override void Build(RectTransform area, float width, float height)
        {
            RectTransform content = Ui.ScrollArea(area, 0f, 0f, width, height, out _scroll);
            float w = width - 20f;
            float y = 0f;

            TabUi.Section(content, 0f, y, w, L10n.T("APPEARANCE", "ВНЕШНИЙ ВИД"));
            y += 32f;

            _opacity = TabUi.Value(content, 0f, y, w, L10n.T("Background transparency", "Прозрачность фона"),
                delegate { return Mathf.RoundToInt(Settings.MenuOpacity.Value * 100f); },
                delegate (int v)
                {
                    Settings.MenuOpacity.Value = Mathf.Clamp01(v / 100f);
                    Theme.Apply(Settings.MenuTheme.Value);
                    if (Plugin.MainMenu != null) Plugin.MainMenu.Rebuild();
                },
                0, 90, 5, "%");
            y += 44f;

            _themeButton = Ui.Button(content, ThemeLabel(), delegate
            {
                int index = System.Array.IndexOf(Theme.ThemeNames, Theme.Current);
                index = (index + 1) % Theme.ThemeNames.Length;
                string name = Theme.ThemeNames[index];
                Settings.MenuTheme.Value = name;
                Theme.Apply(name);
                Ui.SetLabel(_themeButton, ThemeLabel());
                if (Plugin.MainMenu != null) Plugin.MainMenu.Rebuild();
            }, 15f);
            Ui.SetRect(_themeButton.GetComponent<RectTransform>(), 0f, y, w, 34f);
            y += 40f;

            _fontButton = Ui.Button(content, FontLabel(), delegate
            {
                int index = System.Array.IndexOf(Ui.FontNames, Settings.MenuFont.Value);
                index = (index + 1) % Ui.FontNames.Length;
                string name = Ui.FontNames[index];
                Settings.MenuFont.Value = name;
                if (Plugin.MainMenu != null) Plugin.MainMenu.SetFont(name);
            }, 15f);
            Ui.SetRect(_fontButton.GetComponent<RectTransform>(), 0f, y, w, 34f);
            y += 46f;

            _animateBanner = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Banner animation", "Анимация баннера"),
                delegate { return Settings.BannerAnimate.Value; },
                delegate (bool v)
                {
                    Settings.BannerAnimate.Value = v;
                    if (Plugin.MainMenu != null) Plugin.MainMenu.Rebuild();
                },
                L10n.T("static stars and text when off", "статические звёзды и текст"));
            y += 44f;

            _bannerFps = TabUi.Value(content, 0f, y, w,
                L10n.T("Banner FPS", "FPS баннера"),
                delegate { return Settings.BannerFps.Value; },
                delegate (int v) { Settings.BannerFps.Value = Mathf.Clamp(v, 1, 30); },
                1, 30, 1, "");
            y += 50f;

            TabUi.Section(content, 0f, y, w, L10n.T("INTERFACE", "ИНТЕРФЕЙС"));
            y += 32f;

            _scale = TabUi.Value(content, 0f, y, w, L10n.T("Menu scale", "Масштаб меню"),
                delegate { return Mathf.RoundToInt(Settings.UiScale.Value * 100f); },
                delegate (int v)
                {
                    Settings.UiScale.Value = v / 100f;
                    if (Plugin.MainMenu != null) Plugin.MainMenu.ApplyScale(v / 100f);
                },
                70, 160, 5, "%");
            y += 44f;

            _russian = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Russian interface", "Русский интерфейс"),
                delegate { return Settings.AutoRussian.Value && L10n.Russian; },
                delegate (bool v)
                {
                    Settings.AutoRussian.Value = v;
                    Ui.Reset();
                    L10n.Init();
                    if (Plugin.MainMenu != null)
                    {
                        Plugin.MainMenu.Toast(v
                            ? L10n.T("Russian enabled", "Русский включён")
                            : L10n.T("English enabled", "Английский включён"));
                        Plugin.MainMenu.Rebuild();
                    }
                },
                L10n.T("uses the translation dictionary", "использует словарь русификатора"));
            y += 46f;

            _lockInput = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Freeze character in menu", "Блокировать управление в меню"),
                delegate { return Settings.LockInput.Value; },
                delegate (bool v) { Settings.LockInput.Value = v; },
                L10n.T("no movement, camera or attacks", "движение, камера и атаки"));
            y += 46f;

            _overlay = TabUi.Toggle(content, 0f, y, w,
                L10n.T("Active cheats overlay", "Оверлей активных читов"),
                delegate { return Settings.ShowOverlay.Value; },
                delegate (bool v) { Settings.ShowOverlay.Value = v; }, null);
            y += 56f;

            TabUi.Section(content, 0f, y, w, L10n.T("HOTKEYS", "ГОРЯЧИЕ КЛАВИШИ"));
            y += 32f;

            _keys.Add(AddKey(content, y, w, L10n.T("Open menu", "Открыть меню"),
                delegate { return Settings.MenuKey.Value; },
                delegate (KeyCode k) { Settings.MenuKey.Value = k; }));
            y += 44f;

            _keys.Add(AddKey(content, y, w, L10n.T("Toggle overlay", "Показать оверлей"),
                delegate { return Settings.OverlayKey.Value; },
                delegate (KeyCode k) { Settings.OverlayKey.Value = k; }));
            y += 44f;

            _keys.Add(AddKey(content, y, w, L10n.T("Toggle god mode", "Вкл/выкл бессмертие"),
                delegate { return Settings.GodKey.Value; },
                delegate (KeyCode k) { Settings.GodKey.Value = k; }));
            y += 54f;

            TabUi.Section(content, 0f, y, w, L10n.T("PRESETS", "ПРЕСЕТЫ"));
            y += 32f;

            for (int i = 0; i < 3; i++)
            {
                int slot = i;
                float bw = 110f;

                UiText label = Ui.Label("Preset" + i, content,
                    L10n.T("Preset ", "Пресет ") + (i + 1), 16f, Theme.Text, TextAlignmentOptions.Left);
                Ui.SetRect(label.rectTransform, 14f, y + 8f, w - 2 * bw - 40f, 24f);

                Button save = TabUi.Action(content, w - 2 * bw - 14f, y, bw, 36f,
                    L10n.T("Save", "Сохранить"),
                    delegate
                    {
                        Presets.Save(slot);
                        if (Plugin.MainMenu != null)
                            Plugin.MainMenu.Toast(L10n.T("Preset saved", "Пресет сохранён"));
                    });

                Button load = TabUi.Action(content, w - bw - 4f, y, bw, 36f,
                    L10n.T("Load", "Загрузить"),
                    delegate
                    {
                        bool ok = Presets.Load(slot);
                        if (Plugin.MainMenu != null)
                            Plugin.MainMenu.Toast(ok
                                ? L10n.T("Preset loaded", "Пресет загружен")
                                : L10n.T("Preset is empty", "Пресет пуст"));
                    });

                _loadButtons[i] = load;
                y += 42f;
            }

            y += 12f;
            TabUi.Section(content, 0f, y, w, L10n.T("TOOLS", "ИНСТРУМЕНТЫ"));
            y += 32f;

            float half = (w - 12f) * 0.5f;
            TabUi.Action(content, 0f, y, half, 42f, L10n.T("Reload catalog", "Обновить каталог"),
                delegate
                {
                    GameApi.InvalidateCatalogs();
                    L10n.ClearCache();
                    if (Plugin.MainMenu != null) Plugin.MainMenu.Toast(L10n.T("Catalog reloaded", "Каталог обновлён"));
                });

            TabUi.Action(content, half + 12f, y, half, 42f, L10n.T("Save profile", "Сохранить профиль"),
                delegate
                {
                    GameApi.SaveProfile();
                    if (Plugin.MainMenu != null) Plugin.MainMenu.Toast(L10n.T("Profile saved", "Профиль сохранён"));
                });
            y += 56f;

            _note = Ui.Label("Note", content, "", 14f, Theme.Muted, TextAlignmentOptions.TopLeft);
            Ui.SetRect(_note.rectTransform, 0f, y, w, 180f);
            _note.enableWordWrapping = true;
            y += 186f;

            content.sizeDelta = new Vector2(0f, y + 8f);
        }

        private ScrollRect _scroll;

        private static KeyRow AddKey(RectTransform parent, float y, float width, string label,
            Func<KeyCode> get, Action<KeyCode> set)
        {
            KeyRow row = new KeyRow();
            row.Build(parent, width, label, get, set);
            Ui.SetRect(row.Root, 0f, y, width, 38f);
            return row;
        }

        private static string ThemeLabel()
        {
            string key = Theme.Current;
            string ru = "Зелёная";

            switch (key)
            {
                case "amber": ru = "Янтарь"; break;
                case "cyan": ru = "Циан"; break;
                case "magenta": ru = "Пурпур"; break;
                case "black": ru = "Чёрная"; break;
                case "red": ru = "Красная"; break;
            }

            return L10n.T("Theme: ", "Тема: ") + L10n.T(key, ru);
        }

        private static string FontLabel()
        {
            return L10n.T("Font: ", "Шрифт: ") + Settings.MenuFont.Value;
        }

        public override void Refresh()
        {
            if (_opacity != null) _opacity.Refresh();
            if (_animateBanner != null) _animateBanner.Refresh();
            if (_bannerFps != null) _bannerFps.Refresh();
            if (_scale != null) _scale.Refresh();
            if (_russian != null) _russian.Refresh();
            if (_lockInput != null) _lockInput.Refresh();
            if (_overlay != null) _overlay.Refresh();

            for (int i = 0; i < _keys.Count; i++) _keys[i].Refresh();

            for (int i = 0; i < _loadButtons.Length; i++)
            {
                if (_loadButtons[i] != null)
                    _loadButtons[i].interactable = Presets.Has(i);
            }

            if (_note != null)
            {
                _note.text =
                    L10n.T("Hotkey: ", "Горячая клавиша: ") + Settings.MenuKey.Value + "   •   " +
                    L10n.T("overlay: ", "оверлей: ") + Settings.OverlayKey.Value + "\n" +
                    L10n.T("Config: BepInEx/config/peppa.atlyss.cheatmenu.cfg",
                        "Конфиг: BepInEx/config/peppa.atlyss.cheatmenu.cfg") + "\n" +
                    L10n.T("Item and enemy names come from the running game, so they follow the installed translation.",
                        "Названия предметов и врагов читаются из запущенной игры, поэтому совпадают с русификатором.") + "\n" +
                    L10n.T("Backups made from the Experimental tab are stored in BepInEx/cheatmenu_backups.",
                        "Бэкапы из экспериментальной вкладки лежат в BepInEx/cheatmenu_backups.") + "\n" +
                    L10n.T("Host-only cheats are marked in the list.", "Читы, доступные только хосту, помечены.");
            }
        }
    }
}