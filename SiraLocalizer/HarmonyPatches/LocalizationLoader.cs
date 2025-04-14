using System.Collections.Generic;
using BGLib.Polyglot;
using HarmonyLib;
using SiraLocalizer.Records;
using UnityEngine;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(LocalizationAsyncInstaller), "LoadResourcesBeforeInstall")]
    internal class LocalizationLoader
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix(IList<TextAsset> assets)
        {
            LocalizationDefinition.Remove("beat-saber");
            LocalizationDefinition.Add("beat-saber", "Beat Saber", assets);
        }
    }
}
