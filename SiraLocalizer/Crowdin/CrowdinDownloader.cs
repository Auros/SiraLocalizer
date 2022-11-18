using IPA.Utilities;
using Newtonsoft.Json;
using Polyglot;
using SiraLocalizer.Utilities;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace SiraLocalizer.Crowdin
{
    public class CrowdinDownloader : IInitializable, IDisposable
    {
        private const string kCrowdinHost = "https://distributions.crowdin.net";
        private const string kDistributionKey = "b8d0ace786d64ba14775878o9lk";

        private static readonly string kDataFolder = Path.Combine(UnityGame.UserDataPath, "SiraLocalizer");
        private static readonly string kLocalizationsFolder = Path.Combine(kDataFolder, "Localizations", "Downloaded");
        private static readonly string kDownloadedFolder = Path.Combine(kLocalizationsFolder, "Content");
        private static readonly string kManifestFilePath = Path.Combine(kLocalizationsFolder, "manifest.json");

        private readonly SiraLog _logger;
        private readonly LocalizationManager _localizationManager;
        private readonly Config _config;

        private readonly List<LocalizationAsset> _loadedAssets = new();

        internal CrowdinDownloader(SiraLog logger, LocalizationManager localizationManager, Config config)
        {
            _logger = logger;
            _localizationManager = localizationManager;
            _config = config;
        }

        public async void Initialize()
        {
            try
            {
                if (_config.automaticallyDownloadLocalizations)
                {
                    await DownloadLocalizationsAsync(CancellationToken.None);
                }
                else
                {
                    await LoadLocalizationSheetsAsync(CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load Crowdin translations");
                _logger.Error(ex);
            }
        }

        public void Dispose()
        {
            ClearLoadedAssets();
        }

        public async Task LoadLocalizationSheetsAsync(CancellationToken cancellationToken)
        {
            string manifestContent = await GetManifestContentAsync();

            if (manifestContent == null)
            {
                return;
            }

            CrowdinDistributionManifest manifest = JsonConvert.DeserializeObject<CrowdinDistributionManifest>(manifestContent);

            await LoadLocalizationSheetsAsync(manifest, cancellationToken);
        }

        public async Task DownloadLocalizationsAsync(CancellationToken cancellationToken)
        {
            string manifestContent = await GetManifestContentAsync();

            if (manifestContent == null)
            {
                return;
            }

            CrowdinDistributionManifest manifest = JsonConvert.DeserializeObject<CrowdinDistributionManifest>(manifestContent);

            if (!await CheckIfUpdateAvailableAsync(manifest))
            {
                _logger.Info("Translations are up-to-date");
                await LoadLocalizationSheetsAsync(manifest, cancellationToken);
                return;
            }

            // wipe existing files to avoid conflicts if names changed
            if (Directory.Exists(kDownloadedFolder))
            {
                Directory.Delete(kDownloadedFolder, true);
            }

            Directory.CreateDirectory(kDownloadedFolder);

            foreach (string fileName in manifest.files)
            {
                // file name has a leading slash so we have to remove that
                string relativeFilePath = fileName.Substring(1);
                string fullPath = Path.Combine(kDownloadedFolder, relativeFilePath);
                string id = fileName.Substring(1, fileName.Length - 5);

                if (!LocalizationDefinition.IsDefinitionLoaded(id))
                {
                    _logger.Trace($"'{id}' does not belong to a loaded LocalizedPlugin; ignored");
                    continue;
                }

                await DownloadFileAsync($"{kCrowdinHost}/{kDistributionKey}/content/{relativeFilePath}?timestamp={manifest.timestamp}", fullPath);
            }

            using (var writer = new StreamWriter(kManifestFilePath))
            {
                await writer.WriteAsync(manifestContent);
            }

            await LoadLocalizationSheetsAsync(manifest, cancellationToken);
        }

        public async Task<bool> CheckForUpdatesAsync()
        {
            string manifestContent = await GetManifestContentAsync();

            if (manifestContent == null)
            {
                return false;
            }

            CrowdinDistributionManifest manifest = JsonConvert.DeserializeObject<CrowdinDistributionManifest>(manifestContent);

            return await CheckIfUpdateAvailableAsync(manifest);
        }

        private async Task<string> GetManifestContentAsync()
        {
            string url = $"{kCrowdinHost}/{kDistributionKey}/manifest.json";
            string manifestContent;

            _logger.Info($"Fetching Crowdin data at '{url}'");

            using (var request = UnityWebRequest.Get(url))
            {
                UnityWebRequestAsyncOperation asyncOperation = await request.SendWebRequest();

                if (!asyncOperation.isDone)
                {
                    _logger.Error($"UnityWebRequest for '{url}' failed");
                    return null;
                }

                if (!request.IsSuccessResponseCode())
                {
                    _logger.Error($"'{url}' responded with {request.responseCode} ({request.error})");
                    return null;
                }

                manifestContent = request.downloadHandler.text;
            }

            return manifestContent;
        }

        private async Task<bool> CheckIfUpdateAvailableAsync(CrowdinDistributionManifest remoteManifest)
        {
            if (!File.Exists(kManifestFilePath) || !Directory.Exists(kDownloadedFolder)) return true;

            foreach (string fileName in remoteManifest.files)
            {
                string id = fileName.Substring(1, fileName.Length - 5);
                string fullPath = Path.Combine(kDownloadedFolder, fileName.Substring(1));

                if (LocalizationDefinition.IsDefinitionLoaded(id) && !File.Exists(fullPath))
                {
                    return true;
                }
            }

            CrowdinDistributionManifest localManifest = await ReadLocalManifestAsync();

            return localManifest.timestamp != remoteManifest.timestamp;
        }

        private async Task<CrowdinDistributionManifest> ReadLocalManifestAsync()
        {
            using FileStream file = File.OpenRead(kManifestFilePath);
            using StreamReader reader = new(file);

            return JsonConvert.DeserializeObject<CrowdinDistributionManifest>(await reader.ReadToEndAsync());
        }

        private async Task DownloadFileAsync(string url, string filePath)
        {
            _logger.Info($"Downloading '{url}'");

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Accept-Encoding", "gzip");

                UnityWebRequestAsyncOperation asyncOperation = await request.SendWebRequest();

                if (!asyncOperation.isDone)
                {
                    _logger.Error($"UnityWebRequest for '{url}' failed");
                    return;
                }

                if (!request.IsSuccessResponseCode())
                {
                    _logger.Error($"'{url}' responded with {request.responseCode} ({request.error})");
                    return;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var contentStream = new MemoryStream(request.downloadHandler.data))
                using (var file = new FileStream(filePath, FileMode.Create))
                {
                    if (request.GetResponseHeader("Content-Encoding") == "gzip")
                    {
                        using (var gzipStream = new GZipStream(contentStream, CompressionMode.Decompress))
                        {
                            await gzipStream.CopyToAsync(file);
                        }
                    }
                    else
                    {
                        await contentStream.CopyToAsync(file);
                    }

                    await file.FlushAsync();
                }
            }
        }

        private async Task LoadLocalizationSheetsAsync(CrowdinDistributionManifest manifest, CancellationToken cancellationToken)
        {
            ClearLoadedAssets();

            if (!Directory.Exists(kDownloadedFolder))
            {
                return;
            }

            foreach (string fileName in manifest.files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string id = fileName.Substring(1, fileName.Length - 5);
                string fullPath = Path.Combine(kDownloadedFolder, fileName.Substring(1));

                if (LocalizationDefinition.IsDefinitionLoaded(id))
                {
                    if (File.Exists(fullPath))
                    {
                        await AddLocalizationSheetFromFileAsync(fullPath);
                    }
                    else
                    {
                        _logger.Error($"File '{fullPath}' not found");
                    }
                }
                else
                {
                    _logger.Warn($"No localized plugin registered for '{id}'");
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            LocalizationImporter.Refresh();
        }

        private void ClearLoadedAssets()
        {
            foreach (LocalizationAsset asset in _loadedAssets)
            {
                _localizationManager.DeregisterTranslation(asset);
            }

            _loadedAssets.Clear();
        }

        private async Task AddLocalizationSheetFromFileAsync(string filePath)
        {
            _logger.Info($"Adding '{filePath}'");

            using (var reader = new StreamReader(filePath))
            {
                string text = await reader.ReadToEndAsync();

                var localizationAsset = new LocalizationAsset { TextAsset = new TextAsset(text), Format = GoogleDriveDownloadFormat.CSV };
                _localizationManager.RegisterTranslation(localizationAsset);
                _loadedAssets.Add(localizationAsset);
            }
        }
    }
}
