using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraLocalizer.Utilities;
using SiraUtil.Logging;
using UnityEngine.Networking;

namespace SiraLocalizer.Crowdin
{
    internal class CrowdinDownloader : ILocalizationProvider, ILocalizationDownloader
    {
        private const string kCrowdinHost = "https://distributions.crowdin.net";
        private const string kDistributionKey = "b8d0ace786d64ba14775878o9lk";

        private static readonly string kDataFolder = Path.Combine(UnityGame.UserDataPath, "SiraLocalizer");
        private static readonly string kLocalizationsFolder = Path.Combine(kDataFolder, "Localizations", "Downloaded");
        private static readonly string kDownloadedFolder = Path.Combine(kLocalizationsFolder, "Content");
        private static readonly string kManifestFilePath = Path.Combine(kLocalizationsFolder, "manifest.json");

        private readonly SiraLog _logger;

        internal CrowdinDownloader(SiraLog logger)
        {
            _logger = logger;
        }

        public string name => "Crowdin";

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
        }

        public async Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken)
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

            _logger.Info($"Fetching Crowdin manifest");

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

        public async IAsyncEnumerable<LocalizationFile> GetLocalizationAssetsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(kDownloadedFolder))
            {
                yield break;
            }

            CrowdinDistributionManifest manifest = await ReadLocalManifestAsync();

            foreach (string fileName in manifest.files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string id = fileName.Substring(1, fileName.Length - 5);
                string fullPath = Path.Combine(kDownloadedFolder, fileName.Substring(1));

                if (!LocalizationDefinition.IsDefinitionLoaded(id))
                {
                    _logger.Warn($"No localized plugin registered for '{id}'");
                    continue;
                }

                if (!File.Exists(fullPath))
                {
                    _logger.Error($"File '{fullPath}' not found");
                    continue;
                }

                using StreamReader reader = new(fullPath);
                string content = await reader.ReadToEndAsync();
                yield return new LocalizationFile(content, 1000);
            }
        }
    }
}
