using Polyglot;
using System.Linq;
using UnityEngine;
using SiraUtil.Interfaces;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Globalization;

namespace SiraLocalizer
{
    internal class Localizer : ILocalizer
    {
        private static readonly Language[] kSupportedLanguages = { Language.English, Language.French, Language.German, Language.Italian, Language.Portuguese_Brazil, Language.Russian, Language.Simplified_Chinese, Language.Korean };
        private static readonly Dictionary<string, LocalizationData> _lockedAssetCache = new Dictionary<string, LocalizationData>();

        private readonly Config _config;

        public Localizer(Config config)
        {
            _config = config;

            // Add ours FIRST
            AddLocalizationSheetFromAssembly("SiraLocalizer.Resources.sira-locale.csv", GoogleDriveDownloadFormat.CSV);
            AddLocalizationSheetFromAssembly("SiraLocalizer.Resources.contributors.csv", GoogleDriveDownloadFormat.CSV, true);
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
            IEnumerable<Language> languages = kSupportedLanguages;

            if (_config.showIncompleteTranslations)
            {
                languages = kSupportedLanguages.Union(GetLanguagesInSheets(_lockedAssetCache.Values.Where(x => x.shadowLocalization == false).Select(x => x.asset)));
            }

            Localization.Instance.SupportedLanguages.Clear();
            Localization.Instance.SupportedLanguages.AddRange(languages.OrderBy(lang => lang));

            if (_config.language < 0)
            {
                Language potential = AutoDetectLanguage();

                if (Localization.Instance.SupportedLanguages.Contains(potential))
                {
                    _config.language = potential;
                    Localization.Instance.SelectLanguage(_config.language);
                }
            }

            Localization.Instance.InvokeOnLocalize();
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

        private List<Language> GetLanguagesInSheets(IEnumerable<LocalizationAsset> assets)
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

            var presentLanguages = new List<Language>();
            
            foreach (int lang in Enum.GetValues(typeof(Language)))
            {
                foreach (List<string> localizations in localizationsTable.Values)
                {
                    if (!string.IsNullOrWhiteSpace(localizations.ElementAtOrDefault(lang)))
                    {
                        presentLanguages.Add((Language)lang);
                        break;
                    }
                }
            }

            return presentLanguages;
        }

        private Language AutoDetectLanguage()
        {
            string name = CultureInfo.CurrentUICulture.Name;

            if (string.IsNullOrEmpty(name))
            {
                return Language.English;
            }

            string[] parts = name.Split('-');
            string iso639 = parts[0];
            string bcp47 = parts.Length >= 2 ? parts[1] : string.Empty;

            Plugin.Log.Info($"User language: '{name}' (ISO 639-1 code: '{iso639}', BCP-47 code: '{bcp47}')");

            switch (iso639)
            {
                case "fr":
                    return Language.French;

                case "es":
                    return Language.Spanish;

                case "de":
                    return Language.German;
                    
                case "it":
                    return Language.Italian;
                    
                case "pt":
                    return bcp47 == "BR" ? Language.Portuguese_Brazil : Language.Portuguese;
                    
                case "ru":
                    return Language.Russian;
                    
                case "el":
                    return Language.Greek;
                    
                case "tr":
                    return Language.Turkish;
                    
                case "da":
                    return Language.Danish;
                    
                case "nb":
                    return Language.Norwegian;
                    
                case "sv":
                    return Language.Swedish;
                    
                case "nl":
                    return Language.Dutch;
                    
                case "pl":
                    return Language.Polish;
                    
                case "fi":
                    return Language.Finnish;
                    
                case "ja":
                    return Language.Japanese;
                    
                case "zh":
                    if (bcp47 == "Hant" || bcp47 == "HK" || bcp47 == "MO" || bcp47 == "TW")
                    {
                        return Language.Traditional_Chinese;
                    }
                    else
                    {
                        return Language.Simplified_Chinese;
                    }

                case "ko":
                    return Language.Korean;
                    
                case "cs":
                    return Language.Czech;
                    
                case "hu":
                    return Language.Hungarian;
                    
                case "ro":
                    return Language.Romanian;
                    
                case "th":
                    return Language.Thai;
                    
                case "bg":
                    return Language.Bulgarian;
                    
                case "he":
                    return Language.Hebrew;

                case "ar":
                    return Language.Arabic;

                case "bs":
                    return Language.Bosnian;

                default:
                    return Language.English;
            }
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