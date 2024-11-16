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
            container.BindMemoryPool<FlyingTextEffect, FlyingTextEffect.Pool>().WithInitialSize(20).FromComponentInNewPrefab(CreateFlyingTextEffectPrefab(____flyingTextEffectPrefab)).WhenInjectedInto<ItalicizedFlyingTextSpawner>();
        }

        private static FlyingTextEffect CreateFlyingTextEffectPrefab(FlyingTextEffect original)
        {
            FlyingTextEffect flyingTextEffect = Object.Instantiate(original);
            flyingTextEffect.name = "ItalicizedFlyingTextEffect";
            flyingTextEffect.gameObject.SetActive(false);

            TextMeshPro text = flyingTextEffect._text;
            text.font = GetFontAsset(text);
            text.fontStyle = FontStyles.Italic | FontStyles.UpperCase;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 3;

            return flyingTextEffect;
        }

        private static TMP_FontAsset GetFontAsset(TextMeshPro text)
        {
            if (_fontAsset == null)
            {
                _fontAsset = FontAssetHelper.CopyFontAsset(FontLoader.tekoBoldFontAsset, text.fontMaterial);
                _fontAsset.italicStyle = 18;
            }

            return _fontAsset;
        }
    }
}
