using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BGLib.Polyglot;
using JetBrains.Annotations;
using SiraLocalizer.Providers;
using SiraLocalizer.Records;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace SiraLocalizer
{
    internal class LocalizationManager : IAffinity, IInitializable, IDisposable
    {
        internal const float kMinimumTranslatedPercent = 0.50f;

        // Unicode white space characters + line breaks https://www.fileformat.info/info/unicode/category/Zs/list.htm
        private static readonly char[] kWhiteSpaceCharacters = [' ', '\n', '\r', '\t', '\x00A0', '\x1680', '\x2000', '\x2001', '\x2002', '\x2003', '\x2004', '\x2005', '\x2006', '\x2007', '\x2008', '\x2009', '\x200A', '\x202F', '\x205F', '\x3000'];

        private readonly SiraLog _logger;
        private readonly Settings _config;
        private readonly List<ILocalizationProvider> _localizationProviders;
        private readonly List<ILocalizationDownloader> _localizationDownloaders;
        private readonly SettingsManager _settingsManager;

        private readonly List<LocalizationFile> _localizationFiles = new();

        public LocalizationManager(SiraLog logger, Settings config, List<ILocalizationProvider> localizationProviders, List<ILocalizationDownloader> localizationDownloaders, SettingsManager settingsManager)
        {
            _logger = logger;
            _config = config;
            _localizationProviders = localizationProviders;
            _localizationDownloaders = localizationDownloaders;
            _settingsManager = settingsManager;
        }

        public async void Initialize()
        {
            try
            {
                if (_config.automaticallyDownloadLocalizations)
                {
                    await CheckForUpdatesAndDownloadIfAvailable(CancellationToken.None);
                }

                await RegisterLocalizationsAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void Dispose()
        {
            DeregisterLocalizations();
        }

        internal async Task CheckForUpdatesAndDownloadIfAvailable(CancellationToken cancellationToken)
        {
            List<ILocalizationDownloader> list = await CheckForUpdatesAsync(cancellationToken);

            if (list.Count == 0)
            {
                return;
            }

            await DownloadLocalizationsAsync(list, cancellationToken);
        }

        internal async Task<List<ILocalizationDownloader>> CheckForUpdatesAsync(CancellationToken cancellationToken)
        {
            _logger.Info("Checking for updates");

            List<ILocalizationDownloader> list = new();

            foreach (ILocalizationDownloader localizationDownloader in _localizationDownloaders)
            {
                try
                {

                    if (await localizationDownloader.CheckForUpdatesAsync(cancellationToken))
                    {
                        _logger.Info($"Updates available from {localizationDownloader.name}");

                        list.Add(localizationDownloader);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error occured while checking for updates for {localizationDownloader.name} ({localizationDownloader.GetType().FullName})\n{ex}");
                }
            }

            return list;
        }

        internal async Task DownloadLocalizationsAsync(List<ILocalizationDownloader> localizationDownloaders, CancellationToken cancellationToken)
        {
            foreach (ILocalizationDownloader localizationDownloader in localizationDownloaders)
            {
                _logger.Info($"Downloading updates from {localizationDownloader.name}");

                try
                {
                    await localizationDownloader.DownloadLocalizationsAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error occured while downloading for updates for {localizationDownloader.name} ({localizationDownloader.GetType().FullName})\n{ex}");
                }
            }
        }

        internal async Task ReloadLocalizations(CancellationToken cancellationToken)
        {
            DeregisterLocalizations();
            await RegisterLocalizationsAsync(cancellationToken);
        }

        private async Task RegisterLocalizationsAsync(CancellationToken cancellationToken)
        {
            foreach (ILocalizationProvider localizationProvider in _localizationProviders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await foreach (LocalizationFile file in localizationProvider.GetLocalizationAssetsAsync(cancellationToken))
                    {
                        _localizationFiles.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error occured while adding localizations from {localizationProvider.GetType().FullName}\n{ex}");
                }
            }

            LocalizationImporter.ImportFromFiles(Localization.Instance.inputFiles);
        }

        private void DeregisterLocalizations()
        {
            _localizationFiles.Clear();
        }

        internal List<TranslationStatus> GetTranslationStatuses(Locale language)
        {
            var languageStrings = Localization.Instance._languageStrings;
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

                    if (strings.Count == 0)
                    {
                        continue;
                    }

                    string english = strings[(int)LocalizationLanguage.English];

                    if (key.Equals(english, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    int words = english.Split(kWhiteSpaceCharacters, StringSplitOptions.RemoveEmptyEntries).Length;
                    total += words;

                    if (!string.IsNullOrWhiteSpace(strings.ElementAtOrDefault((int)language)))
                    {
                        translated += words;
                    }
                }

                statuses.Add(new TranslationStatus(def.name, total, translated));
            }

            return statuses;
        }

        [AffinityPatch(typeof(LocalizationImporter), nameof(LocalizationImporter.ImportFromFiles))]
        [AffinityPostfix]
        [UsedImplicitly]
        private void LocalizationImporter_PostImportFromFiles()
        {
            // prevent exceptions on our end from breaking Polyglot's load process
            try
            {
                AddLocalizationFilesToPolyglot();
                UpdateSupportedLanguages();
            }
            catch (Exception ex)
            {
                _logger.Critical(ex);
            }
        }

        private void AddLocalizationFilesToPolyglot()
        {
            foreach (LocalizationFile localizationFile in _localizationFiles.OrderBy(l => l.priority))
            {
                ImportTextFile(localizationFile.content);
            }
        }

        /// <summary>
        /// Similar to <see cref="LocalizationImporter.ImportTextFile"/> but doesn't touch English strings if they already exist.
        /// </summary>
        /// <param name="text">The localization file in CSV format.</param>
        private void ImportTextFile(string text)
        {
            text = text.Replace("\r\n", "\n");
            List<List<string>> list = CsvReader.Parse(text);
            var languageStrings = Localization.Instance._languageStrings;

            foreach (List<string> row in list.SkipWhile(r => r[0] != "Polyglot").Skip(1))
            {
                string key = row[0];

                if (string.IsNullOrEmpty(key) || LocalizationImporter.IsLineBreak(key) || row.Count <= 1)
                {
                    continue;
                }

                string longestString = string.Empty;

                foreach (string str in row.Skip(2))
                {
                    if (longestString.Length < str.Length)
                    {
                        longestString = str;
                    }
                }

                char[] chars = row[2].ToCharArray();
                Array.Reverse(chars);
                string reversed = new(chars);

                row.Add(row[0]);
                row.Add(reversed);
                row.Add(longestString);

                // remove key and context
                row.RemoveAt(0);
                row.RemoveAt(0);

                if (languageStrings.TryGetValue(key, out List<string> existingValues))
                {
                    // keep English, overwrite everything else
                    row[0] = existingValues[0];
                }

                languageStrings[key] = row;
            }
        }

        private void UpdateSupportedLanguages()
        {
            if (Localization._instance == null)
            {
                return;
            }

            IEnumerable<Locale> languages = GetSupportedLanguages();

            List<LocalizationLanguage> supportedLanguages = Localization.Instance._localization.supportedLanguages;
            supportedLanguages.Clear();
            supportedLanguages.AddRange(languages.Cast<LocalizationLanguage>());

            Localization.Instance.SelectedLanguage = _settingsManager.settings.misc.language.ToLocalizationLanguage();
        }

        private IEnumerable<Locale> GetSupportedLanguages()
        {
            var languageStrings = Localization.Instance._languageStrings;

            if (!languageStrings.TryGetValue("LANGUAGE_THIS", out List<string> languageNames))
            {
                yield break;
            }

            foreach (int lang in Enum.GetValues(typeof(Locale)))
            {
                if (string.IsNullOrWhiteSpace(languageNames.ElementAtOrDefault(lang))) continue;
                if ((Locale)lang is Locale.DebugKeys or Locale.DebugEnglishReverted or Locale.DebugEntryWithMaxLength) continue;

                int count = 0;

                foreach (List<string> localizations in languageStrings.Values)
                {
                    if (!string.IsNullOrWhiteSpace(localizations.ElementAtOrDefault(lang)))
                    {
                        ++count;
                    }
                }

                float percentTranslated = (float)count / languageStrings.Count;

                if (percentTranslated > kMinimumTranslatedPercent)
                {
                    yield return (Locale)lang;
                }
            }
        }
    }
}
