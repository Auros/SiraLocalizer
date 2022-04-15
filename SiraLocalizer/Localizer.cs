using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Polyglot;
using SiraLocalizer.HarmonyPatches;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace SiraLocalizer
{
    internal class Localizer : ILocalizer, IInitializable, IDisposable
    {
        internal const float kMinimumTranslatedPercent = 0.50f;

        // Unicode white space characters + line breaks https://www.fileformat.info/info/unicode/category/Zs/list.htm
        private static readonly char[] kWhiteSpaceCharacters = new[] { ' ', '\n', '\r', '\t', '\x00A0', '\x1680', '\x2000', '\x2001', '\x2002', '\x2003', '\x2004', '\x2005', '\x2006', '\x2007', '\x2008', '\x2009', '\x200A', '\x202F', '\x205F', '\x3000' };
        private static readonly FieldInfo kLanguageStringsField = typeof(LocalizationImporter).GetField("languageStrings", BindingFlags.NonPublic | BindingFlags.Static);

        private readonly SiraLog _logger;

        private readonly List<LocalizationAssetWithPriority> _assets = new List<LocalizationAssetWithPriority>();

        public Localizer(SiraLog logger)
        {
            _logger = logger;
        }

        public async void Initialize()
        {
            LocalizationImporter_Initialize.preInitialize += LocalizationImporter_PreInitialize;
            LocalizationImporter_Initialize.postInitialize += LocalizationImporter_PostInitialize;

            await Task.WhenAll(
                AddLocalizationAssetFromAssemblyAsync("SiraLocalizer.Resources.sira-localizer.csv", GoogleDriveDownloadFormat.CSV),
                AddLocalizationAssetFromAssemblyAsync("SiraLocalizer.Resources.contributors.csv", GoogleDriveDownloadFormat.CSV));
        }

        public void Dispose()
        {
            LocalizationImporter_Initialize.preInitialize -= LocalizationImporter_PreInitialize;
            LocalizationImporter_Initialize.postInitialize -= LocalizationImporter_PostInitialize;

            Localization.Instance.InputFiles.RemoveAll(f => _assets.Any(l => l.localizationAsset == f));
            LocalizationImporter.Refresh();
        }

        public LocalizationAsset AddLocalizationAsset(LocalizationAsset localizationAsset)
        {
            Localization.Instance.InputFiles.Add(localizationAsset);
            LocalizationImporter.Refresh();
            return localizationAsset;
        }

        public LocalizationAsset AddLocalizationAsset(string content, GoogleDriveDownloadFormat format)
        {
            return AddLocalizationAsset(new LocalizationAsset() { Format = GoogleDriveDownloadFormat.CSV, TextAsset = new TextAsset(content) });
        }

        public LocalizationAsset AddLocalizationAssetFromAssembly(string resourceName, GoogleDriveDownloadFormat format)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)))
            {
                string content = reader.ReadToEnd();
                return AddLocalizationAsset(content, format);
            }
        }

        public void RemoveLocalizationAsset(LocalizationAsset localizationAsset)
        {
            Localization.Instance.InputFiles.RemoveAll(f => f == localizationAsset);
        }

        public async Task<LocalizationAsset> AddLocalizationAssetFromAssemblyAsync(string resourceName, GoogleDriveDownloadFormat format)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)))
            {
                string content = await reader.ReadToEndAsync();
                return AddLocalizationAsset(content, format);
            }
        }

        internal void RegisterTranslation(string content, GoogleDriveDownloadFormat format, int priority)
        {
            RegisterTranslation(new LocalizationAsset { TextAsset = new TextAsset(content), Format = format }, priority);
        }

        internal void RegisterTranslation(LocalizationAsset localizationAsset, int priority = 0)
        {
            if (localizationAsset == null) throw new InvalidOperationException();

            _assets.Add(new LocalizationAssetWithPriority(localizationAsset, priority));
        }

        internal void DeregisterTranslation(LocalizationAsset localizationAsset)
        {
            _assets.RemoveAll(a => a.localizationAsset == localizationAsset);
            Localization.Instance.InputFiles.RemoveAll(f => f == localizationAsset);
        }

        internal List<TranslationStatus> GetTranslationStatuses(Locale language)
        {
            var languageStrings = (Dictionary<string, List<string>>)kLanguageStringsField.GetValue(null);
            var statuses = new List<TranslationStatus>();

            foreach (LocalizationDefinition def in LocalizationDefinition.loadedDefinitions)
            {
                int total = 0;
                int translated = 0;

                foreach (string key in def.keys)
                {
                    if (!languageStrings.ContainsKey(key))
                    {
                        _logger.Warn($"Key '{key}' does not exist");
                        continue;
                    }

                    List<string> strings = languageStrings[key];

                    if (strings.Count == 0) continue;

                    string english = strings[(int)Language.English];
                    int words = english.Split(kWhiteSpaceCharacters, StringSplitOptions.RemoveEmptyEntries).Length;
                    total += words;

                    if (strings.Count >= (int)language - 1 && !string.IsNullOrWhiteSpace(strings[(int)language]))
                    {
                        translated += words;
                    }
                }

                statuses.Add(new TranslationStatus(def.name, total, translated));
            }

            return statuses;
        }

        private void LocalizationImporter_PreInitialize()
        {
            // make sure localizations are always loaded after whatever already existed in InputFiles
            Localization.Instance.InputFiles.RemoveAll(f => _assets.Any(l => l.localizationAsset == f));
            Localization.Instance.InputFiles.AddRange(_assets.OrderBy(l => l.priority).Select(l => l.localizationAsset));
        }

        private void LocalizationImporter_PostInitialize()
        {
            UpdateSupportedLanguages();
        }

        private void UpdateSupportedLanguages()
        {
            IEnumerable<Locale> languages = GetSupportedLanguages();

            Localization.Instance.SupportedLanguages.Clear();
            Localization.Instance.SupportedLanguages.AddRange(languages.Select(lang => (Language)lang));
        }

        private List<Locale> GetSupportedLanguages()
        {
            var languageStrings = (Dictionary<string, List<string>>)kLanguageStringsField.GetValue(null);
            var presentLanguages = new List<Locale>();
            List<string> languageNames = languageStrings["LANGUAGE_THIS"];

            foreach (int lang in Enum.GetValues(typeof(Locale)))
            {
                if (string.IsNullOrWhiteSpace(languageNames.ElementAtOrDefault(lang))) continue;

                int count = 0;

                foreach (List<string> localizations in languageStrings.Values)
                {
                    if (!string.IsNullOrWhiteSpace(localizations.ElementAtOrDefault(lang)))
                    {
                        count++;
                    }
                }

                float percentTranslated = (float)count / languageStrings.Count;

                if (percentTranslated > kMinimumTranslatedPercent)
                {
                    presentLanguages.Add((Locale)lang);
                }
            }

            return presentLanguages;
        }

        public readonly struct TranslationStatus
        {
            public string name { get; }
            public int totalStrings { get; }
            public int translatedStrings { get; }
            public float percentTranslated { get; }

            public TranslationStatus(string name, int totalStrings, int translatedStrings)
            {
                this.name = name;
                this.totalStrings = totalStrings;
                this.translatedStrings = translatedStrings;
                this.percentTranslated = totalStrings > 0 ? 100f * translatedStrings / totalStrings : 0;
            }
        }

        private readonly struct LocalizationAssetWithPriority
        {
            public LocalizationAsset localizationAsset { get; }
            public int priority { get; }

            public LocalizationAssetWithPriority(LocalizationAsset localizationAsset, int priority)
            {
                this.localizationAsset = localizationAsset;
                this.priority = priority;
            }
        }
    }
}
