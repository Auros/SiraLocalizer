using Polyglot;
using System.Linq;
using UnityEngine;
using SiraUtil.Interfaces;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Globalization;
using System.IO;
using SiraLocalizer.Crowdin;

namespace SiraLocalizer
{
    internal class Localizer : ILocalizer
    {
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
            IEnumerable<Locale> languages = GetSupportedLanguages();

            if (_config.showIncompleteTranslations)
            {
                languages = languages.Union(GetLanguagesInSheets(_lockedAssetCache.Values.Where(x => x.shadowLocalization == false).Select(x => x.asset)));
            }

            Localization.Instance.SupportedLanguages.Clear();
            Localization.Instance.SupportedLanguages.AddRange(languages.OrderBy(lang => lang).Select(lang => (Language)lang));

            if (_config.language < 0)
            {
                Locale potential = AutoDetectLanguage();

                if (Localization.Instance.SupportedLanguages.Contains((Language)potential))
                {
                    _config.language = potential;
                    Localization.Instance.SelectLanguage((Language)_config.language);
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

        private List<Locale> GetSupportedLanguages()
        {
            List<Locale> languages = new List<Locale>();
            Stream fileContent = null;

            if (File.Exists(CrowdinDownloader.kLanguagesFilePath))
            {
                try
                {
                    fileContent = File.OpenRead(CrowdinDownloader.kLanguagesFilePath);
                }
                catch (IOException ex)
                {
                    Plugin.Log.Error("Failed to load languages from file; falling back to built-in languages");
                    Plugin.Log.Error(ex.ToString());
                }
            }
            else
            {
                fileContent = Assembly.GetExecutingAssembly().GetManifestResourceStream("SiraLocalizer.Resources.languages.txt");
            }

            using (var reader = new StreamReader(fileContent))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (Enum.TryParse(line, out Locale locale))
                    {
                        languages.Add(locale);
                    }
                }
            }

            return languages;
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
            
            foreach (int lang in Enum.GetValues(typeof(Locale)))
            {
                foreach (List<string> localizations in localizationsTable.Values)
                {
                    if (!string.IsNullOrWhiteSpace(localizations.ElementAtOrDefault(lang)))
                    {
                        presentLanguages.Add((Locale)lang);
                        break;
                    }
                }
            }

            return presentLanguages;
        }

        private Locale AutoDetectLanguage()
        {
            string name = CultureInfo.CurrentUICulture.Name;

            if (string.IsNullOrEmpty(name))
            {
                return Locale.English;
            }

            string[] parts = name.Split('-');
            string iso639 = parts[0];
            string bcp47 = parts.Length >= 2 ? parts[1] : string.Empty;

            Plugin.Log.Info($"User language: '{name}' (ISO 639-1 code: '{iso639}', BCP-47 code: '{bcp47}')");

            switch (iso639)
            {
                case "fr":
                    return Locale.French;

                case "es":
                    return Locale.Spanish;

                case "de":
                    return Locale.German;
                    
                case "it":
                    return Locale.Italian;
                    
                case "pt":
                    return bcp47 == "BR" ? Locale.Portuguese_Brazil : Locale.Portuguese;
                    
                case "ru":
                    return Locale.Russian;
                    
                case "el":
                    return Locale.Greek;
                    
                case "tr":
                    return Locale.Turkish;
                    
                case "da":
                    return Locale.Danish;
                    
                case "nb":
                    return Locale.Norwegian;
                    
                case "sv":
                    return Locale.Swedish;
                    
                case "nl":
                    return Locale.Dutch;
                    
                case "pl":
                    return Locale.Polish;
                    
                case "fi":
                    return Locale.Finnish;
                    
                case "ja":
                    return Locale.Japanese;
                    
                case "zh":
                    if (bcp47 == "Hant" || bcp47 == "HK" || bcp47 == "MO" || bcp47 == "TW")
                    {
                        return Locale.Traditional_Chinese;
                    }
                    else
                    {
                        return Locale.Simplified_Chinese;
                    }

                case "ko":
                    return Locale.Korean;
                    
                case "cs":
                    return Locale.Czech;
                    
                case "hu":
                    return Locale.Hungarian;
                    
                case "ro":
                    return Locale.Romanian;
                    
                case "th":
                    return Locale.Thai;
                    
                case "bg":
                    return Locale.Bulgarian;
                    
                case "he":
                    return Locale.Hebrew;

                case "ar":
                    return Locale.Arabic;

                case "bs":
                    return Locale.Bosnian;

                default:
                    return Locale.English;
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