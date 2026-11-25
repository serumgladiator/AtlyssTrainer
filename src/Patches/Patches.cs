using HarmonyLib;
using UnityEngine;

namespace AtlyssTrainer
{
    internal static class Patches
    {
        // ------------------------------------------------------------------ survival

        /// <summary>Blocks all damage routed to the local player while god mode is on.</summary>
        [HarmonyPatch(typeof(StatusEntity), "Take_Damage")]
        private static class GodModePatch
        {
            private static bool Prefix(StatusEntity __instance, ref DamageStruct _dmgStruct)
            {
                Player player = Player._mainPlayer;
                bool isLocal = player != null && player._statusEntity == __instance;

                if (isLocal && Settings.GodMode.Value)
                    return false;

                if (isLocal)
                {
                    float mult = Settings.IncomingMultiplier.Value;
                    if (mult < 0.999f)
                        _dmgStruct._damageValue = Mathf.RoundToInt(_dmgStruct._damageValue * Mathf.Max(0f, mult));
                }

                return true;
            }
        }

        /// <summary>Scales outgoing player damage (host authoritative damage calculation).</summary>
        [HarmonyPatch(typeof(CombatCollider), "Server_ApplyDamage")]
        private static class DamageMultiplierPatch
        {
            private static void Prefix(CombatCollider __instance, ref int _damageValue)
            {
                if (__instance == null || __instance._isParentPlayer == null)
                    return;

                PlayerStats local = GameApi.Stats;
                if (local == null || __instance._isParentPlayer != local)
                    return;

                if (Settings.OneHitKill.Value)
                {
                    _damageValue = 9999999;
                    return;
                }

                float mult = Settings.DamageMultiplier.Value;
                if (mult > 1.001f)
                    _damageValue = Mathf.RoundToInt(_damageValue * mult);
            }
        }

        /// <summary>Stops the day/night cycle so the clock stays where the user put it.</summary>
        [HarmonyPatch(typeof(GameWorldManager), "Server_DayNightCycleRuntime")]
        private static class FreezeTimePatch
        {
            private static bool Prefix()
            {
                return !Settings.FreezeTime.Value;
            }
        }

        // ------------------------------------------------------------------ economy / skills

        /// <summary>Purchases never subtract gold.</summary>
        [HarmonyPatch(typeof(PlayerInventory), "Subtract_Currency")]
        private static class NoCurrencyLossPatch
        {
            private static bool Prefix()
            {
                return !Settings.NoCurrencyLoss.Value;
            }
        }

        [HarmonyPatch(typeof(PlayerInventory), "Cmd_SubtractCurrency")]
        private static class NoCurrencyLossCmdPatch
        {
            private static bool Prefix()
            {
                return !Settings.NoCurrencyLoss.Value;
            }
        }

        /// <summary>Skills never enter cooldown.</summary>
        [HarmonyPatch(typeof(PlayerCasting), "New_CooldownSlot")]
        private static class NoCooldownPatch
        {
            private static bool Prefix()
            {
                return !Settings.NoCooldowns.Value;
            }
        }

        // ------------------------------------------------------------------ menu input lock

        private static bool Blocked
        {
            get { return Menu.IsOpenNow && Settings.LockInput.Value; }
        }

        [HarmonyPatch(typeof(CameraFunction), "Update")]
        private static class CameraUpdateLockPatch
        {
            private static bool Prefix()
            {
                return !Blocked;
            }
        }

        [HarmonyPatch(typeof(CameraFunction), "Handle_CameraControl")]
        private static class CameraLockPatch
        {
            private static bool Prefix()
            {
                return !Blocked;
            }
        }

        [HarmonyPatch(typeof(PlayerCombat), "Init_Attack")]
        private static class AttackLockPatch
        {
            private static bool Prefix()
            {
                return !Blocked;
            }
        }

        [HarmonyPatch(typeof(PlayerCasting), "Client_SkillControl")]
        private static class CastControlLockPatch
        {
            private static bool Prefix()
            {
                return !Blocked;
            }
        }

        [HarmonyPatch(typeof(PlayerInteract), "Handle_InteractControl")]
        private static class InteractLockPatch
        {
            private static bool Prefix()
            {
                return !Blocked;
            }
        }
    }
}
