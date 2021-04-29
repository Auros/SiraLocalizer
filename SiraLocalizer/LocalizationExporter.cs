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
        // languages supported by the base game
        private static readonly Language[] kSupportedLanguages = new[] { Language.English, Language.French, Language.Spanish, Language.German, Language.Japanese, Language.Korean };

        // keys added by SiraLocalizer
        private static readonly (string, string)[] kAdditionalKeys = new (string, string)[]
        {
            ("MENU_TRANSLATED_BY", "Translated by"),
            ("FLYING_TEXT_MISS", "Miss")
        };

        public static void DumpBaseGameLocalization()
        {
            string filePath = Path.Combine(UnityGame.InstallPath, "localization.csv");
            int numberOfLanguages = Enum.GetNames(typeof(Locale)).Length - 2; // don't include Locale.None and Locale.English
            string commas = new string(',', numberOfLanguages);

            Plugin.Log.Info($"Dumping base game localization to '{filePath}'");

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    LocalizationAsset baseGameAsset = Localization.Instance.InputFiles.First();
                    List<List<string>> rows = CsvReader.Parse(baseGameAsset.TextAsset.text);

                    writer.WriteLine("Polyglot,100," + commas);

                    string[] languages = new string[28]; // there are 28 actual languages in the Polyglot.Language enum

                    foreach (List<string> row in rows.SkipWhile(r => r[0] != "Polyglot").Skip(1))
                    {
                        string key     = row.ElementAtOrDefault(0);
                        string context = row.ElementAtOrDefault(1);

                        foreach (int supportedLanguage in kSupportedLanguages)
                        {
                            languages[supportedLanguage] = EscapeCsvValue(row.ElementAtOrDefault(supportedLanguage + 2));
                        }

                        writer.WriteLine($"{EscapeCsvValue(key)},{EscapeCsvValue(context)},{string.Join(",", languages)}" + commas);
                    }

                    foreach ((string key, string value) in kAdditionalKeys)
                    {
                        writer.WriteLine($"{EscapeCsvValue(key)},,{EscapeCsvValue(value)}" + commas);
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Could not dump base game localization");
                Plugin.Log.Error(ex.ToString());
            }
        }

        private static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n')) return value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
