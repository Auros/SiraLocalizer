using TMPro;
using Zenject;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;

namespace SiraLocalizer.UI
{
    internal class FontLoader : IInitializable, IDisposable
    {
        private readonly string[] kFontNamesToRemove = { "NotoSansJP-Medium SDF", "NotoSansKR-Medium SDF", "SourceHanSansCN-Bold-SDF-Common-1(2k)", "SourceHanSansCN-Bold-SDF-Common-2(2k)", "SourceHanSansCN-Bold-SDF-Uncommon(2k)" };
        private readonly FontReplacementStrategy[] kFontReplacementStrategies = new[]
        {
            new FontReplacementStrategy
            {
                targetFontNames = new[] { "Teko-Medium SDF" },
                fontNamesToAdd = new[] { "Teko-Medium SDF Latin-1 Supplement", "Oswald-Medium SDF Cyrillic", "SourceHanSans-Medium SDF" }
            },
            new FontReplacementStrategy
            {
                targetFontNames = new[] { "Teko-Bold SDF" },
                fontNamesToAdd = new[] { "Teko-Bold SDF Latin-1 Supplement", "Oswald-Bold SDF Cyrillic", "SourceHanSans-Medium SDF" }
            },
        };

        private readonly FontAssetHelper _fontAssetHelper;

        private readonly List<TMP_FontAsset> _fallbackFontAssets = new List<TMP_FontAsset>();
        private readonly List<TMP_FontAsset> _processedFontAssets = new List<TMP_FontAsset>();

        public FontLoader(FontAssetHelper fontAssetHelper)
        {
            _fontAssetHelper = fontAssetHelper;
        }

        public void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            SharedCoroutineStarter.instance.StartCoroutine(LoadFontAssets());
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyFallbackFonts();
        }

        private IEnumerator LoadFontAssets()
        {
            Plugin.Log.Info($"Loading fonts");

            AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromStreamAsync(Assembly.GetExecutingAssembly().GetManifestResourceStream("SiraLocalizer.Resources.fonts.assets"));
            yield return assetBundleCreateRequest;

            AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;

            if (!assetBundleCreateRequest.isDone || !assetBundle)
            {
                Plugin.Log.Error("Failed to load fonts asset bundle; some characters may not display as expected");
                yield break;
            }

            foreach (string fontName in kFontReplacementStrategies.SelectMany(s => s.fontNamesToAdd).Distinct())
            {
                LoadFontAsset(assetBundle, fontName);
            }

            assetBundle.Unload(false);

            ApplyFallbackFonts();
        }

        private void LoadFontAsset(AssetBundle assetBundle, string name)
        {
            TMP_FontAsset fontAsset = assetBundle.LoadAsset<TMP_FontAsset>(name);

            if (fontAsset == null)
            {
                Plugin.Log.Error($"Font '{name}' could not be loaded; some characters may not display as expected");
                return;
            }

            Plugin.Log.Info($"Font '{name}' loaded successfully");

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
                    AddFallbacksToFont(fontAsset, strategy.fontNamesToAdd.Select(n => _fallbackFontAssets.Find(f => f.name == n)).Where(f => f));
                }

                // force update any text that has already rendered
                foreach (TMP_Text text in Object.FindObjectsOfType<TMP_Text>())
                {
                    text.SetAllDirty();
                }
            }
        }

        private void AddFallbacksToFont(TMP_FontAsset fontAsset, IEnumerable<TMP_FontAsset> fallbacks)
        {
            Plugin.Log.Info($"Adding fallbacks to '{fontAsset.name}' ({(uint)fontAsset.GetHashCode()})");

            fontAsset.fallbackFontAssetTable.RemoveAll(f => kFontNamesToRemove.Contains(f.name));

            foreach (TMP_FontAsset fallback in fallbacks.Reverse())
            {
                TMP_FontAsset fallbackCopy = _fontAssetHelper.CopyFontAsset(fallback, fontAsset.material);

                // insert as first possible fallback font
                fontAsset.fallbackFontAssetTable.Insert(0, fallbackCopy);
            }

            _processedFontAssets.Add(fontAsset);
        }

        private struct FontReplacementStrategy
        {
            public string[] targetFontNames;
            public string[] fontNamesToAdd;
        }
    }
}
