using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Polyglot;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace SiraLocalizer
{
    internal class LocalizationManager : IAffinity, IInitializable, IDisposable
    {
        internal const float kMinimumTranslatedPercent = 0.50f;

        // Unicode white space characters + line breaks https://www.fileformat.info/info/unicode/category/Zs/list.htm
        private static readonly char[] kWhiteSpaceCharacters = new[] { ' ', '\n', '\r', '\t', '\x00A0', '\x1680', '\x2000', '\x2001', '\x2002', '\x2003', '\x2004', '\x2005', '\x2006', '\x2007', '\x2008', '\x2009', '\x200A', '\x202F', '\x205F', '\x3000' };
        private static readonly FieldInfo kLanguageStringsField = typeof(LocalizationImporter).GetField("languageStrings", BindingFlags.NonPublic | BindingFlags.Static);

        private readonly SiraLog _logger;
        private readonly Settings _config;
        private readonly List<ILocalizationProvider> _localizationProviders;
        private readonly List<ILocalizationDownloader> _localizationDownloaders;

        private readonly List<LocalizationFile> _localizationFiles = new();

        public LocalizationManager(SiraLog logger, Settings config, List<ILocalizationProvider> localizationProviders, List<ILocalizationDownloader> localizationDownloaders)
        {
            _logger = logger;
            _config = config;
            _localizationProviders = localizationProviders;
            _localizationDownloaders = localizationDownloaders;
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

            if (list == null)
            {
                return;
            }

            await DownloadLocalizationsAsync(list, cancellationToken);
        }

        internal async Task<List<ILocalizationDownloader>> CheckForUpdatesAsync(CancellationToken cancellationToken)
        {
            _logger.Info("Checking for updates");

            List<ILocalizationDownloader> list = null;

            foreach (ILocalizationDownloader localizationDownloader in _localizationDownloaders)
            {
                if (await localizationDownloader.CheckForUpdatesAsync(cancellationToken))
                {
                    _logger.Info($"Updates available from {localizationDownloader.name}");

                    list ??= new();
                    list.Add(localizationDownloader);
                }
            }

            return list;
        }

        internal async Task DownloadLocalizationsAsync(List<ILocalizationDownloader> localizationDownloaders, CancellationToken cancellationToken)
        {
            foreach (ILocalizationDownloader localizationDownloader in localizationDownloaders)
            {
                _logger.Info($"Downloading updates from {localizationDownloader.name}");
                await localizationDownloader.DownloadLocalizationsAsync(cancellationToken);
            }
        }

        private async Task RegisterLocalizationsAsync(CancellationToken cancellationToken)
        {
            foreach (ILocalizationProvider localizationProvider in _localizationProviders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await foreach (LocalizationFile file in localizationProvider.GetLocalizationAssetsAsync(cancellationToken))
                {
                    _localizationFiles.Add(file);
                }
            }

            LocalizationImporter.Refresh();
        }

        private void DeregisterLocalizations()
        {
            RemoveLocalizationFilesFromPolyglot();

            _localizationFiles.Clear();

            LocalizationImporter.Refresh();
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

        [AffinityPatch(typeof(LocalizationImporter), "ImportFromFiles")]
        [AffinityPrefix]
        [UsedImplicitly]
        private void LocalizationImporter_PreInitialize()
        {
            _logger.Info("LocalizationImporter_PreInitialize");
            // make sure localizations are always loaded after whatever already existed in InputFiles
            RemoveLocalizationFilesFromPolyglot();
            AddLocalizationFilesToPolyglot();
        }

        [AffinityPatch(typeof(LocalizationImporter), "ImportFromFiles")]
        [AffinityPostfix]
        [UsedImplicitly]
        private void LocalizationImporter_PostInitialize()
        {
            _logger.Info("LocalizationImporter_PostInitialize");
            UpdateSupportedLanguages();
        }

        private void RemoveLocalizationFilesFromPolyglot()
        {
            Localization.Instance.InputFiles.RemoveAll(f => _localizationFiles.Any(l => l.localizationAsset == f));
        }

        private void AddLocalizationFilesToPolyglot()
        {
            Localization.Instance.InputFiles.AddRange(_localizationFiles.OrderBy(l => l.priority).Select(l => l.localizationAsset));
        }

        private void UpdateSupportedLanguages()
        {
            IEnumerable<Locale> languages = GetSupportedLanguages();

            Localization.Instance.SupportedLanguages.Clear();
            Localization.Instance.SupportedLanguages.AddRange(languages.Select(lang => (Language)lang));
        }

        private IEnumerable<Locale> GetSupportedLanguages()
        {
            var languageStrings = (Dictionary<string, List<string>>)kLanguageStringsField.GetValue(null);
            List<string> languageNames = languageStrings["LANGUAGE_THIS"];

            foreach (int lang in Enum.GetValues(typeof(Locale)))
            {
                if (string.IsNullOrWhiteSpace(languageNames.ElementAtOrDefault(lang))) continue;

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
