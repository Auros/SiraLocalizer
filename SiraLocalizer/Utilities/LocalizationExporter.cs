using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BGLib.Polyglot;
using IPA.Utilities;
using SiraUtil.Logging;
using Zenject;

namespace SiraLocalizer.Utilities
{
    internal class LocalizationExporter : IInitializable
    {
        // languages supported by the base game
        private static readonly LocalizationLanguage[] kSupportedLanguages = [LocalizationLanguage.French, LocalizationLanguage.Spanish, LocalizationLanguage.German, LocalizationLanguage.Japanese, LocalizationLanguage.Korean];

        // keys that aren't actually used
        private static readonly string[] kLocalizationKeyIgnoreList =
        {
            "PSVR_SAFE_AREA_CONFIRMATION_TEXT",
            "PSVR2_CONTROLLER_REQUEST",
        };

        // because there's typos and weirdness
        private static readonly Dictionary<string, (Regex find, string replace)> kCorrections = new()
        {
            { "MISSION_HELP_MIN_HANDS_MOVEMENT_TITLE", (new Regex(@"\.</color>"), "</color>.") },
            { "MISSION_HELP_MAX_HANDS_MOVEMENT", (new Regex(@"\.</color>"), "</color>.") },
            { "LABEL_MULTIPLAYER_MAINTENANCE_UPCOMING", (new Regex(@"maintatance"), "maintenance") },
            { "TEXT_INVALID_PASSWORD", (new Regex(@"You"), "Your") },
        };

        private readonly SiraLog _logger;
        private readonly LocalizationModel _localizationModel;

        internal LocalizationExporter(SiraLog logger, LocalizationModel localizationModel)
        {
            _logger = logger;
            _localizationModel = localizationModel;
        }

        public void Initialize()
        {
            DumpBaseGameLocalization();
        }

        private void DumpBaseGameLocalization()
        {
            string filePath = Path.Combine(UnityGame.InstallPath, "beat-saber.csv");
            int numberOfLanguages = Enum.GetNames(typeof(Locale)).Length - 1; // don't include Locale.English

            _logger.Info($"Dumping base game localization to '{filePath}'");

            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Polyglot,100," + new string(',', numberOfLanguages));

                    foreach (LocalizationAsset baseGameAsset in _localizationModel.inputFiles.Take(2))
                    {
                        _logger.Info($"Processing '{baseGameAsset.TextAsset.name}'");

                        List<List<string>> rows = CsvReader.Parse(baseGameAsset.TextAsset.text);

                        foreach (List<string> row in rows.SkipWhile(r => r[0] != "Polyglot").Skip(1))
                        {
                            string key = row.ElementAtOrDefault(0);

                            if (kLocalizationKeyIgnoreList.Contains(key))
                            {
                                continue;
                            }

                            string context = row.ElementAtOrDefault(1);
                            string english = row.ElementAtOrDefault(2);
                            string[] languages = new string[numberOfLanguages];

                            if (key.Equals(english, StringComparison.Ordinal))
                            {
                                continue;
                            }

                            foreach (int supportedLanguage in kSupportedLanguages)
                            {
                                languages[supportedLanguage - 1] = EscapeCsvValue(row.ElementAtOrDefault(supportedLanguage + 2));
                            }

                            if (kCorrections.TryGetValue(key, out var rule))
                            {
                                string result = rule.find.Replace(english, rule.replace);

                                if (result == english)
                                {
                                    _logger.Warn($"Rule for '{key}' ('{rule.find}' -> '{rule.replace}') did nothing on '{english}'");
                                }
                                else
                                {
                                    english = result;
                                }
                            }

                            writer.WriteLine($"{EscapeCsvValue(key)},{EscapeCsvValue(context)},{EscapeCsvValue(english)},{string.Join(",", languages)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Could not dump base game localization");
                _logger.Error(ex.ToString());
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
