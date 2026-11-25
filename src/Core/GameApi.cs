using System;
using System.Collections.Generic;
using System.Reflection;
using Mirror;
using UnityEngine;

namespace AtlyssTrainer
{
    /// <summary>Thin, defensive bridge to the game API. Everything here is reflection-free
    /// except the scriptable-asset caches, which the game keeps private.</summary>
    internal static class GameApi
    {
        private static List<ScriptableItem> _items;
        private static List<ScriptableCreep> _creeps;

        // ---------------------------------------------------------------- player

        public static Player LocalPlayer
        {
            get { return Player._mainPlayer; }
        }

        public static bool InGame
        {
            get
            {
                Player p = LocalPlayer;
                return p != null && p._pStats != null && p._statusEntity != null && p._pInventory != null;
            }
        }

        public static bool IsHost
        {
            get { return NetworkServer.active; }
        }

        public static string GameVersion
        {
            get
            {
                try
                {
                    string version = Application.version;
                    return string.IsNullOrEmpty(version) ? "?" : version;
                }
                catch
                {
                    return "?";
                }
            }
        }

        public static PlayerStats Stats
        {
            get { Player p = LocalPlayer; return p == null ? null : p._pStats; }
        }

        public static PlayerInventory Inventory
        {
            get { Player p = LocalPlayer; return p == null ? null : p._pInventory; }
        }

        public static StatusEntity Status
        {
            get { Player p = LocalPlayer; return p == null ? null : p._statusEntity; }
        }

        public static PlayerEquipment Equipment
        {
            get { Player p = LocalPlayer; return p == null ? null : p._pEquipment; }
        }

        public static MapInstance Map
        {
            get { Player p = LocalPlayer; return p == null ? null : p._playerMapInstance; }
        }

        public static GameWorldManager World
        {
            get { return GameWorldManager._current; }
        }

        public static int MaxHealth
        {
            get
            {
                PlayerStats s = Stats;
                if (s == null) return 1;
                int v = s._runtimeStatStruct._maxHealth;
                return v > 0 ? v : 1;
            }
        }

        public static int MaxMana
        {
            get
            {
                PlayerStats s = Stats;
                if (s == null) return 1;
                int v = s._runtimeStatStruct._maxMana;
                return v > 0 ? v : 1;
            }
        }

        public static int MaxStamina
        {
            get
            {
                PlayerStats s = Stats;
                if (s == null) return 1;
                int v = s._runtimeStatStruct._maxStamina;
                return v > 0 ? v : 1;
            }
        }

        // ---------------------------------------------------------------- catalogs

        public static List<ScriptableItem> Items
        {
            get
            {
                if (_items == null || _items.Count == 0)
                    BuildItemCatalog();
                return _items;
            }
        }

        public static List<ScriptableCreep> Creeps
        {
            get
            {
                if (_creeps == null || _creeps.Count == 0)
                    BuildCreepCatalog();
                return _creeps;
            }
        }

        private static void BuildItemCatalog()
        {
            _items = new List<ScriptableItem>();
            var seen = new HashSet<string>();

            GameManager gm = GameManager._current;
            Dictionary<string, ScriptableItem> cache = ReadCache<ScriptableItem>(gm, "_cachedScriptableItems");
            if (cache != null)
            {
                foreach (var kv in cache)
                {
                    ScriptableItem item = kv.Value;
                    if (item == null || string.IsNullOrEmpty(item._itemName)) continue;
                    if (seen.Add(item._itemName))
                        _items.Add(item);
                }
            }

            if (_items.Count == 0)
            {
                ScriptableItem[] all = Resources.FindObjectsOfTypeAll<ScriptableItem>();
                for (int i = 0; i < all.Length; i++)
                {
                    ScriptableItem item = all[i];
                    if (item == null || string.IsNullOrEmpty(item._itemName)) continue;
                    if (seen.Add(item._itemName))
                        _items.Add(item);
                }
            }

            _items.Sort(delegate (ScriptableItem a, ScriptableItem b)
            {
                return string.Compare(L10n.Game(a._itemName), L10n.Game(b._itemName), StringComparison.CurrentCultureIgnoreCase);
            });

            Plugin.Log.LogInfo("Item catalog: " + _items.Count);
        }

        private static void BuildCreepCatalog()
        {
            _creeps = new List<ScriptableCreep>();
            var seen = new HashSet<string>();

            GameManager gm = GameManager._current;
            Dictionary<string, ScriptableCreep> cache = ReadCache<ScriptableCreep>(gm, "_cachedScriptableCreeps");
            if (cache != null)
            {
                foreach (var kv in cache)
                {
                    ScriptableCreep creep = kv.Value;
                    if (creep == null || string.IsNullOrEmpty(creep._creepName)) continue;
                    if (seen.Add(creep._creepName))
                        _creeps.Add(creep);
                }
            }

            if (_creeps.Count == 0)
            {
                ScriptableCreep[] all = Resources.FindObjectsOfTypeAll<ScriptableCreep>();
                for (int i = 0; i < all.Length; i++)
                {
                    ScriptableCreep creep = all[i];
                    if (creep == null || string.IsNullOrEmpty(creep._creepName)) continue;
                    if (seen.Add(creep._creepName))
                        _creeps.Add(creep);
                }
            }

            _creeps.Sort(delegate (ScriptableCreep a, ScriptableCreep b)
            {
                return string.Compare(L10n.Game(a._creepName), L10n.Game(b._creepName), StringComparison.CurrentCultureIgnoreCase);
            });

            Plugin.Log.LogInfo("Creep catalog: " + _creeps.Count);
        }

        private static Dictionary<string, T> ReadCache<T>(GameManager gm, string fieldName) where T : UnityEngine.Object
        {
            if (gm == null) return null;

            try
            {
                FieldInfo f = typeof(GameManager).GetField(fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (f == null)
                {
                    Plugin.Log.LogWarning("GameManager field not found: " + fieldName);
                    return null;
                }

                return f.GetValue(gm) as Dictionary<string, T>;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("ReadCache " + fieldName + ": " + e.Message);
                return null;
            }
        }

        public static void InvalidateCatalogs()
        {
            _items = null;
            _creeps = null;
        }

        // ---------------------------------------------------------------- cheats

        public static void TickCheats()
        {
            if (!InGame) return;

            StatusEntity status = Status;
            if (status == null) return;

            if (Settings.GodMode.Value || Settings.KeepBuffs.Value)
            {
                if (status._currentHealth < MaxHealth)
                    status.Network_currentHealth = MaxHealth;
            }

            if (Settings.InfiniteMana.Value && status._currentMana < MaxMana)
                status.Network_currentMana = MaxMana;

            if (Settings.InfiniteStamina.Value && status._currentStamina < MaxStamina)
                status.Network_currentStamina = MaxStamina;

            TickCombat(status);
            TickMovement();
            TickNoclip();
            TickEnemies();
            TickCasting();
        }

        // ---------------------------------------------------------------- combat helpers

        private static bool _critApplied;
        private static float _critOrig, _mcritOrig;
        private static bool _parryApplied;
        private static bool _parryOrig;
        private static float _parryBufferOrig;
        private static bool _accuracyApplied;
        private static float _accuracyOrig;
        private static bool _reflectApplied;
        private static bool _reflectOrig;
        private static float _reflectPercentOrig;
        private static bool _absorbApplied;
        private static float _absorbOrig;
        private static float _purgeTimer;
        private static float _globalCdTimer;

        private static void TickCombat(StatusEntity status)
        {
            PlayerStats stats = Stats;
            if (stats != null)
            {
                if (Settings.AlwaysCrit.Value)
                {
                    if (!_critApplied)
                    {
                        _critOrig = stats._runtimeStatStruct._criticalRate;
                        _mcritOrig = stats._runtimeStatStruct._magicCriticalRate;
                        _critApplied = true;
                    }

                    stats._runtimeStatStruct._criticalRate = 1f;
                    stats._runtimeStatStruct._magicCriticalRate = 1f;
                    stats._statStruct._criticalRate = 1f;
                    stats._statStruct._magicCriticalRate = 1f;
                }
                else if (_critApplied)
                {
                    stats._runtimeStatStruct._criticalRate = _critOrig;
                    stats._runtimeStatStruct._magicCriticalRate = _mcritOrig;
                    stats._statStruct._criticalRate = _critOrig;
                    stats._statStruct._magicCriticalRate = _mcritOrig;
                    _critApplied = false;
                }
            }

            if (Settings.AutoParry.Value)
            {
                if (!_parryApplied)
                {
                    _parryOrig = status._autoParry;
                    _parryBufferOrig = status._parryBufferApply;
                    _parryApplied = true;
                }

                status._autoParry = true;
                status._parryBufferApply = 1f;
            }
            else if (_parryApplied)
            {
                status._autoParry = _parryOrig;
                status._parryBufferApply = _parryBufferOrig;
                _parryApplied = false;
            }

            if (Settings.PerfectAccuracy.Value)
            {
                if (!_accuracyApplied)
                {
                    _accuracyOrig = status._hitAccuracyPercent;
                    _accuracyApplied = true;
                }

                status._hitAccuracyPercent = 1f;
            }
            else if (_accuracyApplied)
            {
                status._hitAccuracyPercent = _accuracyOrig;
                _accuracyApplied = false;
            }

            if (Settings.ReflectDamage.Value)
            {
                if (!_reflectApplied)
                {
                    _reflectOrig = status._reflectDamage;
                    _reflectPercentOrig = status._reflectPercent;
                    _reflectApplied = true;
                }

                status._reflectDamage = true;
                status._reflectPercent = Mathf.Clamp(Settings.ReflectPercent.Value / 100f, 0f, 10f);
            }
            else if (_reflectApplied)
            {
                status._reflectDamage = _reflectOrig;
                status._reflectPercent = _reflectPercentOrig;
                _reflectApplied = false;
            }

            float absorb = Mathf.Clamp01(Settings.DamageAbsorption.Value);
            if (absorb > 0.001f)
            {
                if (!_absorbApplied)
                {
                    _absorbOrig = status._damageAbsorbtion;
                    _absorbApplied = true;
                }

                status._damageAbsorbtion = absorb;
            }
            else if (_absorbApplied)
            {
                status._damageAbsorbtion = _absorbOrig;
                _absorbApplied = false;
            }

            if (Settings.PurgeConditions.Value)
            {
                _purgeTimer -= Time.deltaTime;
                if (_purgeTimer <= 0f)
                {
                    _purgeTimer = 1f;
                    try { status.Clear_AllConditions(); } catch { }
                }
            }
        }

        private static void TickCasting()
        {
            Player p = LocalPlayer;
            if (p == null || p._pCasting == null) return;

            PlayerCasting casting = p._pCasting;

            if (Settings.NoCooldowns.Value)
            {
                _globalCdTimer -= Time.deltaTime;
                if (_globalCdTimer <= 0f)
                {
                    _globalCdTimer = 0.2f;
                    casting._underGlobalCooldown = false;
                    casting._globalCooldown = 0f;
                    if (casting._skillCoolDownSlots != null) casting._skillCoolDownSlots.Clear();
                }
            }

            if (Settings.InstantCast.Value && casting._isCasting)
                casting._castTimer = 999f;
        }

        private static float _baseSpeed = -1f;
        private static float _baseJump = -1f;
        private static bool _movementApplied;

        private static void TickMovement()
        {
            Player p = LocalPlayer;
            if (p == null || p._pMove == null) return;

            PlayerMove mv = p._pMove;

            TickGravity(mv);

            float speed = Settings.SpeedMultiplier.Value;
            float jump = Settings.JumpMultiplier.Value;
            int jumps = Settings.MaxJumps.Value;
            bool dash = Settings.NoDashCooldown.Value;

            bool active = speed > 1.001f || jump > 1.001f || jumps != 2 || dash;

            if (!active)
            {
                if (_movementApplied)
                {
                    if (_baseSpeed > 0f) mv._movSpeed = _baseSpeed;
                    if (_baseJump > 0f) mv._jumpForce = _baseJump;
                    mv._maxJumps = 2;
                    _baseSpeed = -1f;
                    _baseJump = -1f;
                    _movementApplied = false;
                }
                return;
            }

            if (_baseSpeed <= 0f) _baseSpeed = mv._movSpeed;
            if (_baseJump <= 0f) _baseJump = mv._standardJumpForce;

            if (speed > 1.001f)
                mv._movSpeed = _baseSpeed * speed;

            if (jump > 1.001f)
                mv._jumpForce = _baseJump * jump;

            mv._maxJumps = Mathf.Clamp(jumps, 1, 20);

            if (dash)
                ResetDashCooldown(mv);

            _movementApplied = true;
        }

        private static FieldInfo _gravityField;
        private static float _gravityBase = -1f;
        private static bool _gravityApplied;

        private static void TickGravity(PlayerMove mv)
        {
            float mult = Settings.GravityScale.Value;

            if (_gravityField == null)
            {
                _gravityField = typeof(PlayerMove).GetField("gravityScale",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }

            if (_gravityField == null) return;

            if (Mathf.Abs(mult - 1f) < 0.001f)
            {
                if (_gravityApplied)
                {
                    if (_gravityBase >= 0f) _gravityField.SetValue(mv, _gravityBase);
                    _gravityApplied = false;
                }
                return;
            }

            if (!_gravityApplied)
            {
                object current = _gravityField.GetValue(mv);
                _gravityBase = current is float ? (float)current : 1f;
                _gravityApplied = true;
            }

            _gravityField.SetValue(mv, _gravityBase * Mathf.Clamp(mult, 0f, 10f));
        }

        private static void TickNoclip()
        {
            Player p = LocalPlayer;
            if (p == null || p._pMove == null) return;

            CharacterController controller = p._pMove._playerController;

            bool active = Settings.Noclip.Value && !Menu.IsOpenNow;

            if (!active)
            {
                if (_noclipApplied && controller != null)
                {
                    controller.enabled = true;
                    _noclipApplied = false;
                }
                return;
            }

            if (!IsHost)
                return;

            if (controller != null && controller.enabled)
            {
                controller.enabled = false;
                _noclipApplied = true;
            }

            Camera camera = null;
            if (CameraFunction._current != null) camera = CameraFunction._current._mainCamera;
            if (camera == null) camera = Camera.main;

            Vector3 direction = Vector3.zero;

            if (camera != null)
            {
                Vector3 forward = camera.transform.forward;
                Vector3 right = camera.transform.right;
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();

                if (Input.GetKey(KeyCode.W)) direction += forward;
                if (Input.GetKey(KeyCode.S)) direction -= forward;
                if (Input.GetKey(KeyCode.D)) direction += right;
                if (Input.GetKey(KeyCode.A)) direction -= right;
            }

            if (Input.GetKey(KeyCode.Space)) direction += Vector3.up;
            if (Input.GetKey(KeyCode.LeftControl)) direction -= Vector3.up;

            if (direction.sqrMagnitude > 0.001f)
            {
                float speed = 10f * Mathf.Max(1f, Settings.SpeedMultiplier.Value);
                p.transform.position += direction.normalized * speed * Time.deltaTime;
            }
        }

        private static bool _noclipApplied;

        private static float _aiTimer;
        private static bool _aiApplied;

        private static void TickEnemies()
        {
            if (!Settings.FreezeAI.Value)
            {
                if (_aiApplied)
                {
                    Creep[] all = UnityEngine.Object.FindObjectsOfType<Creep>();
                    for (int i = 0; i < all.Length; i++)
                        if (all[i] != null) all[i]._disableAggro = false;

                    _aiApplied = false;
                }
                return;
            }

            _aiTimer -= Time.deltaTime;
            if (_aiTimer > 0f) return;
            _aiTimer = 0.5f;

            Creep[] creeps = UnityEngine.Object.FindObjectsOfType<Creep>();
            for (int i = 0; i < creeps.Length; i++)
            {
                Creep c = creeps[i];
                if (c == null) continue;

                c._disableAggro = true;
                try { c.Reset_AggroParameters(); } catch { }
            }

            _aiApplied = true;
        }

        private static FieldInfo _dashCooldownField;

        private static void ResetDashCooldown(PlayerMove mv)
        {
            if (_dashCooldownField == null)
            {
                _dashCooldownField = typeof(PlayerMove).GetField("dashCoolDown",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }

            if (_dashCooldownField != null)
                _dashCooldownField.SetValue(mv, 0f);
        }

        public static string Replenish()
        {
            if (!InGame) return L10n.T("Not in game", "Нет активного персонажа");

            StatusEntity status = Status;
            if (status == null) return L10n.T("No status entity", "Персонаж не найден");

            status.Replenish_All();
            return L10n.T("Restored", "Восстановлено");
        }

        public static void AddCurrency(int amount)
        {
            PlayerInventory inv = Inventory;
            if (inv == null) return;

            int target = inv._heldCurrency + amount;
            if (target < 0) target = 0;

            if (IsHost)
                inv.Add_Currency(amount);
            inv.Network_heldCurrency = target;
        }

        public static void SetCurrency(int value)
        {
            PlayerInventory inv = Inventory;
            if (inv == null) return;

            value = Mathf.Max(0, value);

            if (IsHost && value > inv._heldCurrency)
                inv.Add_Currency(value - inv._heldCurrency);

            inv.Network_heldCurrency = value;
        }

        public static bool GiveItem(ScriptableItem item, int quantity)
        {
            PlayerInventory inv = Inventory;
            if (inv == null || item == null) return false;

            try
            {
                int maxStack = Mathf.Max(1, item._maxStackAmount);
                int qty = Mathf.Clamp(quantity, 1, maxStack);

                ItemData data = new ItemData();
                data._itemName = item._itemName;
                data._quantity = qty;
                data._maxQuantity = maxStack;

                inv.Add_Item(data, true);
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("GiveItem: " + e);
                return false;
            }
        }

        /// <summary>Adds the item if needed and equips the first matching inventory entry.</summary>
        public static string EquipItem(ScriptableItem item)
        {
            if (!InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");

            PlayerEquipment eq = Equipment;
            PlayerInventory inv = Inventory;

            if (eq == null || inv == null)
                return L10n.T("No player", "Персонаж не найден");

            ScriptableEquipment gear = item as ScriptableEquipment;
            if (gear == null)
                return L10n.T("This item cannot be equipped", "Этот предмет нельзя надеть");

            try
            {
                ItemData data = inv.Fetch_HeldItem(item);
                if (data == null)
                {
                    GiveItem(item, 1);
                    data = inv.Fetch_HeldItem(item);
                }

                if (data == null)
                    return L10n.T("Equip failed: item not in inventory", "Не удалось надеть: предмета нет в инвентаре");

                eq.Init_EquipItem(data);
                return L10n.T("Equipped: ", "Надето: ") + L10n.Game(item._itemName);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("EquipItem: " + e);
                return L10n.T("Equip error", "Ошибка экипировки");
            }
        }

        // ---------------------------------------------------------------- classes

        private static List<ScriptablePlayerBaseClass> _classes;

        public static List<ScriptablePlayerBaseClass> Classes
        {
            get
            {
                if (_classes == null || _classes.Count == 0)
                {
                    _classes = new List<ScriptablePlayerBaseClass>();
                    Dictionary<string, ScriptablePlayerBaseClass> cache =
                        ReadCache<ScriptablePlayerBaseClass>(GameManager._current, "_cachedScriptablePlayerClasses");

                    if (cache != null)
                    {
                        foreach (var kv in cache)
                        {
                            if (kv.Value != null && !string.IsNullOrEmpty(kv.Value._className))
                                _classes.Add(kv.Value);
                        }
                    }

                    _classes.Sort(delegate (ScriptablePlayerBaseClass a, ScriptablePlayerBaseClass b)
                    {
                        return string.Compare(L10n.Game(a._className), L10n.Game(b._className),
                            StringComparison.CurrentCultureIgnoreCase);
                    });
                }

                return _classes;
            }
        }

        public static string SetClass(ScriptablePlayerBaseClass cls)
        {
            if (!InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");
            if (cls == null) return L10n.T("Class not found", "Класс не найден");

            PlayerStats st = Stats;
            if (st == null) return L10n.T("No player", "Персонаж не найден");

            if (!IsHost)
                return L10n.T("Class change requires hosting", "Смена класса доступна только хосту");

            try
            {
                st._class = cls;
                st.Network_syncClass = cls._className;
                st.Network_syncClassTier = 0;
                st.Apply_StatCalculations();
                return L10n.T("Class: ", "Класс: ") + L10n.Game(cls._className);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("SetClass: " + e);
                return L10n.T("Class change failed", "Не удалось сменить класс");
            }
        }

        // ---------------------------------------------------------------- teleport

        public sealed class SpawnTag
        {
            public string Tag;
            public Vector3 Position;
        }

        public static List<SpawnTag> SpawnTags()
        {
            List<SpawnTag> result = new List<SpawnTag>();
            MapInstance map = Map;
            if (map == null || map._instanceSpawnPoints == null) return result;

            var seen = new HashSet<string>();

            for (int i = 0; i < map._instanceSpawnPoints.Count; i++)
            {
                SpawnPoint point = map._instanceSpawnPoints[i];
                if (point == null) continue;

                string tag = point._spawnPointTag;
                if (string.IsNullOrEmpty(tag)) continue;
                if (!seen.Add(tag)) continue;

                SpawnTag entry = new SpawnTag();
                entry.Tag = tag;
                entry.Position = point.transform.position;
                result.Add(entry);
            }

            result.Sort(delegate (SpawnTag a, SpawnTag b)
            {
                return string.Compare(a.Tag, b.Tag, StringComparison.CurrentCultureIgnoreCase);
            });

            return result;
        }

        public static string TeleportTo(string spawnTag)
        {
            if (!InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");
            if (!IsHost) return L10n.T("Teleport requires hosting", "Телепорт доступен только хосту");

            MapInstance map = Map;
            Player player = LocalPlayer;
            if (map == null || player == null) return L10n.T("No map data", "Нет данных о карте");

            try
            {
                map.SendPeer_ToSpawnPoint(player.gameObject, spawnTag);
                return L10n.T("Teleported: ", "Телепорт: ") + spawnTag;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("TeleportTo: " + e);
                return L10n.T("Teleport failed", "Ошибка телепорта");
            }
        }

        public static void SetLevel(int level)
        {
            PlayerStats s = Stats;
            if (s == null) return;
            s.Network_currentLevel = Mathf.Clamp(level, 1, 999);
        }

        public static void SetExp(int exp)
        {
            PlayerStats s = Stats;
            if (s == null) return;
            s.Network_currentExp = Mathf.Max(0, exp);
        }

        public static void SetAttributePoints(int points)
        {
            PlayerStats s = Stats;
            if (s == null) return;
            s.Network_currentAttributePoints = Mathf.Max(0, points);
        }

        public static void SetSkillPoints(int points)
        {
            PlayerStats s = Stats;
            if (s == null) return;
            s._currentSkillPoints = Mathf.Max(0, points);
        }

        /// <summary>Writes a stat value into both the synced struct and the locally used runtime struct.</summary>
        public static void SetStat(string key, int value)
        {
            PlayerStats s = Stats;
            if (s == null) return;

            StatStruct st = s._statStruct;
            StatStruct rt = s._runtimeStatStruct;
            ApplyStat(ref st, key, value);
            ApplyStat(ref rt, key, value);
            s.Network_statStruct = st;
            s._runtimeStatStruct = rt;
        }

        public static int GetStat(string key)
        {
            PlayerStats s = Stats;
            if (s == null) return 0;
            StatStruct st = s._runtimeStatStruct;
            return ReadStat(ref st, key);
        }

        private static void ApplyStat(ref StatStruct st, string key, int value)
        {
            switch (key)
            {
                case "hp": st._maxHealth = value; break;
                case "mana": st._maxMana = value; break;
                case "stamina": st._maxStamina = value; break;
                case "attack": st._attackPower = value; break;
                case "magic": st._magicPower = value; break;
                case "dex": st._dexPower = value; break;
                case "defense": st._defense = value; break;
                case "mdefense": st._magicDefense = value; break;
                case "crit": st._criticalRate = value / 100f; break;
                case "mcrit": st._magicCriticalRate = value / 100f; break;
                case "evasion": st._evasion = value / 100f; break;
                case "fireres": st._fireResist = value; break;
                case "waterres": st._waterResist = value; break;
                case "natureres": st._natureResist = value; break;
                case "earthres": st._earthResist = value; break;
                case "holyres": st._holyResist = value; break;
                case "shadowres": st._shadowResist = value; break;
            }
        }

        private static int ReadStat(ref StatStruct st, string key)
        {
            switch (key)
            {
                case "hp": return st._maxHealth;
                case "mana": return st._maxMana;
                case "stamina": return st._maxStamina;
                case "attack": return st._attackPower;
                case "magic": return st._magicPower;
                case "dex": return st._dexPower;
                case "defense": return st._defense;
                case "mdefense": return st._magicDefense;
                case "crit": return Mathf.RoundToInt(st._criticalRate * 100f);
                case "mcrit": return Mathf.RoundToInt(st._magicCriticalRate * 100f);
                case "evasion": return Mathf.RoundToInt(st._evasion * 100f);
                case "fireres": return st._fireResist;
                case "waterres": return st._waterResist;
                case "natureres": return st._natureResist;
                case "earthres": return st._earthResist;
                case "holyres": return st._holyResist;
                case "shadowres": return st._shadowResist;
            }
            return 0;
        }

        public static void RecalculateStats()
        {
            PlayerStats s = Stats;
            if (s == null) return;
            s.Apply_StatCalculations();
        }

        public static void SaveProfile()
        {
            if (ProfileDataManager._current != null)
                ProfileDataManager._current.Save_ProfileData();
        }

        // ---------------------------------------------------------------- world

        /// <summary>Sets the world clock. The local map instance always changes (client-side),
        /// the global day/night cycle only changes when hosting.</summary>
        public static void SetTime(int hour24, bool global)
        {
            hour24 = ((hour24 % 24) + 24) % 24;

            bool pm = hour24 >= 12;
            int hour12 = hour24 % 12;
            if (hour12 == 0) hour12 = 12;

            WorldTime wt = (hour24 >= 20 || hour24 < 6) ? WorldTime.NIGHT : WorldTime.DAY;
            ClockSetting cs = pm ? ClockSetting.PM : ClockSetting.AM;

            MapInstance map = Map;
            if (map != null)
            {
                map.Network_instanceTime = hour12;
                map.Network_instanceClockSetting = cs;
                map.Network_instanceWorldTime = wt;
            }

            GameWorldManager world = World;
            if (world != null && (global ? IsHost : true))
            {
                world._timeDisplay = hour12;
                world._clockSetting = cs;
                world._worldTime = wt;
            }
        }

        public static int GetHour24()
        {
            GameWorldManager world = World;
            if (world == null) return 12;

            int hour12 = Mathf.Clamp(world._timeDisplay, 1, 12);
            bool pm = world._clockSetting == ClockSetting.PM;

            if (hour12 == 12) return pm ? 12 : 0;
            return pm ? hour12 + 12 : hour12;
        }

        // ---------------------------------------------------------------- creeps

        public static bool SpawnCreep(ScriptableCreep creep, int quantity, int level)
        {
            if (!IsHost)
                return false;

            AtlyssNetworkManager nm = AtlyssNetworkManager._current;
            if (nm == null || nm._networkObjectLibrary == null || nm._networkObjectLibrary._creepObject == null)
                return false;

            Player player = LocalPlayer;
            if (player == null) return false;

            int count = Mathf.Clamp(quantity, 1, 25);

            for (int i = 0; i < count; i++)
            {
                try
                {
                    float angle = (360f / count) * i;
                    Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * 3f;

                    GameObject go = UnityEngine.Object.Instantiate(nm._networkObjectLibrary._creepObject);
                    go.transform.position = player.transform.position + offset + Vector3.up * 0.2f;

                    Creep c = go.GetComponent<Creep>();
                    if (c == null)
                    {
                        UnityEngine.Object.Destroy(go);
                        continue;
                    }

                    if (creep != null)
                    {
                        c._scriptCreep = creep;
                        c._creepNameID = creep._creepName;
                        c.Network_creepNameID = creep._creepName;
                        c.Network_currentCreepCondition = CreepCondition.SPAWNING;
                        c._suppressWandererBehavior = false;
                    }

                    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, player.gameObject.scene);

                    NetworkServer.Spawn(go, (NetworkConnection)null);

                    if (level > 0)
                        c.Network_creepLevel = level;

                    int modifier = Settings.SpawnModifier.Value;
                    if (modifier > 0)
                        c.Network_statModifierID = modifier;

                    c.Apply_BaseCreepData();
                    c.BaseCreep_OnSpawn();
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("SpawnCreep: " + e);
                    return false;
                }
            }

            return true;
        }

        public static int KillNearbyCreeps(float radius)
        {
            int killed = 0;
            Creep[] creeps = UnityEngine.Object.FindObjectsOfType<Creep>();
            Player player = LocalPlayer;
            if (player == null) return 0;

            for (int i = 0; i < creeps.Length; i++)
            {
                Creep c = creeps[i];
                if (c == null) continue;
                if (Vector3.Distance(c.transform.position, player.transform.position) > radius) continue;

                try
                {
                    if (c._statusEntity != null)
                    {
                        c._statusEntity.Network_currentHealth = 0;
                        if (IsHost)
                            c.BaseCreep_OnDeath();
                    }
                    killed++;
                }
                catch (Exception e)
                {
                    Plugin.Log.LogWarning("KillNearby: " + e.Message);
                }
            }

            return killed;
        }

        public static int KillAllCreeps()
        {
            return KillNearbyCreeps(999999f);
        }

        // ---------------------------------------------------------------- world extras

        public static void SetWeather(bool enabled)
        {
            MapInstance map = Map;
            if (map != null)
                map.Network_isWeatherEnabled = enabled;
        }

        public sealed class MapEntry
        {
            public string Caption;
            public string SubScene;
            public string SpawnTag;
        }

        public static List<MapEntry> Maps()
        {
            List<MapEntry> result = new List<MapEntry>();

            Dictionary<string, ScriptableMapData> cache =
                ReadCache<ScriptableMapData>(GameManager._current, "_cachedScriptableMapDatas");

            if (cache == null) return result;

            foreach (var kv in cache)
            {
                ScriptableMapData map = kv.Value;
                if (map == null || string.IsNullOrEmpty(map._subScene)) continue;

                MapEntry entry = new MapEntry();
                entry.Caption = string.IsNullOrEmpty(map._mapCaptionTitle) ? kv.Key : map._mapCaptionTitle;
                entry.SubScene = map._subScene;
                entry.SpawnTag = map._spawnPointTag;
                result.Add(entry);
            }

            result.Sort(delegate (MapEntry a, MapEntry b)
            {
                return string.Compare(L10n.Game(a.Caption), L10n.Game(b.Caption), StringComparison.CurrentCultureIgnoreCase);
            });

            return result;
        }

        public static string TravelTo(MapEntry map)
        {
            if (!InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");
            if (map == null) return L10n.T("Map not found", "Карта не найдена");

            Player player = LocalPlayer;
            if (player == null) return L10n.T("No player", "Персонаж не найден");

            try
            {
                player.Cmd_SceneTransport(map.SubScene, map.SpawnTag, ZoneDifficulty.NORMAL);
                return L10n.T("Traveling: ", "Переход: ") + L10n.Game(map.Caption);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("TravelTo: " + e);
                return L10n.T("Travel failed", "Ошибка перехода");
            }
        }

        public static int SetSpawners(bool disabled)
        {
            CreepSpawner[] spawners = UnityEngine.Object.FindObjectsOfType<CreepSpawner>();
            int changed = 0;

            for (int i = 0; i < spawners.Length; i++)
            {
                CreepSpawner s = spawners[i];
                if (s == null) continue;

                try
                {
                    if (disabled) s.Disable_Spawner();
                    else s.Enable_Spawner();
                    changed++;
                }
                catch (Exception e)
                {
                    Plugin.Log.LogWarning("SetSpawners: " + e.Message);
                }
            }

            return changed;
        }

        // ---------------------------------------------------------------- items extras

        public static string DropItem(ScriptableItem item, int quantity)
        {
            if (!InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");
            if (!IsHost) return L10n.T("Dropping items requires hosting", "Дроп предметов доступен только хосту");
            if (item == null) return L10n.T("No item", "Предмет не выбран");

            try
            {
                int maxStack = Mathf.Max(1, item._maxStackAmount);
                ItemData data = new ItemData();
                data._itemName = item._itemName;
                data._quantity = Mathf.Clamp(quantity, 1, maxStack);
                data._maxQuantity = maxStack;

                Player player = LocalPlayer;
                Vector3 position = player.transform.position + player.transform.forward * 2f + Vector3.up;

                GameManager._current.Server_SpawnNetItemObject(null, data, null, position, 0, player.gameObject.scene);
                return L10n.T("Dropped: ", "Выброшено: ") + L10n.Game(item._itemName);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("DropItem: " + e);
                return L10n.T("Drop failed", "Не удалось выбросить");
            }
        }

        public static string DuplicateItem(ScriptableItem item)
        {
            if (!InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");

            PlayerInventory inv = Inventory;
            if (inv == null || item == null) return L10n.T("No player", "Персонаж не найден");

            try
            {
                ItemData held = inv.Fetch_HeldItem(item);
                if (held == null)
                {
                    GiveItem(item, 1);
                    return L10n.T("Added: ", "Добавлено: ") + L10n.Game(item._itemName);
                }

                ItemData clone = new ItemData();
                clone._itemName = held._itemName;
                clone._quantity = Mathf.Max(1, held._quantity);
                clone._maxQuantity = held._maxQuantity;
                clone._modifierID = held._modifierID;
                clone._useDamageTypeOverride = held._useDamageTypeOverride;
                clone._damageTypeOverride = held._damageTypeOverride;

                inv.Add_Item(clone, false);
                return L10n.T("Duplicated: ", "Дублировано: ") + L10n.Game(item._itemName);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("DuplicateItem: " + e);
                return L10n.T("Duplicate failed", "Не удалось дублировать");
            }
        }

        public static string GiveAllItems()
        {
            if (!InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");

            int given = 0;
            List<ScriptableItem> all = Items;

            for (int i = 0; i < all.Count; i++)
            {
                try
                {
                    GiveItem(all[i], 1);
                    given++;
                }
                catch
                {
                    // skip broken entries
                }
            }

            return L10n.T("Items given: ", "Выдано предметов: ") + given;
        }

        // ---------------------------------------------------------------- character extras

        public static PlayerAppearanceStruct Appearance
        {
            get
            {
                Player p = LocalPlayer;
                return p == null || p._pVisual == null ? new PlayerAppearanceStruct() : p._pVisual._playerAppearanceStruct;
            }
        }

        public static void SetAppearance(PlayerAppearanceStruct appearance)
        {
            Player p = LocalPlayer;
            if (p == null || p._pVisual == null) return;

            try
            {
                p._pVisual.Network_playerAppearanceStruct = appearance;
                p._pVisual.Cmd_SendNew_PlayerAppearanceStruct(appearance);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("SetAppearance: " + e);
            }
        }

        public static List<ProfessionStruct> Professions
        {
            get
            {
                PlayerStats s = Stats;
                return s == null || s._playerProfessions == null
                    ? new List<ProfessionStruct>()
                    : s._playerProfessions;
            }
        }

        public static void SetProfessionLevel(int index, int level)
        {
            PlayerStats s = Stats;
            if (s == null || s._playerProfessions == null) return;
            if (index < 0 || index >= s._playerProfessions.Count) return;

            ProfessionStruct value = s._playerProfessions[index];
            value._professionLvl = Mathf.Max(0, level);
            value._professionXP = 0;
            s._playerProfessions[index] = value;
        }

        public static string SetClassTier(int tier)
        {
            if (!InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");
            if (!IsHost) return L10n.T("Class tier requires hosting", "Ранг класса доступен только хосту");

            PlayerStats s = Stats;
            if (s == null) return L10n.T("No player", "Персонаж не найден");

            s.Network_syncClassTier = Mathf.Clamp(tier, 0, 9);
            return L10n.T("Class tier: ", "Ранг класса: ") + s._syncClassTier;
        }

        public static string UnlockAllSkills()
        {
            if (!InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");

            Player p = LocalPlayer;
            if (p == null || p._pCasting == null) return L10n.T("No player", "Персонаж не найден");

            int unlocked = 0;

            try
            {
                int count = p._pCasting._skillLibrary == null ? 0 : p._pCasting._skillLibrary.Count;
                for (int i = 0; i < count; i++)
                {
                    p._pCasting.Client_Unlock_Skill(i);
                    unlocked++;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("UnlockAllSkills: " + e);
            }

            return L10n.T("Skills unlocked: ", "Открыто навыков: ") + unlocked;
        }

        public static string CompleteActiveQuests()
        {
            if (!InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");

            Player p = LocalPlayer;
            if (p == null || p._pQuest == null) return L10n.T("No player", "Персонаж не найден");

            int completed = 0;

            try
            {
                List<QuestProgressStruct> quests = p._pQuest._questProgressData;
                if (quests != null)
                {
                    for (int i = quests.Count - 1; i >= 0; i--)
                    {
                        p._pQuest.Client_CompleteQuest(i);
                        completed++;
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("CompleteActiveQuests: " + e);
            }

            return L10n.T("Quests completed: ", "Завершено квестов: ") + completed;
        }

        public static string UnlockAllWaypoints()
        {
            if (!InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");

            Player p = LocalPlayer;
            MapInstance map = Map;
            if (p == null || map == null || map._instanceSpawnPoints == null)
                return L10n.T("No map data", "Нет данных о карте");

            if (p._waypointAttunements == null)
                p._waypointAttunements = new List<string>();

            int added = 0;

            for (int i = 0; i < map._instanceSpawnPoints.Count; i++)
            {
                SpawnPoint point = map._instanceSpawnPoints[i];
                if (point == null || string.IsNullOrEmpty(point._spawnPointTag)) continue;
                if (p._waypointAttunements.Contains(point._spawnPointTag)) continue;

                p._waypointAttunements.Add(point._spawnPointTag);
                added++;
            }

            return L10n.T("Waypoints unlocked: ", "Открыто точек: ") + added;
        }

        public static List<Player> OtherPlayers()
        {
            List<Player> result = new List<Player>();
            Player local = LocalPlayer;
            Player[] all = UnityEngine.Object.FindObjectsOfType<Player>();

            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i] != local)
                    result.Add(all[i]);
            }

            return result;
        }

        public static string TeleportToPlayer(Player target)
        {
            if (!InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");
            if (target == null) return L10n.T("Player not found", "Игрок не найден");

            Player local = LocalPlayer;
            local.transform.position = target.transform.position + target.transform.forward * -1.5f + Vector3.up * 0.5f;
            return L10n.T("Teleported to ", "Телепорт к ") + target._nickname;
        }

        public static string TeleportToCursor()
        {
            if (!InGame) return L10n.T("Load a character first", "Сначала загрузите персонажа");

            Player player = LocalPlayer;
            Camera camera = null;

            if (CameraFunction._current != null) camera = CameraFunction._current._mainCamera;
            if (camera == null) camera = Camera.main;
            if (camera == null) return L10n.T("No camera", "Камера не найдена");

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Vector3 position;
            if (Physics.Raycast(ray, out hit, 500f))
                position = hit.point + Vector3.up * 1.2f;
            else
                position = ray.GetPoint(15f);

            player.transform.position = position;
            return L10n.T("Teleported", "Телепорт выполнен");
        }

        public static string SetCharacterScale(float scale)
        {
            Player p = LocalPlayer;
            if (p == null) return L10n.T("No player", "Персонаж не найден");

            p.transform.localScale = Vector3.one * Mathf.Clamp(scale, 0.3f, 3f);
            return L10n.T("Scale: ", "Масштаб: ") + Mathf.RoundToInt(scale * 100f) + "%";
        }
    }
}
