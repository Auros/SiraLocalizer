using Newtonsoft.Json;
using Polyglot;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        private const string kDistributionKey = "b8d0ace786d64ba14775878o9lk";

        private static readonly string kDataFolder = Path.Combine(Application.persistentDataPath, "SiraLocalizer");
        private static readonly string kLocalizationsFolder = Path.Combine(kDataFolder, "Localizations");
        private static readonly string kContentFolder = Path.Combine(kLocalizationsFolder, "Content");
        private static readonly string kManifestFilePath = Path.Combine(kLocalizationsFolder, "manifest.json");

        private readonly Localizer _localizer;
        private readonly List<LocalizationAsset> _loadedAssets;

        internal CrowdinDownloader(Localizer localizer)
        {
            _localizer = localizer;
            _loadedAssets = new List<LocalizationAsset>();
        }

        public async void Initialize()
        {
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
            using (var client = new HttpClient())
            {
                var cancellationTokenSource = new CancellationTokenSource();

                string url = $"{kCrowdinHost}/{kDistributionKey}/manifest.json";
                Plugin.Log.Info($"Fetching Crowdin data at '{url}'");
                HttpResponseMessage response = await client.GetAsync(url);

                string manifestContent = await response.Content.ReadAsStringAsync();
                CrowdinDistributionManifest manifest = JsonConvert.DeserializeObject<CrowdinDistributionManifest>(manifestContent);

                Task loadTask = LoadLocalizationSheets(manifest, cancellationTokenSource.Token);

                if (await ShouldDownloadContent(manifest))
                {
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
                        string relativeFilePath = fileName.Substring(1);
                        string fullPath = Path.Combine(kContentFolder, relativeFilePath);
                        string id = fileName.Substring(1, fileName.Length - 5);

                        if (!LocalizationDefinition.IsDefinitionLoaded(id))
                        {
                            Plugin.Log.Trace($"'{id}' does not belong to a loaded LocalizedPlugin; ignored");
                            continue;
                        }

                        await DownloadFile(client, $"{kCrowdinHost}/{kDistributionKey}/content/{relativeFilePath}", fullPath);
                    }

                    using (var writer = new StreamWriter(kManifestFilePath))
                    {
                        await writer.WriteAsync(manifestContent);
                    }

                    loadTask = LoadLocalizationSheets(manifest, CancellationToken.None);
                }
                else
                {
                    Plugin.Log.Info("Translations are up-to-date");
                }
            }
        }

        private async Task<bool> ShouldDownloadContent(CrowdinDistributionManifest remoteManifest)
        {
            if (!File.Exists(kManifestFilePath) || !Directory.Exists(kContentFolder)) return true;

            foreach (string fileName in remoteManifest.Files)
            {
                string id = fileName.Substring(1, fileName.Length - 5);
                string fullPath = Path.Combine(kContentFolder, fileName.Substring(1));

                if (LocalizationDefinition.IsDefinitionLoaded(id) && !File.Exists(fullPath))
                {
                    return true;
                }
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

            HttpResponseMessage response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Plugin.Log.Error($"'{url}' responded with {(int)response.StatusCode} {response.StatusCode} ({response.ReasonPhrase})");
                return;
            }

            Stream contentStream = await response.Content.ReadAsStreamAsync();

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (FileStream file = new FileStream(filePath, FileMode.Create))
            {
                if (response.Content.Headers.ContentEncoding.Contains("gzip"))
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

        private async Task LoadLocalizationSheets(CrowdinDistributionManifest manifest, CancellationToken cancellationToken)
        {
            foreach (LocalizationAsset asset in _loadedAssets)
            {
                _localizer.DeregisterTranslation(asset);
            }

            _loadedAssets.Clear();

            if (Directory.Exists(kContentFolder))
            {
                foreach (string fileName in manifest.Files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string id = fileName.Substring(1, fileName.Length - 5);
                    string fullPath = Path.Combine(kContentFolder, fileName.Substring(1));

                    if (LocalizationDefinition.TryGetLoadedDefinition(id, out LocalizationDefinition def))
                    {
                        if (File.Exists(fullPath))
                        {
                            await AddLocalizationSheetFromFile(fullPath);
                        }
                        else
                        {
                            Plugin.Log.Error($"File '{fullPath}' not found");
                        }
                    }
                    else
                    {
                        Plugin.Log.Warn($"No localized plugin registered for '{id}'");
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            LocalizationImporter.Refresh();
        }

        private async Task AddLocalizationSheetFromFile(string filePath)
        {
            Plugin.Log.Info($"Adding '{filePath}'");

            using (StreamReader reader = new StreamReader(filePath))
            {
                string text = await reader.ReadToEndAsync();

                var localizationAsset = new LocalizationAsset { TextAsset = new TextAsset(text), Format = GoogleDriveDownloadFormat.CSV };
                _localizer.RegisterTranslation(localizationAsset);
                _loadedAssets.Add(localizationAsset);
            }
        }
    }
}
