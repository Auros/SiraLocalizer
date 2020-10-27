using Polyglot;
using System.Linq;
using UnityEngine;
using IPA.Utilities;
using System.Reflection;
using SiraUtil.Interfaces;
using System.Collections.Generic;

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
            Localization.Instance.GetField<List<LocalizationAsset>, Localization>("inputFiles").Add(localizationAsset);
            LocalizationImporter.Refresh();
            RecalculateLanguages();
        }

        public void RecalculateLanguages()
        {
            FieldInfo field = typeof(LocalizationImporter).GetField("languageStrings", BindingFlags.NonPublic | BindingFlags.Static);
            var supported = new HashSet<Language>();
            var locTable = (Dictionary<string, List<string>>)field.GetValue(null);
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
            Localization.Instance.GetField<List<Language>, Localization>("supportedLanguages").Clear();
            Localization.Instance.GetField<List<Language>, Localization>("supportedLanguages").AddRange(supported);
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
            List<Language> supported = new List<Language>();
            supported.AddRange(Localization.Instance.GetField<List<Language>, Localization>("supportedLanguages"));
            var locTable = new Dictionary<string, List<string>>();
            for (int i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                List<List<string>> textData;
                if (asset.Format == GoogleDriveDownloadFormat.CSV)
                {
                    textData = CsvReader.Parse(asset.TextAsset.text.Replace("\r\n", "\n"));
                }
                else
                {
                    textData = TsvReader.Parse(asset.TextAsset.text.Replace("\r\n", "\n"));
                }
                bool isValid = false;
                for (int a = 0; a < textData.Count; a++)
                {
                    List<string> valList = textData[a];
                    string keyName = valList[0];
                    if (!string.IsNullOrEmpty(keyName) && !LocalizationImporter.IsLineBreak(keyName) && valList.Count > 1)
                    {
                        if (!isValid && keyName.StartsWith("Polyglot"))
                        {
                            isValid = true;
                        }
                        else if (isValid)
                        {
                            valList.RemoveAt(0);
                            valList.RemoveAt(0);
                            if (locTable.ContainsKey(keyName))
                            {
                                locTable[keyName] = valList;
                            }
                            else
                            {
                                locTable.Add(keyName, valList);
                            }
                        }
                    }
                }
            }
            ISet<int> validLanguages = new HashSet<int>();
            foreach (var value in locTable.Values)
            {
                for (int i = 0; i < value.Count; i++)
                {
                    if (!string.IsNullOrEmpty(value.ElementAtOrDefault(i)))
                    {
                        validLanguages.Add(i);
                    }
                }
            }
            supported.Clear();
            for (int i = 0; i < validLanguages.Count; i++)
            {
                supported.Add((Language)validLanguages.ElementAt(i));
            }
            return supported;
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