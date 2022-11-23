using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraLocalizer.Features;
using SiraLocalizer.Records;
using SiraLocalizer.Utilities;
using SiraUtil.Logging;
using UnityEngine.Networking;

namespace SiraLocalizer.Providers.Crowdin
{
    internal class CrowdinDownloader : ILocalizationProvider, ILocalizationDownloader
    {
        private const string kCrowdinHost = "https://distributions.crowdin.net";
        private const string kDistributionKey = "b8d0ace786d64ba14775878o9lk";

        private static readonly string kDataFolder = Path.Combine(UnityGame.UserDataPath, "SiraLocalizer");
        private static readonly string kLocalizationsFolder = Path.Combine(kDataFolder, "Localizations", "Downloaded");
        private static readonly string kDownloadedFolder = Path.Combine(kLocalizationsFolder, "Content");
        private static readonly string kManifestFilePath = Path.Combine(kLocalizationsFolder, "manifest.json");

        private static readonly Regex kValidPathRegex = new(@"^\/[A-Za-z\-_]+(?:\/[A-Za-z\-_]+)*\.csv$");

        private readonly SiraLog _logger;

        internal CrowdinDownloader(SiraLog logger)
        {
            _logger = logger;
        }

        public string name => "Crowdin";

        public async IAsyncEnumerable<LocalizationFile> GetLocalizationAssetsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(kDownloadedFolder))
            {
                yield break;
            }

            CrowdinDistributionManifest manifest = await ReadLocalManifestAsync();

            foreach (string filePath in manifest.files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ParsedPathData parsed = ParsePath(filePath);

                if (!LocalizationDefinition.IsDefinitionLoaded(parsed.id))
                {
                    _logger.Warn($"No localized plugin registered for '{parsed.id}'");
                    continue;
                }

                if (!File.Exists(parsed.pathOnDisk))
                {
                    _logger.Error($"File '{parsed.pathOnDisk}' not found");
                    continue;
                }

                using FileStream fileStream = File.OpenRead(parsed.pathOnDisk);
                using GZipStream gzipStream = new(fileStream, CompressionMode.Decompress);
                using StreamReader reader = new(gzipStream);

                string content = await reader.ReadToEndAsync();
                yield return new LocalizationFile(content, 1000);
            }
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
                return;
            }

            // wipe existing files to avoid conflicts if names changed
            if (Directory.Exists(kDownloadedFolder))
            {
                Directory.Delete(kDownloadedFolder, true);
            }

            Directory.CreateDirectory(kDownloadedFolder);

            foreach (string filePath in manifest.files)
            {
                ParsedPathData parsed = ParsePath(filePath);

                if (!LocalizationDefinition.IsDefinitionLoaded(parsed.id))
                {
                    _logger.Trace($"'{parsed.id}' does not belong to a loaded {nameof(LocalizedPlugin)}; ignored");
                    continue;
                }

                await DownloadFileAsync($"{kCrowdinHost}/{kDistributionKey}/content/{parsed.relativePath}?timestamp={manifest.timestamp}", parsed.pathOnDisk);
            }

            StreamWriter writer = new(kManifestFilePath);
            await writer.WriteAsync(manifestContent);
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

            foreach (string filePath in remoteManifest.files)
            {
                ParsedPathData parsed = ParsePath(filePath);

                if (LocalizationDefinition.IsDefinitionLoaded(parsed.id) && !File.Exists(parsed.pathOnDisk))
                {
                    return true;
                }
            }

            CrowdinDistributionManifest localManifest = await ReadLocalManifestAsync();

            return localManifest.timestamp != remoteManifest.timestamp;
        }

        private ParsedPathData ParsePath(string filePath)
        {
            if (!kValidPathRegex.IsMatch(filePath))
            {
                throw new ArgumentException($"Path '{filePath}' is invalid", nameof(filePath));
            }

            string relativePath = filePath.Substring(1);
            string pathOnDisk = Path.Combine(kDownloadedFolder, relativePath) + ".gz";
            string id = Path.ChangeExtension(relativePath, null);

            return new ParsedPathData
            {
                id = id,
                pathOnDisk = pathOnDisk,
                relativePath = relativePath,
            };
        }

        private struct ParsedPathData
        {
            public string id { get; init; }

            public string pathOnDisk { get; init; }

            public string relativePath { get; init; }
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

            using var request = UnityWebRequest.Get(url);

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

            using var contentStream = new MemoryStream(request.downloadHandler.data);
            using var fileStream = new FileStream(filePath, FileMode.Create);

            if (request.GetResponseHeader("Content-Encoding") == "gzip")
            {
                await contentStream.CopyToAsync(fileStream);
            }
            else
            {
                using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
                await contentStream.CopyToAsync(gzipStream);
            }

            await fileStream.FlushAsync();
        }
    }
}
