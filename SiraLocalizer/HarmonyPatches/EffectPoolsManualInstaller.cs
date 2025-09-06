using HarmonyLib;
using SiraLocalizer.UI;
using TMPro;
using Zenject;
using Object = UnityEngine.Object;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
    internal static class EffectPoolsManualInstaller_ManualInstallBindings
    {
        private static TMP_FontAsset _fontAsset;

        public static void Postfix(DiContainer container, bool shortBeatEffect, FlyingTextEffect ____flyingTextEffectPrefab)
        {
            FlyingTextEffect flyingTextEffect = CreateFlyingTextEffectPrefab(container, ____flyingTextEffectPrefab);
            container.BindMemoryPool<FlyingTextEffect, FlyingTextEffect.Pool>().WithInitialSize(20).FromComponentInNewPrefab(flyingTextEffect).WhenInjectedInto<ItalicizedFlyingTextSpawner>();
        }

        private static FlyingTextEffect CreateFlyingTextEffectPrefab(DiContainer container, FlyingTextEffect original)
        {
            FlyingTextEffect flyingTextEffect = Object.Instantiate(original);
            flyingTextEffect.name = "ItalicizedFlyingTextEffect";
            flyingTextEffect.gameObject.SetActive(false);

            TextMeshPro text = flyingTextEffect._text;
            text.font = GetFontAsset(container, text);
            text.fontStyle = FontStyles.Bold | FontStyles.Italic | FontStyles.UpperCase;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 3;

            return flyingTextEffect;
        }

        private static TMP_FontAsset GetFontAsset(DiContainer container, TextMeshPro text)
        {
            if (_fontAsset == null)
            {
                TMP_FontAsset original = container.Resolve<FontLoader>().tekoBoldFontAsset;
                _fontAsset = FontAssetHelper.CopyFontAsset(original, text.fontMaterial, $"{original.name} - ItalicizedFlyingTextEffect");
                _fontAsset.italicStyle = 18;
                _fontAsset.boldSpacing = 2f;
            }

            return _fontAsset;
        }
    }
}
