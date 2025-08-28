using System;
using BGLib.Polyglot;
using HarmonyLib;
using IPA.Utilities;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(LocalizationImporter), nameof(LocalizationImporter.ImportFromFiles))]
    internal static class LocalizationImporter_Initialize
    {
        public static void Postfix()
        {
            if (Localization._instance == null)
            {
                return;
            }

            Localization.Instance.GetField<Action<LocalizationModel>, LocalizationModel>("_onChangeLanguage")?.Invoke(Localization.Instance);
        }
    }

    [HarmonyPatch(typeof(Localization), nameof(Localization.SetSingletonInstance))]
    internal static class Localization_SetSingletonInstance
    {
        public static void Postfix()
        {
            Localization.Instance.GetField<Action<LocalizationModel>, LocalizationModel>("_onChangeLanguage")?.Invoke(Localization.Instance);
        }
    }
}
