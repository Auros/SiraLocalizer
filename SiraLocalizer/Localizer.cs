using Polyglot;
using System.Linq;
using UnityEngine;
using SiraUtil.Interfaces;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace SiraLocalizer
{
    internal class Localizer : ILocalizer
    {
        private static readonly Dictionary<string, LocalizationData> _lockedAssetCache = new Dictionary<string, LocalizationData>();

        public Localizer()
        {
            // Add ours FIRST
            AddLocalizationSheetFromAssembly("SiraLocalizer.Resources.sira-locale.csv", GoogleDriveDownloadFormat.CSV);
        }

        public LocalizationAsset AddLocalizationSheet(string localizationAsset, GoogleDriveDownloadFormat type, string id, bool shadow = false)
        {
            var asset = new LocalizationAsset
            {
                Format = type,
                TextAsset = new TextAsset(localizationAsset)
            };
            if (!_lockedAssetCache.ContainsKey(id))
            {
                _lockedAssetCache.Add(id, new LocalizationData(asset, shadow));
            }
            AddLocalizationSheet(asset);
            return asset;
        }

        public void AddLocalizationSheet(LocalizationAsset localizationAsset, bool shadow = false)
        {
            var loc = _lockedAssetCache.Where(x => x.Value.asset == localizationAsset || x.Value.asset.TextAsset.text == localizationAsset.TextAsset.text).FirstOrDefault();
            if (loc.Equals(default(KeyValuePair<string, LocalizationAsset>)))
            {
                return;
            }
            Localization.Instance.InputFiles.Add(localizationAsset);
            LocalizationImporter.Refresh();
            RecalculateLanguages();
        }

        public void RecalculateLanguages()
        {
            var supported = new HashSet<Language>();
            var nonShadowLanguages = GetLanguagesInSheets(_lockedAssetCache.Values.Where(x => x.shadowLocalization == false).Select(x => x.asset).ToArray());

            // We know English will always be present.
            supported.Add(Language.English);
            for (int i = 0; i < _lockedAssetCache.Count; i++)
            {
                var locData = _lockedAssetCache.Values.ElementAt(i);
                var langs = GetLanguagesInSheets(locData.asset);
                if (locData.shadowLocalization)
                {
                    if (!LanguageMatch(nonShadowLanguages, langs))
                    {
                        continue;
                    }
                }
                foreach (var lang in langs)
                {
                    supported.Add(lang);
                }
            }
            Localization.Instance.SupportedLanguages.Clear();
            Localization.Instance.SupportedLanguages.AddRange(supported);
            Localization.Instance.InvokeOnLocalize();
        }

        private bool LanguageMatch(List<Language> pairOne, List<Language> pairTwo)
        {
            for (int i = 0; i < pairOne.Count; i++)
            {
                for (int x = 0; x < pairTwo.Count; x++)
                {
                    if (pairOne[i] == pairTwo[x])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public LocalizationAsset AddLocalizationSheetFromAssembly(string assemblyPath, GoogleDriveDownloadFormat type, bool shadow = false)
        {
            SiraUtil.Utilities.AssemblyFromPath(assemblyPath, out Assembly assembly, out string path);
            string content = SiraUtil.Utilities.GetResourceContent(assembly, path);
            var locSheet = AddLocalizationSheet(content, type, path, shadow);
            if (!_lockedAssetCache.ContainsKey(path))
            {
                _lockedAssetCache.Add(path, new LocalizationData(locSheet, shadow));
            }
            return locSheet;
        }

        public void RemoveLocalizationSheet(LocalizationAsset localizationAsset)
        {
            var loc = _lockedAssetCache.Where(x => x.Value.asset == localizationAsset || x.Value.asset.TextAsset.text == localizationAsset.TextAsset.text).FirstOrDefault();
            if (!loc.Equals(default(KeyValuePair<string, LocalizationAsset>)))
            {
                _lockedAssetCache.Remove(loc.Key);
            }
            RecalculateLanguages();
        }

        public void RemoveLocalizationSheet(string key)
        {
            _lockedAssetCache.Remove(key);
            RecalculateLanguages();
        }

        internal List<Language> GetLanguagesInSheets(params LocalizationAsset[] assets)
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

            var supportedLanguages = new List<Language>();

            foreach (int lang in Enum.GetValues(typeof(Language)))
            {
                foreach (List<string> localizations in localizationsTable.Values)
                {
                    if (!string.IsNullOrWhiteSpace(localizations.ElementAtOrDefault(lang)))
                    {
                        supportedLanguages.Add((Language)lang);
                        break;
                    }
                }
            }

            return supportedLanguages;
        }

        private struct LocalizationData
        {
            public LocalizationAsset asset;
            public bool shadowLocalization;

            public LocalizationData(LocalizationAsset asset, bool shadow)
            {
                this.asset = asset;
                shadowLocalization = shadow;
            }
        }
    }
}