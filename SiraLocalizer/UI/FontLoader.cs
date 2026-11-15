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
    internal class FontLoader : IInitializable, IDisposable, ILocalize
    {
        private static readonly string[] kFontNamesToRemove = ["NotoSansJP-Medium SDF", "NotoSansKR-Medium SDF", "SourceHanSansCN-Bold-SDF-Common-1(2k)", "SourceHanSansCN-Bold-SDF-Common-2(2k)", "SourceHanSansCN-Bold-SDF-Uncommon(2k)"];

        // The multiple faces of SourceHanSans cover the same characters, but each draws the Han characters
        // shared between languages (e.g. 关) the way its region does. The base one is the Japanese face.
        private static readonly ReplacementFont kCjkReplacementFont = new("SourceHanSans-Medium SDF", new Dictionary<Locale, string>()
        {
            { Locale.SimplifiedChinese, "SourceHanSansSC-Medium SDF" },
            { Locale.TraditionalChinese, "SourceHanSansTC-Medium SDF" },
            { Locale.Korean, "SourceHanSansK-Medium SDF" },
        });

        private static readonly FontReplacementStrategy[] kFontReplacementStrategies =
        [
            new FontReplacementStrategy(
                "Teko-Medium SDF",
                [new ReplacementFont("Teko-Medium SDF Latin-1 Supplement"), new ReplacementFont("Oswald-Medium SDF Cyrillic"), new ReplacementFont("NotoSansHebrew-Medium SDF"), kCjkReplacementFont]),
            new FontReplacementStrategy(
                "Teko-Bold SDF",
                [new ReplacementFont("Teko-Bold SDF Latin-1 Supplement"), new ReplacementFont("Oswald-Bold SDF Cyrillic"), new ReplacementFont("NotoSansHebrew-Bold SDF"), kCjkReplacementFont]),
        ];

        private readonly SiraLog _logger;

        private readonly List<TMP_FontAsset> _fallbackFontAssets = [];
        private readonly List<TMP_FontAsset> _processedFontAssets = [];

        internal TMP_FontAsset tekoBoldFontAsset { get; private set; }

        public FontLoader(SiraLog logger)
        {
            _logger = logger;
        }

        public async void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Localization.Instance.AddOnLocalizeEvent(this);

            try
            {
                await LoadFontAssetsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void OnLocalize(LocalizationModel localization)
        {
            ApplyFallbackFonts(true);
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Localization.Instance.RemoveOnLocalizeEvent(this);

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

            foreach (string fontName in kFontReplacementStrategies.SelectMany(s => s.fontsToAdd.SelectMany(f => f.allFontNames)).Distinct())
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

        private void ApplyFallbackFonts(bool reapply = false)
        {
            if (!_fallbackFontAssets.Any()) return;

            var locale = (Locale)Localization.Instance.SelectedLanguage;

            if (reapply)
            {
                foreach (TMP_FontAsset fontAsset in _processedFontAssets)
                {
                    fontAsset.fallbackFontAssetTable.RemoveAll(_fallbackFontAssets.Contains);
                }

                _processedFontAssets.Clear();
            }

            TMP_FontAsset[] fontAssets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();

            foreach (FontReplacementStrategy strategy in kFontReplacementStrategies)
            {
                IEnumerable<TMP_FontAsset> originalFontAssets = fontAssets.Where(f => !_processedFontAssets.Contains(f) && strategy.targetFontName == f.name);
                List<TMP_FontAsset> fallbacks = [.. strategy.fontsToAdd.Select(r => _fallbackFontAssets.Find(f => f.name == r.GetLocalizedFontName(locale))).Where(f => f != null)];

                foreach (TMP_FontAsset fontAsset in originalFontAssets)
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

        private record FontReplacementStrategy(string targetFontName, ReplacementFont[] fontsToAdd);

        private record ReplacementFont
        {
            private readonly Dictionary<Locale, string> _variants = [];
            private readonly string _defaultName;

            public ReplacementFont(string name)
            {
                _defaultName = name;

                allFontNames = [name];
            }

            public ReplacementFont(string defaultName, Dictionary<Locale, string> variants)
            {
                _defaultName = defaultName;
                _variants = variants;

                allFontNames = [.. variants.Values, defaultName];
            }

            public string[] allFontNames { get; }

            public string GetLocalizedFontName(Locale locale)
            {
                return _variants.TryGetValue(locale, out string name) ? name : _defaultName;
            }
        }
    }
}
