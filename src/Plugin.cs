using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace AtlyssTrainer
{
    [BepInPlugin(Guid, "Atlyss Trainer", Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "peppa.atlyss.cheatmenu";
        public const string Version = "1.4.88";

        public static Plugin Instance;
        public static ManualLogSource Log;

        internal static Menu MainMenu;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            Settings.Init(Config);
            Theme.Apply(Settings.MenuTheme.Value);
            L10n.Init();

            Log.LogInfo("Game version: " + GameApi.GameVersion + ", trainer " + Version);

            try
            {
                new Harmony(Guid).PatchAll();
                Log.LogInfo("Patches applied.");
            }
            catch (Exception e)
            {
                Log.LogError("Patch failure: " + e);
            }

            Log.LogInfo("Atlyss Trainer loaded. Press " + Settings.MenuKey.Value + ".");
        }

        private void Update()
        {
            try
            {
                if (Input.GetKeyDown(Settings.MenuKey.Value))
                {
                    if (MainMenu == null)
                        MainMenu = Menu.Create();

                    MainMenu.Toggle();
                }

                if (Settings.OverlayKey.Value != KeyCode.None && Input.GetKeyDown(Settings.OverlayKey.Value))
                    Settings.ShowOverlay.Value = !Settings.ShowOverlay.Value;

                if (Settings.GodKey.Value != KeyCode.None && Input.GetKeyDown(Settings.GodKey.Value))
                    Settings.GodMode.Value = !Settings.GodMode.Value;

                if (MainMenu != null)
                    MainMenu.Tick();

                GameApi.TickCheats();
                Overlay.Tick();
            }
            catch (Exception e)
            {
                Log.LogError("Update: " + e);
            }
        }

        private void LateUpdate()
        {
            try
            {
                if (MainMenu != null)
                    MainMenu.LateTick();
            }
            catch (Exception e)
            {
                Log.LogError("LateUpdate: " + e);
            }
        }

        private void OnDestroy()
        {
            if (MainMenu != null)
                MainMenu.DestroyMenu();
        }
    }

    internal static class Settings
    {
        public static ConfigEntry<KeyCode> MenuKey;
        public static ConfigEntry<KeyCode> OverlayKey;
        public static ConfigEntry<KeyCode> GodKey;
        public static ConfigEntry<float> UiScale;
        public static ConfigEntry<bool> AutoRussian;
        public static ConfigEntry<bool> LockInput;
        public static ConfigEntry<bool> ShowOverlay;
        public static ConfigEntry<float> MenuOpacity;
        public static ConfigEntry<string> MenuTheme;
        public static ConfigEntry<string> MenuFont;
        public static ConfigEntry<bool> BannerAnimate;
        public static ConfigEntry<int> BannerFps;

        public static ConfigEntry<bool> GodMode;
        public static ConfigEntry<bool> InfiniteMana;
        public static ConfigEntry<bool> InfiniteStamina;
        public static ConfigEntry<bool> OneHitKill;
        public static ConfigEntry<float> DamageMultiplier;
        public static ConfigEntry<float> IncomingMultiplier;
        public static ConfigEntry<bool> AlwaysCrit;
        public static ConfigEntry<bool> PerfectAccuracy;
        public static ConfigEntry<bool> AutoParry;
        public static ConfigEntry<bool> ReflectDamage;
        public static ConfigEntry<float> ReflectPercent;
        public static ConfigEntry<float> DamageAbsorption;
        public static ConfigEntry<bool> NoCooldowns;
        public static ConfigEntry<bool> InstantCast;
        public static ConfigEntry<bool> PurgeConditions;
        public static ConfigEntry<bool> FreezeTime;
        public static ConfigEntry<bool> KeepBuffs;

        public static ConfigEntry<float> SpeedMultiplier;
        public static ConfigEntry<float> JumpMultiplier;
        public static ConfigEntry<int> MaxJumps;
        public static ConfigEntry<bool> NoDashCooldown;
        public static ConfigEntry<float> GravityScale;
        public static ConfigEntry<bool> Noclip;

        public static ConfigEntry<bool> NoCurrencyLoss;
        public static ConfigEntry<bool> Weather;
        public static ConfigEntry<bool> FreezeAI;
        public static ConfigEntry<int> SpawnModifier;

        public static ConfigEntry<string> Preset1;
        public static ConfigEntry<string> Preset2;
        public static ConfigEntry<string> Preset3;

        public static void Init(ConfigFile cfg)
        {
            MenuKey = cfg.Bind("General", "MenuKey", KeyCode.Insert, "Key that opens the cheat menu.");
            OverlayKey = cfg.Bind("General", "OverlayKey", KeyCode.F3, "Key that toggles the active-cheats overlay.");
            GodKey = cfg.Bind("General", "GodModeKey", KeyCode.None, "Optional hotkey for god mode.");
            UiScale = cfg.Bind("General", "UiScale", 1.0f, "Menu scale multiplier.");
            AutoRussian = cfg.Bind("General", "AutoRussian", true, "Use Russian UI when the Russian translation mod is installed.");
            LockInput = cfg.Bind("General", "LockInput", true, "Freeze the character and camera while the menu is open.");
            ShowOverlay = cfg.Bind("General", "ShowOverlay", true, "Show the active cheats overlay.");
            MenuOpacity = cfg.Bind("General", "MenuOpacity", 0.15f, "Menu background transparency (0 = solid, 0.9 = almost invisible).");
            MenuTheme = cfg.Bind("General", "MenuTheme", "green", "Menu colour theme.");
            MenuFont = cfg.Bind("General", "MenuFont", "Consolas", "Menu font family.");
            BannerAnimate = cfg.Bind("General", "BannerAnimate", true, "Animate the cracktro banner.");
            BannerFps = cfg.Bind("General", "BannerFps", 5, "Banner animation updates per second (1-30).");

            GodMode = cfg.Bind("Cheats", "GodMode", false, "Ignore all damage taken by the local player (host only).");
            InfiniteMana = cfg.Bind("Cheats", "InfiniteMana", false, "Keep mana at maximum.");
            InfiniteStamina = cfg.Bind("Cheats", "InfiniteStamina", false, "Keep stamina at maximum.");
            OneHitKill = cfg.Bind("Cheats", "OneHitKill", false, "Deal massive damage with every hit (host only).");
            DamageMultiplier = cfg.Bind("Cheats", "DamageMultiplier", 1f, "Outgoing damage multiplier (host only).");
            IncomingMultiplier = cfg.Bind("Cheats", "IncomingMultiplier", 1f, "Damage taken multiplier (0 = invulnerable).");
            AlwaysCrit = cfg.Bind("Cheats", "AlwaysCrit", false, "Force every hit to crit.");
            PerfectAccuracy = cfg.Bind("Cheats", "PerfectAccuracy", false, "Never miss.");
            AutoParry = cfg.Bind("Cheats", "AutoParry", false, "Parry everything automatically.");
            ReflectDamage = cfg.Bind("Cheats", "ReflectDamage", false, "Reflect a share of incoming damage.");
            ReflectPercent = cfg.Bind("Cheats", "ReflectPercent", 100f, "Reflected damage percentage.");
            DamageAbsorption = cfg.Bind("Cheats", "DamageAbsorption", 0f, "Flat damage absorption (0..1).");
            NoCooldowns = cfg.Bind("Cheats", "NoCooldowns", false, "Skills never go on cooldown.");
            InstantCast = cfg.Bind("Cheats", "InstantCast", false, "Cast skills instantly.");
            PurgeConditions = cfg.Bind("Cheats", "PurgeConditions", false, "Clear all debuffs every second.");
            FreezeTime = cfg.Bind("Cheats", "FreezeTime", false, "Stop the day/night cycle (host only).");
            KeepBuffs = cfg.Bind("Cheats", "KeepBuffs", false, "Refill health every frame so conditions cannot kill you.");

            SpeedMultiplier = cfg.Bind("Movement", "SpeedMultiplier", 1f, "Movement speed multiplier (1 = vanilla).");
            JumpMultiplier = cfg.Bind("Movement", "JumpMultiplier", 1f, "Jump force multiplier (1 = vanilla).");
            MaxJumps = cfg.Bind("Movement", "MaxJumps", 2, "Maximum number of jumps (2 = vanilla).");
            NoDashCooldown = cfg.Bind("Movement", "NoDashCooldown", false, "Removes the dash cooldown.");
            GravityScale = cfg.Bind("Movement", "GravityScale", 1f, "Gravity multiplier for the player.");
            Noclip = cfg.Bind("Movement", "Noclip", false, "Fly through walls (host only).");

            NoCurrencyLoss = cfg.Bind("Economy", "NoCurrencyLoss", false, "Purchases do not subtract gold.");
            Weather = cfg.Bind("World", "Weather", true, "Weather state on the local map instance.");
            FreezeAI = cfg.Bind("Enemies", "FreezeAI", false, "Disable enemy aggro and behaviour (host only).");
            SpawnModifier = cfg.Bind("Enemies", "SpawnModifier", 0, "Stat modifier id applied to spawned enemies.");

            Preset1 = cfg.Bind("Presets", "Slot1", "", "Preset slot 1");
            Preset2 = cfg.Bind("Presets", "Slot2", "", "Preset slot 2");
            Preset3 = cfg.Bind("Presets", "Slot3", "", "Preset slot 3");
        }
    }
}
