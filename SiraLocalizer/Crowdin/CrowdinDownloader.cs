using Newtonsoft.Json;
using Polyglot;
using SiraUtil.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SiraLocalizer.Crowdin
{
    public class CrowdinDownloader : IInitializable
    {
        private const string kCrowdinHost = "https://distributions.crowdin.net";
        private const string kDistributionKey = "ba7660f1409c7f368c973c8o9lk";

        private const string kLanguagesUrl = "https://gitcdn.link/repo/Auros/SiraLocalizer/main/SiraLocalizer/Resources/languages.txt";
        private const string kContributorsUrl = "https://gitcdn.link/repo/Auros/SiraLocalizer/main/SiraLocalizer/Resources/contributors.csv";

        private static readonly string kDataFolder = Path.Combine(Application.persistentDataPath, "SiraLocalizer");
        private static readonly string kLocalizationsFolder = Path.Combine(kDataFolder, "Localizations");
        private static readonly string kContentFolder = Path.Combine(kLocalizationsFolder, "Content");
        private static readonly string kManifestFilePath = Path.Combine(kLocalizationsFolder, "manifest.json");
        private static readonly string kContributorsFilePath = Path.Combine(kDataFolder, "contributors.csv");

        internal static readonly string kLanguagesFilePath = Path.Combine(kDataFolder, "languages.txt");

        private readonly ILocalizer _localizer;
        private readonly Config _config;
        private readonly List<LocalizationAsset> _loadedAssets;

        internal CrowdinDownloader([Inject(Id = "SIRA.Localizer")] ILocalizer localizer, Config config)
        {
            _localizer = localizer;
            _config = config;
            _loadedAssets = new List<LocalizationAsset>();
        }

        public async void Initialize()
        {
            if (!_config.autoDownloadNewLocalizations) return;

            try
            {
                await DownloadLocalizations();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Failed to load Crowdin translations");
                Plugin.Log.Error(ex);
            }
        }

        public async Task DownloadLocalizations()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            Task loadTask = LoadLocalizationSheets(cancellationTokenSource.Token);

            using (var client = new HttpClient())
            {
                string url = $"{kCrowdinHost}/{kDistributionKey}/manifest.json";
                Plugin.Log.Info($"Fetching Crowdin data at '{url}'");
                HttpResponseMessage response = await client.GetAsync(url);

                string manifestContent = await response.Content.ReadAsStringAsync();
                CrowdinDistributionManifest manifest = JsonConvert.DeserializeObject<CrowdinDistributionManifest>(manifestContent);

                if (!await ShouldDownloadContent(manifest))
                {
                    Plugin.Log.Info("Translations are up-to-date");
                    return;
                }

                // cancel and wait for completion (and ignore result)
                cancellationTokenSource.Cancel();
                await loadTask.ContinueWith(t => { });

                if (Directory.Exists(kContentFolder))
                {
                    Directory.Delete(kContentFolder, true);
                }

                Directory.CreateDirectory(kContentFolder);

                foreach (var fileName in manifest.Files)
                {
                    // file name has a leading slash so we have to remove that
                    string filePath = Path.Combine(kContentFolder, fileName.Substring(1));
                    await DownloadFile(client, $"{kCrowdinHost}/{kDistributionKey}/content{fileName}", filePath);
                }

                // always redownload contributors & available languages if translations changed since we don't currently have a way to figure out if those files have changed
                await DownloadFile(client, kContributorsUrl, kContributorsFilePath);
                await DownloadFile(client, kLanguagesUrl, kLanguagesFilePath);

                using (var writer = new StreamWriter(kManifestFilePath))
                {
                    await writer.WriteAsync(manifestContent);
                }

                await LoadLocalizationSheets(CancellationToken.None);
            }
        }

        private async Task<bool> ShouldDownloadContent(CrowdinDistributionManifest remoteManifest)
        {
            if (!File.Exists(kManifestFilePath) || !File.Exists(kContributorsFilePath) || !File.Exists(kLanguagesFilePath)) return true;
            if (!Directory.Exists(kContentFolder)) return true;

            foreach (string fileName in remoteManifest.Files)
            {
                if (!File.Exists(Path.Combine(kContentFolder, fileName.Substring(1)))) return true;
            }

            CrowdinDistributionManifest localManifest = null;

            using (FileStream file = File.OpenRead(kManifestFilePath))
            using (var reader = new StreamReader(file))
            {
                localManifest = JsonConvert.DeserializeObject<CrowdinDistributionManifest>(await reader.ReadToEndAsync());
            }

            return localManifest.Timestamp != remoteManifest.Timestamp;
        }

        private async Task DownloadFile(HttpClient client, string url, string filePath)
        {
            Plugin.Log.Info($"Downloading '{url}'");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept-Encoding", "gzip");

            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Plugin.Log.Error($"'{url}' responded with {(int)response.StatusCode} {response.StatusCode} ({response.ReasonPhrase})");
                return;
            }

            Stream contentStream = await response.Content.ReadAsStreamAsync();

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (FileStream file = File.OpenWrite(filePath))
            {
                if (response.Headers.TryGetValues("Content-Encoding", out IEnumerable<string> values) && values.Contains("gzip"))
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

        private async Task LoadLocalizationSheets(CancellationToken cancellationToken)
        {
            foreach (LocalizationAsset asset in _loadedAssets)
            {
                _localizer.RemoveLocalizationSheet(asset);
            }

            _loadedAssets.Clear();

            if (Directory.Exists(kContentFolder))
            {
                foreach (string filePath in Directory.EnumerateFiles(kContentFolder, "*.csv", SearchOption.TopDirectoryOnly))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await AddLocalizationSheetFromFile(filePath);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (File.Exists(kContributorsFilePath))
            {
                await AddLocalizationSheetFromFile(kContributorsFilePath);
            }

            _localizer.RecalculateLanguages();
        }

        private async Task AddLocalizationSheetFromFile(string filePath)
        {
            Plugin.Log.Info($"Adding '{filePath}'");

            using (StreamReader reader = new StreamReader(filePath))
            {
                string text = await reader.ReadToEndAsync();

                _loadedAssets.Add(_localizer.AddLocalizationSheet(text, GoogleDriveDownloadFormat.CSV, filePath));
            }
        }
    }
}
