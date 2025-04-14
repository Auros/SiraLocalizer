using System;
using BGLib.Polyglot;
using HarmonyLib;
using IPA.Utilities;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(LocalizationImporter), nameof(LocalizationImporter.Initialize))]
    internal static class LocalizationImporter_Initialize
    {
        public static void Postfix(LocalizationModel settings)
        {
            settings.GetField<Action<LocalizationModel>, LocalizationModel>("_onChangeLanguage")?.Invoke(settings);
        }
    }
}
