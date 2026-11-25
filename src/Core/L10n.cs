using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using BepInEx;
using Newtonsoft.Json;

namespace AtlyssTrainer
{
    /// <summary>
    /// Mirrors the Russian translation mod: exact phrase dictionary first, then its regex table.
    /// All text shown by this menu comes from the game itself, so item names stay in sync with
    /// whatever the installed translation mod produces.
    /// </summary>
    internal static class L10n
    {
        public static bool Russian;
        public static bool CyrillicFont = true;

        private static Dictionary<string, string> _exact;
        private static List<KeyValuePair<Regex, string>> _regex;
        private static readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        public static void Init()
        {
            Russian = false;
            _exact = null;
            _regex = null;
            _cache.Clear();

            if (!Settings.AutoRussian.Value)
                return;

            try
            {
                string dir = Path.Combine(Paths.GameRootPath, "Translation");
                string phrases = Path.Combine(dir, "phrases.json");
                string regexFile = Path.Combine(dir, "regex_phrases.json");
                string plugin = Path.Combine(Paths.PluginPath, "ATLYSSTranslations.dll");

                if (!File.Exists(phrases) || !File.Exists(regexFile))
                    return;

                if (!File.Exists(plugin) && !Directory.Exists(dir))
                    return;

                // The translation mod ships the Cyrillic font; without it the game font has no
                // Cyrillic glyphs, so Russian labels would render as squares.
                if (Directory.GetFiles(Paths.PluginPath, "fonts.bundle", SearchOption.AllDirectories).Length == 0)
                    return;

                _exact = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(phrases));

                var rawRegex = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(regexFile));
                _regex = new List<KeyValuePair<Regex, string>>();
                if (rawRegex != null)
                {
                    foreach (var kv in rawRegex)
                    {
                        try
                        {
                            _regex.Add(new KeyValuePair<Regex, string>(
                                new Regex(kv.Key, RegexOptions.CultureInvariant), kv.Value));
                        }
                        catch
                        {
                            // Ignore patterns the game engine version cannot compile.
                        }
                    }
                }

                Russian = true;
                Plugin.Log.LogInfo("Russian translation detected: " + _exact.Count + " phrases, " + _regex.Count + " patterns.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Translation dictionary unavailable: " + e.Message);
                Russian = false;
            }
        }

        public static string T(string english, string russian)
        {
            return Russian ? russian : english;
        }

        /// <summary>Translates a game-provided string (item / creep name) through the mod dictionary.</summary>
        public static string Game(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text == null ? "" : text;

            if (!Russian)
                return text;

            string cached;
            if (_cache.TryGetValue(text, out cached))
                return cached;

            string result = text;

            if (_exact != null)
            {
                string exact;
                if (_exact.TryGetValue(text, out exact) && !string.IsNullOrEmpty(exact))
                    result = exact;
                else if (_regex != null)
                {
                    for (int i = 0; i < _regex.Count; i++)
                    {
                        var pair = _regex[i];
                        if (pair.Key.IsMatch(text))
                        {
                            try
                            {
                                result = pair.Key.Replace(text, pair.Value);
                            }
                            catch
                            {
                                result = text;
                            }
                            break;
                        }
                    }
                }
            }

            _cache[text] = result;
            return result;
        }

        public static void ClearCache()
        {
            _cache.Clear();
        }
    }
}
