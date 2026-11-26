using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AtlyssTrainer
{
    internal sealed class EnchantTab : Tab
    {
        private sealed class ItemRow
        {
            public Image Background;
            public Image Strip;
            public UiText Name;
            public UiText Meta;
            public ItemData Data;
        }

        private sealed class EnchantRow
        {
            public Image Background;
            public Image Strip;
            public UiText Name;
            public ScriptableStatModifier Mod;
        }

        private readonly List<ItemRow> _itemRows = new List<ItemRow>();
        private readonly List<EnchantRow> _enchantRows = new List<EnchantRow>();
        private readonly List<ItemData> _items = new List<ItemData>();
        private readonly List<ScriptableStatModifier> _enchants = new List<ScriptableStatModifier>();
        private readonly List<ScriptableStatModifier> _enchantsFiltered = new List<ScriptableStatModifier>();

        private SearchBox _itemSearch;
        private SearchBox _enchantSearch;
        private VirtualList _itemList;
        private VirtualList _enchantList;
        private ValueRow _strength;
        private ValueRow _red;
        private ValueRow _green;
        private ValueRow _blue;
        private Image _swatch;
        private UiText _selection;
        private UiText _status;

        private ItemData _selectedItem;
        private ScriptableStatModifier _selectedEnchant;
        private int _strengthValue = 1;
        private int _r = 255;
        private int _g = 255;
        private int _b = 255;
        private int _itemCount = -1;

        private const float ListWidth = 400f;
        private const float EnchantRowsHeight = 204f;

        public override string Title
        {
            get { return L10n.T("Enchants", "Зачарования"); }
        }

        public override string Hint
        {
            get { return L10n.T("Pick an item, an enchant, strength and colour, then apply", "Выберите предмет, зачарование, силу и цвет, затем примените"); }
        }

        public override void Build(RectTransform area, float width, float height)
        {
            _itemSearch = new SearchBox();
            _itemSearch.Build(area, ListWidth, 36f, L10n.T("Search items...", "Поиск предметов..."),
                delegate { RebuildItems(); });
            Ui.SetRect(_itemSearch.Root, 0f, 0f, ListWidth, 36f);

            _itemList = new VirtualList();
            _itemList.Build(area, 0f, 42f, ListWidth, height - 42f, 38f, 12, CreateItemRow, BindItemRow);

            float rx = ListWidth + 16f;
            float rw = width - rx;

            _enchantSearch = new SearchBox();
            _enchantSearch.Build(area, rw, 36f, L10n.T("Search enchants...", "Поиск зачарований..."),
                delegate { RebuildEnchants(); });
            Ui.SetRect(_enchantSearch.Root, rx, 0f, rw, 36f);

            _enchantList = new VirtualList();
            _enchantList.Build(area, rx, 42f, rw, EnchantRowsHeight, 34f, 6, CreateEnchantRow, BindEnchantRow);

            _selection = Ui.Label("Selection", area, "", 13f, Theme.Muted, TextAlignmentOptions.Left);
            Ui.SetRect(_selection.rectTransform, rx, 250f, rw, 18f);
            _selection.enableWordWrapping = false;

            _strength = TabUi.Value(area, rx, 272f, rw, L10n.T("Strength (x)", "Сила (x)"),
                delegate { return _strengthValue; },
                delegate (int v) { _strengthValue = Mathf.Clamp(v, 1, 10); },
                1, 10, 1, "");
            Ui.SetRect(_strength.Root, rx, 272f, rw, 38f);

            float colorWidth = rw - 140f;

            _red = TabUi.Value(area, rx, 318f, colorWidth, "R",
                delegate { return _r; },
                delegate (int v) { _r = Mathf.Clamp(v, 0, 255); UpdateSwatch(); },
                0, 255, 5, "");
            Ui.SetRect(_red.Root, rx, 318f, colorWidth, 38f);

            _green = TabUi.Value(area, rx, 360f, colorWidth, "G",
                delegate { return _g; },
                delegate (int v) { _g = Mathf.Clamp(v, 0, 255); UpdateSwatch(); },
                0, 255, 5, "");
            Ui.SetRect(_green.Root, rx, 360f, colorWidth, 38f);

            _blue = TabUi.Value(area, rx, 402f, colorWidth, "B",
                delegate { return _b; },
                delegate (int v) { _b = Mathf.Clamp(v, 0, 255); UpdateSwatch(); },
                0, 255, 5, "");
            Ui.SetRect(_blue.Root, rx, 402f, colorWidth, 38f);

            _swatch = Ui.Img("Swatch", area, Color.white, Ui.Square);
            Ui.SetRect(_swatch.rectTransform, rx + rw - 120f, 318f, 120f, 122f);
            Ui.Border(_swatch.rectTransform, Theme.Line, 2f);

            float halfWidth = (rw - 8f) * 0.5f;

            Button apply = Ui.Button(area, L10n.T("Apply enchant", "Зачаровать"), delegate { ApplySelected(); }, 15f);
            Ui.SetRect(apply.GetComponent<RectTransform>(), rx, 452f, halfWidth, 34f);

            Button remove = Ui.Button(area, L10n.T("Remove enchant", "Снять зачарование"), delegate { RemoveSelected(); }, 14f);
            Ui.SetRect(remove.GetComponent<RectTransform>(), rx + halfWidth + 8f, 452f, halfWidth, 34f);

            _status = Ui.Label("Status", area, "", 13f, Theme.Muted, TextAlignmentOptions.Left);
            Ui.SetRect(_status.rectTransform, rx, 494f, rw, 18f);

            RebuildItems();
            RebuildEnchants();
            UpdateSelection();
        }

        private void CreateItemRow(int slot, RectTransform row)
        {
            ItemRow r = new ItemRow();

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
            btn.onClick.AddListener(new UnityEngine.Events.UnityAction(delegate { SelectItem(r.Data); }));

            r.Strip = Ui.Img("Strip", row, Theme.Accent, Ui.Square);
            Ui.SetRect(r.Strip.rectTransform, 4f, 4f, 4f, 30f);

            r.Name = Ui.Label("Name", row, "", 16f, Theme.Text, TextAlignmentOptions.Left);
            Ui.Stretch(r.Name.rectTransform, 16f, 0f, 120f, 0f);

            r.Meta = Ui.Label("Meta", row, "", 12f, Theme.Muted, TextAlignmentOptions.Right);
            Ui.Stretch(r.Meta.rectTransform, 0f, 0f, 14f, 0f);

            _itemRows.Add(r);
        }

        private void BindItemRow(int slot, int index)
        {
            if (slot >= _itemRows.Count) return;

            ItemRow r = _itemRows[slot];
            ItemData data = index >= 0 && index < _items.Count ? _items[index] : null;
            r.Data = data;

            if (data == null)
            {
                r.Name.text = "";
                r.Meta.text = "";
                return;
            }

            ScriptableItem item = Enchanting.ItemOf(data);
            Color color = item != null ? Theme.Rarity(item._itemRarity) : Theme.Text;

            r.Name.text = L10n.Game(data._itemName);
            r.Name.color = color;
            r.Strip.color = color;

            string tag = Enchanting.TagOf(data._modifierID);
            r.Meta.text = tag ?? L10n.T("no enchant", "без зачарования");
            r.Meta.color = tag != null ? Theme.Accent : Theme.Muted;

            bool selected = _selectedItem != null && data == _selectedItem;
            r.Background.color = selected
                ? new Color(Theme.Accent.r * 0.3f, Theme.Accent.g * 0.3f, Theme.Accent.b * 0.3f, 1f)
                : (index % 2 == 0 ? Theme.Row : Theme.RowAlt);
        }

        private void CreateEnchantRow(int slot, RectTransform row)
        {
            EnchantRow r = new EnchantRow();

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
            btn.onClick.AddListener(new UnityEngine.Events.UnityAction(delegate { SelectEnchant(r.Mod); }));

            r.Strip = Ui.Img("Strip", row, Theme.Accent, Ui.Square);
            Ui.SetRect(r.Strip.rectTransform, 4f, 4f, 4f, 26f);

            r.Name = Ui.Label("Name", row, "", 14f, Theme.Text, TextAlignmentOptions.Left);
            Ui.Stretch(r.Name.rectTransform, 16f, 0f, 14f, 0f);

            _enchantRows.Add(r);
        }

        private void BindEnchantRow(int slot, int index)
        {
            if (slot >= _enchantRows.Count) return;

            EnchantRow r = _enchantRows[slot];
            ScriptableStatModifier mod = index >= 0 && index < _enchantsFiltered.Count ? _enchantsFiltered[index] : null;
            r.Mod = mod;

            if (mod == null)
            {
                r.Name.text = "";
                return;
            }

            r.Name.text = mod._modifierTag;
            r.Strip.color = mod._glowTintColor;
            r.Name.color = mod == _selectedEnchant ? Theme.Accent : Theme.Text;
        }

        private void RebuildItems()
        {
            _items.Clear();

            List<ItemData> all = Enchanting.EnchantableItems();
            string query = _itemSearch == null ? "" : _itemSearch.Value.Trim();

            for (int i = 0; i < all.Count; i++)
            {
                ItemData data = all[i];

                if (query.Length > 0)
                {
                    string display = L10n.Game(data._itemName);
                    if (display.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0 &&
                        data._itemName.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                _items.Add(data);
            }

            _itemCount = all.Count;
            if (_itemList != null) _itemList.SetCount(_items.Count);
        }

        private void RebuildEnchants()
        {
            _enchants.Clear();
            _enchants.AddRange(Enchanting.Enchants());

            _enchantsFiltered.Clear();
            string query = _enchantSearch == null ? "" : _enchantSearch.Value.Trim();

            for (int i = 0; i < _enchants.Count; i++)
            {
                ScriptableStatModifier mod = _enchants[i];
                if (query.Length > 0 && mod._modifierTag.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0)
                    continue;

                _enchantsFiltered.Add(mod);
            }

            if (_enchantList != null) _enchantList.SetCount(_enchantsFiltered.Count);
        }

        private void SelectItem(ItemData data)
        {
            if (data == null) return;

            _selectedItem = data;
            _status.text = "";
            UpdateSelection();
            if (_itemList != null) _itemList.Refresh(true);
        }

        private void SelectEnchant(ScriptableStatModifier mod)
        {
            if (mod == null) return;

            _selectedEnchant = mod;
            Color glow = mod._glowTintColor;
            _r = Mathf.RoundToInt(glow.r * 255f);
            _g = Mathf.RoundToInt(glow.g * 255f);
            _b = Mathf.RoundToInt(glow.b * 255f);

            _status.text = "";
            UpdateSelection();
            UpdateSwatch();
            if (_enchantList != null) _enchantList.Refresh(true);
        }

        private void ApplySelected()
        {
            string message = Enchanting.Apply(_selectedItem, _selectedEnchant, _strengthValue, CurrentColor());
            Finish(message);
        }

        private void RemoveSelected()
        {
            string message = Enchanting.Remove(_selectedItem);
            Finish(message);
        }

        private void Finish(string message)
        {
            _status.text = message;
            if (Plugin.MainMenu != null) Plugin.MainMenu.Toast(message);

            RebuildItems();
            if (_itemList != null) _itemList.Refresh(true);
            UpdateSelection();
        }

        private Color CurrentColor()
        {
            return new Color(_r / 255f, _g / 255f, _b / 255f, 1f);
        }

        private void UpdateSwatch()
        {
            if (_swatch != null) _swatch.color = CurrentColor();
        }

        private void UpdateSelection()
        {
            if (_selection == null) return;

            if (_selectedItem == null)
            {
                _selection.text = L10n.T("No item selected", "Предмет не выбран");
                _selection.color = Theme.Muted;
                return;
            }

            string item = L10n.Game(_selectedItem._itemName);
            string tag = Enchanting.TagOf(_selectedItem._modifierID);
            string enchant = _selectedEnchant != null ? _selectedEnchant._modifierTag : L10n.T("none", "нет");

            _selection.text = item + "   |   " + L10n.T("current: ", "сейчас: ") + (tag ?? L10n.T("none", "нет"))
                + "   |   " + L10n.T("selected: ", "выбрано: ") + enchant;
            _selection.color = Theme.Text;
        }

        public override void OnShow()
        {
            if (GameApi.Items.Count == 0)
                GameApi.InvalidateCatalogs();

            RebuildEnchants();
            RebuildItems();
            UpdateSelection();
        }

        public override void Refresh()
        {
            if (_strength != null) _strength.Refresh();
            if (_red != null) _red.Refresh();
            if (_green != null) _green.Refresh();
            if (_blue != null) _blue.Refresh();
            if (_itemSearch != null) _itemSearch.Refresh();
            if (_enchantSearch != null) _enchantSearch.Refresh();

            if (GameApi.InGame && Enchanting.EnchantableItems().Count != _itemCount)
                RebuildItems();
        }
    }
}
