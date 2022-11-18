using HarmonyLib;
using SiraLocalizer.UI;
using TMPro;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
    internal static class EffectPoolsManualInstaller_ManualInstallBindings
    {
        public static void Postfix(DiContainer container, bool shortBeatEffect, FlyingTextEffect ____flyingTextEffectPrefab)
        {
            container.BindMemoryPool<FlyingTextEffect, FlyingTextEffect.Pool>().WithInitialSize(20).FromComponentInNewPrefab(CreateFlyingTextEffectPrefab(____flyingTextEffectPrefab)).WhenInjectedInto<ItalicizedFlyingTextSpawner>();
        }

        private static FlyingTextEffect CreateFlyingTextEffectPrefab(FlyingTextEffect original)
        {
            GameObject gameObject = Object.Instantiate(original.gameObject);
            gameObject.SetActive(false);
            gameObject.name = "ItalicizedFlyingTextEffect";

            TextMeshPro text = gameObject.GetComponentInChildren<TextMeshPro>();
            text.fontStyle = FontStyles.Bold | FontStyles.Italic | FontStyles.UpperCase;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 3;

            return gameObject.GetComponent<FlyingTextEffect>();
        }
    }
}
