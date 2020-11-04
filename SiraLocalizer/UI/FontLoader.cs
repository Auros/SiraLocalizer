using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace SiraLocalizer.UI
{
    internal class FontLoader : IInitializable, IDisposable
    {
        private static readonly string kLatin1SupplementFontName = "Teko-Medium SDF Latin-1 Supplement";
        private static readonly string kSimplifiedChineseFontName = "SourceHanSansSC-Medium SDF";

        private static readonly string[] kTargetFontNames = { "Teko-Medium SDF", "Teko-Medium SDF No Glow", "Teko-Medium SDF No Glow Billboard" };
        private static readonly string[] kFontNamesToRemove = { kLatin1SupplementFontName, kSimplifiedChineseFontName, "SourceHanSansCN-Bold-SDF-Common-1(2k)", "SourceHanSansCN-Bold-SDF-Common-2(2k)", "SourceHanSansCN-Bold-SDF-Uncommon(2k)" };

        private readonly GameScenesManager _gameScenesManager;
        
        private readonly List<TMP_FontAsset> _fallbackFontAssets = new List<TMP_FontAsset>();

        private FontLoader(GameScenesManager gameScenesManager)
        {
            _gameScenesManager = gameScenesManager;
        }

        public void Initialize()
        {
            _gameScenesManager.transitionDidFinishEvent += OnTransitionDidFinish;

            SharedCoroutineStarter.instance.StartCoroutine(LoadFontAssets());
        }

        public void Dispose()
        {
            _gameScenesManager.transitionDidFinishEvent -= OnTransitionDidFinish;
        }

        private void OnTransitionDidFinish(ScenesTransitionSetupDataSO setupData, DiContainer container)
        {
            AddFallbackFonts();
        }

        private IEnumerator LoadFontAssets()
        {
            Plugin.Log.Debug($"Loading fonts");

            AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromStreamAsync(Assembly.GetExecutingAssembly().GetManifestResourceStream("SiraLocalizer.Resources.fonts.assets"));
            yield return assetBundleCreateRequest;

            AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;

            if (!assetBundleCreateRequest.isDone || !assetBundle)
            {
                Plugin.Log.Error("Failed to load fonts asset bundle; some characters may not display as expected");
                yield break;
            }

            AddFont(assetBundle, kLatin1SupplementFontName);
            AddFont(assetBundle, kSimplifiedChineseFontName);

            assetBundle.Unload(false);

            AddFallbackFonts();
        }

        private void AddFont(AssetBundle assetBundle, string name)
        {
            TMP_FontAsset fontAsset = assetBundle.LoadAsset<TMP_FontAsset>(name);

            if (fontAsset == null)
            {
                Plugin.Log.Error($"Font '{name}' could not be loaded; some characters may not display as expected");
                return;
            }

            Plugin.Log.Debug($"Font '{name}' loaded successfully");

            _fallbackFontAssets.Add(fontAsset);
        }

        private void AddFallbackFonts()
        {
            if (_fallbackFontAssets == null) return;

            IEnumerable<TMP_FontAsset> originalFontAssets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().Where(f => kTargetFontNames.Contains(f.name));

            foreach (TMP_FontAsset fontAsset in originalFontAssets)
            {
                ApplyFallbacks(fontAsset, _fallbackFontAssets);
            }

            // force update any text that has already rendered
            foreach (TMP_Text text in Object.FindObjectsOfType<TMP_Text>())
            {
                text.SetAllDirty();
            }
        }

        private void ApplyFallbacks(TMP_FontAsset fontAsset, IList<TMP_FontAsset> fallbacks)
        {
            Plugin.Log.Debug($"Adding fallbacks to '{fontAsset.name}' ({(uint)fontAsset.GetHashCode()})");

            fontAsset.fallbackFontAssetTable.RemoveAll(f => kFontNamesToRemove.Contains(f.name));

            foreach (TMP_FontAsset fallback in fallbacks.Reverse())
            {
                TMP_FontAsset fallbackCopy = Object.Instantiate(fallback);

                fallbackCopy.name = fallback.name;
                fallbackCopy.material.shader = fontAsset.material.shader;
                fallbackCopy.material.shaderKeywords = fontAsset.material.shaderKeywords;

                // insert as first possible fallback fonts
                fontAsset.fallbackFontAssetTable.Insert(0, fallbackCopy);
            }
        }
    }
}
