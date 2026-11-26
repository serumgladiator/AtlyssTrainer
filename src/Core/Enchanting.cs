using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace AtlyssTrainer
{
    /// <summary>Applies trainer enchants: a cloned game stat modifier with custom strength and glow colour.</summary>
    internal static class Enchanting
    {
        public const int CustomIdBase = 900000;

        private static readonly Dictionary<string, int> Created = new Dictionary<string, int>();
        private static FieldInfo _modifierCache;

        public static Dictionary<int, ScriptableStatModifier> ModifierCache()
        {
            GameManager gm = GameManager._current;
            if (gm == null) return null;

            if (_modifierCache == null)
                _modifierCache = AccessTools.Field(typeof(GameManager), "_cachedScriptableStatModifiers");

            return _modifierCache == null ? null : _modifierCache.GetValue(gm) as Dictionary<int, ScriptableStatModifier>;
        }

        public static List<ScriptableStatModifier> Enchants()
        {
            var list = new List<ScriptableStatModifier>();
            Dictionary<int, ScriptableStatModifier> cache = ModifierCache();
            if (cache == null) return list;

            foreach (KeyValuePair<int, ScriptableStatModifier> kv in cache)
            {
                ScriptableStatModifier mod = kv.Value;
                if (mod == null || kv.Key >= CustomIdBase || string.IsNullOrEmpty(mod._modifierTag)) continue;
                list.Add(mod);
            }

            list.Sort(delegate (ScriptableStatModifier a, ScriptableStatModifier b)
            {
                return string.Compare(a._modifierTag, b._modifierTag, StringComparison.CurrentCultureIgnoreCase);
            });

            return list;
        }

        public static string TagOf(int modifierID)
        {
            if (modifierID <= 0) return null;

            Dictionary<int, ScriptableStatModifier> cache = ModifierCache();
            if (cache == null) return null;

            ScriptableStatModifier mod;
            return cache.TryGetValue(modifierID, out mod) && mod != null ? mod._modifierTag : null;
        }

        public static Color GlowOf(int modifierID)
        {
            Dictionary<int, ScriptableStatModifier> cache = ModifierCache();
            ScriptableStatModifier mod;
            if (cache == null || !cache.TryGetValue(modifierID, out mod) || mod == null) return Color.white;
            return mod._glowTintColor;
        }

        public static List<ItemData> EnchantableItems()
        {
            var result = new List<ItemData>();
            PlayerInventory inv = GameApi.Inventory;
            if (inv == null || inv._heldItems == null) return result;

            Dictionary<string, ScriptableItem> byName = ItemsByName();

            for (int i = 0; i < inv._heldItems.Count; i++)
            {
                ItemData data = inv._heldItems[i];
                if (data == null) continue;

                ScriptableItem item;
                if (byName.TryGetValue(data._itemName, out item) && item is ScriptableEquipment)
                    result.Add(data);
            }

            return result;
        }

        public static ScriptableItem ItemOf(ItemData data)
        {
            if (data == null) return null;

            ScriptableItem item;
            return ItemsByName().TryGetValue(data._itemName, out item) ? item : null;
        }

        private static Dictionary<string, ScriptableItem> ItemsByName()
        {
            var byName = new Dictionary<string, ScriptableItem>();
            List<ScriptableItem> all = GameApi.Items;

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i] != null && !byName.ContainsKey(all[i]._itemName))
                    byName.Add(all[i]._itemName, all[i]);
            }

            return byName;
        }

        public static string Apply(ItemData data, ScriptableStatModifier source, int strength, Color color)
        {
            if (!GameApi.InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");
            if (data == null) return L10n.T("Select an item", "Выберите предмет");
            if (source == null) return L10n.T("Select an enchant", "Выберите зачарование");

            try
            {
                int id = CreateModifier(source, strength, color);
                if (id <= 0) return L10n.T("Enchant failed", "Не удалось зачаровать");

                Swap(data, id);
                return L10n.T("Enchanted: ", "Зачаровано: ") + source._modifierTag;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Enchant: " + e);
                return L10n.T("Enchant error", "Ошибка зачарования");
            }
        }

        public static string Remove(ItemData data)
        {
            if (!GameApi.InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");
            if (data == null) return L10n.T("Select an item", "Выберите предмет");

            try
            {
                Swap(data, 0);
                return L10n.T("Enchant removed", "Зачарование снято");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Remove enchant: " + e);
                return L10n.T("Enchant error", "Ошибка зачарования");
            }
        }

        private static void Swap(ItemData data, int modifierID)
        {
            PlayerEquipment eq = GameApi.Equipment;
            bool worn = data._isEquipped && eq != null;

            if (worn) eq.Init_UnEquipItem(data);
            data._modifierID = modifierID;
            if (worn) eq.Init_EquipItem(data);
        }

        private static int CreateModifier(ScriptableStatModifier source, int strength, Color color)
        {
            Dictionary<int, ScriptableStatModifier> cache = ModifierCache();
            if (cache == null) return 0;

            int mult = Mathf.Clamp(strength, 1, 10);
            Color32 c = color;
            string key = source._modifierID + "|" + mult + "|" + c.r + "," + c.g + "," + c.b;

            int id;
            if (Created.TryGetValue(key, out id) && cache.ContainsKey(id))
                return id;

            id = CustomIdBase;
            while (cache.ContainsKey(id)) id++;

            ScriptableStatModifier clone = UnityEngine.Object.Instantiate(source);
            clone._modifierID = id;
            clone._modifierTag = source._modifierTag + " x" + mult;
            clone._modifierPercentage = source._modifierPercentage * mult;
            clone._glowTintColor = new Color(c.r / 255f, c.g / 255f, c.b / 255f, 1f);
            clone._modifierStatStruct = ScaleStats(source._modifierStatStruct, mult);

            cache[id] = clone;
            Created[key] = id;
            return id;
        }

        private static StatStruct ScaleStats(StatStruct s, int m)
        {
            s._maxHealth *= m;
            s._maxMana *= m;
            s._maxStamina *= m;
            s._experience *= m;
            s._attackPower *= m;
            s._magicPower *= m;
            s._dexPower *= m;
            s._criticalRate *= m;
            s._magicCriticalRate *= m;
            s._evasion *= m;
            s._defense *= m;
            s._magicDefense *= m;
            s._fireResist *= m;
            s._waterResist *= m;
            s._natureResist *= m;
            s._earthResist *= m;
            s._holyResist *= m;
            s._shadowResist *= m;
            return s;
        }
    }
}
