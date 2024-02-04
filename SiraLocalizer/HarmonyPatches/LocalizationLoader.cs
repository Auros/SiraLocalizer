using System.Collections.Generic;
using BGLib.Polyglot;
using HarmonyLib;
using SiraLocalizer.Records;
using UnityEngine;

namespace BeatSaberMarkupLanguage.Harmony_Patches
{
    [HarmonyPatch(typeof(LocalizationAsyncInstaller), "LoadResourcesBeforeInstall")]
    [HarmonyPriority(Priority.First)]
    internal class LocalizationLoader
    {
        public static void Prefix(IList<TextAsset> assets)
        {
            LocalizationDefinition.Remove("beat-saber");
            LocalizationDefinition.Add("beat-saber", "Beat Saber", assets);
        }
    }
}
