using IPA.Utilities;
using Newtonsoft.Json;
using Polyglot;
using SiraLocalizer.Utilities;
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
                Plugin.log.Error("Failed to load Crowdin translations");
                Plugin.log.Error(ex);
            }
        }

        public void Dispose()
        {
            ClearLoadedAssets();
        }

        public async Task DownloadLocalizations()
        {
            string url = $"{kCrowdinHost}/{kDistributionKey}/manifest.json";
            string manifestContent;

            Plugin.log.Info($"Fetching Crowdin data at '{url}'");

            using (var request = UnityWebRequest.Get(url))
            {
                UnityWebRequestAsyncOperation asyncOperation = await request.SendWebRequest();

                if (!asyncOperation.isDone)
                {
                    Plugin.log.Error($"UnityWebRequest for '{url}' failed");
                    return;
                }

                if (!request.IsSuccessResponseCode())
                {
                    Plugin.log.Error($"'{url}' responded with {request.responseCode} ({request.error})");
                    return;
                }

                manifestContent = request.downloadHandler.text;
            }

            var cancellationTokenSource = new CancellationTokenSource();
            CrowdinDistributionManifest manifest = JsonConvert.DeserializeObject<CrowdinDistributionManifest>(manifestContent);

            Task loadTask = LoadLocalizationSheets(manifest, cancellationTokenSource.Token);

            if (!await ShouldDownloadContent(manifest))
            {
                Plugin.log.Info("Translations are up-to-date");
                return;
            }

            // cancel and wait for completion (and ignore result)
            cancellationTokenSource.Cancel();
            await loadTask.ContinueWith(t => { });

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
                    Plugin.log.Trace($"'{id}' does not belong to a loaded LocalizedPlugin; ignored");
                    continue;
                }

                await DownloadFile($"{kCrowdinHost}/{kDistributionKey}/content/{relativeFilePath}", fullPath);
            }

            using (var writer = new StreamWriter(kManifestFilePath))
            {
                await writer.WriteAsync(manifestContent);
            }

            await LoadLocalizationSheets(manifest, CancellationToken.None);
        }

        private async Task<bool> ShouldDownloadContent(CrowdinDistributionManifest remoteManifest)
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

            CrowdinDistributionManifest localManifest = null;

            using (FileStream file = File.OpenRead(kManifestFilePath))
            using (var reader = new StreamReader(file))
            {
                localManifest = JsonConvert.DeserializeObject<CrowdinDistributionManifest>(await reader.ReadToEndAsync());
            }

            return localManifest.timestamp != remoteManifest.timestamp;
        }

        private async Task DownloadFile(string url, string filePath)
        {
            Plugin.log.Info($"Downloading '{url}'");

            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Accept-Encoding", "gzip");

                UnityWebRequestAsyncOperation asyncOperation = await request.SendWebRequest();

                if (!asyncOperation.isDone)
                {
                    Plugin.log.Error($"UnityWebRequest for '{url}' failed");
                    return;
                }

                if (!request.IsSuccessResponseCode())
                {
                    Plugin.log.Error($"'{url}' responded with {request.responseCode} ({request.error})");
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

        private async Task LoadLocalizationSheets(CrowdinDistributionManifest manifest, CancellationToken cancellationToken)
        {
            ClearLoadedAssets();

            if (Directory.Exists(kDownloadedFolder))
            {
                foreach (string fileName in manifest.files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string id = fileName.Substring(1, fileName.Length - 5);
                    string fullPath = Path.Combine(kDownloadedFolder, fileName.Substring(1));

                    if (LocalizationDefinition.IsDefinitionLoaded(id))
                    {
                        if (File.Exists(fullPath))
                        {
                            await AddLocalizationSheetFromFile(fullPath);
                        }
                        else
                        {
                            Plugin.log.Error($"File '{fullPath}' not found");
                        }
                    }
                    else
                    {
                        Plugin.log.Warn($"No localized plugin registered for '{id}'");
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            LocalizationImporter.Refresh();
        }

        private void ClearLoadedAssets()
        {
            foreach (LocalizationAsset asset in _loadedAssets)
            {
                _localizer.DeregisterTranslation(asset);
            }

            _loadedAssets.Clear();
        }

        private async Task AddLocalizationSheetFromFile(string filePath)
        {
            Plugin.log.Info($"Adding '{filePath}'");

            using (var reader = new StreamReader(filePath))
            {
                string text = await reader.ReadToEndAsync();

                var localizationAsset = new LocalizationAsset { TextAsset = new TextAsset(text), Format = GoogleDriveDownloadFormat.CSV };
                _localizer.RegisterTranslation(localizationAsset);
                _loadedAssets.Add(localizationAsset);
            }
        }
    }
}
