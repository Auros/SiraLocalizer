using Polyglot;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace SiraLocalizer
{
    internal class Localizer
    {
        private const float kMinimumTranslatedPercent = 0.90f;

        private static readonly Dictionary<LocalizationAsset, LocalizationData> _localizationAssets = new Dictionary<LocalizationAsset, LocalizationData>();

        public Localizer()
        {
            // Add ours FIRST
            AddLocalizationSheetFromAssembly("SiraLocalizer.Resources.sira-locale.csv", GoogleDriveDownloadFormat.CSV, true);
            AddLocalizationSheetFromAssembly("SiraLocalizer.Resources.contributors.csv", GoogleDriveDownloadFormat.CSV, false);
        }

        public LocalizationAsset AddLocalizationSheet(LocalizationAsset localizationAsset)
        {
            return AddLocalizationSheet(localizationAsset, false);
        }

        public LocalizationAsset AddLocalizationSheet(string content, GoogleDriveDownloadFormat type)
        {
            return AddLocalizationSheet(content, type, false);
        }

        public LocalizationAsset AddLocalizationSheetFromAssembly(string assemblyPath, GoogleDriveDownloadFormat type)
        {
            return AddLocalizationSheetFromAssembly(assemblyPath, type, false);
        }

        public void UpdateSupportedLanguages()
        {
            IEnumerable<Locale> languages = GetLanguagesInSheets(_localizationAssets.Where(x => x.Value.builtin).Select(x => x.Key));

            Localization.Instance.SupportedLanguages.Clear();
            Localization.Instance.SupportedLanguages.AddRange(languages.Select(lang => (Language)lang));

            Localization.Instance.InvokeOnLocalize();
        }

        public void RemoveLocalizationSheet(LocalizationAsset localizationAsset)
        {
            _localizationAssets.Remove(localizationAsset);

            Localization.Instance.InputFiles.RemoveAll(la => la == localizationAsset);
            LocalizationImporter.Refresh();

            UpdateSupportedLanguages();
        }

        internal LocalizationAsset AddLocalizationSheet(LocalizationAsset localizationAsset, bool builtin)
        {
            _localizationAssets.Add(localizationAsset, new LocalizationData(builtin));

            Localization.Instance.InputFiles.Add(localizationAsset);
            LocalizationImporter.Refresh();

            UpdateSupportedLanguages();

            return localizationAsset;
        }

        internal LocalizationAsset AddLocalizationSheet(string content, GoogleDriveDownloadFormat type, bool builtin)
        {
            var asset = new LocalizationAsset
            {
                Format = type,
                TextAsset = new TextAsset(content)
            };

            return AddLocalizationSheet(asset, builtin);
        }

        internal LocalizationAsset AddLocalizationSheetFromAssembly(string assemblyPath, GoogleDriveDownloadFormat type, bool builtin)
        {
            SiraUtil.Utilities.AssemblyFromPath(assemblyPath, out Assembly assembly, out string path);
            string content = SiraUtil.Utilities.GetResourceContent(assembly, path);
            return AddLocalizationSheet(content, type, builtin);
        }

        private List<Locale> GetLanguagesInSheets(IEnumerable<LocalizationAsset> assets)
        {
            var localizationsTable = new Dictionary<string, List<string>>();

            foreach (LocalizationAsset asset in assets)
            {
                List<List<string>> lines;
                string text = asset.TextAsset.text.Replace("\r\n", "\n");

                if (asset.Format == GoogleDriveDownloadFormat.CSV)
                {
                    lines = CsvReader.Parse(text);
                }
                else
                {
                    lines = TsvReader.Parse(text);
                }

                foreach (List<string> line in lines.SkipWhile(l => l[0] != "Polyglot").Skip(1))
                {
                    string keyName = line[0];

                    if (!string.IsNullOrWhiteSpace(keyName) && line.Count > 1)
                    {
                        List<string> localizations = line.Skip(2).ToList();

                        if (localizationsTable.ContainsKey(keyName))
                        {
                            localizationsTable[keyName] = localizations;
                        }
                        else
                        {
                            localizationsTable.Add(keyName, localizations);
                        }
                    }
                }
            }

            var presentLanguages = new List<Locale>();
            List<string> languageNames = localizationsTable["LANGUAGE_THIS"];
            
            foreach (int lang in Enum.GetValues(typeof(Locale)))
            {
                if (string.IsNullOrWhiteSpace(languageNames.ElementAtOrDefault(lang))) continue;

                int count = 0;

                foreach (List<string> localizations in localizationsTable.Values)
                {
                    if (!string.IsNullOrWhiteSpace(localizations.ElementAtOrDefault(lang)))
                    {
                        count++;
                    }
                }

                float percentTranslated = (float)count / localizationsTable.Count;

                if (percentTranslated > kMinimumTranslatedPercent)
                {
                    presentLanguages.Add((Locale)lang);
                }
            }

            return presentLanguages;
        }

        private struct LocalizationData
        {
            public bool builtin;

            public LocalizationData(bool builtin)
            {
                this.builtin = builtin;
            }
        }
    }
}