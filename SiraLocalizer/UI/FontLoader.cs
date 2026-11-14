using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BGLib.Polyglot;
using SiraLocalizer.Utilities;
using SiraUtil.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Zenject;
using Object = UnityEngine.Object;

namespace SiraLocalizer.UI
{
    internal class FontLoader : IInitializable, IDisposable
    {
        // The faces of SourceHanSans-Medium.ttc cover the same characters, but each draws the Han characters shared between
        // languages (e.g. 关) the way its region does. This one is the Japanese face; the others replace it for their language.
        private const string kDefaultCjkFontName = "SourceHanSans-Medium SDF";
        private static readonly Dictionary<LocalizationLanguage, string> kRegionalCjkFontNames = new()
        {
            { LocalizationLanguage.Simplified_Chinese, "SourceHanSansSC-Medium SDF" },
            { LocalizationLanguage.Traditional_Chinese, "SourceHanSansTC-Medium SDF" },
            { LocalizationLanguage.Korean, "SourceHanSansK-Medium SDF" },
        };

        private static readonly string[] kFontNamesToRemove = ["NotoSansJP-Medium SDF", "NotoSansKR-Medium SDF", "SourceHanSansCN-Bold-SDF-Common-1(2k)", "SourceHanSansCN-Bold-SDF-Common-2(2k)", "SourceHanSansCN-Bold-SDF-Uncommon(2k)"];
        private static readonly FontReplacementStrategy[] kFontReplacementStrategies =
        [
            new FontReplacementStrategy(
                ["Teko-Medium SDF"],
                ["Teko-Medium SDF Latin-1 Supplement", "Oswald-Medium SDF Cyrillic", "NotoSansHebrew-Medium SDF", kDefaultCjkFontName]),
            new FontReplacementStrategy(
                ["Teko-Bold SDF"],
                ["Teko-Bold SDF Latin-1 Supplement", "Oswald-Bold SDF Cyrillic", "NotoSansHebrew-Bold SDF", kDefaultCjkFontName]),
        ];

        private readonly SiraLog _logger;
        private readonly LocalizationModel _localizationModel;

        private readonly List<TMP_FontAsset> _fallbackFontAssets = [];
        private readonly List<TMP_FontAsset> _processedFontAssets = [];

        internal TMP_FontAsset tekoBoldFontAsset { get; private set; }

        public FontLoader(SiraLog logger, LocalizationModel localizationModel)
        {
            _logger = logger;
            _localizationModel = localizationModel;
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
                fontAsset.fallbackFontAssetTable.RemoveAll(_fallbackFontAssets.Contains);
            }

            foreach (TMP_FontAsset fontAsset in _fallbackFontAssets)
            {
                Object.Destroy(fontAsset);
            }

            _processedFontAssets.Clear();
            _fallbackFontAssets.Clear();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyFallbackFonts();
        }

        private async Task LoadFontAssetsAsync()
        {
            _logger.Info($"Loading fonts");

            await Task.WhenAll(LoadFallbackFontsAsync(), LoadTekoBoldAsync());

            ApplyFallbackFonts();
        }

        private async Task LoadTekoBoldAsync()
        {
            AsyncOperationHandle<TMP_FontAsset> asyncOperationHandle = await Addressables.LoadAssetAsync<TMP_FontAsset>("Assets/Visuals/Fonts/Teko-Bold/Teko-Bold SDF.asset");
            tekoBoldFontAsset = asyncOperationHandle.Result;
        }

        private async Task LoadFallbackFontsAsync()
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SiraLocalizer.Resources.Assets");
            AssetBundleCreateRequest assetBundleCreateRequest = await AssetBundle.LoadFromStreamAsync(stream);
            AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;

            if (assetBundle == null)
            {
                _logger.Error("Failed to load fonts asset bundle; some characters may not display as expected");
                return;
            }

            await assetBundle.LoadAllAssetsAsync();

            foreach (string fontName in kFontReplacementStrategies.SelectMany(s => s.fontNamesToAdd).Concat(kRegionalCjkFontNames.Values).Distinct())
            {
                LoadFontAsset(assetBundle, fontName);
            }

            await assetBundle.UnloadAsync(false);
        }

        private void LoadFontAsset(AssetBundle assetBundle, string name)
        {
            // this is basically instant since we've called LoadAllAssetsAsync previously
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
            TMP_FontAsset cjkFontAsset = GetCjkFontAsset();

            foreach (FontReplacementStrategy strategy in kFontReplacementStrategies)
            {
                var fallbacks = strategy.fontNamesToAdd.Select(n => n == kDefaultCjkFontName ? cjkFontAsset : FindFallbackFontAsset(n)).Where(f => f != null).ToList();

                foreach (TMP_FontAsset fontAsset in fontAssets.Where(f => !_processedFontAssets.Contains(f) && strategy.targetFontNames.Contains(f.name)))
                {
                    AddFallbacksToFont(fontAsset, fallbacks);
                }
            }

            // force update any text that has already rendered
            foreach (TMP_Text text in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                text.SetAllDirty();
            }
        }

        private TMP_FontAsset FindFallbackFontAsset(string name)
        {
            return _fallbackFontAssets.Find(f => f.name == name);
        }

        private TMP_FontAsset GetCjkFontAsset()
        {
            LocalizationLanguage language = _localizationModel.SelectedLanguage;

            if (kRegionalCjkFontNames.TryGetValue(language, out string name) && FindFallbackFontAsset(name) is TMP_FontAsset fontAsset)
            {
                return fontAsset;
            }

            return FindFallbackFontAsset(kDefaultCjkFontName);
        }

        private void AddFallbacksToFont(TMP_FontAsset fontAsset, List<TMP_FontAsset> fallbacks)
        {
            _logger.Info($"Adding fallbacks to '{fontAsset.name}' ({fontAsset.GetInstanceID()}): {string.Join(", ", fallbacks.Select(f => f.name))}");

            fontAsset.fallbackFontAssetTable.RemoveAll(f => kFontNamesToRemove.Contains(f.name));
            fontAsset.fallbackFontAssetTable.InsertRange(0, fallbacks);

            if (fontAsset.m_CharacterLookupDictionary != null)
            {
                fontAsset.ClearFallbackCharacterTable();
            }

            _processedFontAssets.Add(fontAsset);
        }

        private record FontReplacementStrategy(string[] targetFontNames, string[] fontNamesToAdd);
    }
}
