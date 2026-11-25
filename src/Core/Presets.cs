using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BepInEx.Configuration;

namespace AtlyssTrainer
{
    /// <summary>Named snapshots of the cheat toggles stored in the BepInEx config.</summary>
    internal static class Presets
    {
        private static ConfigEntry<string> Slot(int index)
        {
            switch (index)
            {
                case 1: return Settings.Preset2;
                case 2: return Settings.Preset3;
                default: return Settings.Preset1;
            }
        }

        public static string Save(int index)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("god=").Append(B(Settings.GodMode.Value));
            sb.Append(";onehit=").Append(B(Settings.OneHitKill.Value));
            sb.Append(";dmg=").Append(F(Settings.DamageMultiplier.Value));
            sb.Append(";inc=").Append(F(Settings.IncomingMultiplier.Value));
            sb.Append(";mana=").Append(B(Settings.InfiniteMana.Value));
            sb.Append(";stam=").Append(B(Settings.InfiniteStamina.Value));
            sb.Append(";crit=").Append(B(Settings.AlwaysCrit.Value));
            sb.Append(";acc=").Append(B(Settings.PerfectAccuracy.Value));
            sb.Append(";parry=").Append(B(Settings.AutoParry.Value));
            sb.Append(";refl=").Append(B(Settings.ReflectDamage.Value));
            sb.Append(";reflp=").Append(F(Settings.ReflectPercent.Value));
            sb.Append(";absorb=").Append(F(Settings.DamageAbsorption.Value));
            sb.Append(";cd=").Append(B(Settings.NoCooldowns.Value));
            sb.Append(";cast=").Append(B(Settings.InstantCast.Value));
            sb.Append(";purge=").Append(B(Settings.PurgeConditions.Value));
            sb.Append(";freeze=").Append(B(Settings.FreezeTime.Value));
            sb.Append(";speed=").Append(F(Settings.SpeedMultiplier.Value));
            sb.Append(";jump=").Append(F(Settings.JumpMultiplier.Value));
            sb.Append(";jumps=").Append(Settings.MaxJumps.Value);
            sb.Append(";dash=").Append(B(Settings.NoDashCooldown.Value));
            sb.Append(";grav=").Append(F(Settings.GravityScale.Value));
            sb.Append(";noclip=").Append(B(Settings.Noclip.Value));
            sb.Append(";money=").Append(B(Settings.NoCurrencyLoss.Value));
            sb.Append(";ai=").Append(B(Settings.FreezeAI.Value));

            Slot(index).Value = sb.ToString();
            return sb.ToString();
        }

        public static bool Load(int index)
        {
            string raw = Slot(index).Value;
            if (string.IsNullOrEmpty(raw)) return false;

            Dictionary<string, string> values = new Dictionary<string, string>();
            string[] parts = raw.Split(';');

            for (int i = 0; i < parts.Length; i++)
            {
                string[] pair = parts[i].Split('=');
                if (pair.Length == 2) values[pair[0]] = pair[1];
            }

            Settings.GodMode.Value = ReadBool(values, "god");
            Settings.OneHitKill.Value = ReadBool(values, "onehit");
            Settings.DamageMultiplier.Value = ReadFloat(values, "dmg", 1f);
            Settings.IncomingMultiplier.Value = ReadFloat(values, "inc", 1f);
            Settings.InfiniteMana.Value = ReadBool(values, "mana");
            Settings.InfiniteStamina.Value = ReadBool(values, "stam");
            Settings.AlwaysCrit.Value = ReadBool(values, "crit");
            Settings.PerfectAccuracy.Value = ReadBool(values, "acc");
            Settings.AutoParry.Value = ReadBool(values, "parry");
            Settings.ReflectDamage.Value = ReadBool(values, "refl");
            Settings.ReflectPercent.Value = ReadFloat(values, "reflp", 100f);
            Settings.DamageAbsorption.Value = ReadFloat(values, "absorb", 0f);
            Settings.NoCooldowns.Value = ReadBool(values, "cd");
            Settings.InstantCast.Value = ReadBool(values, "cast");
            Settings.PurgeConditions.Value = ReadBool(values, "purge");
            Settings.FreezeTime.Value = ReadBool(values, "freeze");
            Settings.SpeedMultiplier.Value = ReadFloat(values, "speed", 1f);
            Settings.JumpMultiplier.Value = ReadFloat(values, "jump", 1f);
            Settings.MaxJumps.Value = ReadInt(values, "jumps", 2);
            Settings.NoDashCooldown.Value = ReadBool(values, "dash");
            Settings.GravityScale.Value = ReadFloat(values, "grav", 1f);
            Settings.Noclip.Value = ReadBool(values, "noclip");
            Settings.NoCurrencyLoss.Value = ReadBool(values, "money");
            Settings.FreezeAI.Value = ReadBool(values, "ai");

            return true;
        }

        public static bool Has(int index)
        {
            return !string.IsNullOrEmpty(Slot(index).Value);
        }

        private static string B(bool value)
        {
            return value ? "1" : "0";
        }

        private static string F(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static bool ReadBool(Dictionary<string, string> values, string key)
        {
            string raw;
            return values.TryGetValue(key, out raw) && raw == "1";
        }

        private static float ReadFloat(Dictionary<string, string> values, string key, float fallback)
        {
            string raw;
            float value;
            if (values.TryGetValue(key, out raw) &&
                float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return value;

            return fallback;
        }

        private static int ReadInt(Dictionary<string, string> values, string key, int fallback)
        {
            string raw;
            int value;
            if (values.TryGetValue(key, out raw) && int.TryParse(raw, out value))
                return value;

            return fallback;
        }
    }
}
