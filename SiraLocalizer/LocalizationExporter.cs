using IPA.Utilities;
using Polyglot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SiraLocalizer
{
    internal class LocalizationExporter
    {
        // keys that aren't actually used
        private static readonly string[] kLocalizationKeyIgnoreList = { "MP_MISSING_SONG_ENTITLEMENT", "LANGUAGE_EN", "LANGUAGE_SC" };

        // keys added by SiraLocalizer
        private static readonly (string, string)[] kAdditionalKeys = new[]
        {
            ("MENU_LANGUAGE_THIS", "English"),
            ("MENU_TRANSLATED_BY", "Translated by"),
            ("LEVEL_FAILED", " Level\nFailed"),
            ("FLYING_TEXT_MISS", "Miss"),
            ("LABEL_COMBO", "Combo"),
            ("OBJECTIVE_COMPARISON_MINIMUM", "Min"),
            ("OBJECTIVE_COMPARISON_MAXIMUM", "Max")
        };

        // because there's typos and weirdness
        private static readonly Dictionary<string, (string find, string replace)> kCorrections = new Dictionary<string, (string, string)>
        {
            { "MISSION_HELP_MIN_HANDS_MOVEMENT_TITLE", (".</color>", "</color>.") },
            { "MISSION_HELP_MAX_HANDS_MOVEMENT", (".</color>", "</color>.") },
            { "LABEL_MULTIPLAYER_MAINTENANCE_UPCOMING", ("maintatance", "maintenance") },
            { "HINT_OPTIONS_BUTTON", ("Settings", "Options") },
            { "SETTINGS_OCULUS_MRC_INFO", ("Mixed Reality Capture Setup Guide", "Getting Started With Mixed Reality Capture") },
            { "TEXT_INVALID_PASSWORD", ("You", "Your") }
        };

        public void DumpBaseGameLocalization()
        {
            string filePath = Path.Combine(UnityGame.InstallPath, "localization.csv");
            int numberOfLanguages = Enum.GetNames(typeof(Locale)).Length - 2; // don't include Locale.None and Locale.English
            string commas = new string(',', numberOfLanguages);

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    LocalizationAsset baseGameAsset = Localization.Instance.InputFiles.First();
                    List<List<string>> rows = CsvReader.Parse(baseGameAsset.TextAsset.text);

                    writer.WriteLine("Polyglot,100," + commas);

                    foreach (List<string> row in rows.SkipWhile(r => r[0] != "Polyglot").Skip(1))
                    {
                        if (string.IsNullOrEmpty(row[0]) || kLocalizationKeyIgnoreList.Contains(row[0])) continue;

                        string key     = row.ElementAtOrDefault(0);
                        string context = row.ElementAtOrDefault(1);
                        string english = row.ElementAtOrDefault(2)?.TrimEnd();

                        if (kCorrections.TryGetValue(key, out var rule))
                        {
                            if (!english.Contains(rule.find)) Plugin.Log.Warn($"Rule for '{key}' ('{rule.find}' -> '{rule.replace}') won't do anything on '{english}'");

                            english = english.Replace(rule.find, rule.replace);
                        }

                        writer.WriteLine($"{EscapeCsvValue(key)},{EscapeCsvValue(context)},{EscapeCsvValue(english)}" + commas);
                    }

                    foreach ((string key, string value) in kAdditionalKeys)
                    {
                        writer.WriteLine($"{EscapeCsvValue(key)},,{EscapeCsvValue(value)}" + commas);
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Could not dump base game localization: " + ex);
            }
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n')) return value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
