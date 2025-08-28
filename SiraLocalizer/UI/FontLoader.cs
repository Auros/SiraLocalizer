using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SiraLocalizer.Utilities;
using SiraUtil.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Zenject;
using Object = UnityEngine.Object;

namespace SiraLocalizer.UI
{
    internal class FontLoader : IInitializable, IDisposable
    {
        private static readonly string[] kFontNamesToRemove = ["NotoSansJP-Medium SDF", "NotoSansKR-Medium SDF", "SourceHanSansCN-Bold-SDF-Common-1(2k)", "SourceHanSansCN-Bold-SDF-Common-2(2k)", "SourceHanSansCN-Bold-SDF-Uncommon(2k)"];
        private static readonly FontReplacementStrategy[] kFontReplacementStrategies =
        [
            new FontReplacementStrategy(
                ["Teko-Medium SDF"],
                ["Teko-Medium SDF Latin-1 Supplement", "Oswald-Medium SDF Cyrillic", "NotoSansHebrew-Medium SDF", "SourceHanSans-Medium SDF"]),
            new FontReplacementStrategy(
                ["Teko-Bold SDF"],
                ["Teko-Bold SDF Latin-1 Supplement", "Oswald-Bold SDF Cyrillic", "NotoSansHebrew-Bold SDF", "SourceHanSans-Medium SDF"]),
        ];

        private readonly SiraLog _logger;

        private readonly List<TMP_FontAsset> _fallbackFontAssets = [];
        private readonly List<TMP_FontAsset> _createdFontAssets = [];
        private readonly List<TMP_FontAsset> _processedFontAssets = [];

        internal static TMP_FontAsset tekoBoldFontAsset { get; private set; }

        public FontLoader(SiraLog logger)
        {
            _logger = logger;
        }

        public async void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            try
            {
                await LoadFontAssetsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            foreach (TMP_FontAsset fontAsset in _processedFontAssets)
            {
                fontAsset.fallbackFontAssetTable.RemoveAll(f => _createdFontAssets.Contains(f) || _fallbackFontAssets.Contains(f));
            }

            foreach (TMP_FontAsset fontAsset in _createdFontAssets)
            {
                Object.Destroy(fontAsset);
            }

            foreach (TMP_FontAsset fontAsset in _fallbackFontAssets)
            {
                Object.Destroy(fontAsset);
            }

            _processedFontAssets.Clear();
            _createdFontAssets.Clear();
            _fallbackFontAssets.Clear();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyFallbackFonts();
        }

        private async Task LoadFontAssetsAsync()
        {
            _logger.Info($"Loading fonts");

            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SiraLocalizer.Resources.Assets");
            AssetBundleCreateRequest assetBundleCreateRequest = await AssetBundle.LoadFromStreamAsync(stream);
            AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;

            if (assetBundle == null)
            {
                _logger.Error("Failed to load fonts asset bundle; some characters may not display as expected");
                return;
            }

            foreach (string fontName in kFontReplacementStrategies.SelectMany(s => s.fontNamesToAdd).Distinct())
            {
                LoadFontAsset(assetBundle, fontName);
            }

            await assetBundle.UnloadAsync(false);

            tekoBoldFontAsset = await LoadTekoBoldAsync();

            ApplyFallbackFonts();
        }

        private Task<TMP_FontAsset> LoadTekoBoldAsync()
        {
            TaskCompletionSource<TMP_FontAsset> taskCompletionSource = new();
            AsyncOperationHandle<TMP_FontAsset> asyncOperationHandle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<TMP_FontAsset>("Assets/Visuals/Fonts/Teko-Bold/Teko-Bold SDF.asset");
            asyncOperationHandle.Completed += (aoh) =>
            {
                taskCompletionSource.SetResult(aoh.Result);
            };
            return taskCompletionSource.Task;
        }

        private void LoadFontAsset(AssetBundle assetBundle, string name)
        {
            TMP_FontAsset fontAsset = assetBundle.LoadAsset<TMP_FontAsset>(name);

            if (fontAsset == null)
            {
                _logger.Error($"Font '{name}' could not be loaded; some characters may not display as expected");
                return;
            }

            _logger.Info($"Font '{name}' loaded successfully");

            _fallbackFontAssets.Add(fontAsset);
        }

        private void ApplyFallbackFonts()
        {
            if (!_fallbackFontAssets.Any()) return;

            TMP_FontAsset[] fontAssets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();

            foreach (FontReplacementStrategy strategy in kFontReplacementStrategies)
            {
                IEnumerable<TMP_FontAsset> originalFontAssets = fontAssets.Where(f => !_processedFontAssets.Contains(f) && strategy.targetFontNames.Contains(f.name));

                foreach (TMP_FontAsset fontAsset in originalFontAssets)
                {
                    AddFallbacksToFont(fontAsset, strategy.fontNamesToAdd.Select(n => _fallbackFontAssets.Find(f => f.name == n)).Where(f => f != null));
                }

                // force update any text that has already rendered
                foreach (TMP_Text text in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
                {
                    text.SetAllDirty();
                }
            }
        }

        private void AddFallbacksToFont(TMP_FontAsset fontAsset, IEnumerable<TMP_FontAsset> fallbacks)
        {
            _logger.Info($"Adding fallbacks to '{fontAsset.name}' ({fontAsset.GetInstanceID()})");

            fontAsset.fallbackFontAssetTable.RemoveAll(f => kFontNamesToRemove.Contains(f.name));

            foreach (TMP_FontAsset fallback in fallbacks.Reverse())
            {
                // creating a copy is necessary to prevent fonts from overwriting each others' atlases
                TMP_FontAsset fallbackCopy = FontAssetHelper.CopyFontAsset(fallback, fontAsset.material);
                _createdFontAssets.Add(fallbackCopy);

                // insert as first possible fallback font
                fontAsset.fallbackFontAssetTable.Insert(0, fallbackCopy);
            }

            _processedFontAssets.Add(fontAsset);
        }

        private record FontReplacementStrategy(string[] targetFontNames, string[] fontNamesToAdd);
    }
}
