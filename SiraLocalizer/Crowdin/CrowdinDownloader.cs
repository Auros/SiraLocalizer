using Newtonsoft.Json;
using Polyglot;
using SiraUtil.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SiraLocalizer.Crowdin
{
    public class CrowdinDownloader : IInitializable
    {
        private const string kDistributionKey = "ba7660f1409c7f368c973c8o9lk";

        private static readonly string kLocalizationsFolder = Path.Combine(Application.persistentDataPath, "Localizations");
        private static readonly string kContentFolder = Path.Combine(kLocalizationsFolder, "Content");
        private static readonly string kManifestFilePath = Path.Combine(kLocalizationsFolder, "manifest.json");

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
                LoadLocalizationSheets();

                if (await DownloadLocalizations())
                {
                    Plugin.Log.Info("Reloading localization assets");
                    LoadLocalizationSheets();
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Failed to load Crowdin translations");
                Plugin.Log.Error(ex);
            }
        }

        public async Task<bool> DownloadLocalizations()
        {
            using (var client = new HttpClient())
            {
                string url = $"https://distributions.crowdin.net/{kDistributionKey}/manifest.json";
                Plugin.Log.Info($"Fetching Crowdin data at '{url}'");
                HttpResponseMessage response = await client.GetAsync(url);

                string manifestContent = await response.Content.ReadAsStringAsync();
                CrowdinDistributionManifest manifest = JsonConvert.DeserializeObject<CrowdinDistributionManifest>(manifestContent);

                if (!await ShouldDownloadContent(manifest)) return false;

                if (Directory.Exists(kContentFolder))
                {
                    Directory.Delete(kContentFolder, true);
                }

                Directory.CreateDirectory(kContentFolder);

                foreach (var fileName in manifest.Files)
                {
                    url = $"https://distributions.crowdin.net/{kDistributionKey}/content{fileName}";
                    Plugin.Log.Info($"Downloading '{url}'");

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("Accept-Encoding", "gzip");

                    response = await client.SendAsync(request);

                    using (FileStream file = File.OpenWrite(Path.Combine(kContentFolder, fileName.Substring(1))))
                    using (var gzipStream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress))
                    {
                        await gzipStream.CopyToAsync(file);
                        await file.FlushAsync();
                    }
                }

                using (var writer = new StreamWriter(kManifestFilePath))
                {
                    await writer.WriteAsync(manifestContent);
                }

                return true;
            }
        }

        private async Task<bool> ShouldDownloadContent(CrowdinDistributionManifest remoteManifest)
        {
            if (!File.Exists(kManifestFilePath)) return true;
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

        private void LoadLocalizationSheets()
        {
            foreach (LocalizationAsset asset in _loadedAssets)
            {
                _localizer.RemoveLocalizationSheet(asset);
            }

            _loadedAssets.Clear();

            if (!Directory.Exists(kContentFolder)) return;

            foreach (string filePath in Directory.EnumerateFiles(kContentFolder, "*.csv"))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    Plugin.Log.Info($"Adding '{filePath}'");
                    _loadedAssets.Add(_localizer.AddLocalizationSheet(reader.ReadToEnd(), GoogleDriveDownloadFormat.CSV, filePath));
                }
            }

            _localizer.RecalculateLanguages();
        }
    }
}
